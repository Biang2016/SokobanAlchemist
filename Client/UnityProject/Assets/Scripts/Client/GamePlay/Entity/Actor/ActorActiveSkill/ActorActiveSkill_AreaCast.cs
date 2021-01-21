using System;
using System.Collections;
using System.Collections.Generic;
using BiangLibrary;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public abstract class ActorActiveSkill_AreaCast : ActorActiveSkill
{
    public enum CastAreaType
    {
        /// <summary>
        /// R=1单点；R=2十字形；……
        /// 要求中心点在施法范围内
        /// </summary>
        CircleCast,

        /// <summary>
        /// 要求整个效果矩形范围在施法范围内
        /// </summary>
        RectCast,

        /// <summary>
        /// 角色面前的带状区域，沿角色Forward方向为深度，垂直方向为宽度
        /// 无施法范围要求
        /// </summary>
        FrontRectCast,

        /// <summary>
        /// 角色面前的扇状区域，R=1为面前单点，R=2为面前第一排1格+第二排3格；……
        /// 无施法范围要求
        /// </summary>
        FrontFanCast,

        /// <summary>
        /// 角色面前的三角形区域，R=1为面前单点，R=2为面前第一排3格+第二排1格；……
        /// 无施法范围要求
        /// </summary>
        FrontTriangleCast
    }

    public enum TargetInclination
    {
        [LabelText("以目标为中心正态分布")]
        TargetCenteredNormalDistribution,

        [LabelText("尽可能远离施法者")]
        CasterExpelled,

        [LabelText("以施法者为中心正态分布")]
        CasterCenteredNormalDistribution,

        [LabelText("纯随机")]
        PurelyRandom, // 当施法目标不存在时按纯随机施法
    }

    [LabelText("判定范围类型")]
    public CastAreaType M_CastAreaType;

    [LabelText("施法偏好")]
    [ValidateInput("ValidateCastAreaTypeAndTargetInclination", "当前施法偏好和判定范围类型不兼容")]
    public TargetInclination M_TargetInclination;

    internal ActorPropertyValue MaxTargetCount = new ActorPropertyValue();

    private bool ValidateCastAreaTypeAndTargetInclination(TargetInclination newValue)
    {
        if (M_CastAreaType == CastAreaType.FrontRectCast || M_CastAreaType == CastAreaType.FrontFanCast || M_CastAreaType == CastAreaType.FrontTriangleCast)
        {
            if (newValue == TargetInclination.TargetCenteredNormalDistribution)
            {
                return false;
            }
        }

        return true;
    }

    private bool DontNeedCastingArea => M_CastAreaType == CastAreaType.FrontRectCast || M_CastAreaType == CastAreaType.FrontFanCast || M_CastAreaType == CastAreaType.FrontTriangleCast;

    [LabelText("施法于地表")]
    public bool CastOnTopLayer;

    [LabelText("扇形角度90/180/270")]
    [ShowIf("NeedFanAngle")]
    public int FanAngle;

    private bool NeedFanAngle => M_CastAreaType == CastAreaType.FrontFanCast;

    [LabelText("准度标准差/格")]
    public float AccurateStandardDeviation;

    [LabelText("技能范围标识类型")]
    [ValueDropdown("GetAllBattleIndicatorNames", DropdownTitle = "选择技能范围标识类型")]
    public string BattleIndicatorTypeName;

    private ushort BattleIndicatorTypeIndex => ConfigManager.GetBattleIndicatorTypeIndex(BattleIndicatorTypeName);

    [LabelText("技能投影最高Z偏移")]
    public int ProjectOffsetZMax;

    [LabelText("技能投影最低Z偏移")]
    public int ProjectOffsetZMin;

    [LabelText("技能范围标识填充色")]
    public Color GridWarningColorFill;

    [LabelText("技能范围标识描边色高亮")]
    public Color GridWarningColorBorderHighlight;

    [LabelText("技能范围标识描边色模糊")]
    public Color GridWarningColorBorderDim;

    [LabelText("释放特效")]
    [ValueDropdown("GetAllFXTypeNames", DropdownTitle = "选择技能释放特效")]
    public string CastFX;

    [LabelText("释放特效尺寸")]
    public float CastFXScale;

    internal Dictionary<GridPos3D, GridWarning> GridWarningDict = new Dictionary<GridPos3D, GridWarning>();

    private List<GridPos3D> SkillAreaGPs = new List<GridPos3D>(16);
    protected List<GridPos3D> RealSkillEffectGPs = new List<GridPos3D>(16);

    protected override bool ValidateSkillTrigger()
    {
        if (!base.ValidateSkillTrigger()) return false;
        GridPos3D targetGP = GetTargetGP();
        GridRect castingRect = GetCastingRect();
        if (!DontNeedCastingArea && !castingRect.Contains(targetGP))
        {
            return false;
        }

        return true;
    }

    private GridPos3D GetTargetGP()
    {
        GridPos3D targetGP = GridPos3D.Zero;
        switch (M_TargetInclination)
        {
            case TargetInclination.TargetCenteredNormalDistribution:
            {
                targetGP = Actor.ActorAIAgent.TargetActorGP;
                break;
            }
            case TargetInclination.CasterExpelled:
            case TargetInclination.CasterCenteredNormalDistribution:
            case TargetInclination.PurelyRandom:
            {
                targetGP = Actor.CurWorldGP;
                break;
            }
        }

        return targetGP;
    }

    private GridRect GetCastingRect()
    {
        int castingRadius = GetValue(ActorSkillPropertyType.CastingRadius);
        int xMin = Actor.CurWorldGP.x - (castingRadius - 1);
        int zMin = Actor.CurWorldGP.z - (castingRadius - 1);
        return new GridRect(xMin, zMin, castingRadius * 2 + 1, castingRadius * 2 + 1);
    }

    protected override void PrepareSkillInfo()
    {
        base.PrepareSkillInfo();

        GridPos3D skillCastPosition = GridPos3D.Zero;
        GridPos3D targetGP = GetTargetGP();
        GridRect castingRect = GetCastingRect();

        int effectRadius = GetValue(ActorSkillPropertyType.EffectRadius);
        int width = GetValue(ActorSkillPropertyType.Width);
        int depth = GetValue(ActorSkillPropertyType.Depth);

        int xMin_SkillCastPos = castingRect.x_min;
        int xMax_SkillCastPos = castingRect.x_max;
        int zMin_SkillCastPos = castingRect.z_min;
        int zMax_SkillCastPos = castingRect.z_max;

        if (M_CastAreaType == CastAreaType.RectCast)
        {
            xMin_SkillCastPos += width / 2 - 1;
            xMax_SkillCastPos -= width / 2 + 1 - 1;
            zMin_SkillCastPos += width / 2 - 1;
            zMax_SkillCastPos -= width / 2 + 1 - 1;
        }

        switch (M_TargetInclination)
        {
            case TargetInclination.TargetCenteredNormalDistribution:
            {
                int xRadius = Mathf.Min(Mathf.Abs(xMin_SkillCastPos - targetGP.x), Mathf.Abs(xMax_SkillCastPos - targetGP.x));
                int zRadius = Mathf.Min(Mathf.Abs(zMin_SkillCastPos - targetGP.z), Mathf.Abs(zMax_SkillCastPos - targetGP.z));
                skillCastPosition = new GridPos3D(CommonUtils.RandomGaussianInt(targetGP.x - xRadius, targetGP.x + xRadius), targetGP.y, CommonUtils.RandomGaussianInt(targetGP.z - zRadius, targetGP.z + zRadius));
                break;
            }
            case TargetInclination.CasterCenteredNormalDistribution:
            {
                skillCastPosition = new GridPos3D(CommonUtils.RandomGaussianInt(xMin_SkillCastPos, xMax_SkillCastPos), targetGP.y, CommonUtils.RandomGaussianInt(zMin_SkillCastPos, zMax_SkillCastPos));
                break;
            }
            case TargetInclination.PurelyRandom:
            {
                skillCastPosition = new GridPos3D(Random.Range(xMin_SkillCastPos, xMax_SkillCastPos), targetGP.y, Random.Range(zMin_SkillCastPos, zMax_SkillCastPos));
                break;
            }
            case TargetInclination.CasterExpelled:
            {
                int xSign = Mathf.RoundToInt(Random.value * 2f - 1f);
                int zSign = Mathf.RoundToInt(Random.value * 2f - 1f);
                int x = xMin_SkillCastPos;
                int z = zMin_SkillCastPos;
                if (xSign == 0) x = Random.Range(xMin_SkillCastPos, xMax_SkillCastPos);
                if (zSign == 0) z = Random.Range(zMin_SkillCastPos, zMax_SkillCastPos);
                if (xSign == -1) x = xMin_SkillCastPos;
                if (xSign == 1) x = xMax_SkillCastPos;
                if (zSign == -1) z = zMin_SkillCastPos;
                if (zSign == 1) z = zMax_SkillCastPos;
                skillCastPosition = new GridPos3D(x, targetGP.y, z);
                break;
            }
        }

        switch (M_CastAreaType)
        {
            case CastAreaType.CircleCast:
            {
                for (int xDiff = -(effectRadius - 1); xDiff <= (effectRadius - 1); xDiff++)
                {
                    for (int zDiff = -(effectRadius - 1); zDiff <= (effectRadius - 1); zDiff++)
                    {
                        if (Mathf.Abs(xDiff) + Mathf.Abs(zDiff) < effectRadius)
                        {
                            GridPos3D gp = skillCastPosition + new GridPos3D(xDiff, 0, zDiff);
                            AddGP(gp);
                        }
                    }
                }

                break;
            }
            case CastAreaType.RectCast:
            {
                for (int xDiff = -(width - 1) / 2; xDiff <= width / 2; xDiff++)
                {
                    for (int zDiff = -(width - 1) / 2; zDiff <= width / 2; zDiff++)
                    {
                        GridPos3D gp = skillCastPosition + new GridPos3D(xDiff, 0, zDiff);
                        AddGP(gp);
                    }
                }

                break;
            }
            case CastAreaType.FrontRectCast:
            {
                int xSign = Mathf.RoundToInt(Actor.CurForward.x);
                int zSign = Mathf.RoundToInt(Actor.CurForward.z);
                if (xSign == 0)
                {
                    for (int xDiff = -(effectRadius - 1); xDiff <= (effectRadius - 1); xDiff++)
                    {
                        for (int zDiff = 1 * zSign; Mathf.Abs(zDiff) <= depth; zDiff += zSign)
                        {
                            GridPos3D gp = Actor.CurWorldGP + new GridPos3D(xDiff, 0, zDiff);
                            AddGP(gp);
                        }
                    }
                }
                else if (zSign == 0)
                {
                    for (int xDiff = 1 * xSign; Mathf.Abs(xDiff) <= depth; xDiff += xSign)
                    {
                        for (int zDiff = -(effectRadius - 1); zDiff <= (effectRadius - 1); zDiff++)
                        {
                            GridPos3D gp = Actor.CurWorldGP + new GridPos3D(xDiff, 0, zDiff);
                            AddGP(gp);
                        }
                    }
                }

                break;
            }
            case CastAreaType.FrontFanCast:
            {
                int xSign = Mathf.RoundToInt(Actor.CurForward.x);
                int zSign = Mathf.RoundToInt(Actor.CurForward.z);
                if (xSign == 0)
                {
                    for (int xDiff = -(effectRadius - 1); xDiff <= (effectRadius - 1); xDiff++)
                    {
                        for (int zDiff = 1 * zSign; Mathf.Abs(zDiff) <= effectRadius; zDiff += zSign)
                        {
                            if (Mathf.Abs(xDiff) < Mathf.Abs(zDiff))
                            {
                                GridPos3D gp = Actor.CurWorldGP + new GridPos3D(xDiff, 0, zDiff);
                                AddGP(gp);
                            }
                        }
                    }
                }
                else if (zSign == 0)
                {
                    for (int xDiff = 1 * xSign; Mathf.Abs(xDiff) <= effectRadius; xDiff += xSign)
                    {
                        for (int zDiff = -(effectRadius - 1); zDiff <= (effectRadius - 1); zDiff++)
                        {
                            if (Mathf.Abs(zDiff) < Mathf.Abs(xDiff))
                            {
                                GridPos3D gp = Actor.CurWorldGP + new GridPos3D(xDiff, 0, zDiff);
                                AddGP(gp);
                            }
                        }
                    }
                }

                break;
            }
            case CastAreaType.FrontTriangleCast:
            {
                int xSign = Mathf.RoundToInt(Actor.CurForward.x);
                int zSign = Mathf.RoundToInt(Actor.CurForward.z);
                if (xSign == 0)
                {
                    for (int xDiff = -(effectRadius - 1); xDiff <= (effectRadius - 1); xDiff++)
                    {
                        for (int zDiff = 1 * zSign; Mathf.Abs(zDiff) <= effectRadius; zDiff += zSign)
                        {
                            if (Mathf.Abs(xDiff) + Mathf.Abs(zDiff) <= effectRadius)
                            {
                                GridPos3D gp = Actor.CurWorldGP + new GridPos3D(xDiff, 0, zDiff);
                                AddGP(gp);
                            }
                        }
                    }
                }
                else if (zSign == 0)
                {
                    for (int xDiff = 1 * xSign; Mathf.Abs(xDiff) <= effectRadius; xDiff += xSign)
                    {
                        for (int zDiff = -(effectRadius - 1); zDiff <= (effectRadius - 1); zDiff++)
                        {
                            if (Mathf.Abs(xDiff) + Mathf.Abs(zDiff) <= effectRadius)
                            {
                                GridPos3D gp = Actor.CurWorldGP + new GridPos3D(xDiff, 0, zDiff);
                                AddGP(gp);
                            }
                        }
                    }
                }

                break;
            }
        }

        void AddGP(GridPos3D gp)
        {
            SkillAreaGPs.Add(gp);
            if (CheckAreaGPValid(gp, out GridPos3D realGP))
            {
                RealSkillEffectGPs.Add(realGP);
            }
        }
    }

    private bool CheckAreaGPValid(GridPos3D gp, out GridPos3D worldGP)
    {
        if (!CastOnTopLayer) // 如果没有施法于地表的要求，则原GP即可满足要求
        {
            worldGP = gp;
            return true;
        }

        if (WorldManager.Instance.CurrentWorld.BoxProject(GridPos3D.Down,
            gp + GridPos3D.Up * ProjectOffsetZMax,
            ProjectOffsetZMax - ProjectOffsetZMin + 1,
            false, out worldGP, out Box _))
        {
            return true;
        }

        return false;
    }

    protected override IEnumerator WingUp(float wingUpTime)
    {
        if (BattleIndicatorTypeIndex != 0)
        {
            foreach (GridPos3D gp in SkillAreaGPs)
            {
                GridWarning gw = GameObjectPoolManager.Instance.BattleIndicatorDict[BattleIndicatorTypeIndex].AllocateGameObject<GridWarning>(WorldManager.Instance.BattleIndicatorRoot);
                gw.SetFillColor(GridWarningColorFill).SetBorderHighlightColor(GridWarningColorBorderHighlight).SetBorderDimColor(GridWarningColorBorderDim);
                gw.transform.position = gp;
                GridWarningDict.Add(gp, gw);
            }
        }

        yield return base.WingUp(wingUpTime);
    }

    protected override IEnumerator Cast(float castDuration)
    {
        UpdateSkillEffectRealPositions();
        if (!string.IsNullOrWhiteSpace(CastFX))
        {
            ushort fxTypeIndex = ConfigManager.GetFXTypeIndex(CastFX);
            if (fxTypeIndex != 0)
            {
                foreach (GridPos3D gp in RealSkillEffectGPs)
                {
                    FX fx = FXManager.Instance.PlayFX(CastFX, gp, CastFXScale);
                }
            }
        }

        yield return base.Cast(castDuration);
    }

    private void ClearWhenSkillFinishedOrInterrupted()
    {
        foreach (KeyValuePair<GridPos3D, GridWarning> kv in GridWarningDict)
        {
            kv.Value.PoolRecycle();
        }

        GridWarningDict.Clear();
        SkillAreaGPs.Clear();
        RealSkillEffectGPs.Clear();
    }

    public override void OnCastPhaseComplete()
    {
        base.OnCastPhaseComplete();
        ClearWhenSkillFinishedOrInterrupted();
    }

    public override void Interrupt()
    {
        base.Interrupt();
        ClearWhenSkillFinishedOrInterrupted();
    }

    protected override IEnumerator Recover(float recoveryTime)
    {
        yield return base.Recover(recoveryTime);
    }

    public override void OnFixedUpdate(float fixedDeltaTime)
    {
        base.OnFixedUpdate(fixedDeltaTime);
        UpdateSkillEffectGridWarning();
    }

    public override void OnTick(float tickDeltaTime)
    {
        base.OnTick(tickDeltaTime);
        UpdateSkillEffectRealPositions();
    }

    private void UpdateSkillEffectRealPositions()
    {
        RealSkillEffectGPs.Clear();
        foreach (GridPos3D gp in SkillAreaGPs)
        {
            bool valid = CheckAreaGPValid(gp, out GridPos3D realGP);
            if (valid) RealSkillEffectGPs.Add(realGP);
            if (GridWarningDict.TryGetValue(gp, out GridWarning gridWarning))
            {
                gridWarning.SetShown(valid);
                gridWarning.transform.position = realGP;
            }
        }
    }

    private void UpdateSkillEffectGridWarning()
    {
        foreach (GridPos3D gp in SkillAreaGPs)
        {
            if (GridWarningDict.TryGetValue(gp, out GridWarning gridWarning))
            {
                gridWarning.OnProcess(WingUpRatio);
            }
        }
    }

    protected override void ChildClone(ActorActiveSkill cloneData)
    {
        base.ChildClone(cloneData);
        ActorActiveSkill_AreaCast newAAS = (ActorActiveSkill_AreaCast) cloneData;
        newAAS.M_CastAreaType = M_CastAreaType;
        newAAS.M_TargetInclination = M_TargetInclination;
        newAAS.MaxTargetCount = MaxTargetCount;
        newAAS.CastOnTopLayer = CastOnTopLayer;
        newAAS.FanAngle = FanAngle;
        newAAS.AccurateStandardDeviation = AccurateStandardDeviation;
        newAAS.BattleIndicatorTypeName = BattleIndicatorTypeName;
        newAAS.ProjectOffsetZMax = ProjectOffsetZMax;
        newAAS.ProjectOffsetZMin = ProjectOffsetZMin;
        newAAS.GridWarningColorFill = GridWarningColorFill;
        newAAS.GridWarningColorBorderHighlight = GridWarningColorBorderHighlight;
        newAAS.GridWarningColorBorderDim = GridWarningColorBorderDim;
        newAAS.CastFX = CastFX;
        newAAS.CastFXScale = CastFXScale;
    }

    public override void CopyDataFrom(ActorActiveSkill srcData)
    {
        base.CopyDataFrom(srcData);
        ActorActiveSkill_AreaCast srcAAS = (ActorActiveSkill_AreaCast) srcData;
        M_CastAreaType = srcAAS.M_CastAreaType;
        M_TargetInclination = srcAAS.M_TargetInclination;
        MaxTargetCount = srcAAS.MaxTargetCount;
        CastOnTopLayer = srcAAS.CastOnTopLayer;
        FanAngle = srcAAS.FanAngle;
        AccurateStandardDeviation = srcAAS.AccurateStandardDeviation;
        BattleIndicatorTypeName = srcAAS.BattleIndicatorTypeName;
        ProjectOffsetZMax = srcAAS.ProjectOffsetZMax;
        ProjectOffsetZMin = srcAAS.ProjectOffsetZMin;
        GridWarningColorFill = srcAAS.GridWarningColorFill;
        GridWarningColorBorderHighlight = srcAAS.GridWarningColorBorderHighlight;
        GridWarningColorBorderDim = srcAAS.GridWarningColorBorderDim;
        CastFX = srcAAS.CastFX;
        CastFXScale = srcAAS.CastFXScale;
    }
}