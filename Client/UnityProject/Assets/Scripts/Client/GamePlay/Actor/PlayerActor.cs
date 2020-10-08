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

    private ButtonState BS_Up_Last;
    private ButtonState BS_Right_Last;
    private ButtonState BS_Down_Last;
    private ButtonState BS_Left_Last;

    public void Initialize(PlayerNumber playerNumber)
    {
        base.Initialize();
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
    }

    private float Skill1_PressDuration;

    protected override void FixedUpdate()
    {
        if (!IsRecycled)
        {
            #region Move

            CurMoveAttempt = Vector3.zero;
            if (BS_Up.Pressed) CurMoveAttempt.z += 1;
            if (BS_Down.Pressed) CurMoveAttempt.z -= 1;
            if (BS_Left.Pressed) CurMoveAttempt.x -= 1;
            if (BS_Right.Pressed) CurMoveAttempt.x += 1;

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

            bool skill_0_Down = ControlManager.Instance.Battle_Skill[(int) PlayerNumber, 0].Down;
            bool skill_0_Pressed = ControlManager.Instance.Battle_Skill[(int) PlayerNumber, 0].Pressed;
            bool skill_0_Up = ControlManager.Instance.Battle_Skill[(int) PlayerNumber, 0].Up;
            bool skill_1_Down = ControlManager.Instance.Battle_Skill[(int) PlayerNumber, 1].Down;
            bool skill_1_Pressed = ControlManager.Instance.Battle_Skill[(int) PlayerNumber, 1].Pressed;
            bool skill_1_Up = ControlManager.Instance.Battle_Skill[(int) PlayerNumber, 1].Up;

            if (skill_1_Down)
            {
                Lift();
                Skill1_PressDuration = 0;
            }

            if (skill_1_Pressed)
            {
                ThrowCharge();
                Skill1_PressDuration += Time.fixedDeltaTime;
            }

            if (skill_1_Up)
            {
                Skill1_PressDuration = 0;
                ThrowOrPut();
            }

            if (skill_0_Down)
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