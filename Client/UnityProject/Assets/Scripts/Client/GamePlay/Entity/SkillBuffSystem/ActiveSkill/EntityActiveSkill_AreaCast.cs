using System;
using System.Collections;
using System.Collections.Generic;
using BiangLibrary;
using BiangLibrary.CloneVariant;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[Serializable]
public abstract class EntityActiveSkill_AreaCast : EntityActiveSkill
{
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

    #region 施法范围

    public const int CastAreaMatrixExtend = 7;

    [ShowInInspector]
    [LabelText("施法范围")]
    [PropertyOrder(1)]
    [TableMatrix(DrawElementMethod = "DrawColoredEnumElement", ResizableColumns = false, SquareCells = true, RowHeight = 20)]
    private bool[,] CastAreaMatrix_Editor = new bool[CastAreaMatrixExtend * 2 + 1, CastAreaMatrixExtend * 2 + 1];

    [PropertyOrder(1)]
    [ButtonGroup("施法范围")]
    [Button("显示施法范围", ButtonSizes.Medium), GUIColor(0, 1f, 0)]
    private void ShowCastArea()
    {
        for (int x = 0; x < CastAreaMatrix_Editor.GetLength(0); x++)
        for (int z = 0; z < CastAreaMatrix_Editor.GetLength(1); z++)
        {
            CastAreaMatrix_Editor[x, z] = false;
        }

        foreach (GridPos3D gridPos3D in CastAreaGridPosList)
        {
            CastAreaMatrix_Editor[gridPos3D.x, CastAreaMatrixExtend * 2 - gridPos3D.z] = true;
        }
    }

    [PropertyOrder(1)]
    [ButtonGroup("施法范围")]
    [Button("保存施法范围", ButtonSizes.Medium), GUIColor(0.4f, 0.8f, 1)]
    private void SaveCastArea()
    {
        CastAreaGridPosList.Clear();
        for (int x = 0; x < CastAreaMatrix_Editor.GetLength(0); x++)
        for (int z = 0; z < CastAreaMatrix_Editor.GetLength(1); z++)
        {
            if (CastAreaMatrix_Editor[x, z])
            {
                CastAreaGridPosList.Add(new GridPos3D(x, 0, CastAreaMatrixExtend * 2 - z));
            }
        }
    }

    [HideInInspector]
    public List<GridPos3D> CastAreaGridPosList = new List<GridPos3D>();

#if UNITY_EDITOR
    private static bool DrawColoredEnumElement(Rect rect, bool value)
    {
        if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
        {
            value = !value;
            GUI.changed = true;
            Event.current.Use();
        }

        UnityEditor.EditorGUI.DrawRect(rect.Padding(1), value ? CommonUtils.HTMLColorToColor("#5AFF00") : CommonUtils.HTMLColorToColor("#000000"));

        return value;
    }
#endif

    #endregion

    [LabelText("施法偏好")]
    public TargetInclination M_TargetInclination;

    [LabelText("施法目标")]
    [ShowIf("M_TargetInclination", TargetInclination.TargetCenteredNormalDistribution)]
    public ActorAIAgent.TargetEntityType TargetEntityType;

    internal EntityPropertyValue MaxTargetCount = new EntityPropertyValue();

    [LabelText("施法于地表")]
    public bool CastOnTopLayer;

    [LabelText("准度标准差/格")]
    public float AccurateStandardDeviation;

    [LabelText("@\"技能范围标识类型\t\"+BattleIndicatorTypeName")]
    public TypeSelectHelper BattleIndicatorTypeName = new TypeSelectHelper {TypeDefineType = TypeDefineType.BattleIndicator};

    private ushort BattleIndicatorTypeIndex => ConfigManager.GetTypeIndex(TypeDefineType.BattleIndicator, BattleIndicatorTypeName.TypeName);

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

    [LabelText("@\"释放特效\t\"+CastFX")]
    public FXConfig CastFX = new FXConfig();

    internal Dictionary<GridPos3D, GridWarning> GridWarningDict = new Dictionary<GridPos3D, GridWarning>();

    private List<GridPos3D> SkillAreaGPs = new List<GridPos3D>(16);
    protected List<GridPos3D> RealSkillEffectGPs = new List<GridPos3D>(16);

    private GridPos3D GetTargetGP()
    {
        GridPos3D targetGP = GridPos3D.Zero;

        switch (M_TargetInclination)
        {
            case TargetInclination.TargetCenteredNormalDistribution:
            {
                if (Entity is Actor actor)
                {
                    targetGP = actor.ActorAIAgent.AIAgentTargetDict[TargetEntityType].TargetGP;
                }
                else if (Entity is Box box)
                {
                    // todo Box AI
                }

                break;
            }
            case TargetInclination.CasterExpelled:
            case TargetInclination.CasterCenteredNormalDistribution:
            case TargetInclination.PurelyRandom:
            {
                targetGP = Entity.WorldGP;
                break;
            }
        }

        return targetGP;
    }

