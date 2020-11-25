using System;
using System.Collections.Generic;
using BiangStudio;
using BiangStudio.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public abstract class ActorActiveSkill_AreaCast : ActorActiveSkill
{
    public enum CastAreaType
    {
        /// <summary>
        /// R=<see cref="EffectRadius"/>
        /// R=1单点；R=2十字形；……
        /// 要求中心点在施法范围内
        /// </summary>
        CircleCast,

        /// <summary>
        /// 宽度=<see cref="EffectRadius"/>
        /// 长度=<see cref="Depth"/>
        /// 要求整个效果矩形范围在施法范围内
        /// </summary>
        RectCast,

        /// <summary>
        /// 宽度=<see cref="EffectRadius"/>
        /// 长度=<see cref="Depth"/>
        /// 角色面前的带状区域，沿角色Forward方向为深度，垂直方向为宽度
        /// 无施法范围要求
        /// </summary>
        FrontRectCast,

        /// <summary>
        /// R=<see cref="EffectRadius"/>
        /// 角色面前的扇状区域，R=1为面前单点，R=2为面前第一排一格+第二排三格；……
        /// 无施法范围要求
        /// </summary>
        FrontFanCast
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

    private bool ValidateCastAreaTypeAndTargetInclination(TargetInclination newValue)
    {
        if (M_CastAreaType == CastAreaType.FrontRectCast || M_CastAreaType == CastAreaType.FrontFanCast)
        {
            if (newValue == TargetInclination.TargetCenteredNormalDistribution)
            {
                return false;
            }
        }

        return true;
    }

    [LabelText("施法正方形范围边长")]
    [HideIf("DontNeedCastingArea")]
    public int CastingRadius;

    private bool DontNeedCastingArea => M_CastAreaType == CastAreaType.FrontRectCast || M_CastAreaType == CastAreaType.FrontFanCast;

    [LabelText("效果半径")]
    [ShowIf("NeedEffectRadius")]
    public int EffectRadius;

    private bool NeedEffectRadius => M_CastAreaType == CastAreaType.CircleCast || M_CastAreaType == CastAreaType.FrontRectCast || M_CastAreaType == CastAreaType.FrontFanCast;

    [LabelText("宽度X")]
    [ShowIf("NeedWidth")]
    public int Width;

    private bool NeedWidth => M_CastAreaType == CastAreaType.RectCast;

    [LabelText("深度Z")]
    [ShowIf("NeedDepth")]
    public int Depth;

    private bool NeedDepth => M_CastAreaType == CastAreaType.FrontRectCast || M_CastAreaType == CastAreaType.RectCast;

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

    internal Dictionary<GridPos3D, GridWarning> GridWarningDict = new Dictionary<GridPos3D, GridWarning>();

    public override void OnInit()
    {
        base.OnInit();
    }

    public override void Clear()
    {
        base.Clear();
        foreach (KeyValuePair<GridPos3D, GridWarning> kv in GridWarningDict)
        {
            kv.Value.PoolRecycle();
        }

        GridWarningDict.Clear();
        SkillAreaGPs.Clear();
        RealSkillEffectGPs.Clear();
    }

    private List<GridPos3D> SkillAreaGPs = new List<GridPos3D>(16);
    protected List<GridPos3D> RealSkillEffectGPs = new List<GridPos3D>(16);

    protected override bool ValidateSkillTrigger()
    {
        Clear();
        GridPos3D targetGP = GridPos3D.Zero;
        GridPos3D skillCastPosition = GridPos3D.Zero;
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

        int xMin = Actor.CurWorldGP.x - (CastingRadius - 1);
        int xMax = Actor.CurWorldGP.x + (CastingRadius - 1);
        int zMin = Actor.CurWorldGP.z - (CastingRadius - 1);
        int zMax = Actor.CurWorldGP.z + (CastingRadius - 1);

        int xMin_SkillCastPos = xMin;
        int xMax_SkillCastPos = xMax;
        int zMin_SkillCastPos = zMin;
        int zMax_SkillCastPos = zMax;

        if (M_CastAreaType == CastAreaType.RectCast)
        {
            xMin_SkillCastPos += Width / 2 - 1;
            xMax_SkillCastPos -= Width / 2 + 1 - 1;
            zMin_SkillCastPos += Depth / 2 - 1;
            zMax_SkillCastPos -= Depth / 2 + 1 - 1;
        }

        if (targetGP.x <= xMax && targetGP.x >= xMin && targetGP.z <= zMax && targetGP.z >= zMin)
        {
            switch (M_TargetInclination)
            {
                case TargetInclination.TargetCenteredNormalDistribution:
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
        }
        else
        {
            return false;
        }

        switch (M_CastAreaType)
        {
            case CastAreaType.CircleCast:
            {
                for (int xDiff = -(EffectRadius - 1); xDiff <= (EffectRadius - 1); xDiff++)
                {
                    for (int zDiff = -(EffectRadius - 1); zDiff <= (EffectRadius - 1); zDiff++)
                    {
                        if (Mathf.Abs(xDiff) + Mathf.Abs(zDiff) < EffectRadius)
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
                for (int xDiff = -(Width / 2 - 1); xDiff <= (Width / 2 + 1 - 1); xDiff++)
                {
                    for (int zDiff = -(Depth / 2 - 1); zDiff <= (Depth / 2 + 1 - 1); zDiff++)
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
                    for (int xDiff = -(EffectRadius - 1); xDiff <= (EffectRadius - 1); xDiff++)
                    {
                        for (int zDiff = 1 * zSign; Mathf.Abs(zDiff) <= Depth; zDiff += zSign)
                        {
                            GridPos3D gp = skillCastPosition + new GridPos3D(xDiff, 0, zDiff);
                            AddGP(gp);
                        }
                    }
                }
                else if (zSign == 0)
                {
                    for (int xDiff = 1 * xSign; Mathf.Abs(xDiff) <= Depth; xDiff += xSign)
                    {
                        for (int zDiff = -(EffectRadius - 1); zDiff <= (EffectRadius - 1); zDiff++)
                        {
                            GridPos3D gp = skillCastPosition + new GridPos3D(xDiff, 0, zDiff);
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
                    for (int xDiff = -(EffectRadius - 1); xDiff <= (EffectRadius - 1); xDiff++)
                    {
                        for (int zDiff = 1 * zSign; Mathf.Abs(zDiff) <= EffectRadius; zDiff += zSign)
                        {
                            if (Mathf.Abs(xDiff) < Mathf.Abs(zDiff))
                            {
                                GridPos3D gp = skillCastPosition + new GridPos3D(xDiff, 0, zDiff);
                                AddGP(gp);
                            }
                        }
                    }
                }
                else if (zSign == 0)
                {
                    for (int xDiff = 1 * xSign; Mathf.Abs(xDiff) <= EffectRadius; xDiff += xSign)
                    {
                        for (int zDiff = -(EffectRadius - 1); zDiff <= (EffectRadius - 1); zDiff++)
                        {
                            if (Mathf.Abs(zDiff) < Mathf.Abs(xDiff))
                            {
                                GridPos3D gp = skillCastPosition + new GridPos3D(xDiff, 0, zDiff);
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

        return true;
    }

    private bool CheckAreaGPValid(GridPos3D gp, out GridPos3D worldGP)
    {
        if (WorldManager.Instance.CurrentWorld.BoxProject(GridPos3D.Down,
            gp + GridPos3D.Up * ProjectOffsetZMax,
            ProjectOffsetZMax - ProjectOffsetZMin + 1,
            false, out worldGP, out Box _))
        {
            return true;
        }

        return false;
    }

    protected override void WingUp()
    {
        base.WingUp();
        foreach (GridPos3D gp in SkillAreaGPs)
        {
            GridWarning gw = GameObjectPoolManager.Instance.BattleIndicatorDict[BattleIndicatorTypeIndex].AllocateGameObject<GridWarning>(WorldManager.Instance.BattleIndicatorRoot);
            gw.SetFillColor(GridWarningColorFill).SetBorderHighlightColor(GridWarningColorBorderHighlight).SetBorderDimColor(GridWarningColorBorderDim);
            gw.transform.position = gp.ToVector3();
            GridWarningDict.Add(gp, gw);
        }
    }

    protected override void Cast()
    {
        base.Cast();
        UpdateSkillEffectRealPositions();
    }

    protected override void Recover()
    {
        base.Recover();
        Clear();
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
            GridWarningDict[gp].SetShown(valid);
            GridWarningDict[gp].transform.position = realGP.ToVector3();
        }
    }

    protected override void ChildClone(ActorActiveSkill newAS)
    {
        base.ChildClone(newAS);
        ActorActiveSkill_AreaCast asAreaCast = (ActorActiveSkill_AreaCast) newAS;
        asAreaCast.M_CastAreaType = M_CastAreaType;
        asAreaCast.M_TargetInclination = M_TargetInclination;
        asAreaCast.CastingRadius = CastingRadius;
        asAreaCast.EffectRadius = EffectRadius;
        asAreaCast.Width = Width;
        asAreaCast.Depth = Depth;
        asAreaCast.FanAngle = FanAngle;
        asAreaCast.AccurateStandardDeviation = AccurateStandardDeviation;
        asAreaCast.BattleIndicatorTypeName = BattleIndicatorTypeName;
        asAreaCast.ProjectOffsetZMax = ProjectOffsetZMax;
        asAreaCast.ProjectOffsetZMin = ProjectOffsetZMin;
        asAreaCast.GridWarningColorFill = GridWarningColorFill;
        asAreaCast.GridWarningColorBorderHighlight = GridWarningColorBorderHighlight;
        asAreaCast.GridWarningColorBorderDim = GridWarningColorBorderDim;
    }

    public override void CopyDataFrom(ActorActiveSkill srcData)
    {
        base.CopyDataFrom(srcData);
        ActorActiveSkill_AreaCast asAreaCast = (ActorActiveSkill_AreaCast) srcData;
        M_CastAreaType = asAreaCast.M_CastAreaType;
        M_TargetInclination = asAreaCast.M_TargetInclination;
        CastingRadius = asAreaCast.CastingRadius;
        EffectRadius = asAreaCast.EffectRadius;
        Width = asAreaCast.Width;
        Depth = asAreaCast.Depth;
        FanAngle = asAreaCast.FanAngle;
        AccurateStandardDeviation = asAreaCast.AccurateStandardDeviation;
        BattleIndicatorTypeName = asAreaCast.BattleIndicatorTypeName;
        ProjectOffsetZMax = asAreaCast.ProjectOffsetZMax;
        ProjectOffsetZMin = asAreaCast.ProjectOffsetZMin;
        GridWarningColorFill = asAreaCast.GridWarningColorFill;
        GridWarningColorBorderHighlight = asAreaCast.GridWarningColorBorderHighlight;
        GridWarningColorBorderDim = asAreaCast.GridWarningColorBorderDim;
    }
}