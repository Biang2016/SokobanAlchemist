﻿using System;
using System.Collections.Generic;
using BiangLibrary.GameDataFormat.Grid;
using BiangLibrary.GamePlay;
using BiangLibrary.ObjectPool;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Profiling;
using Random = UnityEngine.Random;

public abstract class Entity : PoolObject
{
    #region GUID

    [ReadOnly]
    [HideInEditorMode]
    [ShowInInspector]
    internal uint GUID;

    [ReadOnly]
    [HideInEditorMode]
    [ShowInInspector]
    internal int GUID_Mod_FixedFrameRate;

    private static uint guidGenerator = (uint) ConfigManager.GUID_Separator.Entity;

    protected uint GetGUID()
    {
        return guidGenerator++;
    }

    [ReadOnly]
    [HideInEditorMode]
    public uint InitWorldModuleGUID; // 创建时所属的世界模组GUID

    #endregion

    protected virtual void Awake()
    {
    }

    public override void OnRecycled()
    {
        base.OnRecycled();
        StopAllCoroutines();
        CurrentEntityData = null;
        InitWorldModuleGUID = 0;
        destroyBecauseNotInAnyModuleTick = 0;
        cachedRemoveList_EntityPassiveSkill.Clear();
        cachedRemoveList_EntityActiveSkill.Clear();
        cachedAddList_EntityPassiveSkill.Clear();
        IsDestroying = false;
    }

    public override void OnUsed()
    {
        base.OnUsed();
        CanBeThreatened = true;
    }

    public void Setup(EntityData entityData, uint initWorldModuleGUID)
    {
        CurrentEntityData = entityData;
        InitWorldModuleGUID = initWorldModuleGUID;
        if (GUID == 0)
        {
            GUID = GetGUID();
            if (IsBoxCamp)
            {
                if (SlowlyTick)
                {
                    GUID_Mod_FixedFrameRate = ((int) GUID) % ClientGameManager.Instance.FixedFrameRate_5X;
                }
                else
                {
                    GUID_Mod_FixedFrameRate = ((int) GUID) % ClientGameManager.Instance.FixedFrameRate;
                }
            }
            else
            {
                GUID_Mod_FixedFrameRate = ((int) GUID) % ClientGameManager.Instance.FixedFrameRate_01X;
            }
        }
    }

    protected virtual void Update()
    {
        if (!BattleManager.Instance.IsStart) return;
        if (IsRecycled) return;
        foreach (KeyValuePair<EntitySkillIndex, EntityActiveSkill> kv in EntityActiveSkillDict)
        {
            kv.Value.OnUpdate(Time.deltaTime);
        }
    }

    protected virtual void FixedUpdate()
    {
        if (!BattleManager.Instance.IsStart) return;
        if (IsRecycled) return;
        if (IsBoxCamp)
        {
            if (SlowlyTick)
            {
                if (GUID_Mod_FixedFrameRate == ClientGameManager.Instance.CurrentFixedFrameCount_Mod_FixedFrameRate_5X) // SlowTick Box 5秒一次
                {
                    Tick(5f);
                }
            }
            else
            {
                if (GUID_Mod_FixedFrameRate == ClientGameManager.Instance.CurrentFixedFrameCount_Mod_FixedFrameRate) // Box 1秒一次
                {
                    Tick(1f);
                }
            }
        }
        else
        {
            //if (GUID_Mod_FixedFrameRate == ClientGameManager.Instance.CurrentFixedFrameCount_Mod_FixedFrameRate_01X) // Actor 0.1秒一次
            //{
            Tick(0.02f);
            //}
        }

        foreach (KeyValuePair<EntitySkillIndex, EntityActiveSkill> kv in EntityActiveSkillDict)
        {
            kv.Value.OnFixedUpdate(Time.fixedDeltaTime);
        }

        cachedRemoveList_EntityPassiveSkill.Clear();
        foreach (EntityPassiveSkill eps in EntityPassiveSkills)
        {
            if (eps.MarkAsForget) cachedRemoveList_EntityPassiveSkill.Add(eps);
        }

        cachedRemoveList_EntityActiveSkill.Clear();
        foreach (KeyValuePair<EntitySkillIndex, EntityActiveSkill> kv in EntityActiveSkillDict)
        {
            if (kv.Value.MarkAsForget) cachedRemoveList_EntityActiveSkill.Add(kv.Value);
        }

        foreach (EntityPassiveSkill skill in cachedRemoveList_EntityPassiveSkill)
        {
            ForgetPassiveSkill(skill.SkillGUID);
        }

        cachedRemoveList_EntityPassiveSkill.Clear();

        foreach (EntityActiveSkill skill in cachedRemoveList_EntityActiveSkill)
        {
            ForgetActiveSkill(skill.SkillGUID);
        }

        cachedRemoveList_EntityActiveSkill.Clear();

        foreach (EntityPassiveSkill skill in cachedAddList_EntityPassiveSkill)
        {
            AddNewPassiveSkill(skill);
        }

        cachedAddList_EntityPassiveSkill.Clear();
    }

    protected virtual void Tick(float interval)
    {
        if (!BattleManager.Instance.IsStart) return;
        if (IsRecycled) return;
        EntityStatPropSet.Tick(interval);
        EntityBuffHelper.BuffTick(interval);
        foreach (EntityPassiveSkill eps in EntityPassiveSkills)
        {
            eps.OnTick(interval);
        }

        foreach (KeyValuePair<EntitySkillIndex, EntityActiveSkill> kv in EntityActiveSkillDict)
        {
            kv.Value.OnTick(interval);
        }

        // Auto Delete when not in a module
        if (AutoDeleteWhenNotInModule)
        {
            if (WorldManager.Instance != null)
            {
                WorldModule module = WorldManager.Instance.CurrentWorld.GetModuleByWorldGP(transform.position.ToGridPos3D(), false);
                if (module == null)
                {
                    destroyBecauseNotInAnyModuleTick += interval;
                    if (destroyBecauseNotInAnyModuleTick > DestroyBecauseNotInAnyModuleThreshold)
                    {
                        destroyBecauseNotInAnyModuleTick = 0;
                        DestroySelf(null);
                    }
                }
            }
        }
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
    }

