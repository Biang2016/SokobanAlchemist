using System;
using System.Collections.Generic;
using BiangLibrary.GameDataFormat.Grid;
using BiangLibrary.ObjectPool;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class Entity : PoolObject
{
    #region GUID

    [ReadOnly]
    [HideInEditorMode]
    public uint GUID;

    public int GUID_Mod_FixedFrameRate;

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
    [DisableInPlayMode]
    public EntityStatPropSet RawEntityStatPropSet; // 干数据，禁修改

    [HideInEditorMode]
    [FoldoutGroup("当前战斗数值")]
    [HideLabel]
    [NonSerialized]
    [ShowInInspector]
    public EntityStatPropSet EntityStatPropSet; // 湿数据，随生命周期消亡

    public bool IsFrozen => EntityStatPropSet.IsFrozen;

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

    public GridPosR.Orientation EntityOrientation { get; protected set; }

    protected virtual void SwitchEntityOrientation(GridPosR.Orientation entityOrientation)
    {
        EntityOrientation = entityOrientation;
    }

    #endregion

    #region Occupation

#if UNITY_EDITOR
    /// <summary>
    /// 仅仅用于Entity的Prefab编辑，以供导出成Occupation配置表，（未经旋转过的 )
    /// </summary>
    /// <returns></returns>
    public EntityOccupationData GetEntityOccupationGPs_Editor()
    {
        EntityIndicatorHelper.RefreshEntityIndicatorOccupationData();
        return EntityIndicatorHelper.EntityOccupationData;
    }

#endif

    // 旋转过的局部坐标
    public List<GridPos3D> GetEntityOccupationGPs_Rotated()
    {
        List<GridPos3D> occupation_rotated = ConfigManager.GetEntityOccupationData(EntityTypeIndex).BoxIndicatorGPs_RotatedDict[EntityOrientation];
        return occupation_rotated;
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

    #endregion

    #region BoxExtraData

    public void ApplyBoxExtraSerializeData(EntityExtraSerializeData boxExtraSerializeDataFromModule = null)
    {
        if (boxExtraSerializeDataFromModule != null)
        {
            foreach (EntityPassiveSkill extraPS in boxExtraSerializeDataFromModule.EntityPassiveSkills)
            {
                EntityPassiveSkill newPS = extraPS.Clone();
                AddNewPassiveSkill(newPS, true);
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

    [SerializeReference]
    [FoldoutGroup("初始被动技能")]
    [LabelText("初始被动技能列表")]
    [ListDrawerSettings(ListElementLabelName = "Description")]
    public List<EntityPassiveSkill> RawEntityPassiveSkills = new List<EntityPassiveSkill>(); // 干数据，禁修改

    [NonSerialized]
    [ShowInInspector]
    [FoldoutGroup("当前被动技能")]
    [LabelText("当前被动技能列表")]
    public List<EntityPassiveSkill> EntityPassiveSkills = new List<EntityPassiveSkill>(); // 湿数据，每个Entity生命周期开始前从干数据进行数据拷贝

    internal bool PassiveSkillMarkAsDestroyed = false;

    private List<EntityPassiveSkill> cachedRemoveList_EntityPassiveSkill = new List<EntityPassiveSkill>(5);

    protected void InitPassiveSkills()
    {
        cachedRemoveList_EntityPassiveSkill.Clear();
        if (EntityPassiveSkills.Count > 0)
        {
            foreach (EntityPassiveSkill eps in EntityPassiveSkills)
            {
                if (eps.IsAddedDuringGamePlay)
                {
                    cachedRemoveList_EntityPassiveSkill.Add(eps);
                }
            }

            foreach (EntityPassiveSkill eps in cachedRemoveList_EntityPassiveSkill)
            {
                EntityPassiveSkills.Remove(eps);
            }

            if (EntityPassiveSkills.Count == RawEntityPassiveSkills.Count)
            {
                for (int i = 0; i < RawEntityPassiveSkills.Count; i++)
                {
                    EntityPassiveSkill eps = EntityPassiveSkills[i];
                    eps.Entity = this;
                    eps.IsAddedDuringGamePlay = false; // 从Raw里面拷出来的都即为非动态添加的技能
                    eps.CopyDataFrom(RawEntityPassiveSkills[i]);
                    eps.OnInit();
                    eps.OnRegisterLevelEventID();
                }
            }
            else
            {
                Debug.Log("EntityPassiveSkills的数量和RawEntityPassiveSkills不一致，请检查临时EPS添加情况");
            }
        }
        else
        {
            foreach (EntityPassiveSkill rawAPS in RawEntityPassiveSkills)
            {
                EntityPassiveSkill ps = rawAPS.Clone();
                AddNewPassiveSkill(ps, false);
            }
        }

        PassiveSkillMarkAsDestroyed = false;
    }

    public void AddNewPassiveSkill(EntityPassiveSkill eps, bool isAddedDuringGamePlay)
    {
        EntityPassiveSkills.Add(eps);
        eps.IsAddedDuringGamePlay = isAddedDuringGamePlay;
        eps.Entity = this;
        eps.OnInit();
        eps.OnRegisterLevelEventID();
    }

    protected void UnInitPassiveSkills()
    {
        foreach (EntityPassiveSkill eps in EntityPassiveSkills)
        {
            eps.OnUnRegisterLevelEventID();
            eps.OnUnInit();
        }

        PassiveSkillMarkAsDestroyed = false;
    }

    #endregion

    #region 主动技能

    [SerializeReference]
    [FoldoutGroup("主动技能")]
    [LabelText("主动技能列表")]
    [ListDrawerSettings(ListElementLabelName = "SkillAlias")]
    public List<EntityActiveSkill> RawEntityActiveSkills = new List<EntityActiveSkill>(); // 干数据，禁修改

    [NonSerialized]
    public List<EntityActiveSkill> EntityActiveSkills = new List<EntityActiveSkill>(); // 湿数据，每个Entity生命周期开始前从干数据拷出，结束后清除

    public Dictionary<EntitySkillIndex, EntityActiveSkill> EntityActiveSkillDict = new Dictionary<EntitySkillIndex, EntityActiveSkill>(); // 便于寻找

    private List<EntityActiveSkill> cachedRemoveList_EntityActiveSkill = new List<EntityActiveSkill>(5);

    internal bool ActiveSkillMarkAsDestroyed = false;

    public bool ActiveSkillCanMove
    {
        get
        {
            foreach (EntityActiveSkill eas in EntityActiveSkills)
            {
                if (!eas.CurrentAllowMove) return false;
            }

            return true;
        }
    }

    protected void InitActiveSkills()
    {
        cachedRemoveList_EntityActiveSkill.Clear();
        if (EntityActiveSkills.Count > 0)
        {
            foreach (EntityActiveSkill eas in EntityActiveSkills)
            {
                if (eas.IsAddedDuringGamePlay)
                {
                    cachedRemoveList_EntityActiveSkill.Add(eas);
                }
            }

            foreach (EntityActiveSkill eas in cachedRemoveList_EntityActiveSkill)
            {
                EntityActiveSkills.Remove(eas);
                EntityActiveSkillDict.Remove(eas.EntitySkillIndex);
            }

            if (EntityActiveSkills.Count == RawEntityActiveSkills.Count)
            {
                for (int i = 0; i < RawEntityActiveSkills.Count; i++)
                {
                    EntityActiveSkill eas = EntityActiveSkills[i];
                    eas.CopyDataFrom(RawEntityActiveSkills[i]);
                    eas.Entity = this;
                    eas.IsAddedDuringGamePlay = false; // 从Raw里面拷出来的都即为非动态添加的技能
                    eas.ParentActiveSkill = null;
                    eas.OnInit();
                }
            }
            else
            {
                Debug.Log("EntityActiveSkills的数量和RawEntityActiveSkills不一致，请检查临时EAS添加情况");
            }
        }
        else
        {
            foreach (EntityActiveSkill rawEAS in RawEntityActiveSkills)
            {
                EntityActiveSkill eas = rawEAS.Clone();
                AddNewActiveSkill(eas, false);
            }
        }

        ActiveSkillMarkAsDestroyed = false;
    }

    protected void AddNewActiveSkill(EntityActiveSkill eas, bool isAddedDuringGamePlay)
    {
        EntityActiveSkills.Add(eas);
        eas.Entity = this;
        eas.IsAddedDuringGamePlay = isAddedDuringGamePlay;
        eas.ParentActiveSkill = null;
        eas.OnInit();
        if (!EntityActiveSkillDict.ContainsKey(eas.EntitySkillIndex))
        {
            EntityActiveSkillDict.Add(eas.EntitySkillIndex, eas);
        }
        else
        {
            Debug.LogError($"[主动技能] {name} 主动技能编号重复: {eas.EntitySkillIndex}");
        }
    }

    protected void UnInitActiveSkills()
    {
        foreach (EntityActiveSkill eas in EntityActiveSkills)
        {
            eas.OnUnInit();
        }

        ActiveSkillMarkAsDestroyed = false;
    }

    #endregion

    #endregion

    #region Camp

    [LabelText("阵营")]
    [FoldoutGroup("状态")]
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
        if ((IsPlayerOrFriendCamp) && target.IsEnemyCamp) return true;
        if ((target.IsPlayerOrFriendCamp) && IsEnemyCamp) return true;
        return false;
    }

    public bool IsSameCampOf(Entity target)
    {
        return !IsOpponentCampOf(target);
    }

    public bool IsNeutralCampOf(Entity target)
    {
        if ((IsPlayerOrFriendCamp) && target.IsNeutralCamp) return true;
        if ((target.IsPlayerOrFriendCamp) && IsNeutralCamp) return true;
        return false;
    }

    public bool IsOpponentOrNeutralCampOf(Entity target)
    {
        return IsOpponentCampOf(target) || IsNeutralCampOf(target);
    }

    #endregion

    public override void OnRecycled()
    {
        base.OnRecycled();
        StopAllCoroutines();
        InitWorldModuleGUID = 0;
    }

    public void Setup(uint initWorldModuleGUID)
    {
        InitWorldModuleGUID = initWorldModuleGUID;
    }

    protected virtual void FixedUpdate()
    {
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
    }

    public override int GetHashCode()
    {
        return GUID.GetHashCode();
    }

    #region Utils

    private IEnumerable<string> GetAllBoxTypeNames => ConfigManager.GetAllBoxTypeNames();
    private IEnumerable<string> GetAllFXTypeNames => ConfigManager.GetAllFXTypeNames();

    #endregion
}

public static class EntityUtils
{
    public static bool IsNotNullAndAlive(this Entity entity)
    {
        return entity != null && !entity.IsRecycled && !entity.ActiveSkillMarkAsDestroyed && !entity.PassiveSkillMarkAsDestroyed;
    }
}