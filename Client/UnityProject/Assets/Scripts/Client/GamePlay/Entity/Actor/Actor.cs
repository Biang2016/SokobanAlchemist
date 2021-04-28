using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BiangLibrary.CloneVariant;
using BiangLibrary.GameDataFormat.Grid;
using BiangLibrary.GamePlay.UI;
using DG.Tweening;
using NodeCanvas.Framework;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class Actor : Entity
{
    public static bool ENABLE_ACTOR_MOVE_LOG =
#if UNITY_EDITOR
        false;
#else
        false;
#endif

    private bool forbidAction = false;

    internal bool ForbidAction
    {
        get { return forbidAction; }
        set
        {
            if (forbidAction != value)
            {
                forbidAction = value;
                if (forbidAction)
                {
                    RemoveRigidbody();
                }
                else
                {
                    AddRigidbody();
                }
            }
        }
    }

    internal bool HasRigidbody = true;

    [FoldoutGroup("组件")]
    public Rigidbody RigidBody;

    [FoldoutGroup("组件")]
    public GameObject ActorMoveColliderRoot;

    [FoldoutGroup("组件")]
    public ActorControllerHelper ActorControllerHelper;

    [FoldoutGroup("组件")]
    public ActorPushHelper ActorPushHelper;

    [FoldoutGroup("组件")]
    public ActorFaceHelper ActorFaceHelper;

    [FoldoutGroup("组件")]
    public ActorSkinHelper ActorSkinHelper;

    [FoldoutGroup("组件")]
    public ActorLaunchArcRendererHelper ActorLaunchArcRendererHelper;

    [FoldoutGroup("组件")]
    public ActorBattleHelper ActorBattleHelper;

    [FoldoutGroup("组件")]
    public ActorBoxInteractHelper ActorBoxInteractHelper;

    [FoldoutGroup("组件")]
    public ActorSkillLearningHelper ActorSkillLearningHelper;

    [FoldoutGroup("组件")]
    public Transform LiftBoxPivot;

    public Vector3 ArtPos => ActorSkinHelper.MainArtTransform.position;

    #region EntityHelpers

    internal override EntityArtHelper EntityArtHelper => ActorArtHelper;

    [FoldoutGroup("组件")]
    public ActorArtHelper ActorArtHelper;

    internal override EntityWwiseHelper EntityWwiseHelper => ActorWwiseHelper;

    [FoldoutGroup("组件")]
    public EntityWwiseHelper ActorWwiseHelper;

    internal override EntityModelHelper EntityModelHelper => ActorModelHelper;

    [FoldoutGroup("组件")]
    public EntityModelHelper ActorModelHelper;

    internal override EntityIndicatorHelper EntityIndicatorHelper => ActorIndicatorHelper;

    [FoldoutGroup("组件")]
    public EntityIndicatorHelper ActorIndicatorHelper;

    internal override EntityBuffHelper EntityBuffHelper => ActorBuffHelper;

    [FoldoutGroup("组件")]
    public EntityBuffHelper ActorBuffHelper;

    internal override EntityFrozenHelper EntityFrozenHelper => ActorFrozenHelper;

    [FoldoutGroup("组件")]
    public ActorFrozenHelper ActorFrozenHelper;

    internal override EntityTriggerZoneHelper EntityTriggerZoneHelper => ActorTriggerZoneHelper;

    [FoldoutGroup("组件")]
    public EntityTriggerZoneHelper ActorTriggerZoneHelper;

    internal override EntityCollectHelper EntityCollectHelper => ActorCollectHelper;

    [FoldoutGroup("组件")]
    public EntityCollectHelper ActorCollectHelper;

    internal override EntityGrindTriggerZoneHelper EntityGrindTriggerZoneHelper => ActorGrindTriggerZoneHelper;

    [FoldoutGroup("组件")]
    public EntityGrindTriggerZoneHelper ActorGrindTriggerZoneHelper;

    internal override List<EntityFlamethrowerHelper> EntityFlamethrowerHelpers => ActorFlamethrowerHelpers;

    [FoldoutGroup("组件")]
    public List<EntityFlamethrowerHelper> ActorFlamethrowerHelpers;

    internal override List<EntityLightningGeneratorHelper> EntityLightningGeneratorHelpers => ActorLightningGeneratorHelpers;

    [FoldoutGroup("组件")]
    public List<EntityLightningGeneratorHelper> ActorLightningGeneratorHelpers;

    #endregion

    internal GraphOwner GraphOwner => ActorControllerHelper != null ? ((ActorControllerHelper is EnemyControllerHelper ech) ? ech.GraphOwner : null) : null;
    internal ActorAIAgent ActorAIAgent => ActorControllerHelper != null ? ((ActorControllerHelper is EnemyControllerHelper ech) ? ech.ActorAIAgent : null) : null;

    #region 状态

    [ReadOnly]
    [DisplayAsString]
    [FoldoutGroup("状态")]
    [LabelText("角色分类")]
    public ActorCategory ActorCategory;

    [ReadOnly]
    [DisplayAsString]
    [FoldoutGroup("状态")]
    [LabelText("角色类型")]
    public string ActorType;

    [ReadOnly]
    [DisplayAsString]
    [FoldoutGroup("状态")]
    [LabelText("移动倾向")]
    public Vector3 CurMoveAttempt;

    [ReadOnly]
    [DisplayAsString]
    [FoldoutGroup("状态")]
    [LabelText("上一帧移动倾向")]
    public Vector3 LastMoveAttempt;

    [ReadOnly]
    [DisplayAsString]
    [FoldoutGroup("状态")]
    [LabelText("扔箱子瞄准点移动倾向")]
    public Vector3 CurThrowMoveAttempt;

    [ReadOnly]
    [DisplayAsString]
    [FoldoutGroup("状态")]
    [LabelText("扔箱子瞄准点偏移")]
    public Vector3 CurThrowPointOffset;

    [DisableInEditorMode]
    [ShowInInspector]
    [FoldoutGroup("状态")]
    [LabelText("当前举着的箱子")]
    internal Box CurrentLiftBox = null;

    [DisableInEditorMode]
    [ShowInInspector]
    [FoldoutGroup("状态")]
    [LabelText("是否着地")]
    internal bool IsGrounded = false;

    [ReadOnly]
    [DisplayAsString]
    [ShowInInspector]
    [FoldoutGroup("状态")]
    [LabelText("世界坐标")]
    public override GridPos3D WorldGP
    {
        get { return curWorldGP; }
        set
        {
            if (curWorldGP != value)
            {
                if (!IsFrozen && !IsTriggerEntity)
                {
                    foreach (GridPos3D offset in GetEntityOccupationGPs_Rotated())
                    {
                        GridPos3D gridPos = value + offset;
                        Entity targetGridEntity = WorldManager.Instance.CurrentWorld.GetImpassableEntityByGridPosition(gridPos, GUID, out WorldModule targetGridModule, out GridPos3D localGP);
                        if (targetGridModule == null) return; // 防止角色走入空模组
                        if (targetGridEntity != null)
                        {
                            // 检查改对象是否真的占据这几格，否则是bug
                            bool correctOccupation = false;
                            if (targetGridEntity.IsNotNullAndAlive())
                            {
                                List<GridPos3D> occupationGPs = targetGridEntity.GetEntityOccupationGPs_Rotated();
                                if (occupationGPs != null)
                                {
                                    foreach (GridPos3D _offset in occupationGPs)
                                    {
                                        GridPos3D _gridPos = targetGridEntity.WorldGP + _offset;
                                        if (_gridPos == gridPos)
                                        {
                                            correctOccupation = true;
                                            break;
                                        }
                                    }
                                }
                            }

                            if (correctOccupation) return; // 防止角色和其他Entity卡住
                            else
                            {
                                targetGridModule[TypeDefineType.Actor, localGP] = null; // todo 执行到这里说明是bug，先这样处理，以后再研究
                            }
                        }
                    }
                }

                if (!IsFrozen && !IsTriggerEntity) UnRegisterFromModule(curWorldGP, EntityOrientation);
                curWorldGP = value;
                if (!IsFrozen && !IsTriggerEntity) RegisterInModule(curWorldGP, EntityOrientation);
            }
        }
    }

    private GridPos3D curWorldGP;

    /// <summary>
    /// 这是寻路专用节点坐标，和世界坐标略有偏差，Y坐标为角色基底那层的Y坐标
    /// </summary>
    [DisplayAsString]
    [HideInEditorMode]
    [FoldoutGroup("状态")]
    [LabelText("寻路节点坐标")]
    public GridPos3D WorldGP_PF
    {
        get
        {
            int xmin = int.MaxValue;
            int ymin = int.MaxValue;
            int zmin = int.MaxValue;
            foreach (GridPos3D offset in GetEntityOccupationGPs_Rotated())
            {
                if (offset.x < xmin) xmin = offset.x;
                if (offset.y < ymin) ymin = offset.y;
                if (offset.z < zmin) zmin = offset.z;
            }

            return WorldGP + new GridPos3D(xmin, ymin, zmin);
        }
    }

    public void UnRegisterFromModule(GridPos3D oldWorldGP, GridPosR.Orientation oldOrientation)
    {
        if (IsRecycling) return;
        if (IsTriggerEntity) return;
        List<GridPos3D> occupationData_Rotated = ConfigManager.GetEntityOccupationData(EntityTypeIndex).EntityIndicatorGPs_RotatedDict[oldOrientation];
        foreach (GridPos3D offset in occupationData_Rotated)
        {
            GridPos3D gridPos = oldWorldGP + offset;
            WorldModule currentGridModule = WorldManager.Instance.CurrentWorld.GetModuleByWorldGP(gridPos, true);
            if (currentGridModule != null)
            {
                GridPos3D currentGridLocalGP = currentGridModule.WorldGPToLocalGP(gridPos);
                currentGridModule[TypeDefineType.Actor, currentGridLocalGP] = null;
            }
        }
    }

    public void RegisterInModule(GridPos3D newWorldGP, GridPosR.Orientation newOrientation)
    {
        if (IsRecycling) return;
        if (IsTriggerEntity) return;
        foreach (GridPos3D offset in ConfigManager.GetEntityOccupationData(EntityTypeIndex).EntityIndicatorGPs_RotatedDict[newOrientation])
        {
            GridPos3D gridPos = newWorldGP + offset;
            WorldModule currentGridModule = WorldManager.Instance.CurrentWorld.GetModuleByWorldGP(gridPos, true);
            if (currentGridModule != null)
            {
                GridPos3D currentGridLocalGP = currentGridModule.WorldGPToLocalGP(gridPos);
                currentGridModule[TypeDefineType.Actor, currentGridLocalGP] = this;
            }
        }
    }

    #endregion

    #region 旋转朝向

    internal override void SwitchEntityOrientation(GridPosR.Orientation newOrientation)
    {
        if (EntityOrientation == newOrientation) return;

        // Actor由于限制死平面必须是正方形，因此可以用左下角坐标相减得到核心坐标偏移量；在旋转时应用此偏移量，可以保证平面正方形仍在老位置
        GridPos offset = ActorRotateWorldGPOffset(ActorWidth, newOrientation) - ActorRotateWorldGPOffset(ActorWidth, EntityOrientation);
        if (!IsFrozen && !IsTriggerEntity) UnRegisterFromModule(curWorldGP, EntityOrientation);
        base.SwitchEntityOrientation(newOrientation);
        if (!IsFrozen && !IsTriggerEntity) RegisterInModule(curWorldGP, newOrientation);

        transform.position = new Vector3(offset.x + curWorldGP.x, transform.position.y, offset.z + curWorldGP.z);
        transform.rotation = Quaternion.Euler(0, (int) newOrientation * 90f, 0);
        WorldGP = GridPos3D.GetGridPosByTrans(transform, 1);
    }

    public static GridPos ActorRotateWorldGPOffset(int actorWidth, GridPosR.Orientation orientation) // todo 这里的先决条件是所有的Actor都以左下角作为原点
    {
        switch (orientation)
        {
            case GridPosR.Orientation.Up:
                return GridPos.Zero;
            case GridPosR.Orientation.Right:
                return new GridPos(0, actorWidth - 1);
            case GridPosR.Orientation.Down:
                return new GridPos(actorWidth - 1, actorWidth - 1);
            case GridPosR.Orientation.Left:
                return new GridPos(actorWidth - 1, 0);
        }

        return GridPos.Zero;
    }

    #endregion

    #region Occupation

    public const int ACTOR_MAX_HEIGHT = 4;
    public int ActorWidth => EntityIndicatorHelper.EntityOccupationData.ActorWidth;
    public int ActorHeight => EntityIndicatorHelper.EntityOccupationData.ActorHeight;

    public bool CheckIsGrounded(float checkDistance, out GridPos3D nearestGroundGP)
    {
        bool isGrounded = false;
        nearestGroundGP = GridPos3D.Zero;
        float nearestDistance = float.MaxValue;
        foreach (GridPos3D offset in GetEntityOccupationGPs_Rotated())
        {
            Vector3 startPos = transform.position + offset;
            if (WorldManager.Instance == null || WorldManager.Instance.CurrentWorld == null)
            {
                Debug.Log(name);
            }

            bool gridGrounded = WorldManager.Instance.CurrentWorld.CheckIsGroundByPos(startPos, checkDistance, true, out GridPos3D groundGP);
            isGrounded |= gridGrounded;
            if (gridGrounded)
            {
                float dist = (nearestGroundGP - startPos).magnitude;
                if (nearestDistance > dist)
                {
                    nearestDistance = dist;
                    nearestGroundGP = groundGP;
                }
            }
        }

        return isGrounded;
    }

    #endregion

    #region 特效

    [FoldoutGroup("特效")]
    [LabelText("@\"踢特效\t\"+KickFX")]
    public FXConfig KickFX = new FXConfig();

    [FoldoutGroup("特效")]
    [LabelText("踢特效锚点")]
    public Transform KickFXPivot;

    [FoldoutGroup("特效")]
    [LabelText("@\"受伤特效\t\"+InjureFX")]
    public FXConfig InjureFX = new FXConfig();

    [FoldoutGroup("特效")]
    [LabelText("@\"生命恢复特效\t\"+HealFX")]
    public FXConfig HealFX = new FXConfig();

    [FoldoutGroup("特效")]
    [LabelText("@\"死亡特效\t\"+DieFX")]
    public FXConfig DieFX = new FXConfig();

    #endregion

    [FoldoutGroup("属性")]
    [LabelText("频繁更新")]
    public bool FrequentUpdate = false;

    [FoldoutGroup("属性")]
    [LabelText("角色阶级")]
    public ActorClass ActorClass = ActorClass.Normal;

    #region 手感

    [FoldoutGroup("手感")]
    [LabelText("Dash力度")]
    public float DashForce = 150f;

    [FoldoutGroup("手感")]
    [LabelText("起步速度")]
    public float Accelerate = 200f;

    internal float ThrowRadiusMin = 0.75f;

    [FoldoutGroup("手感")]
    [LabelText("踢箱子力量")]
    public float KickForce = 30;

    [FoldoutGroup("手感")]
    [LabelText("扔半径")]
    public float ThrowRadius = 15f;

    [FoldoutGroup("手感")]
    [LabelText("跳跃力")]
    public float ActiveJumpForce = 10f;

    [FoldoutGroup("手感")]
    [LabelText("坠落力")]
    public float FallForce = 0.1f;

    #endregion

    #region 推踢扔举能力

    [FoldoutGroup("推踢扔举能力")]
    [LabelText("推箱子类型")]
    [ListDrawerSettings(ListElementLabelName = "TypeName")]
    public List<TypeSelectHelper> PushableBoxList = new List<TypeSelectHelper>();

    [FoldoutGroup("推踢扔举能力")]
    [LabelText("踢箱子类型")]
    [ListDrawerSettings(ListElementLabelName = "TypeName")]
    public List<TypeSelectHelper> KickableBoxList = new List<TypeSelectHelper>();

    [FoldoutGroup("推踢扔举能力")]
    [LabelText("举箱子类型")]
    [ListDrawerSettings(ListElementLabelName = "TypeName")]
    public List<TypeSelectHelper> LiftableBoxList = new List<TypeSelectHelper>();

    [FoldoutGroup("推踢扔举能力")]
    [LabelText("扔箱子类型")]
    [ListDrawerSettings(ListElementLabelName = "TypeName")]
    public List<TypeSelectHelper> ThrowableBoxList = new List<TypeSelectHelper>();

    #endregion

    #region 死亡

    public void Reborn()
    {
        EntityCollectHelper?.OnReborn();
        EntityStatPropSet.OnReborn();
        EntityBuffHelper.OnReborn();
        ActorBattleHelper.InGameHealthBar.Initialize(ActorBattleHelper, 100, 30);
        ClientGameManager.Instance.PlayerStatHUDPanel.Initialize();
        ActiveSkillMarkAsDestroyed = false;
        PassiveSkillMarkAsDestroyed = false;
    }

    public void ReloadESPS(EntityStatPropSet srcESPS)
    {
        // 财产保留 todo 待重构
        int gold = EntityStatPropSet.Gold.Value;
        int fire = EntityStatPropSet.FireElementFragment.Value;
        int ice = EntityStatPropSet.IceElementFragment.Value;
        int lightning = EntityStatPropSet.LightningElementFragment.Value;

        EntityStatPropSet.OnRecycled();
        srcESPS.ApplyDataTo(EntityStatPropSet);
        EntityStatPropSet.Initialize(this);
        ActorBattleHelper.InGameHealthBar.Initialize(ActorBattleHelper, 100, 30);

        EntityStatPropSet.Gold.SetValue(gold);
        EntityStatPropSet.FireElementFragment.SetValue(fire);
        EntityStatPropSet.IceElementFragment.SetValue(ice);
        EntityStatPropSet.LightningElementFragment.SetValue(lightning);

        ClientGameManager.Instance.PlayerStatHUDPanel.Initialize();
        ActiveSkillMarkAsDestroyed = false;
        PassiveSkillMarkAsDestroyed = false;
    }

    public void ReloadActorSkillLearningData(ActorSkillLearningData srcASLD)
    {
        foreach (EntityPassiveSkill eps in EntityPassiveSkills.ToArray())
        {
            ForgetPassiveSkill(eps.SkillGUID);
        }

        foreach (string skillGUID in EntityActiveSkillGUIDDict.Keys.ToArray())
        {
            ForgetActiveSkill(skillGUID);
        }

        ActorSkillLearningHelper.ActorSkillLearningData.Clear(); // 清空放在遗忘后面，放在添加前面，确保最终结果和srcASLD相同

        foreach (string skillGUID in srcASLD.LearnedPassiveSkillGUIDs)
        {
            EntitySkill rawEntitySkill = ConfigManager.GetEntitySkill(skillGUID);
            if (rawEntitySkill is EntityPassiveSkill eps)
            {
                AddNewPassiveSkill(eps);
            }
        }

        foreach (string skillGUID in srcASLD.LearnedActiveSkillGUIDs)
        {
            EntitySkill rawEntitySkill = ConfigManager.GetEntitySkill(skillGUID);
            if (rawEntitySkill is EntityActiveSkill eas)
            {
                EntitySkillIndex skillIndex = srcASLD.LearnedActiveSkillDict[skillGUID];
                if (AddNewActiveSkill(eas, skillIndex))
                {
                    foreach (KeyValuePair<PlayerControllerHelper.KeyBind, List<EntitySkillIndex>> kv in srcASLD.SkillKeyMappings)
                    {
                        foreach (EntitySkillIndex esi in kv.Value)
                        {
                            if (esi == skillIndex)
                            {
                                BindActiveSkillToKey(eas, kv.Key, false);
                            }
                        }
                    }
                }
            }
        }
    }

    #endregion

    #region 冻结

    [FoldoutGroup("冻结")]
    [LabelText("@\"冻结特效\t\"+FrozeFX")]
    public FXConfig FrozeFX = new FXConfig();

    [FoldoutGroup("冻结")]
    [LabelText("@\"解冻特效\t\"+ThawFX")]
    public FXConfig ThawFX = new FXConfig();

    [SerializeReference]
    [FoldoutGroup("冻结")]
    [LabelText("冻结的箱子被动技能")]
    [ListDrawerSettings(ListElementLabelName = "Description")]
    public List<EntityPassiveSkill> RawFrozenBoxPassiveSkills = new List<EntityPassiveSkill>(); // 干数据，禁修改

    #endregion

    private List<SmoothMove> SmoothMoves = new List<SmoothMove>();

    [ShowInInspector]
    [HideInEditorMode]
    internal bool IsInDungeon = false;

    public enum ActorBehaviourStates
    {
        Idle,
        Frozen,
        Walk,
        Chase,
        Attack,
        Push,
        Dash,
        Vault,
        Kick,
        Escape,
        Jump,
        SmashDown,
        InAirMoving,
    }

    [ReadOnly]
    [FoldoutGroup("状态")]
    [LabelText("行为状态")]
    public ActorBehaviourStates ActorBehaviourState = ActorBehaviourStates.Idle;

    public enum ThrowStates
    {
        None,
        Raising,
        Lifting,
        ThrowCharging,
    }

    [ReadOnly]
    [FoldoutGroup("状态")]
    [LabelText("扔技能状态")]
    public ThrowStates ThrowState = ThrowStates.None;

    public override void OnUsed()
    {
        gameObject.SetActive(true);
        base.OnUsed();
        EntityArtHelper?.OnHelperUsed();
        EntityWwiseHelper.OnHelperUsed();
        EntityModelHelper.OnHelperUsed();
        EntityIndicatorHelper.OnHelperUsed();
        EntityBuffHelper.OnHelperUsed();
        EntityFrozenHelper.OnHelperUsed();
        EntityTriggerZoneHelper?.OnHelperUsed();
        EntityCollectHelper?.OnHelperUsed();
        EntityGrindTriggerZoneHelper?.OnHelperUsed();
        foreach (EntityFlamethrowerHelper h in EntityFlamethrowerHelpers)
        {
            h.OnHelperUsed();
        }

        foreach (EntityLightningGeneratorHelper h in EntityLightningGeneratorHelpers)
        {
            h.OnHelperUsed();
        }

        ActorControllerHelper?.OnHelperUsed();
        ActorPushHelper.OnHelperUsed();
        ActorFaceHelper.OnHelperUsed();
        ActorSkinHelper.OnHelperUsed();
        ActorLaunchArcRendererHelper.OnHelperUsed();
        ActorBattleHelper.OnHelperUsed();
        ActorBoxInteractHelper.OnHelperUsed();
        ActorSkillLearningHelper?.OnHelperUsed();

        ActorMoveColliderRoot.SetActive(true);
    }

    internal bool IsRecycling = false;

    /// <summary>
    /// 在一场游戏中，Player不进行回收，回收会导致玩家的进度丢失
    /// </summary>
    public override void OnRecycled()
    {
        if (ActorClass == ActorClass.FinalBoss)
        {
            WwiseAudioManager.Instance.WwiseBGMConfiguration.SpiderLegEnemyDistanceToPlayer.SetGlobalValue(99999f);
        }

        IsRecycling = true;
        IsInDungeon = false;
        ForbidAction = true;
        if (!HasRigidbody) AddRigidbody();
        RigidBody.drag = 100f;
        RigidBody.velocity = Vector3.zero;

        ActorBehaviourState = ActorBehaviourStates.Idle;
        GraphOwner?.StopBehaviour();
        ActorAIAgent?.Stop();
        CurMoveAttempt = Vector3.zero;
        LastMoveAttempt = Vector3.zero;
        CurThrowMoveAttempt = Vector3.zero;
        CurThrowPointOffset = Vector3.zero;
        CurForward = Vector3.forward;
        WorldGP = GridPos3D.Zero;
        LastWorldGP = GridPos3D.Zero;
        ThrowState = ThrowStates.None;
        ClearJumpParams();
        ThrowWhenDie();

        EntityArtHelper?.OnHelperRecycled();
        EntityWwiseHelper.OnHelperRecycled();
        EntityModelHelper.OnHelperRecycled();
        EntityIndicatorHelper.OnHelperRecycled();
        EntityBuffHelper.OnHelperRecycled();
        EntityFrozenHelper.OnHelperRecycled();
        EntityTriggerZoneHelper?.OnHelperRecycled();
        EntityCollectHelper?.OnHelperRecycled();
        EntityGrindTriggerZoneHelper?.OnHelperRecycled();
        foreach (EntityFlamethrowerHelper h in EntityFlamethrowerHelpers)
        {
            h.OnHelperRecycled();
        }

        foreach (EntityLightningGeneratorHelper h in EntityLightningGeneratorHelpers)
        {
            h.OnHelperRecycled();
        }

        ActorControllerHelper?.OnHelperRecycled();
        ActorPushHelper.OnHelperRecycled();
        ActorFaceHelper.OnHelperRecycled();
        ActorSkinHelper.OnHelperRecycled();
        ActorLaunchArcRendererHelper.OnHelperRecycled();
        ActorBattleHelper.OnHelperRecycled();
        ActorBoxInteractHelper.OnHelperRecycled();
        ActorSkillLearningHelper?.OnHelperRecycled();

        UnInitActiveSkills();
        UnInitPassiveSkills();
        EntityStatPropSet.OnRecycled();

        ActorMoveColliderRoot.SetActive(false);
        SetModelSmoothMoveLerpTime(0);
        SwitchEntityOrientation(GridPosR.Orientation.Up);
        StopAllCoroutines();
        gameObject.SetActive(false);
        BattleManager.Instance.RemoveActor(this);
        base.OnRecycled();
        IsRecycling = false;
    }

    void Awake()
    {
        SmoothMoves = GetComponentsInChildren<SmoothMove>().ToList();
        SetModelSmoothMoveLerpTime(0);
        EntityStatPropSet = new EntityStatPropSet();
    }

    internal float DefaultSmoothMoveLerpTime = 0.02f;

    public void SetModelSmoothMoveLerpTime(float lerpTime)
    {
        if (lerpTime.Equals(0))
        {
            foreach (SmoothMove sm in SmoothMoves)
            {
                sm.enabled = false;
            }
        }
        else
        {
            foreach (SmoothMove sm in SmoothMoves)
            {
                sm.enabled = true;
                sm.SmoothTime = lerpTime;
            }
        }
    }

    /// <summary>
    /// 在一场游戏中，Player不进行反复Setup，除非重载场景进入其他world
    /// 反复Setup将造成玩家的进度丢失
    /// </summary>
    /// <param name="entityData"></param>
    /// <param name="worldGP"></param>
    /// <param name="initWorldModuleGUID"></param>
    public void Setup(EntityData entityData, GridPos3D worldGP, uint initWorldModuleGUID)
    {
        base.Setup(initWorldModuleGUID);
        EntityTypeIndex = entityData.EntityTypeIndex;
        ActorType = entityData.EntityType.TypeName;
        ActorCategory = entityData.EntityTypeIndex == ConfigManager.Actor_PlayerIndex ? ActorCategory.Player : ActorCategory.Creature;
        if (ActorCategory == ActorCategory.Player) ClientGameManager.Instance.BattleMessenger.AddListener<Actor>((uint) Enum_Events.OnPlayerLoaded, OnLoaded);

        curWorldGP = worldGP; // 避免刚Setup就进行占位查询和登记，相关操作WorldModule已经代劳
        WorldGP = worldGP; // 避免刚Setup就进行占位查询和登记，相关操作WorldModule已经代劳
        LastWorldGP = WorldGP;
        EntityOrientation = entityData.EntityOrientation;
        GridPosR.ApplyGridPosToLocalTrans(new GridPosR(curWorldGP.x, curWorldGP.z, entityData.EntityOrientation), transform, 1);

        RawEntityStatPropSet.ApplyDataTo(EntityStatPropSet);
        EntityStatPropSet.Initialize(this);
        ActorBattleHelper.Initialize();
        ActorCollectHelper?.Initialize();
        ActorBoxInteractHelper.Initialize();
        ActorArtHelper.SetPFMoveGridSpeed(0);
        ActorArtHelper.SetIsPushing(false);
        ActorBattleHelper.OnDamaged += (damage) =>
        {
            float distance = (BattleManager.Instance.Player1.transform.position - transform.position).magnitude;
            CameraManager.Instance.FieldCamera.CameraShake(damage, distance);
        };

        InitPassiveSkills();
        InitActiveSkills();
        if (ActorControllerHelper != null)
        {
            if (ActorControllerHelper is PlayerControllerHelper pch)
            {
                pch.OnSetup(PlayerNumber.Player1);
            }
            else if (ActorControllerHelper is EnemyControllerHelper ech)
            {
                ech.OnSetup();
            }
        }

        ActorSkillLearningHelper?.LoadInitSkills();

        ForbidAction = false;
        ApplyEntityExtraSerializeData(entityData.RawEntityExtraSerializeData);
    }

    private void Update()
    {
        if (!BattleManager.Instance.IsStart) return;
        if (!IsRecycled)
        {
            UpdateThrowParabolaLine();
        }
    }

    protected override void Tick(float interval)
    {
        if (!BattleManager.Instance.IsStart) return;
        ActorControllerHelper?.OnTick(interval);
        base.Tick(interval);
    }

    public void OnLoaded(Actor actor)
    {
        if (actor == this)
        {
            SetModelSmoothMoveLerpTime(DefaultSmoothMoveLerpTime);
        }
    }

    public void SetShown(bool shown)
    {
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!BattleManager.Instance.IsStart) return;
        if (IsRecycled) return;
        ActorControllerHelper?.OnFixedUpdate();
        if (ENABLE_ACTOR_MOVE_LOG && WorldGP != LastWorldGP) Debug.Log($"[{Time.frameCount}] [Actor] {name} Move {LastWorldGP} -> {WorldGP}");
        LastWorldGP = WorldGP;
        WorldGP = GridPos3D.GetGridPosByTrans(transform, 1);
    }

    protected override void InitPassiveSkills()
    {
        base.InitPassiveSkills();
        // TODO load learned skills.
    }

    protected override void InitActiveSkills()
    {
        base.InitActiveSkills();
        // TODO load learned skills.
    }

    public void TransportPlayerGridPos(GridPos3D worldGP, float lerpTime = 0)
    {
        SetModelSmoothMoveLerpTime(lerpTime);
        transform.position = worldGP;
        LastWorldGP = WorldGP;
        curWorldGP = worldGP; // 强行移动
        WorldGP = worldGP;
        SetModelSmoothMoveLerpTime(DefaultSmoothMoveLerpTime);
    }

    internal virtual void MoveInternal()
    {
        if (!ActiveSkillCanMove) CurMoveAttempt = Vector3.zero;
        if (!CannotAct && HasRigidbody)
        {
            if (CurMoveAttempt.magnitude > 0)
            {
                if (CurMoveAttempt.x.Equals(0)) RigidBody.velocity = new Vector3(0, RigidBody.velocity.y, RigidBody.velocity.z);
                if (CurMoveAttempt.z.Equals(0)) RigidBody.velocity = new Vector3(RigidBody.velocity.x, RigidBody.velocity.y, 0);
                if (ActorCategory == ActorCategory.Creature)
                {
                    ActorArtHelper.SetPFMoveGridSpeed(ActorAIAgent.NextStraightNodeCount);
                }
                else
                {
                    ActorArtHelper.SetPFMoveGridSpeed(1);
                }

                if (ActorCategory == ActorCategory.Player)
                {
                    if (!IsExecutingAirSkills())
                    {
                        ActorBehaviourState = ActorBehaviourStates.Walk;
                        ActorArtHelper.SetIsWalking(true);
                        ActorArtHelper.SetIsChasing(false);
                    }
                }
                else
                {
                    if (ActorBehaviourState == ActorBehaviourStates.Walk)
                    {
                        ActorArtHelper.SetIsWalking(true);
                        ActorArtHelper.SetIsChasing(false);
                    }
                    else if (ActorBehaviourState == ActorBehaviourStates.Chase)
                    {
                        ActorArtHelper.SetIsWalking(false);
                        ActorArtHelper.SetIsChasing(true);
                    }
                }

                RigidBody.drag = 0;
                RigidBody.mass = 1f;

                if (EntityStatPropSet.MoveSpeed.GetModifiedValue != 0)
                {
                    if (ActorArtHelper.CanPan)
                    {
                        Vector3 velDiff = CurMoveAttempt.normalized * Time.fixedDeltaTime * Accelerate;
                        Vector3 finalVel = new Vector3(RigidBody.velocity.x + velDiff.x, 0, RigidBody.velocity.z + velDiff.z);
                        float finalSpeed = EntityStatPropSet.MoveSpeed.GetModifiedValue / 10f;
                        if (finalVel.magnitude > finalSpeed)
                        {
                            finalVel = finalVel.normalized * finalSpeed;
                        }

                        finalVel.y = RigidBody.velocity.y;
                        RigidBody.AddForce(finalVel - RigidBody.velocity, ForceMode.VelocityChange);
                    }
                    else
                    {
                        RigidBody.drag = 100f;
                        RigidBody.mass = 1f;
                    }
                }

                CurForward = CurMoveAttempt.normalized;

                ActorPushHelper.TriggerOut = true;
                bool isBoxOnFront = Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 1f, LayerManager.Instance.LayerMask_BoxIndicator);
                if (isBoxOnFront)
                {
                    Box box = hit.collider.GetComponentInParent<Box>();
                    if (box && !box.Passable)
                    {
                        ActorArtHelper.SetIsPushing(true);
                    }
                    else
                    {
                        ActorArtHelper.SetIsPushing(false);
                    }
                }
                else
                {
                    ActorArtHelper.SetIsPushing(false);
                }
            }
            else
            {
                if (!IsExecutingAirSkills())
                {
                    ActorArtHelper.SetPFMoveGridSpeed(0);
                    ActorArtHelper.SetIsWalking(false);
                    ActorArtHelper.SetIsChasing(false);
                    ActorArtHelper.SetIsPushing(false);
                    RigidBody.drag = 100f;
                    RigidBody.mass = 1f;
                    ActorPushHelper.TriggerOut = false;
                }
            }

            if (!IsExecutingAirSkills())
            {
                if (CurMoveAttempt.x.Equals(0))
                {
                    SnapToGridX();
                }

                if (CurMoveAttempt.z.Equals(0))
                {
                    SnapToGridZ();
                }
            }
        }
        else
        {
            ActorBehaviourState = ActorBehaviourStates.Idle;
            ActorArtHelper.SetPFMoveGridSpeed(0);
            ActorArtHelper.SetIsWalking(false);
            ActorArtHelper.SetIsChasing(false);
            ActorArtHelper.SetIsPushing(false);
            if (HasRigidbody)
            {
                RigidBody.drag = 0f;
                RigidBody.mass = 1f;
            }

            CurMoveAttempt = Vector3.zero;
        }

        SnapToOrientation();
        WorldGP = transform.position.ToGridPos3D();

        // 着地判定
        IsGrounded = CheckIsGrounded(0.55f, out GridPos3D _);
        JumpingUpTick();
        InAirMovingToTargetPosTick();
        SmashingDownTick();
        if (!IsExecutingAirSkills() || (ActorBehaviourState == ActorBehaviourStates.Jump && JumpReachClimax))
        {
            if (!CannotAct && HasRigidbody)
            {
                if (IsGrounded)
                {
                    RigidBody.constraints |= RigidbodyConstraints.FreezePositionY;
                    SnapToGridY();
                }
                else
                {
                    RigidBody.constraints &= ~RigidbodyConstraints.FreezePositionY;
                    RigidBody.drag = 0f;
                    RigidBody.AddForce(Vector3.down * FallForce, ForceMode.VelocityChange);
                }
            }
        }

        if (IsDestroying)
        {
            if (HasRigidbody) RigidBody.constraints |= RigidbodyConstraints.FreezePositionY;
            SnapToGridY();
        }

        LastMoveAttempt = CurMoveAttempt;
        if (this == BattleManager.Instance.Player1)
        {
            if (HasRigidbody)
            {
                WwiseAudioManager.Instance.WwiseBGMConfiguration.PlayerMovementSpeed.SetGlobalValue(RigidBody.velocity.magnitude);
            }
            else
            {
                WwiseAudioManager.Instance.WwiseBGMConfiguration.PlayerMovementSpeed.SetGlobalValue(0);
            }
        }

        if (ActorClass == ActorClass.FinalBoss)
        {
            WwiseAudioManager.Instance.WwiseBGMConfiguration.SpiderLegEnemyDistanceToPlayer.SetGlobalValue((WorldGP - BattleManager.Instance.Player1.WorldGP).magnitude);
        }
    }

    private void AddRigidbody()
    {
        if (!HasRigidbody && RigidBody == null)
        {
            HasRigidbody = true;
            RigidBody = gameObject.AddComponent<Rigidbody>();
            RigidBody.velocity = Vector3.zero;
            RigidBody.mass = 1f;
            RigidBody.drag = 0f;
            RigidBody.angularDrag = 20f;
            RigidBody.useGravity = true;
            RigidBody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            RigidBody.interpolation = RigidbodyInterpolation.Interpolate;
            RigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
    }

    private void RemoveRigidbody()
    {
        if (HasRigidbody && RigidBody != null)
        {
            Destroy(RigidBody);
            HasRigidbody = false;
        }
    }

    public void SnapToGrid()
    {
        SnapToGridX();
        SnapToGridZ();
    }

    public void SnapToGridX()
    {
        transform.position = new Vector3(WorldGP.x, transform.position.y, transform.position.z);
    }

    public void SnapToGridY()
    {
        transform.position = new Vector3(transform.position.x, WorldGP.y, transform.position.z);
    }

    public void SnapToGridZ()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, WorldGP.z);
    }

    public void SnapToOrientation()
    {
        transform.localRotation = Quaternion.Euler(0, 90f * (int) EntityOrientation, 0);
    }

    private float ThrowChargeTick;
    internal float ThrowChargeMax = 1.5f;

    internal virtual void ThrowChargeAimInternal()
    {
        if (ThrowState == ThrowStates.ThrowCharging)
        {
            if (ThrowChargeTick < ThrowChargeMax)
            {
                ThrowChargeTick += Time.fixedDeltaTime;
            }
            else
            {
                ThrowChargeTick = ThrowChargeMax;
            }

            float radius = ThrowRadius * ThrowChargeTick / ThrowChargeMax + ThrowRadiusMin;
            if (CurThrowPointOffset.magnitude > radius)
            {
                CurThrowPointOffset = CurThrowPointOffset.normalized * radius;
            }
        }
        else
        {
            CurThrowPointOffset = Vector3.zero;
            ThrowChargeTick = 0;
        }
    }

    #region Skills

    internal int temp_DashMaxDistance;

    public void DoDash()
    {
        int finalDashDistance = 0;
        bool lastGridGrounded = true;
        for (int dashDistance = 1; dashDistance <= temp_DashMaxDistance; dashDistance++)
        {
            GridPos3D targetPos = WorldGP + CurForward.ToGridPos3D() * dashDistance;
            if (lastGridGrounded) finalDashDistance = dashDistance - 1;
            lastGridGrounded = WorldManager.Instance.CurrentWorld.CheckIsGroundByPos(targetPos, 5, true, out GridPos3D _);
            Entity targetOccupyEntity = WorldManager.Instance.CurrentWorld.GetImpassableEntityByGridPosition(targetPos, GUID, out WorldModule _, out GridPos3D _);
            if (targetOccupyEntity != null && !(targetOccupyEntity is Actor)) break;
        }

        TransportPlayerGridPos(WorldGP + CurForward.ToGridPos3D() * finalDashDistance, 0.3f);
        temp_DashMaxDistance = 0;
    }

    public void DoKickBox()
    {
        Ray ray = new Ray(transform.position - transform.forward * 0.49f, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 1.49f, LayerManager.Instance.LayerMask_BoxIndicator, QueryTriggerInteraction.Collide))
        {
            Box box = hit.collider.gameObject.GetComponentInParent<Box>();
            if (box && box.Kickable && ActorBoxInteractHelper.CanInteract(InteractSkillType.Kick, box.EntityTypeIndex))
            {
                box.Kick(CurForward, KickForce, this);
                FX kickFX = FXManager.Instance.PlayFX(KickFX, KickFXPivot.position);
            }
        }
    }

    public void SwapBox()
    {
        if (CannotAct) return;
        Ray ray = new Ray(transform.position - transform.forward * 0.49f, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 1.74f, LayerManager.Instance.LayerMask_BoxIndicator, QueryTriggerInteraction.Collide))
        {
            Box box = hit.collider.gameObject.GetComponentInParent<Box>();
            GridPos3D actorSwapBoxMoveAttempt = (hit.collider.transform.position - transform.position).ToGridPos3D().Normalized();
            if (box && box.Pushable && ActorBoxInteractHelper.CanInteract(InteractSkillType.Push, box.EntityTypeIndex))
            {
                box.ForceStopWhenSwapBox(this);

                Vector3 boxIndicatorPos = hit.collider.transform.position;
                GridPos3D boxIndicatorGP_offset = (boxIndicatorPos - box.transform.position).ToGridPos3D();
                GridPos3D boxIndicatorGP = boxIndicatorGP_offset + box.WorldGP;

                // 如果角色面朝方向Box的厚度大于一格，则无法swap
                GridPos3D boxIndicatorGP_behind = boxIndicatorGP + actorSwapBoxMoveAttempt;
                foreach (GridPos3D offset in box.GetEntityOccupationGPs_Rotated())
                {
                    if (offset == boxIndicatorGP_behind - box.WorldGP) return;
                }

                GridPos3D boxWorldGP_before = box.WorldGP;
                GridPos3D boxWorldGP_after = LastWorldGP - boxIndicatorGP + box.WorldGP;
                if (WorldManager.Instance.CurrentWorld.MoveBoxColumn(box.WorldGP, -actorSwapBoxMoveAttempt, Box.States.BeingPushed, true, 0f, true, GUID))
                {
                    if (Box.ENABLE_BOX_MOVE_LOG) Debug.Log($"[{Time.frameCount}] [Box] {box.name} SwapBox {boxWorldGP_before} -> {box.WorldGP}");
                    transform.position = boxIndicatorGP;
                    LastWorldGP = WorldGP;
                    WorldGP = GridPos3D.GetGridPosByTrans(transform, 1);
                    if (ENABLE_ACTOR_MOVE_LOG) Debug.Log($"[{Time.frameCount}] [Actor] {name} Swap {LastWorldGP} -> {WorldGP}");
                }
                else
                {
                    if (Box.ENABLE_BOX_MOVE_LOG) Debug.Log($"[{Time.frameCount}] [Box] {box.name} SwapBox MoveFailed {boxWorldGP_before} -> {boxWorldGP_after}");
                    GridPos3D actorTargetGP = boxIndicatorGP + actorSwapBoxMoveAttempt;
                    Entity targetEntity = WorldManager.Instance.CurrentWorld.GetImpassableEntityByGridPosition(actorTargetGP, GUID, out WorldModule targetModule, out GridPos3D _);
                    if (targetEntity == null || !targetModule.IsNotNullAndAvailable())
                    {
                        transform.position = boxIndicatorGP + actorSwapBoxMoveAttempt;
                        LastWorldGP = WorldGP;
                        WorldGP = GridPos3D.GetGridPosByTrans(transform, 1);
                        if (ENABLE_ACTOR_MOVE_LOG) Debug.Log($"[{Time.frameCount}] [Actor] {name} SwapFailed MoveSuc {LastWorldGP} -> {WorldGP}");
                    }
                    else
                    {
                        if (ENABLE_ACTOR_MOVE_LOG) Debug.Log($"[{Time.frameCount}] [Actor] {name} SwapFailed MoveFailed blocked by {targetEntity.name} {LastWorldGP} -> {WorldGP}");
                    }
                }

                // todo kicking box的swap如何兼容
            }
            else
            {
                if (Box.ENABLE_BOX_MOVE_LOG) Debug.Log($"[{Time.frameCount}] [Box] {box.name} SwapBoxFailed");
            }
        }
    }

    public void Lift()
    {
        if (CurrentLiftBox) return;
        if (CannotAct) return;
        Ray ray = new Ray(transform.position - transform.forward * 0.49f, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 1.49f, LayerManager.Instance.LayerMask_BoxIndicator, QueryTriggerInteraction.Collide))
        {
            Box box = hit.collider.gameObject.GetComponentInParent<Box>();
            if (box && box.Liftable && ActorBoxInteractHelper.CanInteract(InteractSkillType.Lift, box.EntityTypeIndex))
            {
                if (box.BeingLift(this))
                {
                    CurrentLiftBox = box;
                    ThrowState = ThrowStates.Raising;
                    ActorFaceHelper.FacingBoxList.Remove(box);
                    box.transform.parent = LiftBoxPivot.transform.parent;
                    box.transform.DOLocalMove(LiftBoxPivot.transform.localPosition, 0.2f).OnComplete(() =>
                    {
                        if (box.Consumable)
                        {
                            box.State = Box.States.Static;
                            ThrowState = ThrowStates.None;
                            box.LiftThenConsume();
                        }
                        else
                        {
                            box.State = Box.States.Lifted;
                            ThrowState = ThrowStates.Lifting;
                        }
                    });

                    if (box.Consumable)
                    {
                        ThrowState = ThrowStates.None;
                    }
                }
            }
        }
    }

    public void ThrowCharge()
    {
        if (!CurrentLiftBox) return;
        if (ThrowState == ThrowStates.Lifting)
        {
            ThrowState = ThrowStates.ThrowCharging;
            CurThrowPointOffset = transform.forward * ThrowRadiusMin;
        }
    }

    private void UpdateThrowParabolaLine()
    {
        bool isCharging = CurrentLiftBox && ThrowState == ThrowStates.ThrowCharging;
        ActorLaunchArcRendererHelper.SetShown(isCharging);
        if (isCharging)
        {
            ActorLaunchArcRendererHelper.InitializeByOffset(CurThrowPointOffset, 45, 2, 3f);
        }
    }

    public void ThrowOrPut()
    {
        if (CannotAct) return;
        if (CurrentLiftBox && ThrowState == ThrowStates.ThrowCharging)
        {
            if (ActorBoxInteractHelper.CanInteract(InteractSkillType.Throw, CurrentLiftBox.EntityTypeIndex))
            {
                Throw();
            }
            else
            {
                Put();
            }
        }
    }

    public void Throw()
    {
        if (CannotAct) return;
        if (CurrentLiftBox && CurrentLiftBox.Throwable && ThrowState == ThrowStates.ThrowCharging)
        {
            float velocity = ActorLaunchArcRendererHelper.CalculateVelocityByOffset(CurThrowPointOffset, 45);
            ThrowState = ThrowStates.None;
            Vector3 throwVel = (CurThrowPointOffset.normalized + Vector3.up) * velocity;
            CurrentLiftBox.Throw(throwVel, velocity, this);
            CurrentLiftBox = null;
        }
    }

    public void Put()
    {
        if (CannotAct) return;
        if (CurrentLiftBox && ThrowState == ThrowStates.Lifting || ThrowState == ThrowStates.ThrowCharging)
        {
            float velocity = ActorLaunchArcRendererHelper.CalculateVelocityByOffset(CurThrowPointOffset, 45);
            ThrowState = ThrowStates.None;
            Vector3 throwVel = (CurThrowPointOffset.normalized + Vector3.up) * velocity;
            CurrentLiftBox.Put(throwVel, velocity, this);
            CurrentLiftBox = null;
        }
    }

    private void ThrowWhenDie()
    {
        if (CurrentLiftBox)
        {
            ThrowState = ThrowStates.None;
            Vector3 throwVel = Vector3.up * 3;
            if (CurrentLiftBox.Throwable)
            {
                CurrentLiftBox.Throw(throwVel, 3, this);
            }
            else
            {
                CurrentLiftBox.Put(throwVel, 3, this);
            }

            CurrentLiftBox = null;
        }
    }

    #region Jump

    private void ClearJumpParams()
    {
        JumpHeight = 0;
        JumpStartWorldGP = GridPos3D.Zero;
        CurrentJumpForce = 0;
        KeepAddingJumpForce = false;
        JumpReachClimax = false;
        SmashDownTargetPos = Vector3.zero;
        SmashForce = 0;
        SmashReachTarget = false;
        InAirMoveTargetPos = Vector3.zero;
        InAirMoveSpeed = 0;
    }

    [FoldoutGroup("跳跃")]
    [LabelText("跳跃目标高度")]
    public int JumpHeight = 0;

    [FoldoutGroup("跳跃")]
    [LabelText("起跳坐标")]
    [DisplayAsString]
    public GridPos3D JumpStartWorldGP = GridPos3D.Zero;

    [FoldoutGroup("跳跃")]
    [LabelText("起跳力度")]
    [DisplayAsString]
    public float CurrentJumpForce = 0f;

    [FoldoutGroup("跳跃")]
    [LabelText("持续施加起跳力")]
    public bool KeepAddingJumpForce = false;

    [FoldoutGroup("跳跃")]
    [LabelText("跳跃是否抵达最高点")]
    public bool JumpReachClimax = false;

    [FoldoutGroup("跳跃")]
    [LabelText("下砸目标坐标")]
    [DisplayAsString]
    public Vector3 SmashDownTargetPos = Vector3.zero;

    [FoldoutGroup("跳跃")]
    [LabelText("下砸力度")]
    [DisplayAsString]
    public float SmashForce = 0f;

    [FoldoutGroup("跳跃")]
    [LabelText("下砸是否抵达目标")]
    public bool SmashReachTarget = false;

    [FoldoutGroup("跳跃")]
    [LabelText("悬空追击目标坐标")]
    [DisplayAsString]
    public Vector3 InAirMoveTargetPos = Vector3.zero;

    [FoldoutGroup("跳跃")]
    [LabelText("悬空追击移速")]
    [DisplayAsString]
    public float InAirMoveSpeed = 0f;

    public bool IsExecutingAirSkills()
    {
        return ActorBehaviourState == ActorBehaviourStates.Jump || ActorBehaviourState == ActorBehaviourStates.InAirMoving || ActorBehaviourState == ActorBehaviourStates.SmashDown;
    }

    public void SetJumpUpTargetHeight(float jumpForce, int jumpHeight, bool keepAddingForce)
    {
        if (ActorBehaviourState == ActorBehaviourStates.Jump || ActorBehaviourState == ActorBehaviourStates.InAirMoving || ActorBehaviourState == ActorBehaviourStates.SmashDown) return;
        if (CannotAct) return;
        if (HasRigidbody)
        {
            //ActorMoveColliderRoot.SetActive(false);
            JumpHeight = jumpHeight;
            JumpStartWorldGP = WorldGP;
            CurrentJumpForce = jumpForce;
            KeepAddingJumpForce = keepAddingForce;
            ActorBehaviourState = ActorBehaviourStates.Jump;
            JumpReachClimax = false;
            RigidBody.constraints &= ~RigidbodyConstraints.FreezePositionY;
            RigidBody.drag = 0;
            RigidBody.velocity = Vector3.zero;
            RigidBody.AddForce(Vector3.up * CurrentJumpForce, ForceMode.VelocityChange);
        }
    }

    public void JumpingUpTick()
    {
        if (ActorBehaviourState == ActorBehaviourStates.Jump)
        {
            if (CannotAct) return;
            if (HasRigidbody)
            {
                if (KeepAddingJumpForce && !JumpReachClimax)
                {
                    RigidBody.AddForce(Vector3.up * (CurrentJumpForce - RigidBody.velocity.y), ForceMode.VelocityChange);
                }

                if ((transform.position - JumpStartWorldGP).y >= JumpHeight)
                {
                    JumpReachClimax = true;
                }
                else
                {
                    if (HasRigidbody)
                    {
                        if (RigidBody.velocity.y < -1f)
                        {
                            JumpReachClimax = true;
                        }
                    }
                    else
                    {
                        JumpReachClimax = true;
                    }
                }

                if (JumpReachClimax && IsGrounded) ActorBehaviourState = ActorBehaviourStates.Idle;
            }
        }
    }

    public void SetSmashDownTargetPos(Vector3 targetPos, float smashForce)
    {
        if (ActorBehaviourState == ActorBehaviourStates.Jump || ActorBehaviourState == ActorBehaviourStates.InAirMoving)
        {
            if (CannotAct) return;
            if (HasRigidbody)
            {
                //ActorMoveColliderRoot.SetActive(true);
                SmashDownTargetPos = targetPos;
                SmashForce = smashForce;
                ActorBehaviourState = ActorBehaviourStates.SmashDown;
                JumpReachClimax = true;
                SmashReachTarget = false;
                RigidBody.constraints &= ~RigidbodyConstraints.FreezePositionY;
                RigidBody.drag = 0;
                RigidBody.velocity = Vector3.zero;
            }
        }
    }

    public void SmashingDownTick()
    {
        if (ActorBehaviourState != ActorBehaviourStates.SmashDown) return;
        if (CannotAct) return;
        if (HasRigidbody)
        {
            if (transform.position.y > SmashDownTargetPos.y + 1f)
            {
                RigidBody.AddForce(Vector3.up * (-SmashForce - RigidBody.velocity.y), ForceMode.VelocityChange);
            }

            if (IsGrounded)
            {
                ActorBehaviourState = ActorBehaviourStates.Idle;
            }
            else if (RigidBody.velocity.y >= -0.1f)
            {
                ActorBehaviourState = ActorBehaviourStates.Idle;
            }
        }
    }

    public void InAirSetMoveTargetPos(Vector3 targetPos, float moveSpeed)
    {
        if (ActorBehaviourState == ActorBehaviourStates.Jump || ActorBehaviourState == ActorBehaviourStates.InAirMoving)
        {
            if (CannotAct) return;
            if (HasRigidbody)
            {
                InAirMoveTargetPos = targetPos;
                InAirMoveSpeed = moveSpeed;
                ActorBehaviourState = ActorBehaviourStates.InAirMoving;
                RigidBody.velocity = Vector3.zero;
            }
        }
    }

    public void InAirMovingToTargetPosTick()
    {
        if (ActorBehaviourState == ActorBehaviourStates.InAirMoving)
        {
            if (HasRigidbody)
            {
                Vector3 diff = InAirMoveTargetPos - transform.position;
                RigidBody.velocity = Vector3.zero;
                RigidBody.AddForce(diff.normalized * InAirMoveSpeed, ForceMode.VelocityChange);
                if (RigidBody.velocity.magnitude > InAirMoveSpeed)
                {
                    RigidBody.velocity = RigidBody.velocity.normalized * InAirMoveSpeed;
                }
            }
        }
    }

    #endregion

    #endregion

    #region Die

    public override void DestroySelfByModuleRecycle()
    {
        if (IsDestroying) return;
        base.DestroySelfByModuleRecycle();
        IsDestroying = true;
        if (ActorFrozenHelper.FrozenBox)
        {
            ActorFrozenHelper.FrozenBox.DestroySelfByModuleRecycle();
            ActorFrozenHelper.FrozenBox = null;
        }

        if (this != BattleManager.Instance.Player1)
        {
            PoolRecycle();
            IsDestroying = false;
        }
        else
        {
            BattleManager.Instance.SetActorInCombat(GUID, CombatState.Exploring);
            IsDestroying = false;
        }
    }

    public override void DestroySelf(UnityAction callBack = null)
    {
        EntityStatPropSet.FrozenValue.SetValue(0);
        EntityCollectHelper?.OnDie(); // 以免自身掉落物又被自身捡走
        if (IsDestroying) return;
        base.DestroySelf();
        IsDestroying = true;
        if (!IsFrozen && !IsTriggerEntity) UnRegisterFromModule(WorldGP, EntityOrientation);
        foreach (EntityPassiveSkill ps in EntityPassiveSkills)
        {
            ps.OnBeforeDestroyEntity();
        }

        EntityWwiseHelper.OnDestroyed_Common.Post(gameObject);
        BattleManager.Instance.SetActorInCombat(GUID, CombatState.Exploring);
        ActiveSkillAgent.Instance.StartCoroutine(Co_DelayDestroyActor(callBack));
    }

    IEnumerator Co_DelayDestroyActor(UnityAction callBack)
    {
        yield return null;
        foreach (EntityPassiveSkill ps in EntityPassiveSkills)
        {
            ps.OnDestroyEntity();
        }

        if (IsPlayerCamp)
        {
            BattleManager.Instance.LoseGame();
        }
        else
        {
            if (ActorFrozenHelper.FrozenBox)
            {
                ActorFrozenHelper.FrozenBox.DestroySelf();
                ActorFrozenHelper.FrozenBox = null;
            }

            FXManager.Instance.PlayFX(DieFX, transform.position);
            callBack?.Invoke();
            PoolRecycle();
        }

        IsDestroying = false;
    }

    #endregion
}