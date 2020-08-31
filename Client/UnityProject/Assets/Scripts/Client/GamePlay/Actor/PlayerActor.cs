using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerActor : Actor
{
    [LabelText("玩家编号")]
    public PlayerNumber PlayerNumber;

    internal bool skill0_Down;
    internal bool skill0_Up;
    internal bool skill1_Down;
    internal bool skill1_Up;

    void FixedUpdate()
    {
        Vector2 movement = ControlManager.Instance.Battle_Move[(int) PlayerNumber] * Time.fixedDeltaTime * Accelerate;

        #region Move

        CurMoveAttempt = new Vector3(movement.x, 0, movement.y);

        if (CurMoveAttempt.magnitude > 0)
        {
            RigidBody.drag = 0;
            RigidBody.AddForce(CurMoveAttempt);

            if (RigidBody.velocity.magnitude > MoveSpeed)
            {
                RigidBody.AddForce(-RigidBody.velocity * Drag);
            }

            transform.forward = CurMoveAttempt;
            ActorPushHelper.PushTriggerOut();
        }
        else
        {
            RigidBody.drag = 100f;
            ActorPushHelper.PushTriggerReset();
        }

        RigidBody.angularVelocity = Vector3.zero;

        #endregion

        #region Skill

        skill0_Down = ControlManager.Instance.Battle_Skill[(int) PlayerNumber, 0].Down;
        skill0_Up = ControlManager.Instance.Battle_Skill[(int) PlayerNumber, 0].Up;
        skill1_Down = ControlManager.Instance.Battle_Skill[(int) PlayerNumber, 1].Down;
        skill1_Up = ControlManager.Instance.Battle_Skill[(int) PlayerNumber, 1].Up;

        if (skill0_Up)
        {
            Kick();
        }

        #endregion
    }
}

public enum PlayerNumber
{
    Player1 = 0,
    Player2 = 1
}