    private GridRect GetCastingRect()
    {
        int castingRadius = GetValue(EntitySkillPropertyType.CastingRadius);
        int xMin = Entity.WorldGP.x - castingRadius;
        int zMin = Entity.WorldGP.z - castingRadius;
        return new GridRect(xMin, zMin, castingRadius * 2 + 1, castingRadius * 2 + 1);
    }

    protected override void PrepareSkillInfo()
    {
        base.PrepareSkillInfo();

        GridPos3D skillCastPosition = GridPos3D.Zero;
        GridPos3D targetGP = GetTargetGP();
        GridRect castingRect = GetCastingRect();

        int xMin_SkillCastPos = castingRect.x_min;
        int xMax_SkillCastPos = castingRect.x_max;
        int zMin_SkillCastPos = castingRect.z_min;
        int zMax_SkillCastPos = castingRect.z_max;

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

        foreach (GridPos3D matrixOffset in CastAreaGridPosList)
        {
            GridPos3D localOffset = new GridPos3D(matrixOffset.x - CastAreaMatrixExtend, matrixOffset.y, matrixOffset.z - CastAreaMatrixExtend);
            GridPos rotatedGrid = GridPosR.TransformOccupiedPosition(new GridPosR(skillCastPosition.x, skillCastPosition.z, Entity.EntityOrientation), new GridPos(localOffset.x, localOffset.z));
            GridPos3D rotatedGrid3D = new GridPos3D(rotatedGrid.x, skillCastPosition.y, rotatedGrid.z);
            AddGP(rotatedGrid3D);
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
        if (!CastFX.Empty)
        {
            foreach (GridPos3D gp in RealSkillEffectGPs)
            {
                FX fx = FXManager.Instance.PlayFX(CastFX, gp);
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

    protected HashSet<Entity> GetTargetEntities()
    {
        HashSet<Entity> entitySet = new HashSet<Entity>();
        foreach (GridPos3D gp in RealSkillEffectGPs)
        {
            Collider[] colliders_PlayerLayer = Physics.OverlapSphere(gp, 0.3f, LayerManager.Instance.GetTargetEntityLayerMask(Entity.Camp, TargetCamp));
            foreach (Collider c in colliders_PlayerLayer)
            {
                Actor targetActor = c.GetComponentInParent<Actor>();
                if (targetActor.IsNotNullAndAlive() && !entitySet.Contains(targetActor))
                {
                    entitySet.Add(targetActor);
                }

                Box targetBox = c.GetComponentInParent<Box>();
                if (targetBox.IsNotNullAndAlive() && !entitySet.Contains(targetBox))
                {
                    entitySet.Add(targetBox);
                }
            }
        }

        return entitySet;
    }

    protected override void ChildClone(EntityActiveSkill cloneData)
    {
        base.ChildClone(cloneData);
        EntityActiveSkill_AreaCast newEAS = (EntityActiveSkill_AreaCast) cloneData;
        newEAS.M_TargetInclination = M_TargetInclination;
        newEAS.TargetEntityType = TargetEntityType;
        newEAS.MaxTargetCount = MaxTargetCount;
        newEAS.CastOnTopLayer = CastOnTopLayer;
        newEAS.CastAreaGridPosList = CastAreaGridPosList.Clone();
        newEAS.AccurateStandardDeviation = AccurateStandardDeviation;
        newEAS.BattleIndicatorTypeName = BattleIndicatorTypeName;
        newEAS.ProjectOffsetZMax = ProjectOffsetZMax;
        newEAS.ProjectOffsetZMin = ProjectOffsetZMin;
        newEAS.GridWarningColorFill = GridWarningColorFill;
        newEAS.GridWarningColorBorderHighlight = GridWarningColorBorderHighlight;
        newEAS.GridWarningColorBorderDim = GridWarningColorBorderDim;
        newEAS.CastFX = CastFX.Clone();
    }

    public override void CopyDataFrom(EntityActiveSkill srcData)
    {
        base.CopyDataFrom(srcData);
        EntityActiveSkill_AreaCast srcEAS = (EntityActiveSkill_AreaCast) srcData;
        M_TargetInclination = srcEAS.M_TargetInclination;
        TargetEntityType = srcEAS.TargetEntityType;
        MaxTargetCount = srcEAS.MaxTargetCount;
        CastOnTopLayer = srcEAS.CastOnTopLayer;
        CastAreaGridPosList = srcEAS.CastAreaGridPosList.Clone();
        AccurateStandardDeviation = srcEAS.AccurateStandardDeviation;
        BattleIndicatorTypeName = srcEAS.BattleIndicatorTypeName;
        ProjectOffsetZMax = srcEAS.ProjectOffsetZMax;
        ProjectOffsetZMin = srcEAS.ProjectOffsetZMin;
        GridWarningColorFill = srcEAS.GridWarningColorFill;
        GridWarningColorBorderHighlight = srcEAS.GridWarningColorBorderHighlight;
        GridWarningColorBorderDim = srcEAS.GridWarningColorBorderDim;
        CastFX.CopyDataFrom(srcEAS.CastFX);
    }
}