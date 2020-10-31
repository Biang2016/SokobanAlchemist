using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using BiangStudio.GameDataFormat.Grid;
using BiangStudio.ObjectPool;
using DG.Tweening;
using NodeCanvas.Framework;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class Actor : PoolObject
{
    #region GUID

    [ReadOnly]
    [HideInEditorMode]
    public uint GUID;

    private static uint guidGenerator = (uint) ConfigManager.GUID_Separator.Actor;

    private uint GetGUID()
    {
        return guidGenerator++;
    }

    #endregion

    public static bool ActorMoveDebugLog =
#if UNITY_EDITOR
        false;
#else
        false;
#endif

    public Rigidbody RigidBody;
    public ActorCommonHelpers ActorCommonHelpers;

    public Vector3 ArtPos => ActorSkinHelper.MainArtTransform.position;

    internal GameObject ActorMoveColliderRoot => ActorCommonHelpers.ActorMoveColliderRoot;
    internal ActorBuffHelper ActorBuffHelper => ActorCommonHelpers.ActorBuffHelper;
    internal ActorPushHelper ActorPushHelper => ActorCommonHelpers.ActorPushHelper;
    internal ActorFaceHelper ActorFaceHelper => ActorCommonHelpers.ActorFaceHelper;
    internal ActorSkinHelper ActorSkinHelper => ActorCommonHelpers.ActorSkinHelper;
    internal ActorLaunchArcRendererHelper ActorLaunchArcRendererHelper => ActorCommonHelpers.ActorLaunchArcRendererHelper;
    internal ActorBattleHelper ActorBattleHelper => ActorCommonHelpers.ActorBattleHelper;
    internal ActorSkillHelper ActorSkillHelper => ActorCommonHelpers.ActorSkillHelper;
    internal Transform LiftBoxPivot => ActorCommonHelpers.LiftBoxPivot;

    internal GraphOwner GraphOwner;
    internal ActorAIAgent ActorAIAgent;

    [ReadOnly]
    [DisplayAsString]
    [LabelText("角色分类")]
    public ActorCategory ActorCategory;

    [ReadOnly]
    [DisplayAsString]
    [LabelText("角色类型")]
    public string ActorType;

    [ReadOnly]
    [DisplayAsString]
    [LabelText("移动倾向")]
    public Vector3 CurMoveAttempt;

    [ReadOnly]
    [DisplayAsString]
    [LabelText("上一帧移动倾向")]
    public Vector3 LastMoveAttempt;

    [ReadOnly]
    [DisplayAsString]
    [LabelText("扔箱子瞄准点移动倾向")]
    public Vector3 CurThrowMoveAttempt;

    [ReadOnly]
    [DisplayAsString]
    [LabelText("扔箱子瞄准点偏移")]
    public Vector3 CurThrowPointOffset;

    internal Vector3 CurForward
    {
        get { return transform.forward; }
        set { transform.forward = value; }
    }

    [DisplayAsString]
    [LabelText("世界坐标")]
    [BoxGroup("战斗状态")]
    public GridPos3D CurWorldGP;

    [DisplayAsString]
    [LabelText("上帧世界坐标")]
    [BoxGroup("战斗状态")]
    public GridPos3D LastWorldGP;

    [LabelText("阵营")]
    [BoxGroup("战斗状态")]
    [DisableInPlayMode]
    public Camp Camp;

    [ShowInInspector]
    [HideInEditorMode]
    [LabelText("受伤无敌时间")]
    [BoxGroup("战斗状态")]
    public float ImmuneTimeAfterDamaged = 0.4f;

    [BoxGroup("特效")]
    [LabelText("踢特效")]
    [ValueDropdown("GetAllFXTypeNames", DropdownTitle = "选择FX类型")]
    public string KickFX;

    [BoxGroup("特效")]
    [LabelText("踢特效尺寸")]
    public float KickFXScale = 1f;

    [BoxGroup("特效")]
    [LabelText("踢特效锚点")]
    public Transform KickFXPivot;

    [BoxGroup("特效")]
    [LabelText("受伤特效")]
    [ValueDropdown("GetAllFXTypeNames", DropdownTitle = "选择FX类型")]
    public string InjureFX;

    [BoxGroup("特效")]
    [LabelText("受伤特效尺寸")]
    public float InjureFXScale = 1f;

    [BoxGroup("特效")]
    [LabelText("生命恢复特效")]
    [ValueDropdown("GetAllFXTypeNames", DropdownTitle = "选择FX类型")]
    public string HealFX;

    [BoxGroup("特效")]
    [LabelText("生命恢复特效尺寸")]
    [FormerlySerializedAs("GainLifeFXScale")]
    public float HealFXScale = 1f;

    [BoxGroup("特效")]
    [LabelText("命数增加特效")]
    [ValueDropdown("GetAllFXTypeNames", DropdownTitle = "选择FX类型")]
    public string GainLifeFX;

    [BoxGroup("特效")]
    [LabelText("命数增加特效尺寸")]
    [FormerlySerializedAs("GainLifeFXScale")]
    public float GainLifeFXScale = 1f;

    [BoxGroup("特效")]
    [LabelText("死亡特效")]
    [ValueDropdown("GetAllFXTypeNames", DropdownTitle = "选择FX类型")]
    public string DieFX;

    [BoxGroup("特效")]
    [LabelText("死亡特效尺寸")]
    public float DieFXScale = 1f;

    [BoxGroup("初始战斗数值")]
    [HideLabel]
    [DisableInPlayMode]
    public ActorStatPropSet RawActorStatPropSet = new ActorStatPropSet(); // 干数据，禁修改

    [BoxGroup("当前战斗数值")]
    [HideLabel]
    [HideInEditorMode]
    public ActorStatPropSet ActorStatPropSet; // 湿数据，每次Recycle置空，使用时从干数据拷贝

    [BoxGroup("手感")]
    [LabelText("起步速度")]
    public float Accelerate = 10f;

    [BoxGroup("手感")]
    [LabelText("瞄准点移动速度")]
    public float ThrowAimMoveSpeed = 10f;

    protected float ThrowRadiusMin = 0.75f;

    [BoxGroup("手感")]
    [LabelText("踢箱子力量")]
    public float KickForce = 5;

    [BoxGroup("配置")]
    [LabelText("扔半径")]
    public float ThrowRadius = 10f;

    [BoxNameList]
    [BoxGroup("能力")]
    [LabelText("推箱子类型")]
    [ValueDropdown("GetAllBoxTypeNames", IsUniqueList = true, DropdownTitle = "选择箱子类型", DrawDropdownForListElements = false, ExcludeExistingValuesInList = true)]
    public List<string> PushableBoxList = new List<string>();

    [BoxNameList]
    [BoxGroup("能力")]
    [LabelText("踢箱子类型")]
    [ValueDropdown("GetAllBoxTypeNames", IsUniqueList = true, DropdownTitle = "选择箱子类型", DrawDropdownForListElements = false, ExcludeExistingValuesInList = true)]
    public List<string> KickableBoxList = new List<string>();

    [BoxNameList]
    [BoxGroup("能力")]
    [LabelText("举箱子类型")]
    [ValueDropdown("GetAllBoxTypeNames", IsUniqueList = true, DropdownTitle = "选择箱子类型", DrawDropdownForListElements = false, ExcludeExistingValuesInList = true)]
    public List<string> LiftableBoxList = new List<string>();

    [BoxNameList]
    [BoxGroup("能力")]
    [LabelText("扔箱子类型")]
    [ValueDropdown("GetAllBoxTypeNames", IsUniqueList = true, DropdownTitle = "选择箱子类型", DrawDropdownForListElements = false, ExcludeExistingValuesInList = true)]
    public List<string> ThrowableBoxList = new List<string>();

    [BoxName]
    [BoxGroup("死亡")]
    [LabelText("死亡掉落箱子")]
    [ValueDropdown("GetAllBoxTypeNames", IsUniqueList = true, DropdownTitle = "选择箱子类型", DrawDropdownForListElements = false, ExcludeExistingValuesInList = true)]
    public string DieDropBoxTypeName = "None";

    [BoxGroup("死亡")]
    [LabelText("死亡掉落箱子概率%")]
    public uint DieDropBoxProbabilityPercent;

    [BoxGroup("敌兵专用")]
    [LabelText("碰撞伤害")]
    public int CollideDamage;

    [BoxGroup("敌兵专用")]
    [LabelText("碰撞击飞力")]
    public int CollideForce;

    private List<SmoothMove> SmoothMoves = new List<SmoothMove>();

    [DisableInEditorMode]
    [ShowInInspector]
    internal Box CurrentLiftBox = null;

    public enum MovementStates
    {
        Static,
        Moving,
    }

    [ReadOnly]
    [BoxGroup("战斗状态")]
    [LabelText("移动状态")]
    public MovementStates MovementState = MovementStates.Static;

    public enum PushStates
    {
        None,
        Pushing,
    }

    [ReadOnly]
    [BoxGroup("战斗状态")]
    [LabelText("推箱子状态")]
    public PushStates PushState = PushStates.None;

    public enum ThrowStates
    {
        None,
        Raising,
        Lifting,
        ThrowCharging,
    }

    [ReadOnly]
    [BoxGroup("战斗状态")]
    [LabelText("扔技能状态")]
    public ThrowStates ThrowState = ThrowStates.None;

    public override void OnRecycled()
    {
        GraphOwner?.StopBehaviour();
        ActorAIAgent.Stop();
        CurMoveAttempt = Vector3.zero;
        LastMoveAttempt = Vector3.zero;
        CurThrowMoveAttempt = Vector3.zero;
        CurThrowPointOffset = Vector3.zero;
        CurForward = Vector3.forward;
        CurWorldGP = GridPos3D.Zero;
        LastWorldGP = GridPos3D.Zero;
        MovementState = MovementStates.Static;
        PushState = PushStates.None;
        ThrowState = ThrowStates.None;
        ThrowWhenDie();
        ActorBuffHelper.OnRecycled();
        ActorPushHelper.OnRecycled();
        ActorFaceHelper.OnRecycled();
        ActorSkinHelper.OnRecycled();
        ActorLaunchArcRendererHelper.OnRecycled();
        ActorBattleHelper.OnRecycled();
        ActorSkillHelper.OnRecycled();
        ActorStatPropSet.OnRecycled();
        ActorStatPropSet = null;
        RigidBody.drag = 100f;
        RigidBody.velocity = Vector3.zero;
        RigidBody.angularVelocity = Vector3.zero;
        ActorMoveColliderRoot.SetActive(false);
        SetSmoothMovesEnable(false);
        base.OnRecycled();
    }

    public override void OnUsed()
    {
        base.OnUsed();
        ActorBuffHelper.OnUsed();
        ActorPushHelper.OnUsed();
        ActorPushHelper.OnUsed();
        ActorFaceHelper.OnUsed();
        ActorSkinHelper.OnUsed();
        ActorLaunchArcRendererHelper.OnUsed();
        ActorBattleHelper.OnUsed();
        ActorSkillHelper.OnUsed();
        ActorMoveColliderRoot.SetActive(true);
    }

    void Awake()
    {
        ActorAIAgent = new ActorAIAgent(this);
        GraphOwner = GetComponent<GraphOwner>();
        SmoothMoves = GetComponentsInChildren<SmoothMove>().ToList();
        SetSmoothMovesEnable(false);
    }

    private void SetSmoothMovesEnable(bool enable)
    {
        foreach (SmoothMove sm in SmoothMoves)
        {
            sm.enabled = enable;
        }
    }

    public void Initialize(string actorType, ActorCategory actorCategory)
    {
        ActorType = actorType;
        ActorCategory = actorCategory;
        ClientGameManager.Instance.BattleMessenger.AddListener<Actor>((uint) Enum_Events.OnPlayerLoaded, OnLoaded);
        ActorStatPropSet = RawActorStatPropSet.Clone();
        ActorStatPropSet.Initialize(this);
        ActorStatPropSet.Life.OnValueReachZero += ActorBattleHelper.Die;
        ActorBattleHelper.Initialize();
        ActorSkillHelper.Initialize();

        CurWorldGP = GridPos3D.GetGridPosByTrans(transform, 1);
        LastWorldGP = CurWorldGP;
        ActorAIAgent.Start();
        GUID = GetGUID();

        ActorBattleHelper.OnDamaged += (Actor, damage) =>
        {
            float distance = (BattleManager.Instance.Player1.transform.position - transform.position).magnitude;
            CameraManager.Instance.FieldCamera.CameraShake(damage, distance);
        };
    }

    private void Update()
    {
        if (!IsRecycled)
        {
            UpdateThrowParabolaLine();
        }
    }

    public void OnLoaded(Actor actor)
    {
        if (actor == this)
        {
            SetSmoothMovesEnable(true);
        }
    }

    public void SetShown(bool shown)
    {
    }

    protected virtual void FixedUpdate()
    {
        if (!IsRecycled)
        {
            RigidBody.angularVelocity = Vector3.zero;
            if (ActorMoveDebugLog && CurWorldGP != LastWorldGP) Debug.Log($"Move {LastWorldGP} -> {CurWorldGP}");
            LastWorldGP = CurWorldGP;
            CurWorldGP = GridPos3D.GetGridPosByTrans(transform, 1);
        }
    }

    protected virtual void MoveInternal()
    {
        if (CurMoveAttempt.magnitude > 0)
        {
            if (CurMoveAttempt.x.Equals(0)) RigidBody.velocity = new Vector3(0, RigidBody.velocity.y, RigidBody.velocity.z);
            if (CurMoveAttempt.z.Equals(0)) RigidBody.velocity = new Vector3(RigidBody.velocity.x, RigidBody.velocity.y, 0);
            MovementState = MovementStates.Moving;
            RigidBody.drag = 0;

            Vector3 velDiff = CurMoveAttempt.normalized * Time.fixedDeltaTime * Accelerate;
            Vector3 finalVel = RigidBody.velocity + velDiff;
            float finalSpeed = ActorStatPropSet.MoveSpeed.GetModifiedValue / 10f;
            if (finalVel.magnitude > finalSpeed)
            {
                finalVel = finalVel.normalized * finalSpeed;
            }

            RigidBody.AddForce(finalVel - RigidBody.velocity, ForceMode.VelocityChange);

            CurForward = CurMoveAttempt.normalized;
            ActorPushHelper.PushTriggerOut();
        }
        else
        {
            MovementState = MovementStates.Static;
            RigidBody.drag = 100f;
            ActorPushHelper.PushTriggerReset();
        }

        if (CurMoveAttempt.x.Equals(0))
        {
            transform.position = new Vector3(CurWorldGP.x, transform.position.y, transform.position.z);
        }

        if (CurMoveAttempt.z.Equals(0))
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, CurWorldGP.z);
        }

        CurWorldGP = transform.position.ToGridPos3D();
        Box box = WorldManager.Instance.CurrentWorld.GetBoxByGridPosition(CurWorldGP + new GridPos3D(0, -1, 0), out WorldModule module, out GridPos3D localGP, false);
        if (!box)
        {
            transform.position += Vector3.down;
        }

        LastMoveAttempt = CurMoveAttempt;
    }

    private float ThrowChargeTick;
    internal float ThrowChargeMax = 1.5f;

    protected virtual void ThrowChargeAimInternal()
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

    public void Kick()
    {
        Ray ray = new Ray(ArtPos - transform.forward * 0.49f, transform.forward);
        //Debug.DrawRay(ray.origin, ray.direction, Color.red, 0.3f);
        if (Physics.Raycast(ray, out RaycastHit hit, 1.49f, LayerManager.Instance.LayerMask_BoxIndicator, QueryTriggerInteraction.Collide))
        {
            Box box = hit.collider.gameObject.GetComponentInParent<Box>();
            if (box && box.Kickable && ActorSkillHelper.CanInteract(InteractSkillType.Kick, box.BoxTypeIndex))
            {
                box.Kick(CurForward, KickForce, this);
                FX kickFX = FXManager.Instance.PlayFX(KickFX, KickFXPivot.position);
                if (kickFX) kickFX.transform.localScale = Vector3.one * KickFXScale;
            }
        }
    }

    public void Swap()
    {
        Ray ray = new Ray(ArtPos - transform.forward * 0.49f, transform.forward);
        //Debug.DrawRay(ray.origin, ray.direction, Color.red, 0.3f);
        if (Physics.Raycast(ray, out RaycastHit hit, 1.49f, LayerManager.Instance.LayerMask_BoxIndicator, QueryTriggerInteraction.Collide))
        {
            Box box = hit.collider.gameObject.GetComponentInParent<Box>();
            if (box && box.Pushable && ActorSkillHelper.CanInteract(InteractSkillType.Push, box.BoxTypeIndex))
            {
                bool boxAlreadyPushAGrid = box.State == Box.States.BeingPushed && box.transform.position.ToGridPos3D() == box.WorldGP; // 是否已经过了一格界线
                if (box.State == Box.States.Static)
                {
                    if (ActorMoveDebugLog) Debug.Log($"[Actor] SwapActor1 {CurWorldGP} -> {box.WorldGP}");
                    transform.position = box.WorldGP.ToVector3();
                }
                else if (box.State == Box.States.BeingPushed)
                {
                    if (boxAlreadyPushAGrid)
                    {
                        if (ActorMoveDebugLog) Debug.Log($"[Actor] SwapActor2 {CurWorldGP} -> {box.WorldGP}");
                        transform.position = box.WorldGP.ToVector3();
                    }
                    else
                    {
                        if (ActorMoveDebugLog) Debug.Log($"[Actor] SwapActor3 {CurWorldGP} -> {box.LastWorldGP}");
                        transform.position = box.LastWorldGP.ToVector3();
                    }
                }

                box.ForceCancelPush();
                if (!boxAlreadyPushAGrid)
                {
                    if (ActorMoveDebugLog) Debug.Log($"[Box] SwapBox {box.WorldGP} -> {CurWorldGP}");
                    WorldManager.Instance.CurrentWorld.MoveBox(box.WorldGP, CurWorldGP, Box.States.BeingPushed, false, true);
                }

                // todo kicking box的swap如何兼容
            }
        }
    }

    public void Lift()
    {
        if (CurrentLiftBox) return;
        Ray ray = new Ray(ArtPos - transform.forward * 0.49f, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 1.49f, LayerManager.Instance.LayerMask_BoxIndicator, QueryTriggerInteraction.Collide))
        {
            Box box = hit.collider.gameObject.GetComponentInParent<Box>();
            if (box && box.Liftable && ActorSkillHelper.CanInteract(InteractSkillType.Lift, box.BoxTypeIndex))
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
        if (CurrentLiftBox && ThrowState == ThrowStates.ThrowCharging)
        {
            if (ActorSkillHelper.CanInteract(InteractSkillType.Throw, CurrentLiftBox.BoxTypeIndex))
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

#endregion

#region Camp

    public bool IsPlayer => Camp == Camp.Player;
    public bool IsPlayerOrFriend => Camp == Camp.Player || Camp == Camp.Friend;
    public bool IsFriend => Camp == Camp.Friend;
    public bool IsEnemy => Camp == Camp.Enemy;
    public bool IsNeutral => Camp == Camp.None;

    public bool IsOpponentCampOf(Actor target)
    {
        if ((IsPlayerOrFriend) && target.IsEnemy) return true;
        if ((target.IsPlayerOrFriend) && IsEnemy) return true;
        return false;
    }

    public bool IsSameCampOf(Actor target)
    {
        return !IsOpponentCampOf(target);
    }

    public bool IsNeutralCampOf(Actor target)
    {
        if ((IsPlayerOrFriend) && target.IsNeutral) return true;
        if ((target.IsPlayerOrFriend) && IsNeutral) return true;
        return false;
    }

#endregion

#region Utils

    private IEnumerable<string> GetAllBoxTypeNames => ConfigManager.GetAllBoxTypeNames();
    private IEnumerable<string> GetAllFXTypeNames => ConfigManager.GetAllFXTypeNames();

#endregion

#if UNITY_EDITOR
    public bool RenameBoxTypeName(string srcBoxName, string targetBoxName, StringBuilder info)
    {
        bool isDirty = false;
        foreach (FieldInfo fi in GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
        {
            foreach (Attribute a in fi.GetCustomAttributes(false))
            {
                if (a is BoxNameAttribute)
                {
                    if (fi.FieldType == typeof(string))
                    {
                        string fieldValue = (string) fi.GetValue(this);
                        if (fieldValue == srcBoxName)
                        {
                            isDirty = true;
                            info.Append($"替换{name}.{fi.Name} -> '{targetBoxName}'\n");
                            fi.SetValue(this, targetBoxName);
                        }
                    }
                }
                else if (a is BoxNameListAttribute)
                {
                    if (fi.FieldType == typeof(List<string>))
                    {
                        List<string> fieldValueList = (List<string>) fi.GetValue(this);
                        for (int i = 0; i < fieldValueList.Count; i++)
                        {
                            string fieldValue = fieldValueList[i];
                            if (fieldValue == srcBoxName)
                            {
                                isDirty = true;
                                info.Append($"替换于{name}.{fi.Name}\n");
                                fieldValueList[i] = targetBoxName;
                            }
                        }
                    }
                }
            }
        }

        if (this is EnemyActor enemyActor)
        {
            NodeCanvas.Framework.GraphOwner tempGraphOwner = GetComponent<GraphOwner>();
            if (tempGraphOwner)
            {
                Variable<List<string>> liftBoxTypeNames = tempGraphOwner.blackboard.GetVariable<List<string>>("LiftBoxTypeNames");
                if (liftBoxTypeNames != null)
                {
                    for (int index = 0; index < liftBoxTypeNames.value.Count; index++)
                    {
                        string boxTypeName = liftBoxTypeNames.value[index];
                        if (boxTypeName == srcBoxName)
                        {
                            isDirty = true;
                            info.Append($"替换于{name}.FSM.BB.LiftBoxTypeNames\n");
                            liftBoxTypeNames.value[index] = targetBoxName;
                        }
                    }
                }
            }
        }

        return isDirty;
    }

    public bool DeleteBoxTypeName(string srcBoxName, StringBuilder info)
    {
        bool isDirty = false;
        foreach (FieldInfo fi in GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
        {
            foreach (Attribute a in fi.GetCustomAttributes(false))
            {
                if (a is BoxNameAttribute)
                {
                    if (fi.FieldType == typeof(string))
                    {
                        string fieldValue = (string) fi.GetValue(this);
                        if (fieldValue == srcBoxName)
                        {
                            isDirty = true;
                            info.Append($"替换{name}.{fi.Name} -> 'None'\n");
                            fi.SetValue(this, "None");
                        }
                    }
                }
                else if (a is BoxNameListAttribute)
                {
                    if (fi.FieldType == typeof(List<string>))
                    {
                        List<string> fieldValueList = (List<string>) fi.GetValue(this);
                        for (int i = 0; i < fieldValueList.Count; i++)
                        {
                            string fieldValue = fieldValueList[i];
                            if (fieldValue == srcBoxName)
                            {
                                isDirty = true;
                                info.Append($"移除自{name}.{fi.Name}\n");
                                fieldValueList.RemoveAt(i);
                                i--;
                            }
                        }
                    }
                }
            }
        }

        if (this is EnemyActor enemyActor)
        {
            GraphOwner tempGraphOwner = GetComponent<GraphOwner>();
            if (tempGraphOwner)
            {
                Variable<List<string>> liftBoxTypeNames = tempGraphOwner.blackboard.GetVariable<List<string>>("LiftBoxTypeNames");
                if (liftBoxTypeNames != null)
                {
                    for (int index = 0; index < liftBoxTypeNames.value.Count; index++)
                    {
                        string boxTypeName = liftBoxTypeNames.value[index];
                        if (boxTypeName == srcBoxName)
                        {
                            isDirty = true;
                            info.Append($"移除自{name}.FSM.BB.LiftBoxTypeNames\n");
                            liftBoxTypeNames.value.RemoveAt(index);
                            index--;
                        }
                    }
                }
            }
        }

        return isDirty;
    }

#endif
}