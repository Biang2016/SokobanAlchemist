using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActor : Actor
{
    public PlayerNumber PlayerNumber;
    public float MoveSpeed = 10f;

    void FixedUpdate()
    {
        Vector2 movement = Vector2.zero;
        switch (PlayerNumber)
        {
            case PlayerNumber.Player1:
            {
                movement = ControlManager.Instance.Battle_Move_Player1 * Time.fixedDeltaTime * MoveSpeed;
                break;
            }
            case PlayerNumber.Player2:
            {
                movement = ControlManager.Instance.Battle_Move_Player2 * Time.fixedDeltaTime * MoveSpeed;
                break;
            }
        }

        CurMoveAttempt = new Vector3(movement.x, 0, movement.y);
        RigidBody.velocity = CurMoveAttempt;
        if (RigidBody.velocity.magnitude > 0)
        {
            transform.forward = RigidBody.velocity;
            PushTrigger.PushTriggerOut();
        }
        else
        {
            PushTrigger.PushTriggerReset();
        }

        RigidBody.angularVelocity = Vector3.zero;
    }
}

public enum PlayerNumber
{
    Player1 = 1,
    Player2 = 2
}