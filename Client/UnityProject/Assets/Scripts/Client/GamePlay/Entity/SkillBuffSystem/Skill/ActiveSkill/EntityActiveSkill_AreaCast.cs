using System;
using System.Collections;
using System.Collections.Generic;
using BiangLibrary;
using BiangLibrary.CloneVariant;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class EntityActiveSkill_AreaCast : EntityActiveSkill
{
    public enum TargetInclination
    {
        [LabelText("以目标为中心正态分布")]
        TargetCenteredNormalDistribution,

        [LabelText("以施法者为中心正态分布")]
        CasterCenteredNormalDistribution,

        [LabelText("纯随机")]
        PurelyRandom, // 当施法目标不存在时按纯随机施法
    }

    #region 施法范围

    private const int CAST_AREA_MATRIX_EXTEND = 15;

    [ShowInInspector]
    [LabelText("施法范围")]
    [PropertyOrder(1)]
    [TableMatrix(DrawElementMethod = "DrawColoredEnumElement", ResizableColumns = false, SquareCells = true, RowHeight = 20)]
    [OdinSerialize]
    private bool[,] CastAreaMatrix_Editor = new bool[CAST_AREA_MATRIX_EXTEND * 2 + 1, CAST_AREA_MATRIX_EXTEND * 2 + 1];

    [PropertyOrder(1)]
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
            CastAreaMatrix_Editor[gridPos3D.x, CAST_AREA_MATRIX_EXTEND * 2 - gridPos3D.z] = true;
        }
    }

    [PropertyOrder(1)]
    [Button("保存施法范围", ButtonSizes.Medium), GUIColor(0.4f, 0.8f, 1)]
    private void SaveCastArea()
    {
        CastAreaGridPosList.Clear();
        for (int x = 0; x < CastAreaMatrix_Editor.GetLength(0); x++)
        for (int z = 0; z < CastAreaMatrix_Editor.GetLength(1); z++)
        {
            if (CastAreaMatrix_Editor[x, z])
            {
                CastAreaGridPosList.Add(new GridPos3D(x, 0, CAST_AREA_MATRIX_EXTEND * 2 - z));
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

    [LabelText("施法于地表")]
    public bool CastOnTopLayer;

    [LabelText("施法同时作用于下方n层")]
    public int CastOnNLayersBeneath;

    [LabelText("准度标准差/格")]
    public float AccurateStandardDeviation;

    [HideLabel]
    public TypeSelectHelper BattleIndicatorTypeName = new TypeSelectHelper {TypeDefineType = TypeDefineType.BattleIndicator};

    private ushort BattleIndicatorTypeIndex => ConfigManager.GetTypeIndex(TypeDefineType.BattleIndicator, BattleIndicatorTypeName.TypeName);

    [LabelText("技能投影最高Z偏移")]
    public int ProjectOffsetZMax;

    [LabelText("技能投影最低Z偏移")]
    public int ProjectOffsetZMin;

    [LabelText("技能投影分布高度差范围")]
    public int ProjectHeightDeltaRange = 999;

    [LabelText("技能范围标识填充色")]
    public Color GridWarningColorFill;

    [LabelText("技能范围标识描边色高亮")]
    public Color GridWarningColorBorderHighlight;

    [LabelText("技能范围标识描边色模糊")]
    public Color GridWarningColorBorderDim;

    #region 主动技能行为部分

    [SerializeReference]
    [LabelText("具体内容")]
    [ListDrawerSettings(ListElementLabelName = "Description")]
    public List<EntitySkillAction> RawEntitySkillActions = new List<EntitySkillAction>(); // 干数据，禁修改

    [HideInInspector]
    internal List<EntitySkillAction> EntitySkillActions = new List<EntitySkillAction>(); // 湿数据，每个Entity生命周期开始前从干数据数据拷贝，数量永远和干数据相等

    internal bool EntitySkillActionsMarkAsDeleted = false;

    private void InitSkillActions()
    {
        if (EntitySkillActions.Count > 0)
        {
            foreach (EntitySkillAction esa in EntitySkillActions) // 放在Init里清空，避免在UnInit里出现Entity死亡而modify collection的情况
            {
                esa.OnRecycled();
            }

            if (EntitySkillActions.Count == RawEntitySkillActions.Count)
            {
                for (int i = 0; i < RawEntitySkillActions.Count; i++)
                {
                    EntitySkillAction esa = EntitySkillActions[i];
                    esa.Init(Entity);
                    esa.CopyDataFrom(RawEntitySkillActions[i]);
                }
            }
            else
            {
                Debug.Log("EntitySkillActions的数量和RawEntitySkillActions不一致，请检查临时ESA添加情况");
            }
        }
        else
        {
            foreach (EntitySkillAction rawAction in RawEntitySkillActions)
            {
                EntitySkillAction esa = rawAction.Clone();
                esa.Init(Entity);
                EntitySkillActions.Add(esa);
            }
        }

        EntitySkillActionsMarkAsDeleted = false;
    }

    private void UnInitSkillActions()
    {
        EntitySkillActionsMarkAsDeleted = false;
        foreach (EntitySkillAction action in EntitySkillActions)
        {
            action.Entity = null;
            action.UnInit();
        }
    }

    #endregion

    internal Dictionary<GridPos3D, GridWarning> GridWarningDict = new Dictionary<GridPos3D, GridWarning>();

    private List<GridPos3D> SkillAreaGPs = new List<GridPos3D>(16);
    protected List<GridPos3D> RealSkillEffectGPs = new List<GridPos3D>(16);

    public override void OnInit()
    {
        base.OnInit();
        InitSkillActions();
        if (GridWarningDict == null) GridWarningDict = new Dictionary<GridPos3D, GridWarning>();
        if (SkillAreaGPs == null) SkillAreaGPs = new List<GridPos3D>(16);
        if (RealSkillEffectGPs == null) RealSkillEffectGPs = new List<GridPos3D>(16);
    }

    public override void OnUnInit()
    {
        base.OnUnInit();
        UnInitSkillActions();
    }

    protected override bool ValidateSkillTrigger_Subject(TargetEntityType targetEntityType)
    {
        if (!base.ValidateSkillTrigger_Subject(targetEntityType)) return false;
        PrepareSkillInfo(targetEntityType);
        foreach (EntitySkillCondition condition in EntitySkillConditions)
        {
            if (condition is EntitySkillCondition.IEntityCondition entityCondition)
            {
                bool entityFound = false;
                foreach (Entity entity in GetTargetEntities())
                {
                    if (entityCondition.OnCheckConditionOnEntity(entity))
                    {
                        entityFound = true;
                        break;
                    }
                }

                if (!entityFound) return false;
            }
        }

        return true;
    }

    protected override bool ValidateSkillTrigger_HitProbability(TargetEntityType targetEntityType)
    {
        PrepareSkillInfo(targetEntityType);
        switch (M_TargetInclination)
        {
            case TargetInclination.TargetCenteredNormalDistribution:
            case TargetInclination.CasterCenteredNormalDistribution:
            {
                if (Entity is Actor actor)
                {
                    GridPos3D targetGP = actor.ActorControllerHelper.AgentTargetDict[targetEntityType].TargetGP;
                    foreach (GridPos3D realSkillEffectGP in RealSkillEffectGPs)
                    {
                        if (targetGP == realSkillEffectGP) return true;
                    }
                }
                else if (Entity is Box box)
                {
                    // todo Box AI
                }

                break;
            }
            case TargetInclination.PurelyRandom:
            {
                return true;
            }
        }

        return base.ValidateSkillTrigger_HitProbability(targetEntityType);
    }

    private GridPos3D GetTargetGP(TargetEntityType targetEntityType)
    {
        GridPos3D targetGP = GridPos3D.Zero;

        switch (M_TargetInclination)
        {
            case TargetInclination.TargetCenteredNormalDistribution:
            {
                if (Entity is Actor actor)
                {
                    targetGP = actor.ActorControllerHelper.AgentTargetDict[targetEntityType].TargetGP;
                }
                else if (Entity is Box box)
                {
                    // todo Box AI
                }

                break;
            }
            case TargetInclination.CasterCenteredNormalDistribution:
            case TargetInclination.PurelyRandom:
            {
                targetGP = CastEntityWorldGP;
                break;
            }
        }

        return targetGP;
    }

    /// <summary>
    /// 获取技能施法取值范围区域矩形
    /// </summary>
    /// <returns></returns>
    private GridRect GetCastingBoundaryRect()
    {
        int castingRadius = GetValue(EntitySkillPropertyType.CastingRadius);
        int xMin = CastEntityWorldGP.x - castingRadius;
        int zMin = CastEntityWorldGP.z - castingRadius;
        return new GridRect(xMin, zMin, castingRadius * 2 + 1, castingRadius * 2 + 1);
    }

    protected override void PrepareSkillInfo(TargetEntityType targetEntityType)
    {
        base.PrepareSkillInfo(targetEntityType);
        SkillAreaGPs.Clear();
        GridPos3D skillCastPosition = GridPos3D.Zero;
        GridPos3D targetGP = GetTargetGP(targetEntityType);
        GridRect castingBoundaryRect = GetCastingBoundaryRect();

        int xMin_SkillCastPos = castingBoundaryRect.x_min;
        int xMax_SkillCastPos = castingBoundaryRect.x_max;
        int zMin_SkillCastPos = castingBoundaryRect.z_min;
        int zMax_SkillCastPos = castingBoundaryRect.z_max;

        switch (M_TargetInclination)
        {
            case TargetInclination.TargetCenteredNormalDistribution:
            {
                //GaussianRandom gRandom = new GaussianRandom();
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
        }

        foreach (GridPos3D matrixOffset in CastAreaGridPosList)
        {
            GridPos3D localOffset = new GridPos3D(matrixOffset.x - CAST_AREA_MATRIX_EXTEND, matrixOffset.y, matrixOffset.z - CAST_AREA_MATRIX_EXTEND);
            GridPos rotatedGrid = GridPosR.TransformOccupiedPosition(new GridPosR(skillCastPosition.x, skillCastPosition.z, CastEntityOrientation), new GridPos(localOffset.x, localOffset.z));
            GridPos3D rotatedGrid3D = new GridPos3D(rotatedGrid.x, skillCastPosition.y, rotatedGrid.z);
            SkillAreaGPs.Add(rotatedGrid3D);
        }

        RefreshRealSkillEffectGPs();
    }

    private void RefreshRealSkillEffectGPs()
    {
        int highestY = int.MinValue;
        RealSkillEffectGPs.Clear();
        foreach (GridPos3D skillAreaGP in SkillAreaGPs)
        {
            if (!CastOnTopLayer) // 如果没有施法于地表的要求，则原GP即可满足要求
            {
                RealSkillEffectGPs.Add(skillAreaGP);
            }
            else
            {
                if (WorldManager.Instance.CurrentWorld.BoxProject(GridPos3D.Down,
                    skillAreaGP + GridPos3D.Up * ProjectOffsetZMax,
                    ProjectOffsetZMax - ProjectOffsetZMin + 1,
                    false, out GridPos3D worldGP, out Box _))
                {
                    if (highestY < worldGP.y) highestY = worldGP.y;
                    RealSkillEffectGPs.Add(worldGP);
                }
                else
                {
                    RealSkillEffectGPs.Add(-GridPos3D.One);
                }
            }
        }

        if (CastOnTopLayer)
        {
            for (int i = 0; i < RealSkillEffectGPs.Count; i++)
            {
                GridPos3D realSkillAreaGP = RealSkillEffectGPs[i];
                if (realSkillAreaGP.y <= highestY - ProjectHeightDeltaRange) // 在高差限定范围之外的GP不合法
                {
                    RealSkillEffectGPs[i] = -GridPos3D.One;
                }
            }
        }
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

    protected override IEnumerator Cast(TargetEntityType targetEntityType, float castDuration)
    {
        UpdateSkillEffectRealPositions();
        foreach (EntitySkillAction action in EntitySkillActions)
        {
            if (action is EntitySkillAction.IPureAction pureAction)
            {
                pureAction.Execute();
            }
        }

        foreach (Entity targetEntity in GetTargetEntities())
        {
            foreach (EntitySkillAction action in EntitySkillActions)
            {
                if (action is EntitySkillAction.IEntityAction entityAction)
                {
                    entityAction.ExecuteOnEntity(targetEntity);
                }
            }
        }

        foreach (GridPos3D gp in RealSkillEffectGPs)
        {
            if (gp != -GridPos3D.One)
            {
                foreach (EntitySkillAction action in EntitySkillActions)
                {
                    if (action is EntitySkillAction.IWorldGPAction worldGPAction)
                    {
                        worldGPAction.ExecuteOnWorldGP(gp);
                    }
                }
            }
        }

        yield return base.Cast(targetEntityType, castDuration);
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
        RefreshRealSkillEffectGPs();
        for (int i = 0; i < SkillAreaGPs.Count; i++)
        {
            GridPos3D skillAreaGP = SkillAreaGPs[i];
            GridPos3D realSkillAreaGP = RealSkillEffectGPs[i];
            bool valid = realSkillAreaGP != -GridPos3D.One;
            if (GridWarningDict.TryGetValue(skillAreaGP, out GridWarning gridWarning))
            {
                gridWarning.SetShown(valid);
                if (valid)
                {
                    gridWarning.transform.position = realSkillAreaGP;
                }
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
            if (gp != -GridPos3D.One)
            {
                for (int i = 0; i <= CastOnNLayersBeneath; i++)
                {
                    GridPos3D buffCenterGP = gp + GridPos3D.Down * i;
                    Collider[] colliders_PlayerLayer = Physics.OverlapSphere(buffCenterGP, 0.3f, LayerManager.Instance.GetTargetEntityLayerMask(Entity.Camp, TargetCamp));
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
            }
        }

        return entitySet;
    }

    protected override void ChildClone(EntitySkill cloneData)
    {
        base.ChildClone(cloneData);
        EntityActiveSkill_AreaCast newEAS = (EntityActiveSkill_AreaCast) cloneData;
        newEAS.M_TargetInclination = M_TargetInclination;
        newEAS.CastOnTopLayer = CastOnTopLayer;
        newEAS.CastOnNLayersBeneath = CastOnNLayersBeneath;
        newEAS.CastAreaGridPosList = CastAreaGridPosList.Clone<GridPos3D, GridPos3D>();
        newEAS.AccurateStandardDeviation = AccurateStandardDeviation;
        newEAS.BattleIndicatorTypeName = BattleIndicatorTypeName;
        newEAS.ProjectOffsetZMax = ProjectOffsetZMax;
        newEAS.ProjectOffsetZMin = ProjectOffsetZMin;
        newEAS.ProjectHeightDeltaRange = ProjectHeightDeltaRange;
        newEAS.GridWarningColorFill = GridWarningColorFill;
        newEAS.GridWarningColorBorderHighlight = GridWarningColorBorderHighlight;
        newEAS.GridWarningColorBorderDim = GridWarningColorBorderDim;

        // Actions
        foreach (EntitySkillAction rawBoxSkillAction in RawEntitySkillActions)
        {
            EntitySkillAction newESA = rawBoxSkillAction.Clone();
            newEAS.RawEntitySkillActions.Add(newESA);
        }
    }

    public override void CopyDataFrom(EntitySkill srcData)
    {
        base.CopyDataFrom(srcData);
        EntityActiveSkill_AreaCast srcEAS = (EntityActiveSkill_AreaCast) srcData;
        M_TargetInclination = srcEAS.M_TargetInclination;
        CastOnTopLayer = srcEAS.CastOnTopLayer;
        CastOnNLayersBeneath = srcEAS.CastOnNLayersBeneath;
        CastAreaGridPosList = srcEAS.CastAreaGridPosList.Clone<GridPos3D, GridPos3D>();
        AccurateStandardDeviation = srcEAS.AccurateStandardDeviation;
        BattleIndicatorTypeName = srcEAS.BattleIndicatorTypeName;
        ProjectOffsetZMax = srcEAS.ProjectOffsetZMax;
        ProjectOffsetZMin = srcEAS.ProjectOffsetZMin;
        ProjectHeightDeltaRange = srcEAS.ProjectHeightDeltaRange;
        GridWarningColorFill = srcEAS.GridWarningColorFill;
        GridWarningColorBorderHighlight = srcEAS.GridWarningColorBorderHighlight;
        GridWarningColorBorderDim = srcEAS.GridWarningColorBorderDim;

        // Actions
        if (srcEAS.RawEntitySkillActions.Count != RawEntitySkillActions.Count)
        {
            Debug.LogError("EPS_Conditional CopyDataFrom() Action数量不一致");
        }
        else
        {
            for (int i = 0; i < srcEAS.RawEntitySkillActions.Count; i++)
            {
                RawEntitySkillActions[i].CopyDataFrom(srcEAS.RawEntitySkillActions[i]);
            }
        }
    }
}