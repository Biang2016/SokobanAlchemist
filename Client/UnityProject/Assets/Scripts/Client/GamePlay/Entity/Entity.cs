using System;
using System.Collections.Generic;
using BiangLibrary.GameDataFormat.Grid;
using BiangLibrary.GamePlay;
using BiangLibrary.ObjectPool;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

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

    [FoldoutGroup("属性")]
    public bool SlowlyTick = false;

    private static uint guidGenerator = (uint) ConfigManager.GUID_Separator.Entity;

    protected uint GetGUID()
    {
        return guidGenerator++;
    }

    [ReadOnly]
    [HideInEditorMode]
    public uint InitWorldModuleGUID; // 创建时所属的世界模组GUID

    #endregion

    #region Helpers

    internal abstract EntityArtHelper EntityArtHelper { get; }
    internal abstract EntityWwiseHelper EntityWwiseHelper { get; }
    internal abstract EntityModelHelper EntityModelHelper { get; }
    internal abstract EntityIndicatorHelper EntityIndicatorHelper { get; }
    internal abstract EntityBuffHelper EntityBuffHelper { get; }
    internal abstract EntityFrozenHelper EntityFrozenHelper { get; }
    internal abstract EntityTriggerZoneHelper EntityTriggerZoneHelper { get; }
    internal abstract EntityGrindTriggerZoneHelper EntityGrindTriggerZoneHelper { get; }
    internal abstract List<EntityFlamethrowerHelper> EntityFlamethrowerHelpers { get; }
    internal abstract List<EntityLightningGeneratorHelper> EntityLightningGeneratorHelpers { get; }

    #endregion

    [HideInInspector]
    public ushort EntityTypeIndex;

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

    [DisplayAsString]
    [HideInEditorMode]
    [ShowInInspector]
    [FoldoutGroup("状态")]
    [LabelText("被冻结")]
    public bool IsFrozen => EntityStatPropSet.IsFrozen;

    /// <summary>
    /// （受各种控制技能影响）无法动弹
    /// </summary>
    [DisplayAsString]
    [HideInEditorMode]
    [ShowInInspector]
    [FoldoutGroup("状态")]
    [LabelText("无法移动")]
    public bool CannotAct => IsFrozen || EntityBuffHelper.IsShocking || EntityBuffHelper.IsStun || EntityBuffHelper.IsBeingGround || EntityBuffHelper.IsBeingRepulsed;

    [DisplayAsString]
    [HideInEditorMode]
    [ShowInInspector]
    [FoldoutGroup("状态")]
    [LabelText("上帧世界坐标")]
    internal GridPos3D LastWorldGP;

    [DisplayAsString]
    [HideInEditorMode]
    [ShowInInspector]
    [FoldoutGroup("状态")]
    [LabelText("世界坐标")]
    public abstract GridPos3D WorldGP { get; set; }

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

    #region EntityExtraData

    public void ApplyEntityExtraSerializeData(EntityExtraSerializeData rawEntityExtraSerializeDataFromModule = null)
    {
        if (rawEntityExtraSerializeDataFromModule != null)
        {
            foreach (EntityPassiveSkill rawExtraPS in rawEntityExtraSerializeDataFromModule.EntityPassiveSkills)
            {
                EntityPassiveSkill newPS = (EntityPassiveSkill) rawExtraPS.Clone();
                AddNewPassiveSkill(newPS);
            }
        }
    }

    #endregion

    #region 技能

    [SerializeReference]
    [FoldoutGroup("作为喷火器燃料")]
    [LabelText("燃料数据")]
    public EntityFlamethrowerHelper.FlamethrowerFuelData RawFlamethrowerFuelData; // 干数据，禁修改

    #region 被动技能

    [FoldoutGroup("被动技能")]
    [LabelText("被动技能槽位")]
    [InlineEditor(InlineEditorObjectFieldModes.Foldout)]
    public List<EntitySkillSSO> RawEntityPassiveSkillSSOs = new List<EntitySkillSSO>(); // 自带被动技能

    [NonSerialized]
    [ShowInInspector]
    [FoldoutGroup("实时被动技能")]
    [LabelText("实时被动技能列表")]
    public List<EntityPassiveSkill> EntityPassiveSkills = new List<EntityPassiveSkill>(); // 湿数据，每个Entity生命周期开始前从干数据进行数据拷贝

    public Dictionary<string, EntityPassiveSkill> EntityPassiveSkillGUIDDict = new Dictionary<string, EntityPassiveSkill>(); // 便于使用GUID来索引到技能

    internal bool PassiveSkillMarkAsDestroyed = false;

    private List<EntityPassiveSkill> cachedRemoveList_EntityPassiveSkill = new List<EntityPassiveSkill>(8);

    protected virtual void InitPassiveSkills()
    {
        if (EntityPassiveSkills == null) EntityPassiveSkills = new List<EntityPassiveSkill>();
        if (EntityPassiveSkillGUIDDict == null) EntityPassiveSkillGUIDDict = new Dictionary<string, EntityPassiveSkill>();
        if (cachedRemoveList_EntityPassiveSkill == null) cachedRemoveList_EntityPassiveSkill = new List<EntityPassiveSkill>();

        PassiveSkillMarkAsDestroyed = false;

        for (int i = 0; i < RawEntityPassiveSkillSSOs.Count; i++)
        {
            EntityPassiveSkill rawEPS = (EntityPassiveSkill) RawEntityPassiveSkillSSOs[i].EntitySkill;
            EntityPassiveSkill eps = null;
            if (EntityPassiveSkills.Count > i)
            {
                eps = EntityPassiveSkills[i];
            }
            else
            {
                if (!EntityPassiveSkillGUIDDict.ContainsKey(rawEPS.SkillGUID))
                {
                    eps = (EntityPassiveSkill) rawEPS.Clone();
                    EntityPassiveSkills.Add(eps);
                    EntityPassiveSkillGUIDDict.Add(rawEPS.SkillGUID, rawEPS);
                }
                else
                {
                    Debug.Log($"{name}已存在相同GUID的被动技能{rawEPS}");
                }
            }

            if (eps != null)
            {
                eps.Entity = this;
                eps.IsAddedDuringGamePlay = false;
                eps.CopyDataFrom(rawEPS);
            }
        }

        if (EntityPassiveSkills.Count != RawEntityPassiveSkillSSOs.Count)
        {
            Debug.Log($"{name}被动技能数量不一致");
        }

        foreach (EntityPassiveSkill eps in EntityPassiveSkills)
        {
            eps.OnInit();
            eps.OnRegisterLevelEventID();
        }
    }

    public void AddNewPassiveSkill(EntityPassiveSkill eps)
    {
        if (string.IsNullOrWhiteSpace(eps.SkillGUID)) eps.SkillGUID = Guid.NewGuid().ToString("P"); // e.g: (ade24d16-db0f-40af-8794-1e08e2040df3);
        if (!EntityPassiveSkillGUIDDict.ContainsKey(eps.SkillGUID))
        {
            EntityPassiveSkills.Add(eps);
            EntityPassiveSkillGUIDDict.Add(eps.SkillGUID, eps);
            eps.IsAddedDuringGamePlay = true;
            eps.Entity = this;
            eps.OnInit();
            eps.OnRegisterLevelEventID();
        }
        else
        {
            Debug.Log($"{name}添加被动技能失败{eps}");
        }
    }

    protected virtual void UnInitPassiveSkills()
    {
        foreach (EntityPassiveSkill eps in EntityPassiveSkills)
        {
            eps.OnUnRegisterLevelEventID();
            eps.OnUnInit();
        }

        cachedRemoveList_EntityPassiveSkill.Clear();
        foreach (EntityPassiveSkill eps in EntityPassiveSkills)
        {
            if (eps.IsAddedDuringGamePlay) cachedRemoveList_EntityPassiveSkill.Add(eps);
        }

        foreach (EntityPassiveSkill eps in cachedRemoveList_EntityPassiveSkill)
        {
            EntityPassiveSkills.Remove(eps);
            EntityPassiveSkillGUIDDict.Remove(eps.SkillGUID);
        }

        cachedRemoveList_EntityPassiveSkill.Clear();

        PassiveSkillMarkAsDestroyed = false;
    }

    #endregion

    #region 主动技能

    [FoldoutGroup("主动技能")]
    [LabelText("主动技能槽位")]
    [InlineEditor(InlineEditorObjectFieldModes.Foldout)]
    public List<EntitySkillSSO> RawEntityActiveSkillSSOs = new List<EntitySkillSSO>(); // 自带主动技能

    [HideInInspector]
    public SortedDictionary<EntitySkillIndex, EntityActiveSkill> EntityActiveSkillDict = new SortedDictionary<EntitySkillIndex, EntityActiveSkill>(); // 技能编号，只包含所有母技能

    private List<EntitySkillIndex> cachedRemoveList_EntityActiveSkill = new List<EntitySkillIndex>(16);

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

    protected virtual void InitActiveSkills()
    {
        cachedRemoveList_EntityActiveSkill.Clear();
        foreach (KeyValuePair<EntitySkillIndex, EntityActiveSkill> kv in EntityActiveSkillDict)
        {
            if (kv.Value.IsAddedDuringGamePlay) cachedRemoveList_EntityActiveSkill.Add(kv.Key);
        }

        foreach (EntitySkillIndex skillIndex in cachedRemoveList_EntityActiveSkill)
        {
            EntityActiveSkillDict.Remove(skillIndex);
        }

        for (int i = 0; i < RawEntityActiveSkillSSOs.Count; i++)
        {
            EntitySkillSSO sso = RawEntityActiveSkillSSOs[i];
            EntitySkillIndex skillIndex = (EntitySkillIndex) i;
            if (EntityActiveSkillDict.TryGetValue(skillIndex, out EntityActiveSkill eas))
            {
                eas.CopyDataFrom(sso.EntitySkill);
            }
            else
            {
                eas = (EntityActiveSkill) sso.EntitySkill.Clone();
                EntityActiveSkillDict.Add(skillIndex, eas);
            }

            eas.Entity = this;
            eas.IsAddedDuringGamePlay = false;
            eas.ParentActiveSkill = null;
            eas.EntitySkillIndex = skillIndex;
            eas.OnInit();
        }

        ActiveSkillMarkAsDestroyed = false;
    }

    protected void AddNewActiveSkill(EntityActiveSkill eas)
    {
    }

    protected virtual void UnInitActiveSkills()
    {
        foreach (KeyValuePair<EntitySkillIndex, EntityActiveSkill> kv in EntityActiveSkillDict)
        {
            kv.Value.OnUnInit();
        }

        ActiveSkillMarkAsDestroyed = false;
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

    public override void OnRecycled()
    {
        base.OnRecycled();
        StopAllCoroutines();
        InitWorldModuleGUID = 0;
        destroyBecauseNotInAnyModuleTick = 0;
        IsDestroying = false;
    }

    public void Setup(uint initWorldModuleGUID)
    {
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

    protected virtual void FixedUpdate()
    {
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
    }

    protected virtual void Tick(float interval)
    {
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

        if (BattleManager.Instance.IsStart)
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

    #region Destroy

    private float destroyBecauseNotInAnyModuleTick = 0f;
    private float DestroyBecauseNotInAnyModuleThreshold = 1.5f;
    protected bool IsDestroying = false;

    public virtual void DestroySelfByModuleRecycle()
    {
    }

    public virtual void DestroySelf(UnityAction callBack = null)
    {
    }

    #endregion

    protected virtual void OnCollisionEnter(Collision collision)
    {
    }

    public override int GetHashCode()
    {
        return GUID.GetHashCode();
    }

#if UNITY_EDITOR

    [AssetsOnly]
    [Button("刷新关卡编辑器中该实体的形态", ButtonSizes.Large)]
    [GUIColor(0, 1, 0)]
    public void CreateEntityLevelEditor()
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