    [ShowInInspector]
    [FoldoutGroup("属性")]
    [LabelText("是Trigger实体")]
    public bool IsTriggerEntity => EntityIndicatorHelper.EntityOccupationData.IsTriggerEntity;

    [FoldoutGroup("属性")]
    [LabelText("慢刷新")]
    public bool SlowlyTick = false;

    [FoldoutGroup("属性")]
    [LabelText("出模组自动删除")]
    public bool AutoDeleteWhenNotInModule = true;

    public bool IsFrozen => EntityStatPropSet.IsFrozen;

    /// <summary>
    /// （受各种控制技能影响）无法动弹
    /// </summary>
    public bool CannotAct => IsFrozen || EntityBuffHelper.IsStun || EntityBuffHelper.IsBeingGround || EntityBuffHelper.IsBeingRepulsed;

    [FoldoutGroup("属性")]
    [LabelText("能自动落下")]
    public bool CanAutoFallDown = true;

    #region Helpers

    [FoldoutGroup("组件")]
    public EntityArtHelper EntityArtHelper;

    [FoldoutGroup("组件")]
    public EntityWwiseHelper EntityWwiseHelper;

    [FoldoutGroup("组件")]
    public EntityModelHelper EntityModelHelper;

    [FoldoutGroup("组件")]
    public EntityIndicatorHelper EntityIndicatorHelper;

    [FoldoutGroup("组件")]
    public EntityBuffHelper EntityBuffHelper;

    [FoldoutGroup("组件")]
    public EntityFrozenHelper EntityFrozenHelper;

    [FoldoutGroup("组件")]
    public EntityTriggerZoneHelper EntityTriggerZoneHelper;

    [FoldoutGroup("组件")]
    public EntityCollectHelper EntityCollectHelper;

    [FoldoutGroup("组件")]
    public EntityGrindTriggerZoneHelper EntityGrindTriggerZoneHelper;

    [FoldoutGroup("组件")]
    public List<EntityFlamethrowerHelper> EntityFlamethrowerHelpers = new List<EntityFlamethrowerHelper>();

    [FoldoutGroup("组件")]
    public List<EntityLightningGeneratorHelper> EntityLightningGeneratorHelpers = new List<EntityLightningGeneratorHelper>();

    protected List<EntityMonoHelper> EntityMonoHelpers = new List<EntityMonoHelper>(32);

    protected virtual void OnInitHelperList()
    {
        EntityMonoHelpers.Clear();
        EntityMonoHelpers.Add(EntityArtHelper);
        EntityMonoHelpers.Add(EntityWwiseHelper);
        EntityMonoHelpers.Add(EntityModelHelper);
        EntityMonoHelpers.Add(EntityIndicatorHelper);
        EntityMonoHelpers.Add(EntityBuffHelper);
        EntityMonoHelpers.Add(EntityFrozenHelper);
        EntityMonoHelpers.Add(EntityTriggerZoneHelper);
        EntityMonoHelpers.Add(EntityCollectHelper);
        EntityMonoHelpers.Add(EntityGrindTriggerZoneHelper);
        foreach (EntityFlamethrowerHelper h in EntityFlamethrowerHelpers)
        {
            EntityMonoHelpers.Add(h);
        }

        foreach (EntityLightningGeneratorHelper h in EntityLightningGeneratorHelpers)
        {
            EntityMonoHelpers.Add(h);
        }
    }

    #endregion

    [HideInInspector]
    public ushort EntityTypeIndex;

    #region 战斗数值

    [FoldoutGroup("初始战斗数值")]
    [HideLabel]
    [HideInPlayMode]
    public EntityStatPropSet RawEntityStatPropSet; // 干数据，禁修改

    [HideInEditorMode]
    [FoldoutGroup("实时战斗数值")]
    [HideLabel]
    [NonSerialized]
    [ShowInInspector]
    public EntityStatPropSet EntityStatPropSet; // 湿数据，随生命周期消亡

    #endregion

    [DisplayAsString]
    [HideInEditorMode]
    [ShowInInspector]
    [FoldoutGroup("状态")]
    [LabelText("上次世界坐标")]
    internal GridPos3D LastWorldGP;

    [DisplayAsString]
    [HideInEditorMode]
    [ShowInInspector]
    [FoldoutGroup("状态")]
    [LabelText("世界坐标")]
    public abstract GridPos3D WorldGP { get; set; }

    [DisplayAsString]
    [HideInEditorMode]
    [ShowInInspector]
    [FoldoutGroup("状态")]
    [LabelText("实时世界坐标")]
    private GridPos3D realtimeWorldGP;

    internal GridPos3D RealtimeWorldGP
    {
        get { return realtimeWorldGP; }
        set
        {
            if (realtimeWorldGP != value)
            {
                realtimeWorldGP = value;
                if (realtimeWorldGP != GridPos3D.Zero && value != GridPos3D.Zero && !IsRecycled)
                {
                    foreach (EntityPassiveSkill eps in EntityPassiveSkills)
                    {
                        foreach (GridPos3D offset in GetEntityOccupationGPs_Rotated())
                        {
                            eps.OnPassGrid(realtimeWorldGP + offset);
                        }
                    }
                }
            }
        }
    }

