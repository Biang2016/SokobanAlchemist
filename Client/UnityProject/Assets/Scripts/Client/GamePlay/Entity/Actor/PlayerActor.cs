using System;
using System.Collections.Generic;
using BiangLibrary;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerActor : Actor
{
    [LabelText("玩家编号")]
    public PlayerNumber PlayerNumber;

    private ButtonState BS_Up;
    private ButtonState BS_Right;
    private ButtonState BS_Down;
    private ButtonState BS_Left;

    private ButtonState BS_Up_Last;
    private ButtonState BS_Right_Last;
    private ButtonState BS_Down_Last;
    private ButtonState BS_Left_Last;

    private ButtonState BS_Skill_0; // Space/RT
    private ButtonState BS_Skill_1;
    private ButtonState BS_Skill_2; // Shift/South

    // 短按逻辑：短按最优先，短按过程中不接受其他短按，短按那个按键down时记录该轴位置，当位置变化时结束短按，短按结束后短按数据清空
    private bool isQuickMoving = false;
    private float QuickMovePressThreshold = 0.2f;
    private GridPos3D lastMoveUpButtonDownWorldGP = GridPos3D.Zero; // 记录上一次方向键按下时的角色坐标
    private GridPos3D lastMoveRightButtonDownWorldGP = GridPos3D.Zero; // 记录上一次方向键按下时的角色坐标
    private GridPos3D lastMoveDownButtonDownWorldGP = GridPos3D.Zero; // 记录上一次方向键按下时的角色坐标
    private GridPos3D lastMoveLeftButtonDownWorldGP = GridPos3D.Zero; // 记录上一次方向键按下时的角色坐标
    private GridPos3D quickMoveStartWorldGP = GridPos3D.Zero; // 记录短按开始时的角色坐标（按上一次down键的世界坐标取值）
    private Vector3 quickMoveAttempt = Vector3.zero; // 记录当前短按的移动方向，zero为无短按

    public void Initialize(string actorType, ActorCategory actorCategory, PlayerNumber playerNumber)
    {
        base.Initialize(actorType, actorCategory);
        PlayerNumber = playerNumber;
        ActorSkinHelper.Initialize(playerNumber);
        BS_Up = ControlManager.Instance.Battle_MoveButtons[(int) PlayerNumber, (int) GridPosR.Orientation.Up];
        BS_Right = ControlManager.Instance.Battle_MoveButtons[(int) PlayerNumber, (int) GridPosR.Orientation.Right];
        BS_Down = ControlManager.Instance.Battle_MoveButtons[(int) PlayerNumber, (int) GridPosR.Orientation.Down];
        BS_Left = ControlManager.Instance.Battle_MoveButtons[(int) PlayerNumber, (int) GridPosR.Orientation.Left];

        BS_Up_Last = ControlManager.Instance.Battle_MoveButtons_LastFrame[(int) PlayerNumber, (int) GridPosR.Orientation.Up];
        BS_Right_Last = ControlManager.Instance.Battle_MoveButtons_LastFrame[(int) PlayerNumber, (int) GridPosR.Orientation.Right];
        BS_Down_Last = ControlManager.Instance.Battle_MoveButtons_LastFrame[(int) PlayerNumber, (int) GridPosR.Orientation.Down];
        BS_Left_Last = ControlManager.Instance.Battle_MoveButtons_LastFrame[(int) PlayerNumber, (int) GridPosR.Orientation.Left];

        BS_Skill_0 = ControlManager.Instance.Battle_Skill[(int) PlayerNumber, 0];
        BS_Skill_1 = ControlManager.Instance.Battle_Skill[(int) PlayerNumber, 1];
        BS_Skill_2 = ControlManager.Instance.Battle_Skill[(int) PlayerNumber, 2];
    }

    private float Skill_1_PressDuration;

    private float QuickMoveDuration; // 快速移动开始后经过的时间
    private Vector3 QuickMoveStartActorPosition = Vector3.zero;

    protected override void FixedUpdate()
    {
        if (!IsRecycled)
        {
            #region Move

            CurMoveAttempt = Vector3.zero;

            // todo 转视角后按键映射问题

            if (BS_Up.Down) lastMoveUpButtonDownWorldGP = CurWorldGP;
            if (BS_Right.Down) lastMoveRightButtonDownWorldGP = CurWorldGP;
            if (BS_Down.Down) lastMoveDownButtonDownWorldGP = CurWorldGP;
            if (BS_Left.Down) lastMoveLeftButtonDownWorldGP = CurWorldGP;

            if (BS_Up.Pressed) CurMoveAttempt.z += 1;
            if (BS_Right.Pressed) CurMoveAttempt.x += 1;
            if (BS_Down.Pressed) CurMoveAttempt.z -= 1;
            if (BS_Left.Pressed) CurMoveAttempt.x -= 1;

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
                    GridPos3D targetPos = CurWorldGP + rotatedQuickMoveAttemptGP;

                    // Check is there any box occupies the grid
                    Box box = WorldManager.Instance.CurrentWorld.GetBoxByGridPosition(targetPos, out WorldModule module, out GridPos3D _, true);
                    if ((!box && module)
                        || (box && box.Passable)
                        || (box && box.Pushable && ActorPushHelper.Actor.ActorSkillHelper.CanInteract(InteractSkillType.Push, box.BoxTypeIndex) &&
                            WorldManager.Instance.CurrentWorld.CheckCanMoveBoxColumn(box.WorldGP, rotatedQuickMoveAttemptGP, new HashSet<Box>()))) // 能走到才开启短按
                    {
                        // Check is there any actor occupies the grid
                        bool isActorOccupying = false;
                        Collider[] colliders = Physics.OverlapSphere(targetPos, 0.4f, LayerManager.Instance.LayerMask_HitBox_Player | LayerManager.Instance.LayerMask_HitBox_Enemy);
                        foreach (Collider collider in colliders)
                        {
                            Actor actor = collider.GetComponentInParent<Actor>();
                            if (actor != null && actor.GUID != GUID)
                            {
                                isActorOccupying = true;
                            }
                        }

                        if (!isActorOccupying)
                        {
                            CurMoveAttempt = quickMoveAttempt;
                            //Debug.Log("速按" + quickMoveAttempt);
                            isQuickMoving = true;
                        }
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

                    if (quickMoveStartWorldGP == CurWorldGP // 自短按开始后角色该轴位置还未发生变化，则继续施加移动效果
                        && !(QuickMoveDuration > 0.3f && quickMoveSpeed < 0.1f)) // 且短按期间的移动速度不能过低，否则判定为卡住
                    {
                        CurMoveAttempt = quickMoveAttempt;
                        isQuickMoving = true;
                    }
                    else // 停止短按移动
                    {
                        //Debug.Log("速按结束" + quickMoveAttempt);
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
            if (!CurMoveAttempt.x.Equals(0) && !CurMoveAttempt.z.Equals(0))
            {
                // 目前是按住上键，并且有向上移动分量，且向上移动在左右移动之前按下
                if (CurMoveAttempt.z > 0 && BS_Up.Before(BS_Left) && BS_Up.Before(BS_Right))
                {
                    if (BS_Up.PressedDuration > QuickMovePressThreshold)
                    {
                        //Debug.Log("左右打断上");
                        CurMoveAttempt.z = 0;
                    }
                    else
                    {
                        if ((BS_Left.Pressed && BS_Left.PressedDuration > QuickMovePressThreshold)
                            || (BS_Right.Pressed && BS_Right.PressedDuration > QuickMovePressThreshold))
                        {
                            CurMoveAttempt.z = 0;
                        }
                        else
                        {
                            //Debug.Log("左右未能打断上");
                            CurMoveAttempt.x = 0;
                        }
                    }
                }

                // 目前是按住下键，并且有向下移动分量，且向下移动在左右移动之前按下
                if (CurMoveAttempt.z < 0 && BS_Down.Before(BS_Left) && BS_Down.Before(BS_Right))
                {
                    if (BS_Down.PressedDuration > QuickMovePressThreshold)
                    {
                        //Debug.Log("左右打断下");
                        CurMoveAttempt.z = 0;
                    }
                    else
                    {
                        if ((BS_Left.Pressed && BS_Left.PressedDuration > QuickMovePressThreshold)
                            || (BS_Right.Pressed && BS_Right.PressedDuration > QuickMovePressThreshold))
                        {
                            CurMoveAttempt.z = 0;
                        }
                        else
                        {
                            //Debug.Log("左右未能打断下");
                            CurMoveAttempt.x = 0;
                        }
                    }
                }

                // 目前是按住左键，并且有向左移动分量，且向左移动在上下移动之前按下
                if (CurMoveAttempt.x < 0 && BS_Left.Before(BS_Up) && BS_Left.Before(BS_Down))
                {
                    if (BS_Left.PressedDuration > QuickMovePressThreshold)
                    {
                        //Debug.Log("上下打断左");
                        CurMoveAttempt.x = 0;
                    }
                    else
                    {
                        if ((BS_Up.Pressed && BS_Up.PressedDuration > QuickMovePressThreshold)
                            || (BS_Down.Pressed && BS_Down.PressedDuration > QuickMovePressThreshold))
                        {
                            CurMoveAttempt.x = 0;
                        }
                        else
                        {
                            //Debug.Log("上下未能打断左");
                            CurMoveAttempt.z = 0;
                        }
                    }
                }

                // 目前是按住右键，并且有向右移动分量，且向右移动在上下移动之前按下
                if (CurMoveAttempt.x > 0 && BS_Right.Before(BS_Up) && BS_Right.Before(BS_Down))
                {
                    if (BS_Right.PressedDuration > QuickMovePressThreshold)
                    {
                        //Debug.Log("上下打断右");
                        CurMoveAttempt.x = 0;
                    }
                    else
                    {
                        if ((BS_Up.Pressed && BS_Up.PressedDuration > QuickMovePressThreshold)
                            || (BS_Down.Pressed && BS_Down.PressedDuration > QuickMovePressThreshold))
                        {
                            CurMoveAttempt.x = 0;
                        }
                        else
                        {
                            //Debug.Log("上下未能打断右");
                            CurMoveAttempt.z = 0;
                        }
                    }
                }
            }

            // 双向同时按取x轴优先, 理论上不应执行此处，仅为确保万一
            if (!CurMoveAttempt.x.Equals(0) && !CurMoveAttempt.z.Equals(0))
            {
                CurMoveAttempt.z = 0;
            }

            // 相机视角旋转后移动也相应旋转
            CurMoveAttempt = RotateMoveDirectionByCameraRotation(CurMoveAttempt);

            MoveInternal();

            #endregion

            #region Throw Charging

            if (ThrowState == ThrowStates.ThrowCharging && Skill_1_PressDuration > 0.2f)
            {
                if (PlayerNumber == PlayerNumber.Player1)
                {
                    CurThrowPointOffset = transform.forward;
                    //Ray ray = CameraManager.Instance.MainCamera.ScreenPointToRay(ControlManager.Instance.Battle_MousePosition);
                    //Vector3 intersectPoint = CommonUtils.GetIntersectWithLineAndPlane(ray.origin, ray.direction, Vector3.up, transform.position);
                    //CurThrowPointOffset = intersectPoint - transform.position;
                }
                else if (PlayerNumber == PlayerNumber.Player2)
                {
                    CurThrowPointOffset = transform.forward;
                    //CurThrowMoveAttempt = Vector3.zero;
                    //if (ThrowState == ThrowStates.ThrowCharging)
                    //{
                    //    CurThrowMoveAttempt = new Vector3(ControlManager.Instance.Player2_RightStick.x, 0, ControlManager.Instance.Player2_RightStick.y);
                    //    CurThrowMoveAttempt.Normalize();
                    //}

                    //CurThrowPointOffset += CurThrowMoveAttempt * Mathf.Max(ThrowAimMoveSpeed * Mathf.Sqrt(CurThrowPointOffset.magnitude), 2f) * Time.fixedDeltaTime;
                }

                if (!ActorSkillHelper.CanInteract(InteractSkillType.Throw, CurrentLiftBox.BoxTypeIndex))
                {
                    if (Mathf.Abs(CurThrowPointOffset.x) > Mathf.Abs(CurThrowPointOffset.z))
                    {
                        CurThrowPointOffset.z = 0;
                    }
                    else
                    {
                        CurThrowPointOffset.x = 0;
                    }

                    CurThrowPointOffset = CurThrowPointOffset.normalized * ThrowRadiusMin;
                }
            }

            ThrowChargeAimInternal();

            #endregion

            #region Skill

            if (BS_Skill_2.Down)
            {
                Lift();
                Skill_1_PressDuration = 0;
            }

            if (BS_Skill_2.Pressed)
            {
                ThrowCharge();
                Skill_1_PressDuration += Time.fixedDeltaTime;
            }

            if (BS_Skill_2.Up)
            {
                Skill_1_PressDuration = 0;
                ThrowOrPut();
            }

            if (BS_Skill_2.Down)
            {
                VaultOrDash();
            }

            if (BS_Skill_0.Down)
            {
                Kick();
            }

            #endregion
        }

        base.FixedUpdate();
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
        ActorSkinHelper.Initialize(SwitchAvatarPlayerNumber);
    }
}

public enum PlayerNumber
{
    Player1 = 0,
    Player2 = 1
}