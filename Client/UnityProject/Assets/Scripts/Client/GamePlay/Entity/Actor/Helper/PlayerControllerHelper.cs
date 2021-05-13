using System;
using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using BiangLibrary.GameDataFormat.Grid;
using BiangLibrary.GamePlay.UI;
using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerControllerHelper : ActorControllerHelper
{
    [LabelText("玩家编号")]
    [FoldoutGroup("状态")]
    public PlayerNumber PlayerNumber;

    private ButtonState BS_Up;
    private ButtonState BS_Right;
    private ButtonState BS_Down;
    private ButtonState BS_Left;

    public enum KeyBind
    {
        Space_South = 0,
        Shift_East = 1,
        H_LeftTrigger = 2,
        J_RightTrigger = 3,
        K = 4,
        L = 5,
        Num1 = 6,
        Num2 = 7,
        Num3 = 8,
        Num4 = 9,

        MAX
    }

    public static Dictionary<KeyBind, string> KeyMappingStrDict = new Dictionary<KeyBind, string>
    {
        {KeyBind.Space_South, "Space"},
        {KeyBind.Shift_East, "LShift"},
        {KeyBind.H_LeftTrigger, "H"},
        {KeyBind.J_RightTrigger, "J"},
        {KeyBind.K, "K"},
        {KeyBind.L, "L"},
        {KeyBind.Num1, "Num1"},
        {KeyBind.Num2, "Num2"},
        {KeyBind.Num3, "Num3"},
        {KeyBind.Num4, "Num4"},
    };

    private ButtonState[] BS_SkillArray = new ButtonState[(int) KeyBind.MAX];

    [FoldoutGroup("SkillKeyMapping")]
    [HideInEditorMode]
    [ShowInInspector]
    internal SortedDictionary<KeyBind, List<EntitySkillIndex>> SkillKeyMappings = new SortedDictionary<KeyBind, List<EntitySkillIndex>>();

    [HideInPlayMode]
    [FoldoutGroup("SkillKeyMapping")]
    [LabelText("Space/South")]
    public List<EntitySkillIndex> SkillKeyMapping_0 = new List<EntitySkillIndex>(); // 干数据，不编辑

    [HideInPlayMode]
    [FoldoutGroup("SkillKeyMapping")]
    [LabelText("Shift/East")]
    public List<EntitySkillIndex> SkillKeyMapping_1 = new List<EntitySkillIndex>(); // 干数据，不编辑

    [HideInPlayMode]
    [FoldoutGroup("SkillKeyMapping")]
    [LabelText("H/LeftTrigger")]
    public List<EntitySkillIndex> SkillKeyMapping_2 = new List<EntitySkillIndex>(); // 干数据，不编辑

    [HideInPlayMode]
    [FoldoutGroup("SkillKeyMapping")]
    [LabelText("J/RightTrigger")]
    public List<EntitySkillIndex> SkillKeyMapping_3 = new List<EntitySkillIndex>(); // 干数据，不编辑

    [HideInPlayMode]
    [FoldoutGroup("SkillKeyMapping")]
    [LabelText("K/")]
    public List<EntitySkillIndex> SkillKeyMapping_4 = new List<EntitySkillIndex>(); // 干数据，不编辑

    [HideInPlayMode]
    [FoldoutGroup("SkillKeyMapping")]
    [LabelText("L/")]
    public List<EntitySkillIndex> SkillKeyMapping_5 = new List<EntitySkillIndex>(); // 干数据，不编辑

    [HideInPlayMode]
    [FoldoutGroup("SkillKeyMapping")]
    [LabelText("Num1")]
    public List<EntitySkillIndex> SkillKeyMapping_6 = new List<EntitySkillIndex>(); // 干数据，不编辑

    [HideInPlayMode]
    [FoldoutGroup("SkillKeyMapping")]
    [LabelText("Num2")]
    public List<EntitySkillIndex> SkillKeyMapping_7 = new List<EntitySkillIndex>(); // 干数据，不编辑

    [HideInPlayMode]
    [FoldoutGroup("SkillKeyMapping")]
    [LabelText("Num3")]
    public List<EntitySkillIndex> SkillKeyMapping_8 = new List<EntitySkillIndex>(); // 干数据，不编辑

    [HideInPlayMode]
    [FoldoutGroup("SkillKeyMapping")]
    [LabelText("Num4")]
    public List<EntitySkillIndex> SkillKeyMapping_9 = new List<EntitySkillIndex>(); // 干数据，不编辑

    // 短按逻辑：短按最优先，短按过程中不接受其他短按，短按那个按键down时记录该轴位置，当位置变化时结束短按，短按结束后短按数据清空
    private bool isQuickMoving = false;
    private float QuickMovePressThreshold = 0.2f;
    private GridPos3D lastMoveUpButtonDownWorldGP = GridPos3D.Zero; // 记录上一次方向键按下时的角色坐标
    private GridPos3D lastMoveRightButtonDownWorldGP = GridPos3D.Zero; // 记录上一次方向键按下时的角色坐标
    private GridPos3D lastMoveDownButtonDownWorldGP = GridPos3D.Zero; // 记录上一次方向键按下时的角色坐标
    private GridPos3D lastMoveLeftButtonDownWorldGP = GridPos3D.Zero; // 记录上一次方向键按下时的角色坐标
    private GridPos3D quickMoveStartWorldGP = GridPos3D.Zero; // 记录短按开始时的角色坐标（按上一次down键的世界坐标取值）
    private Vector3 quickMoveAttempt = Vector3.zero; // 记录当前短按的移动方向，zero为无短按

    public void OnSetup(PlayerNumber playerNumber)
    {
        PlayerNumber = playerNumber;
        Actor.ActorSkinHelper.Initialize(playerNumber);
        BS_Up = ControlManager.Instance.Battle_MoveButtons[(int) PlayerNumber, (int) GridPosR.Orientation.Up];
        BS_Right = ControlManager.Instance.Battle_MoveButtons[(int) PlayerNumber, (int) GridPosR.Orientation.Right];
        BS_Down = ControlManager.Instance.Battle_MoveButtons[(int) PlayerNumber, (int) GridPosR.Orientation.Down];
        BS_Left = ControlManager.Instance.Battle_MoveButtons[(int) PlayerNumber, (int) GridPosR.Orientation.Left];

        SkillKeyMappings.Clear();
        SkillKeyMappings.Add(KeyBind.Space_South, SkillKeyMapping_0.Clone<EntitySkillIndex, EntitySkillIndex>());
        SkillKeyMappings.Add(KeyBind.Shift_East, SkillKeyMapping_1.Clone<EntitySkillIndex, EntitySkillIndex>());
        SkillKeyMappings.Add(KeyBind.H_LeftTrigger, SkillKeyMapping_2.Clone<EntitySkillIndex, EntitySkillIndex>());
        SkillKeyMappings.Add(KeyBind.J_RightTrigger, SkillKeyMapping_3.Clone<EntitySkillIndex, EntitySkillIndex>());
        SkillKeyMappings.Add(KeyBind.K, SkillKeyMapping_4.Clone<EntitySkillIndex, EntitySkillIndex>());
        SkillKeyMappings.Add(KeyBind.L, SkillKeyMapping_5.Clone<EntitySkillIndex, EntitySkillIndex>());
        SkillKeyMappings.Add(KeyBind.Num1, SkillKeyMapping_6.Clone<EntitySkillIndex, EntitySkillIndex>());
        SkillKeyMappings.Add(KeyBind.Num2, SkillKeyMapping_7.Clone<EntitySkillIndex, EntitySkillIndex>());
        SkillKeyMappings.Add(KeyBind.Num3, SkillKeyMapping_8.Clone<EntitySkillIndex, EntitySkillIndex>());
        SkillKeyMappings.Add(KeyBind.Num4, SkillKeyMapping_9.Clone<EntitySkillIndex, EntitySkillIndex>());
        for (int i = 0; i < (int) KeyBind.MAX; i++)
        {
            BS_SkillArray[i] = ControlManager.Instance.Battle_Skill[(int) PlayerNumber, i];
        }
    }

    private float Skill_1_PressDuration;

    private float QuickMoveDuration; // 快速移动开始后经过的时间
    private Vector3 QuickMoveStartActorPosition = Vector3.zero;

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
        if (Actor.IsNotNullAndAlive() && !UIManager.Instance.IsUIShown<ExitMenuPanel>() && !UIManager.Instance.IsUIShown<ConfirmPanel>())
        {
            #region Move

            Actor.CurMoveAttempt = Vector3.zero;

            if (BS_Up.Down) lastMoveUpButtonDownWorldGP = Actor.WorldGP;
            if (BS_Right.Down) lastMoveRightButtonDownWorldGP = Actor.WorldGP;
            if (BS_Down.Down) lastMoveDownButtonDownWorldGP = Actor.WorldGP;
            if (BS_Left.Down) lastMoveLeftButtonDownWorldGP = Actor.WorldGP;

            if (BS_Up.Pressed) Actor.CurMoveAttempt.z += 1;
            if (BS_Right.Pressed) Actor.CurMoveAttempt.x += 1;
            if (BS_Down.Pressed) Actor.CurMoveAttempt.z -= 1;
            if (BS_Left.Pressed) Actor.CurMoveAttempt.x -= 1;

            // 判定是否短按，如果短按则为玩家坚持按该键一段时间，直到角色位置变化
            if (!isQuickMoving)
            {
                quickMoveAttempt = Vector3.zero;
                int quickMoveAttemptXThisFrame = 0; // X轴短按值
                if (BS_Right.Up && BS_Right.PressedDuration < QuickMovePressThreshold && !BS_Left.Pressed) quickMoveAttemptXThisFrame += 1;
                if (BS_Left.Up && BS_Left.PressedDuration < QuickMovePressThreshold && !BS_Right.Pressed) quickMoveAttemptXThisFrame -= 1;
                if (!quickMoveAttemptXThisFrame.Equals(0)) // 此帧X轴有短按
                {
                    quickMoveStartWorldGP = quickMoveAttemptXThisFrame < 0 ? lastMoveLeftButtonDownWorldGP : lastMoveRightButtonDownWorldGP;
                    quickMoveAttempt.x = quickMoveAttemptXThisFrame;
                }
                else
                {
                    int quickMoveAttemptZThisFrame = 0; // Z轴短按值
                    if (BS_Up.Up && BS_Up.PressedDuration < QuickMovePressThreshold && !BS_Down.Pressed) quickMoveAttemptZThisFrame += 1;
                    if (BS_Down.Up && BS_Down.PressedDuration < QuickMovePressThreshold && !BS_Up.Pressed) quickMoveAttemptZThisFrame -= 1;
                    if (!quickMoveAttemptZThisFrame.Equals(0)) // 此帧Z轴有短按
                    {
                        quickMoveStartWorldGP = quickMoveAttemptZThisFrame < 0 ? lastMoveDownButtonDownWorldGP : lastMoveUpButtonDownWorldGP;
                        quickMoveAttempt.z = quickMoveAttemptZThisFrame;
                    }
                }

                if (quickMoveAttempt != Vector3.zero)
                {
                    Vector3 rotatedQuickMoveAttempt = RotateMoveDirectionByCameraRotation(quickMoveAttempt);
                    GridPos3D rotatedQuickMoveAttemptGP = rotatedQuickMoveAttempt.ToGridPos3D();
                    GridPos3D targetPos = Actor.WorldGP + rotatedQuickMoveAttemptGP;

                    // Check is there any box occupies the grid
                    Entity entity = WorldManager.Instance.CurrentWorld.GetImpassableEntityByGridPosition(targetPos, Actor.GUID, out WorldModule module, out GridPos3D _);
                    if ((entity == null && module.IsNotNullAndAvailable())
                        || (entity is Box box && box.Pushable && Actor.ActorPushHelper.Actor.ActorBoxInteractHelper.CanInteract(InteractSkillType.Push, box.EntityTypeIndex) &&
                            WorldManager.Instance.CurrentWorld.CheckCanMoveBoxColumn(box.WorldGP, rotatedQuickMoveAttemptGP, new HashSet<Box>()))) // 能走到才开启短按
                    {
                        Actor.CurMoveAttempt = quickMoveAttempt;
                        //Debug.Log("速按" + quickMoveAttempt);
                        isQuickMoving = true;
                    }
                }
            }
            else
            {
                // 再长按改方向键取消
                if (quickMoveAttempt.x.Equals(-1) && BS_Left.Pressed && BS_Left.PressedDuration >= QuickMovePressThreshold) isQuickMoving = false;
                if (quickMoveAttempt.x.Equals(1) && BS_Right.Pressed && BS_Right.PressedDuration >= QuickMovePressThreshold) isQuickMoving = false;
                if (quickMoveAttempt.z.Equals(-1) && BS_Down.Pressed && BS_Down.PressedDuration >= QuickMovePressThreshold) isQuickMoving = false;
                if (quickMoveAttempt.z.Equals(1) && BS_Up.Pressed && BS_Up.PressedDuration >= QuickMovePressThreshold) isQuickMoving = false;
                if (isQuickMoving)
                {
                    QuickMoveDuration += Time.fixedDeltaTime;
                    float quickMoveSpeed = (transform.position - QuickMoveStartActorPosition).magnitude / QuickMoveDuration;

                    if (quickMoveStartWorldGP == Actor.WorldGP // 自短按开始后角色该轴位置还未发生变化，则继续施加移动效果
                        && !(QuickMoveDuration > 0.3f && quickMoveSpeed < 0.1f)) // 且短按期间的移动速度不能过低，否则判定为卡住
                    {
                        Actor.CurMoveAttempt = quickMoveAttempt;
                        isQuickMoving = true;
                    }
                    else // 停止短按移动
                    {
                        //Debug.Log("速按结束" + quickMoveAttempt);
                        Actor.CurMoveAttempt = Vector3.zero;
                        quickMoveAttempt = Vector3.zero;
                        isQuickMoving = false;
                    }
                }
                else
                {
                    quickMoveAttempt = Vector3.zero;
                }
            }

            // 双轴操作互相打断逻辑
            // 如果当前方向操作持续时间未超过短按阈值，则另一向操作插入需要按住超过短按阈值才会使方向变化，否则两个短按交替打断行为很难看
            // 反之，如果当前方向操作持续时间已超过短按阈值，则另一向不论持续时间如何可立即插入 -> 可实现移动中快速变道平移的顺畅感觉
            if (!Actor.CurMoveAttempt.x.Equals(0) && !Actor.CurMoveAttempt.z.Equals(0))
            {
                // 目前是按住上键，并且有向上移动分量，且向上移动在左右移动之前按下
                if (Actor.CurMoveAttempt.z > 0 && BS_Up.Before(BS_Left) && BS_Up.Before(BS_Right))
                {
                    if (BS_Up.PressedDuration > QuickMovePressThreshold)
                    {
                        //Debug.Log("左右打断上");
                        Actor.CurMoveAttempt.z = 0;
                    }
                    else
                    {
                        if ((BS_Left.Pressed && BS_Left.PressedDuration > QuickMovePressThreshold)
                            || (BS_Right.Pressed && BS_Right.PressedDuration > QuickMovePressThreshold))
                        {
                            Actor.CurMoveAttempt.z = 0;
                        }
                        else
                        {
                            //Debug.Log("左右未能打断上");
                            Actor.CurMoveAttempt.x = 0;
                        }
                    }
                }

                // 目前是按住下键，并且有向下移动分量，且向下移动在左右移动之前按下
                if (Actor.CurMoveAttempt.z < 0 && BS_Down.Before(BS_Left) && BS_Down.Before(BS_Right))
                {
                    if (BS_Down.PressedDuration > QuickMovePressThreshold)
                    {
                        //Debug.Log("左右打断下");
                        Actor.CurMoveAttempt.z = 0;
                    }
                    else
                    {
                        if ((BS_Left.Pressed && BS_Left.PressedDuration > QuickMovePressThreshold)
                            || (BS_Right.Pressed && BS_Right.PressedDuration > QuickMovePressThreshold))
                        {
                            Actor.CurMoveAttempt.z = 0;
                        }
                        else
                        {
                            //Debug.Log("左右未能打断下");
                            Actor.CurMoveAttempt.x = 0;
                        }
                    }
                }

                // 目前是按住左键，并且有向左移动分量，且向左移动在上下移动之前按下
                if (Actor.CurMoveAttempt.x < 0 && BS_Left.Before(BS_Up) && BS_Left.Before(BS_Down))
                {
                    if (BS_Left.PressedDuration > QuickMovePressThreshold)
                    {
                        //Debug.Log("上下打断左");
                        Actor.CurMoveAttempt.x = 0;
                    }
                    else
                    {
                        if ((BS_Up.Pressed && BS_Up.PressedDuration > QuickMovePressThreshold)
                            || (BS_Down.Pressed && BS_Down.PressedDuration > QuickMovePressThreshold))
                        {
                            Actor.CurMoveAttempt.x = 0;
                        }
                        else
                        {
                            //Debug.Log("上下未能打断左");
                            Actor.CurMoveAttempt.z = 0;
                        }
                    }
                }

                // 目前是按住右键，并且有向右移动分量，且向右移动在上下移动之前按下
                if (Actor.CurMoveAttempt.x > 0 && BS_Right.Before(BS_Up) && BS_Right.Before(BS_Down))
                {
                    if (BS_Right.PressedDuration > QuickMovePressThreshold)
                    {
                        //Debug.Log("上下打断右");
                        Actor.CurMoveAttempt.x = 0;
                    }
                    else
                    {
                        if ((BS_Up.Pressed && BS_Up.PressedDuration > QuickMovePressThreshold)
                            || (BS_Down.Pressed && BS_Down.PressedDuration > QuickMovePressThreshold))
                        {
                            Actor.CurMoveAttempt.x = 0;
                        }
                        else
                        {
                            //Debug.Log("上下未能打断右");
                            Actor.CurMoveAttempt.z = 0;
                        }
                    }
                }
            }

            // 双向同时按取x轴优先, 理论上不应执行此处，仅为确保万一
            if (!Actor.CurMoveAttempt.x.Equals(0) && !Actor.CurMoveAttempt.z.Equals(0))
            {
                Actor.CurMoveAttempt.z = 0;
            }

            // 相机视角旋转后移动也相应旋转
            Actor.CurMoveAttempt = RotateMoveDirectionByCameraRotation(Actor.CurMoveAttempt);

            // 防止踏空
            GridPos3D targetWorldGP = Actor.WorldGP + Actor.CurMoveAttempt.ToGridPos3D();
            bool isGrounded = WorldManager.Instance.CurrentWorld.CheckIsGroundByPos(targetWorldGP, 5f, true, out GridPos3D groundGP);
            if (!isGrounded)
            {
                Actor.CurForward = Actor.CurMoveAttempt.normalized;
                Actor.CurMoveAttempt = Vector3.zero;
                isQuickMoving = false;
                quickMoveStartWorldGP = GridPos3D.Zero;
                quickMoveAttempt = Vector3.zero;
            }

            Actor.MoveInternal();

            #endregion

            #region Throw Charging

            if (Actor.ThrowState == Actor.ThrowStates.ThrowCharging && Skill_1_PressDuration > 0.2f)
            {
                if (PlayerNumber == PlayerNumber.Player1)
                {
                    Actor.CurThrowPointOffset = transform.forward;
                    //Ray ray = CameraManager.Instance.MainCamera.ScreenPointToRay(ControlManager.Instance.Battle_MousePosition);
                    //Vector3 intersectPoint = CommonUtils.GetIntersectWithLineAndPlane(ray.origin, ray.direction, Vector3.up, transform.position);
                    //CurThrowPointOffset = intersectPoint - transform.position;
                }
                else if (PlayerNumber == PlayerNumber.Player2)
                {
                    Actor.CurThrowPointOffset = transform.forward;
                    //CurThrowMoveAttempt = Vector3.zero;
                    //if (ThrowState == ThrowStates.ThrowCharging)
                    //{
                    //    CurThrowMoveAttempt = new Vector3(ControlManager.Instance.Player2_RightStick.x, 0, ControlManager.Instance.Player2_RightStick.y);
                    //    CurThrowMoveAttempt.Normalize();
                    //}

                    //CurThrowPointOffset += CurThrowMoveAttempt * Mathf.Max(ThrowAimMoveSpeed * Mathf.Sqrt(CurThrowPointOffset.magnitude), 2f) * Time.fixedDeltaTime;
                }

                if (!Actor.ActorBoxInteractHelper.CanInteract(InteractSkillType.Throw, Actor.CurrentLiftBox.EntityTypeIndex))
                {
                    if (Mathf.Abs(Actor.CurThrowPointOffset.x) > Mathf.Abs(Actor.CurThrowPointOffset.z))
                    {
                        Actor.CurThrowPointOffset.z = 0;
                    }
                    else
                    {
                        Actor.CurThrowPointOffset.x = 0;
                    }

                    Actor.CurThrowPointOffset = Actor.CurThrowPointOffset.normalized * Actor.ThrowRadiusMin;
                }
            }

            Actor.ThrowChargeAimInternal();

            #endregion

            #region Skill

            if (BS_SkillArray[1].Down)
            {
                Actor.Lift();
                Skill_1_PressDuration = 0;
            }

            if (BS_SkillArray[1].Pressed)
            {
                Actor.ThrowCharge();
                Skill_1_PressDuration += Time.fixedDeltaTime;
            }

            if (BS_SkillArray[1].Up)
            {
                Skill_1_PressDuration = 0;
                Actor.ThrowOrPut();
            }

            for (int i = 0; i < (int) KeyBind.MAX; i++)
            {
                if (BS_SkillArray[i].Down)
                {
                    foreach (EntitySkillIndex skillIndex in SkillKeyMappings[(KeyBind) i])
                    {
                        TriggerSkill(skillIndex, TargetEntityType.Self);
                    }
                }

                BS_SkillArray[i] = ControlManager.Instance.Battle_Skill[(int) PlayerNumber, i];
            }

            #endregion
        }
    }

    private Vector3 RotateMoveDirectionByCameraRotation(Vector3 moveDirection)
    {
        // 相机视角旋转后移动也相应旋转
        switch (CameraManager.Instance.FieldCamera.RotateDirection)
        {
            case GridPosR.Orientation.Up:
            {
                break;
            }
            case GridPosR.Orientation.Down:
            {
                moveDirection.z = -moveDirection.z;
                moveDirection.x = -moveDirection.x;
                break;
            }
            case GridPosR.Orientation.Left:
            {
                float x = moveDirection.x;
                float z = moveDirection.z;
                moveDirection.x = -z;
                moveDirection.z = x;
                break;
            }
            case GridPosR.Orientation.Right:
            {
                float x = moveDirection.x;
                float z = moveDirection.z;
                moveDirection.x = z;
                moveDirection.z = -x;
                break;
            }
        }

        return moveDirection;
    }

    [HideInPlayMode]
    [HideInPrefabAssets]
    [ShowInInspector]
    [NonSerialized]
    [BoxGroup("一键换装")]
    [LabelText("一键换装角色编号")]
    private PlayerNumber SwitchAvatarPlayerNumber;

    [BoxGroup("一键换装")]
    [Button("一键换装")]
    private void SwitchAvatar()
    {
        Actor.ActorSkinHelper.Initialize(SwitchAvatarPlayerNumber);
    }

    private bool TriggerSkill(EntitySkillIndex skillIndex, TargetEntityType targetEntityType)
    {
        if (Actor.CannotAct) return false;
        if (Actor.EntityActiveSkillDict.TryGetValue(skillIndex, out EntityActiveSkill eas))
        {
            bool triggerSuc = eas.CheckCanTriggerSkill(targetEntityType, 100);
            if (triggerSuc)
            {
                eas.TriggerActiveSkill(targetEntityType);
                return true;
            }
            else
            {
                return false;
            }
        }

        return false;
    }
}

public enum PlayerNumber
{
    Player1 = 0,
    Player2 = 1
}