    public Vector3 CurForward
    {
        get
        {
            switch (EntityOrientation)
            {
                case GridPosR.Orientation.Up:
                    return GridPos3D.Forward;
                case GridPosR.Orientation.Right:
                    return GridPos3D.Right;
                case GridPosR.Orientation.Down:
                    return GridPos3D.Back;
                case GridPosR.Orientation.Left:
                    return GridPos3D.Left;
            }

            return Vector3.up;
        }
        set
        {
            if (this is Actor actor && !actor.ActorArtHelper.CanTurn)
            {
                return;
            }

            if (value != Vector3.zero)
            {
                if (value == Vector3.forward) SwitchEntityOrientation(GridPosR.Orientation.Up);
                else if (value == Vector3.back) SwitchEntityOrientation(GridPosR.Orientation.Down);
                else if (value == Vector3.right) SwitchEntityOrientation(GridPosR.Orientation.Right);
                else if (value == Vector3.left) SwitchEntityOrientation(GridPosR.Orientation.Left);
                else Debug.LogWarning($"CurForward invalid: {value}");
            }
        }
    }

    [DisplayAsString]
    [HideInEditorMode]
    [ShowInInspector]
    [FoldoutGroup("状态")]
    [LabelText("模组内坐标")]
    internal GridPos3D LocalGP;

    [FoldoutGroup("习性")]
    [LabelText("偏好地形")]
    public List<TerrainType> PreferTerrainTypes = new List<TerrainType>();

    #region 旋转朝向

    [DisplayAsString]
    [HideInEditorMode]
    [ShowInInspector]
    [FoldoutGroup("状态")]
    [LabelText("旋转朝向")]
    public GridPosR.Orientation EntityOrientation { get; protected set; }

    internal virtual void SwitchEntityOrientation(GridPosR.Orientation entityOrientation)
    {
        EntityOrientation = entityOrientation;
    }

    #endregion

    #region Occupation

    // 旋转过的局部坐标
    public virtual List<GridPos3D> GetEntityOccupationGPs_Rotated()
    {
        List<GridPos3D> occupation_rotated = ConfigManager.GetEntityOccupationData(EntityTypeIndex).EntityIndicatorGPs_RotatedDict[EntityOrientation];
        return occupation_rotated;
    }

    // 旋转过的局部几何中心
    public virtual Vector3 GetEntityGeometryCenter_Rotated()
    {
        return ConfigManager.GetEntityOccupationData(EntityTypeIndex).LocalGeometryCenter_RotatedDict[EntityOrientation];
    }

    public bool IsShapeCuboid()
    {
        return ConfigManager.GetEntityOccupationData(EntityTypeIndex).IsShapeCuboid;
    }

    public bool IsShapePlanSquare()
    {
        return ConfigManager.GetEntityOccupationData(EntityTypeIndex).IsShapeCuboid;
    }

    public BoundsInt EntityBoundsInt => EntityIndicatorHelper.EntityOccupationData.BoundsInt; // ConfigManager不能序列化这个字段，很奇怪

    [DisplayAsString]
    [HideInEditorMode]
    [FoldoutGroup("状态")]
    [LabelText("Entity形心坐标")]
    public Vector3 EntityGeometryCenter => GetEntityGeometryCenter_Rotated() + transform.position;

    [DisplayAsString]
    [HideInEditorMode]
    [FoldoutGroup("状态")]
    [LabelText("Entity底盘形心坐标")]
    public Vector3 EntityBaseCenter => new Vector3(EntityGeometryCenter.x, transform.position.y, EntityGeometryCenter.z);

    public float GetBaseCenterDistanceTo(Entity target, bool ignoreY)
    {
        Vector3 diff = EntityBaseCenter - target.EntityBaseCenter;
        if (ignoreY) diff.y = 0;
        return diff.magnitude;
    }

    /// <summary>
    /// Grid对Grid的最近距离
    /// </summary>
    /// <param name="target"></param>
    /// <param name="ignoreY"></param>
    /// <returns></returns>
    public float GetGridDistanceTo(Entity target, bool ignoreY)
    {
        float minDistance = float.MaxValue;
        foreach (GridPos3D offset in GetEntityOccupationGPs_Rotated())
        {
            GridPos3D gridPos = WorldGP + offset;
            foreach (GridPos3D offset_target in target.GetEntityOccupationGPs_Rotated())
            {
                GridPos3D gridPos_target = target.WorldGP + offset_target;
                if (ignoreY) gridPos_target.y = gridPos.y;
                float dist = (gridPos - gridPos_target).magnitude;
                if (minDistance > dist)
                {
                    minDistance = dist;
                }
            }
        }

        return minDistance;
    }

    /// <summary>
    /// Grid对Grid的最近距离
    /// </summary>
    /// <param name="targetGP"></param>
    /// <param name="ignoreY"></param>
    /// <returns></returns>
    public float GetGridDistanceTo(GridPos3D targetGP, bool ignoreY)
    {
        float minDistance = float.MaxValue;
        foreach (GridPos3D offset in GetEntityOccupationGPs_Rotated())
        {
            GridPos3D gridPos = WorldGP + offset;
            if (ignoreY) targetGP.y = gridPos.y;
            float dist = (gridPos - targetGP).magnitude;
            if (minDistance > dist)
            {
                minDistance = dist;
            }
        }

        return minDistance;
    }

    #endregion

    #region 仇恨 // 临时，未来用量化的仇恨值代替

    [ShowInInspector]
    [FoldoutGroup("状态")]
    internal bool CanBeThreatened = true;

    #endregion

    #region EntityData

    internal EntityData CurrentEntityData;

    public void ApplyEntityExtraSerializeData(EntityExtraSerializeData rawEntityExtraSerializeDataFromModule = null)
    {
        Profiler.BeginSample("ApplyEntityExtraSerializeData");
        if (rawEntityExtraSerializeDataFromModule != null)
        {
            foreach (EntityPassiveSkill rawExtraPS in rawEntityExtraSerializeDataFromModule.EntityPassiveSkills)
            {
                EntityPassiveSkill newPS = (EntityPassiveSkill) rawExtraPS.Clone();
                newPS.IsLevelExtraEntitySkill = true;
                AddNewPassiveSkill(newPS);
            }

            ApplyEntityExtraStates(rawEntityExtraSerializeDataFromModule.EntityDataExtraStates);
        }

        Profiler.EndSample();
    }

