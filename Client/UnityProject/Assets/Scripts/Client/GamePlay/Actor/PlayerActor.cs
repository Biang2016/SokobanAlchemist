using System;
using BiangStudio;
using BiangStudio.GameDataFormat.Grid;
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

    private float Duration_Up;
    private float Duration_Right;
    private float Duration_Down;
    private float Duration_Left;

    private ButtonState BS_Up_Last;
    private ButtonState BS_Right_Last;
    private ButtonState BS_Down_Last;
    private ButtonState BS_Left_Last;

    private ButtonState BS_Skill_0; // Space/RT
    private ButtonState BS_Skill_1;
    private ButtonState BS_Skill_2; // Shift/South

    private float QuickMovePressThreshold = 0.2f;
    private GridPos3D lastMoveButtonDownWorldGP = GridPos3D.Zero; // 记录上一次任一方向键按下时的角色坐标
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

    private float Skill1_PressDuration;

    private int a = 0;

    protected override void FixedUpdate()
    {
        if (!IsRecycled)
        {
            #region Move

            if (BS_Up.Down) Duration_Up = 0;
            if (BS_Right.Down) Duration_Right = 0;
            if (BS_Down.Down) Duration_Down = 0;
            if (BS_Left.Down) Duration_Left = 0;
            if (BS_Up.Down || BS_Right.Down || BS_Down.Down || BS_Left.Down) lastMoveButtonDownWorldGP = CurWorldGP;

            if (BS_Up.Pressed) Duration_Up += Time.fixedDeltaTime;
            if (BS_Right.Pressed) Duration_Right += Time.fixedDeltaTime;
            if (BS_Down.Pressed) Duration_Down += Time.fixedDeltaTime;
            if (BS_Left.Pressed) Duration_Left += Time.fixedDeltaTime;

            CurMoveAttempt = Vector3.zero;
            if (BS_Up.Pressed) CurMoveAttempt.z += 1;
            if (BS_Down.Pressed) CurMoveAttempt.z -= 1;
            if (BS_Left.Pressed) CurMoveAttempt.x -= 1;
            if (BS_Right.Pressed) CurMoveAttempt.x += 1;

            // 如果没有操作，判定是否短按，如果短按则为玩家坚持按该键一段时间，直到角色位置变化
            if (CurMoveAttempt.Equals(Vector3.zero))
            {
                Vector3 quickMoveAttemptThisFrame = Vector3.zero; // 短按值
                if (BS_Up.Up && Duration_Up < QuickMovePressThreshold) quickMoveAttemptThisFrame.z += 1;
                if (BS_Right.Up && Duration_Right < QuickMovePressThreshold) quickMoveAttemptThisFrame.x += 1;
                if (BS_Down.Up && Duration_Down < QuickMovePressThreshold) quickMoveAttemptThisFrame.z -= 1;
                if (BS_Left.Up && Duration_Left < QuickMovePressThreshold) quickMoveAttemptThisFrame.x -= 1;
                if (quickMoveAttemptThisFrame != Vector3.zero) // 此帧有短按
                {
                    quickMoveAttempt = quickMoveAttemptThisFrame;
                    quickMoveStartWorldGP = lastMoveButtonDownWorldGP; // 短按瞬间记录角色位置
                }

                if (quickMoveAttempt != Vector3.zero) // 短按效果仍持续，且自短按开始后角色位置还未发生变化，则继续施加移动效果
                {
                    if (quickMoveStartWorldGP == CurWorldGP)
                    {
                        CurMoveAttempt = quickMoveAttempt;
                    }
                    else // 当角色位置变化时，短按效果结束
                    {
                        quickMoveStartWorldGP = GridPos3D.Zero;
                        quickMoveAttempt = Vector3.zero;
                    }
                }
                else
                {
                    quickMoveStartWorldGP = GridPos3D.Zero;
                    quickMoveAttempt = Vector3.zero;
                }
            }
            else
            {
                quickMoveStartWorldGP = GridPos3D.Zero;
                quickMoveAttempt = Vector3.zero; // 长按打断短按
            }

            // 相机视角旋转后移动也相应旋转
            switch (CameraManager.Instance.FieldCamera.RotateDirection)
            {
                case GridPosR.Orientation.Up:
                {
                    break;
                }
                case GridPosR.Orientation.Down:
                {
                    CurMoveAttempt.z = -CurMoveAttempt.z;
                    CurMoveAttempt.x = -CurMoveAttempt.x;
                    break;
                }
                case GridPosR.Orientation.Left:
                {
                    float x = CurMoveAttempt.x;
                    float z = CurMoveAttempt.z;
                    CurMoveAttempt.x = -z;
                    CurMoveAttempt.z = x;
                    break;
                }
                case GridPosR.Orientation.Right:
                {
                    float x = CurMoveAttempt.x;
                    float z = CurMoveAttempt.z;
                    CurMoveAttempt.x = z;
                    CurMoveAttempt.z = -x;
                    break;
                }
            }

            // 双向移动交替打断
            if (!CurMoveAttempt.x.Equals(0) && !CurMoveAttempt.z.Equals(0))
            {
                if ((LastMoveAttempt.x > 0 && BS_Right_Last.Pressed) || (LastMoveAttempt.x < 0 && BS_Left_Last.Pressed))
                {
                    if (!BS_Up_Last.Pressed && !BS_Down_Last.Pressed)
                    {
                        CurMoveAttempt.x = 0;
                    }
                    else
                    {
                        if ((BS_Up_Last.Pressed && BS_Up.Pressed) || (BS_Down_Last.Pressed && BS_Down.Pressed))
                        {
                            CurMoveAttempt.z = 0;
                        }
                    }
                }

                if ((LastMoveAttempt.z > 0 && BS_Up_Last.Pressed) || (LastMoveAttempt.z < 0 && BS_Down_Last.Pressed))
                {
                    if (!BS_Left_Last.Pressed && !BS_Right_Last.Pressed)
                    {
                        CurMoveAttempt.z = 0;
                    }
                    else
                    {
                        if ((BS_Left_Last.Pressed && BS_Left.Pressed) || (BS_Right_Last.Pressed && BS_Right.Pressed))
                        {
                            CurMoveAttempt.x = 0;
                        }
                    }
                }
            }

            // 双向同时按取x轴优先
            if (!CurMoveAttempt.x.Equals(0) && !CurMoveAttempt.z.Equals(0))
            {
                CurMoveAttempt.z = 0;
            }

            MoveInternal();

            #endregion

            #region Throw Charging

            if (ThrowState == ThrowStates.ThrowCharging && Skill1_PressDuration > 0.2f)
            {
                if (PlayerNumber == PlayerNumber.Player1)
                {
                    Ray ray = CameraManager.Instance.MainCamera.ScreenPointToRay(ControlManager.Instance.Battle_MousePosition);
                    Vector3 intersectPoint = CommonUtils.GetIntersectWithLineAndPlane(ray.origin, ray.direction, Vector3.up, transform.position);
                    CurThrowPointOffset = intersectPoint - transform.position;
                }
                else if (PlayerNumber == PlayerNumber.Player2)
                {
                    CurThrowMoveAttempt = Vector3.zero;
                    if (ThrowState == ThrowStates.ThrowCharging)
                    {
                        CurThrowMoveAttempt = new Vector3(ControlManager.Instance.Player2_RightStick.x, 0, ControlManager.Instance.Player2_RightStick.y);
                        CurThrowMoveAttempt.Normalize();
                    }

                    CurThrowPointOffset += CurThrowMoveAttempt * Mathf.Max(ThrowAimMoveSpeed * Mathf.Sqrt(CurThrowPointOffset.magnitude), 2f) * Time.fixedDeltaTime;
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
                Skill1_PressDuration = 0;
            }

            if (BS_Skill_2.Pressed)
            {
                ThrowCharge();
                Skill1_PressDuration += Time.fixedDeltaTime;
            }

            if (BS_Skill_2.Up)
            {
                Skill1_PressDuration = 0;
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