using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActor : Actor
{
    public PlayerNumber PlayerNumber;
    public float Accelerate = 10f;
    public float MoveSpeed = 10f;
    public float Drag = 10f;

    void FixedUpdate()
    {
        Vector2 movement = Vector2.zero;
        switch (PlayerNumber)
        {
            case PlayerNumber.Player1:
            {
                movement = ControlManager.Instance.Battle_Move_Player1 * Time.fixedDeltaTime * Accelerate;
                break;
            }
            case PlayerNumber.Player2:
            {
                movement = ControlManager.Instance.Battle_Move_Player2 * Time.fixedDeltaTime * Accelerate;
                break;
            }
        }

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
            PushTrigger.PushTriggerOut();
        }
        else
        {
            RigidBody.drag = 100f;
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