    public void RecordEntityExtraSerializeData()
    {
        Profiler.BeginSample("RecordEntityExtraSerializeData");
        if (CurrentEntityData != null && CurrentEntityData.RawEntityExtraSerializeData != null)
        {
            RecordEntityExtraStates(CurrentEntityData.RawEntityExtraSerializeData.EntityDataExtraStates);
        }

        Profiler.EndSample();
    }

    protected virtual void RecordEntityExtraStates(EntityDataExtraStates entityDataExtraStates)
    {
    }

    protected virtual void ApplyEntityExtraStates(EntityDataExtraStates entityDataExtraStates)
    {
    }

    #endregion

    #region 技能

    [SerializeReference]
    [FoldoutGroup("Fuel")]
    [LabelText("As Fuel Data")]
    public EntityFlamethrowerHelper.FlamethrowerFuelData RawFlamethrowerFuelData; // 干数据，禁修改

    public bool HasLearnedSkill(string skillGUID)
    {
        if (EntityPassiveSkillGUIDDict.ContainsKey(skillGUID)) return true;
        if (EntityActiveSkillGUIDDict.ContainsKey(skillGUID)) return true;
        return false;
    }

    #region 被动技能

    [FoldoutGroup("被动技能")]
    [LabelText("被动技能槽位SO")]
    [InlineEditor(InlineEditorObjectFieldModes.Foldout)]
    public List<EntitySkillSO> RawEntityPassiveSkillSOs = new List<EntitySkillSO>(); // 自带被动技能

    [NonSerialized]
    [ShowInInspector]
    [FoldoutGroup("实时被动技能")]
    [LabelText("实时被动技能列表")]
    public List<EntityPassiveSkill> EntityPassiveSkills = new List<EntityPassiveSkill>(); // 湿数据，每个Entity生命周期开始前从干数据进行数据拷贝

    public Dictionary<string, EntityPassiveSkill> EntityPassiveSkillGUIDDict = new Dictionary<string, EntityPassiveSkill>(); // 便于使用GUID来索引到技能

    internal bool PassiveSkillMarkAsDestroyed = false;

    private List<EntityPassiveSkill> cachedRemoveList_EntityPassiveSkill = new List<EntityPassiveSkill>(8);
    private List<EntityPassiveSkill> cachedAddList_EntityPassiveSkill = new List<EntityPassiveSkill>(8);

    /// <summary>
    /// 将携带的被动技能初始化至Prefab配置状态
    /// </summary>
    public virtual void InitPassiveSkills()
    {
        PassiveSkillMarkAsDestroyed = false;
        cachedRemoveList_EntityPassiveSkill.Clear();
        foreach (KeyValuePair<string, EntityPassiveSkill> kv in EntityPassiveSkillGUIDDict)
        {
            bool found = false;
            foreach (EntitySkillSO so in RawEntityPassiveSkillSOs)
            {
                if (kv.Key == so.EntitySkill.SkillGUID)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                cachedRemoveList_EntityPassiveSkill.Add(kv.Value);
            }
        }

        foreach (EntityPassiveSkill removeEPS in cachedRemoveList_EntityPassiveSkill)
        {
            EntityPassiveSkills.Remove(removeEPS);
            EntityPassiveSkillGUIDDict.Remove(removeEPS.SkillGUID);
        }

        cachedRemoveList_EntityPassiveSkill.Clear();
        foreach (EntitySkillSO so in RawEntityPassiveSkillSOs)
        {
            EntityPassiveSkill rawEPS = (EntityPassiveSkill) so.EntitySkill;
            if (EntityPassiveSkillGUIDDict.TryGetValue(so.EntitySkill.SkillGUID, out EntityPassiveSkill eps))
            {
                eps.CopyDataFrom(rawEPS);
            }
            else
            {
                eps = (EntityPassiveSkill) rawEPS.Clone();
                EntityPassiveSkills.Add(eps);
                EntityPassiveSkillGUIDDict.Add(rawEPS.SkillGUID, eps);
                eps.CopyDataFrom(rawEPS);
            }
        }

        foreach (EntityPassiveSkill eps in EntityPassiveSkills)
        {
            eps.Entity = this;
            eps.OnInit();
            eps.OnRegisterLevelEventID();
        }

        foreach (EntityMonoHelper h in EntityMonoHelpers)
        {
            h?.OnInitPassiveSkills();
        }
    }

    public void AddNewPassiveSkill(EntityPassiveSkill eps)
    {
        // 关卡编辑器中额外添加的被动技能一般没有GUID
        if (string.IsNullOrWhiteSpace(eps.SkillGUID)) eps.SkillGUID = Guid.NewGuid().ToString("P"); // e.g: (ade24d16-db0f-40af-8794-1e08e2040df3);
        if (!EntityPassiveSkillGUIDDict.ContainsKey(eps.SkillGUID))
        {
            if (this is Actor actor && actor.ActorSkillLearningHelper != null)
            {
                actor.ActorSkillLearningHelper.LearnPassiveSkill(eps.SkillGUID);
            }

            EntityPassiveSkills.Add(eps);
            EntityPassiveSkillGUIDDict.Add(eps.SkillGUID, eps);
            eps.Entity = this;
            eps.OnInit();
            eps.OnRegisterLevelEventID();

            if (this == BattleManager.Instance.Player1)
            {
                if (!eps.SkillIcon.TypeName.IsNullOrWhitespace() && !eps.SkillName_EN.IsNullOrWhitespace() && eps.OccupySkillGrid)
                {
                    PlayerStatHUD HUD = ClientGameManager.Instance.PlayerStatHUDPanel.PlayerStatHUDs_Player[0];
                    foreach (KeyValuePair<PlayerControllerHelper.KeyBind, ISkillBind> kv in HUD.SkillSlotDict)
                    {
                        if (kv.Value.EmptySkill)
                        {
                            kv.Value.BindSkill(eps);
                            break;
                        }
                    }
                }
            }
        }
        else
        {
            //Debug.Log($"{name}添加被动技能失败{eps}，被动技能已存在");
        }
    }

    public void Async_AddNewPassiveSkill(EntityPassiveSkill eps)
    {
        cachedAddList_EntityPassiveSkill.Add(eps);
    }

    public void ForgetPassiveSkill(string skillGUID)
    {
        if (EntityPassiveSkillGUIDDict.TryGetValue(skillGUID, out EntityPassiveSkill eps))
        {
            if (this is Actor actor && actor.ActorSkillLearningHelper != null)
            {
                actor.ActorSkillLearningHelper.ForgetPassiveSkill(eps.SkillGUID);
            }

            EntityPassiveSkills.Remove(eps);
            EntityPassiveSkillGUIDDict.Remove(eps.SkillGUID);
            eps.OnUnRegisterLevelEventID();
            eps.OnUnInit();

            if (this == BattleManager.Instance.Player1)
            {
                if (!eps.SkillIcon.TypeName.IsNullOrWhitespace() && !eps.SkillName_EN.IsNullOrWhitespace() && eps.OccupySkillGrid)
                {
                    PlayerStatHUD HUD = ClientGameManager.Instance.PlayerStatHUDPanel.PlayerStatHUDs_Player[0];
                    foreach (KeyValuePair<PlayerControllerHelper.KeyBind, ISkillBind> kv in HUD.SkillSlotDict)
                    {
                        if (kv.Value.BoundEntitySkill == eps)
                        {
                            kv.Value.BindSkill(null);
                        }
                    }
                }
            }
        }
    }

    public void Async_ForgetPassiveSkill(string skillGUID)
    {
        if (EntityPassiveSkillGUIDDict.TryGetValue(skillGUID, out EntityPassiveSkill eps))
        {
            eps.MarkAsForget = true;
        }
    }

    protected virtual void UnInitPassiveSkills()
    {
        foreach (EntityPassiveSkill eps in EntityPassiveSkills)
        {
            eps.OnUnRegisterLevelEventID();
            eps.OnUnInit();
        }

        foreach (EntityMonoHelper h in EntityMonoHelpers)
        {
            h?.OnUnInitPassiveSkills();
        }

        PassiveSkillMarkAsDestroyed = false;
    }

    #endregion

    #region 主动技能

    [FoldoutGroup("主动技能")]
    [LabelText("主动技能槽位")]
    [InlineEditor(InlineEditorObjectFieldModes.Foldout)]
    public List<EntitySkillSO> RawEntityActiveSkillSOs = new List<EntitySkillSO>();

    [NonSerialized]
    [ShowInInspector]
    [FoldoutGroup("实时主动技能")]
    [LabelText("实时主动技能列表")]
    public Dictionary<EntitySkillIndex, EntityActiveSkill> EntityActiveSkillDict = new Dictionary<EntitySkillIndex, EntityActiveSkill>(); // 技能编号

    public Dictionary<string, EntityActiveSkill> EntityActiveSkillGUIDDict = new Dictionary<string, EntityActiveSkill>(); // 便于使用GUID来索引到技能

    private List<EntityActiveSkill> cachedRemoveList_EntityActiveSkill = new List<EntityActiveSkill>(16);

    internal bool ActiveSkillMarkAsDestroyed = false;

    public bool ActiveSkillCanMove
    {
        get
        {
            foreach (KeyValuePair<EntitySkillIndex, EntityActiveSkill> kv in EntityActiveSkillDict)
            {
                if (!kv.Value.CurrentAllowMove) return false;
            }

            return true;
        }
    }

    /// <summary>
    /// 将携带的主动技能初始化至Prefab配置状态
    /// </summary>
    public virtual void InitActiveSkills()
    {
        ActiveSkillMarkAsDestroyed = false;
        EntityActiveSkillDict.Clear();
        cachedRemoveList_EntityActiveSkill.Clear();
        foreach (KeyValuePair<string, EntityActiveSkill> kv in EntityActiveSkillGUIDDict)
        {
            bool found = false;
            for (int skillIndexInt = 0; skillIndexInt < RawEntityActiveSkillSOs.Count; skillIndexInt++)
            {
                EntitySkillSO so = RawEntityActiveSkillSOs[skillIndexInt];
                if (kv.Key == so.EntitySkill.SkillGUID)
                {
                    EntitySkillIndex skillIndex = (EntitySkillIndex) skillIndexInt;
                    kv.Value.EntitySkillIndex = skillIndex;
                    EntityActiveSkillDict.Add(skillIndex, kv.Value);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                cachedRemoveList_EntityActiveSkill.Add(kv.Value);
            }
        }

        foreach (EntityActiveSkill removeEAS in cachedRemoveList_EntityActiveSkill)
        {
            EntityActiveSkillDict.Remove(removeEAS.EntitySkillIndex);
            EntityActiveSkillGUIDDict.Remove(removeEAS.SkillGUID);
        }

        cachedRemoveList_EntityActiveSkill.Clear();
        for (int skillIndexInt = 0; skillIndexInt < RawEntityActiveSkillSOs.Count; skillIndexInt++)
        {
            EntitySkillIndex skillIndex = (EntitySkillIndex) skillIndexInt;
            EntitySkillSO so = RawEntityActiveSkillSOs[skillIndexInt];
            EntityActiveSkill rawEAS = (EntityActiveSkill) so.EntitySkill;
            if (EntityActiveSkillGUIDDict.TryGetValue(so.EntitySkill.SkillGUID, out EntityActiveSkill eas))
            {
                eas.CopyDataFrom(rawEAS);
                eas.EntitySkillIndex = skillIndex;
            }
            else
            {
                eas = (EntityActiveSkill) rawEAS.Clone();
                EntityActiveSkillDict.Add(skillIndex, eas);
                EntityActiveSkillGUIDDict.Add(eas.SkillGUID, eas);
                eas.CopyDataFrom(rawEAS);
                eas.EntitySkillIndex = skillIndex;
            }
        }

        foreach (KeyValuePair<EntitySkillIndex, EntityActiveSkill> kv in EntityActiveSkillDict)
        {
            kv.Value.Entity = this;
            kv.Value.ParentActiveSkill = null;
            kv.Value.OnInit();
        }

        foreach (EntityMonoHelper h in EntityMonoHelpers)
        {
            h?.OnInitActiveSkills();
        }
    }

    public bool AddNewActiveSkill(EntityActiveSkill eas)
    {
        foreach (EntitySkillIndex si in Enum.GetValues(typeof(EntitySkillIndex)))
        {
            if (!EntityActiveSkillDict.ContainsKey(si))
            {
                return AddNewActiveSkill(eas, si);
            }
        }

        return false;
    }

    public bool AddNewActiveSkill(EntityActiveSkill eas, EntitySkillIndex skillIndex)
    {
        if (string.IsNullOrWhiteSpace(eas.SkillGUID))
        {
            Debug.LogError($"主动技能GUID为空: {eas.SkillAlias}");
            return false; // 不添加GUID为空的主动技能
        }

        if (!EntityActiveSkillGUIDDict.ContainsKey(eas.SkillGUID))
        {
            if (this is Actor actor && actor.ActorSkillLearningHelper != null)
            {
                actor.ActorSkillLearningHelper.LearnActiveSkill(eas.SkillGUID, skillIndex);
            }

            EntityActiveSkillDict.Add(skillIndex, eas);
            EntityActiveSkillGUIDDict.Add(eas.SkillGUID, eas);
            eas.Entity = this;
            eas.ParentActiveSkill = null;
            eas.EntitySkillIndex = skillIndex;
            eas.OnInit();
            return true;
        }
        else
        {
            //Debug.Log($"{name}添加主动技能失败{eas}，主动技能已存在");
            return false;
        }
    }

    public void BindActiveSkillToKey(EntityActiveSkill eas, PlayerControllerHelper.KeyBind keyBind, bool clearAllExistedSkillInKeyBind)
    {
        if (EntityActiveSkillGUIDDict.ContainsKey(eas.SkillGUID))
        {
            if (this is Actor actor)
            {
                if (actor.ActorControllerHelper is PlayerControllerHelper pch)
                {
                    if (clearAllExistedSkillInKeyBind) pch.SkillKeyMappings[keyBind].Clear();
                    pch.SkillKeyMappings[keyBind].Add(eas.EntitySkillIndex);

                    PlayerStatHUD HUD = ClientGameManager.Instance.PlayerStatHUDPanel.PlayerStatHUDs_Player[0];
                    if (HUD.SkillSlotDict.TryGetValue(keyBind, out ISkillBind iSkillBind))
                    {
                        iSkillBind.BindSkill(eas);
                    }
                }

                actor.ActorSkillLearningHelper?.BindActiveSkillToKey(eas.EntitySkillIndex, keyBind, clearAllExistedSkillInKeyBind);
            }
        }
    }

    public void ForgetActiveSkill(string skillGUID)
    {
        if (EntityActiveSkillGUIDDict.TryGetValue(skillGUID, out EntityActiveSkill eas))
        {
            if (this is Actor actor)
            {
                if (actor.ActorControllerHelper is PlayerControllerHelper pch)
                {
                    foreach (KeyValuePair<PlayerControllerHelper.KeyBind, List<EntitySkillIndex>> kv in pch.SkillKeyMappings)
                    {
                        kv.Value.Remove(eas.EntitySkillIndex);
                        PlayerStatHUD HUD = ClientGameManager.Instance.PlayerStatHUDPanel.PlayerStatHUDs_Player[0];
                        if (HUD.SkillSlotDict.TryGetValue(kv.Key, out ISkillBind iSkillBind))
                        {
                            iSkillBind.BindSkill(null);
                        }
                    }
                }

                actor.ActorSkillLearningHelper?.ForgetActiveSkill(eas.SkillGUID, eas.EntitySkillIndex);
            }

            EntityActiveSkillDict.Remove(eas.EntitySkillIndex);
            EntityActiveSkillGUIDDict.Remove(eas.SkillGUID);
            eas.OnUnInit();
        }
    }

    public void Async_ForgetActiveSkill(string skillGUID)
    {
        if (EntityActiveSkillGUIDDict.TryGetValue(skillGUID, out EntityActiveSkill eas))
        {
            eas.MarkAsForget = true;
        }
    }

    protected virtual void UnInitActiveSkills()
    {
        foreach (KeyValuePair<string, EntityActiveSkill> kv in EntityActiveSkillGUIDDict)
        {
            kv.Value.OnUnInit();
        }

        foreach (EntityMonoHelper h in EntityMonoHelpers)
        {
            h?.OnUnInitActiveSkills();
        }

        ActiveSkillMarkAsDestroyed = false;
    }

    #endregion

    #region Upgrades

    public void GetUpgraded(EntityUpgrade entityUpgrade)
    {
        switch (entityUpgrade.EntityUpgradeType)
        {
            case EntityUpgradeType.Property:
            {
                EntityProperty property = EntityStatPropSet.PropertyDict[entityUpgrade.EntityPropertyType];
                int mod_BaseValue = Mathf.RoundToInt((property.GetBaseValue + entityUpgrade.Delta_BaseValue) * (1f + entityUpgrade.Percent_BaseValue / 100f));
                int mod_MinValue = Mathf.RoundToInt((property.GetMinValue + entityUpgrade.Delta_MinValue) * (1f + entityUpgrade.Percent_MinValue / 100f));
                int mod_MaxValue = Mathf.RoundToInt((property.GetMaxValue + entityUpgrade.Delta_MaxValue) * (1f + entityUpgrade.Percent_MaxValue / 100f));
                property.ChangeProperty(mod_BaseValue, mod_MinValue, mod_MaxValue);
                break;
            }

            case EntityUpgradeType.Stat:
            {
                EntityStat stat = EntityStatPropSet.StatDict[entityUpgrade.EntityStatType];
                int mod_MinValue = Mathf.RoundToInt((stat.MinValue + entityUpgrade.Delta_MinValue) * (1f + entityUpgrade.Percent_MinValue / 100f));
                int mod_MaxValue = Mathf.RoundToInt((stat.MaxValue + entityUpgrade.Delta_MaxValue) * (1f + entityUpgrade.Percent_MaxValue / 100f));
                int mod_AbnormalStatResistance = Mathf.RoundToInt((stat.AbnormalStatResistance + entityUpgrade.Delta_AbnormalStatResistance) * (1f + entityUpgrade.Percent_AbnormalStatResistance / 100f));
                int mod_AutoChange = Mathf.RoundToInt((stat.MaxValue + entityUpgrade.Delta_AutoChange) * (1f + entityUpgrade.Percent_AutoChange / 100f));
                int mod_AutoChangePercent = Mathf.RoundToInt((stat.MaxValue + entityUpgrade.Delta_AutoChangePercent) * (1f + entityUpgrade.Percent_AutoChangePercent / 100f));
                stat.ChangeStat(mod_MinValue, mod_MaxValue, mod_AbnormalStatResistance, mod_AutoChange, mod_AutoChangePercent);
                break;
            }
        }
    }

    #endregion

    #endregion

    #region Camp

    [LabelText("阵营")]
    [FoldoutGroup("属性")]
    [DisableInPlayMode]
    public Camp Camp;

    public bool IsPlayerCamp => Camp == Camp.Player;
    public bool IsPlayerOrFriendCamp => Camp == Camp.Player || Camp == Camp.Friend;
    public bool IsFriendCamp => Camp == Camp.Friend;
    public bool IsEnemyCamp => Camp == Camp.Enemy;
    public bool IsNeutralCamp => Camp == Camp.Neutral || Camp == Camp.Box;
    public bool IsBoxCamp => Camp == Camp.Box;

    public bool IsOpponentCampOf(Entity target)
    {
        if (target == null) return false;
        if ((IsPlayerOrFriendCamp) && target.IsEnemyCamp) return true;
        if ((target.IsPlayerOrFriendCamp) && IsEnemyCamp) return true;
        return false;
    }

    public bool IsSameCampOf(Entity target)
    {
        if (target == null) return false;
        return !IsOpponentCampOf(target);
    }

    public bool IsNeutralCampOf(Entity target)
    {
        if (target == null) return false;
        if ((IsPlayerOrFriendCamp) && target.IsNeutralCamp) return true;
        if ((target.IsPlayerOrFriendCamp) && IsNeutralCamp) return true;
        return false;
    }

    public bool IsOpponentOrNeutralCampOf(Entity target)
    {
        if (target == null) return false;
        return IsOpponentCampOf(target) || IsNeutralCampOf(target);
    }

    #endregion

    #region Destroy

    private float destroyBecauseNotInAnyModuleTick = 0f;
    private float DestroyBecauseNotInAnyModuleThreshold = 1.5f;
    protected bool IsDestroying = false;

    [SerializeField]
    [FoldoutGroup("掉落物")]
    [LabelText("抛射角范围")]
    private float DropConeAngle = 30f;

    [SerializeField]
    [FoldoutGroup("掉落物")]
    [LabelText("抛射速度")]
    private float DropVelocity = 50f;

    public virtual void DestroySelfByModuleRecycle()
    {
    }

    public virtual void DestroySelf(UnityAction callBack = null)
    {
        ThrowElementFragment(EntityStatType.FireElementFragment);
        ThrowElementFragment(EntityStatType.IceElementFragment);
        ThrowElementFragment(EntityStatType.LightningElementFragment);
    }

    private void ThrowElementFragment(EntityStatType elementFragmentType)
    {
        int elementFragmentCount = EntityStatPropSet.StatDict[elementFragmentType].Value;
        int dropElementFragmentCount = 0;
        if (this == BattleManager.Instance.Player1)
        {
            dropElementFragmentCount = Mathf.RoundToInt(elementFragmentCount * 0.5f);
            EntityStatPropSet.StatDict[elementFragmentType].SetValue(EntityStatPropSet.StatDict[elementFragmentType].Value - dropElementFragmentCount);
        }
        else
        {
            dropElementFragmentCount = Random.Range(Mathf.RoundToInt(elementFragmentCount * 0.7f), Mathf.RoundToInt(elementFragmentCount * 1.3f));
        }

        switch (elementFragmentType)
        {
            case EntityStatType.FireElementFragment:
            case EntityStatType.IceElementFragment:
            {
                int dropSmallCount = dropElementFragmentCount % 5;
                int dropMiddleCount = dropElementFragmentCount / 5;
                DropElementFragment("Small", dropSmallCount);
                DropElementFragment("Middle", dropMiddleCount);
                break;
            }
            case EntityStatType.LightningElementFragment:
            {
                DropElementFragment("Small", dropElementFragmentCount);
                break;
            }
        }

        void DropElementFragment(string size, int count)
        {
            WorldModule worldModule = WorldManager.Instance.CurrentWorld.GetModuleByWorldGP(transform.position.ToGridPos3D());
            if (worldModule == null) worldModule = WorldManager.Instance.CurrentWorld.GetModuleByWorldGP(WorldGP);
            if (worldModule == null) worldModule = WorldManager.Instance.CurrentWorld.GetModuleByWorldGP(LastWorldGP);
            if (worldModule)
            {
                for (int i = 0; i < count; i++)
                {
                    Vector2 horizontalVel = Random.insideUnitCircle.normalized * Mathf.Tan(DropConeAngle * Mathf.Deg2Rad);
                    Vector3 dropVel = Vector3.up + new Vector3(horizontalVel.x, 0, horizontalVel.y);
                    ushort index = ConfigManager.GetTypeIndex(TypeDefineType.CollectableItem, size + elementFragmentType);
                    CollectableItem ci = GameObjectPoolManager.Instance.CollectableItemDict[index].AllocateGameObject<CollectableItem>(worldModule.WorldModuleCollectableItemRoot);
                    ci.Initialize(worldModule);
                    ci.ThrowFrom(transform.position, dropVel.normalized * DropVelocity);
                    worldModule.WorldModuleCollectableItems.Add(ci);
                }
            }
        }
    }

    #endregion

    public override int GetHashCode()
    {
        return GUID.GetHashCode();
    }

#if UNITY_EDITOR

    [AssetsOnly]
    [Button("刷新关卡编辑器中该实体的形态", ButtonSizes.Large)]
    [GUIColor(0, 1, 0)]
    public void RefreshEntityLevelEditor()
    {
        GameObject entity_Instance = Instantiate(gameObject); // 这是实例化一个无链接的prefab实例（unpacked completely）
        Entity entity = entity_Instance.GetComponent<Entity>();
        GameObject modelRoot = entity.EntityModelHelper.gameObject;
        GameObject entityIndicatorHelperGO = entity.EntityIndicatorHelper.gameObject;

        TypeDefineType entityType = TypeDefineType.Actor;
        if (this is Actor)
        {
            entityType = TypeDefineType.Actor;
        }
        else if (this is Box)
        {
            entityType = TypeDefineType.Box;
        }

        GameObject entityLevelEditorPrefab = ConfigManager.FindEntityLevelEditorPrefabByName(entityType, name);
        if (entityLevelEditorPrefab)
        {
            string entity_LevelEditor_Prefab_Path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(entityLevelEditorPrefab);
            GameObject entity_LevelEditor_Instance = PrefabUtility.LoadPrefabContents(entity_LevelEditor_Prefab_Path); // 这是实例化一个在预览场景里的prefab实例，为了能够顺利删除子GameObject
            Entity_LevelEditor entity_LevelEditor = entity_LevelEditor_Instance.GetComponent<Entity_LevelEditor>();

            entity_LevelEditor.ModelRoot = null;
            entity_LevelEditor.IndicatorHelperGO = null;
            List<Transform> children = new List<Transform>();
            for (int i = 0; i < entity_LevelEditor.transform.childCount; i++)
            {
                Transform child = entity_LevelEditor.transform.GetChild(i);
                children.Add(child);
            }

            foreach (Transform child in children)
            {
                DestroyImmediate(child.gameObject);
            }

            modelRoot.transform.parent = entity_LevelEditor_Instance.transform;
            if (entity_LevelEditor.ModelRoot) DestroyImmediate(entity_LevelEditor.ModelRoot);
            entity_LevelEditor.ModelRoot = modelRoot;

            entityIndicatorHelperGO.transform.parent = entity_LevelEditor_Instance.transform;
            if (entity_LevelEditor.IndicatorHelperGO) DestroyImmediate(entity_LevelEditor.IndicatorHelperGO);
            entity_LevelEditor.IndicatorHelperGO = entityIndicatorHelperGO;

            entity_LevelEditor.EntityData.EntityType.TypeDefineType = entityType;
            entity_LevelEditor.EntityData.EntityType.TypeSelection = name;
            entity_LevelEditor.EntityData.EntityType.RefreshGUID();

            PrefabUtility.SaveAsPrefabAsset(entity_LevelEditor_Instance, entity_LevelEditor_Prefab_Path, out bool suc); // 保存回改Prefab的Asset
            DestroyImmediate(entity_LevelEditor_Instance);
        }
        else
        {
            PrefabManager.Instance.LoadPrefabs(); // todo delete this line
            GameObject EntityBase_LevelEditor_Prefab = PrefabManager.Instance.GetPrefab($"{entityType}Base_LevelEditor");
            GameObject entity_LevelEditor_Instance = (GameObject) PrefabUtility.InstantiatePrefab(EntityBase_LevelEditor_Prefab); // 这是实例化一个在当前场景里的prefab实例（有链接），为了能够顺利保存成Variant

            Entity_LevelEditor entity_LevelEditor = entity_LevelEditor_Instance.GetComponent<Entity_LevelEditor>();
            modelRoot.transform.parent = entity_LevelEditor_Instance.transform;
            if (entity_LevelEditor.ModelRoot) DestroyImmediate(entity_LevelEditor.ModelRoot);
            entity_LevelEditor.ModelRoot = modelRoot;

            entityIndicatorHelperGO.transform.parent = entity_LevelEditor_Instance.transform;
            if (entity_LevelEditor.IndicatorHelperGO) DestroyImmediate(entity_LevelEditor.IndicatorHelperGO);
            entity_LevelEditor.IndicatorHelperGO = entityIndicatorHelperGO;

            entity_LevelEditor.EntityData.EntityType.TypeDefineType = entityType;
            entity_LevelEditor.EntityData.EntityType.TypeSelection = name;
            entity_LevelEditor.EntityData.EntityType.RefreshGUID();

            string entity_LevelEditor_PrefabPath = ConfigManager.FindEntityLevelEditorPrefabPathByName(entityType, name); // 保存成Variant
            PrefabUtility.SaveAsPrefabAsset(entity_LevelEditor_Instance, entity_LevelEditor_PrefabPath, out bool suc);
            DestroyImmediate(entity_LevelEditor_Instance);
        }

        DestroyImmediate(entity_Instance);
    }

#endif
}

public static class EntityUtils
{
    public static bool IsNotNullAndAlive(this Entity entity)
    {
        return entity != null && !entity.IsRecycled && !entity.ActiveSkillMarkAsDestroyed && !entity.PassiveSkillMarkAsDestroyed;
    }
}