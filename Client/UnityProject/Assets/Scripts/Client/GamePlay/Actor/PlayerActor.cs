using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerActor : Actor
{
    [LabelText("玩家编号")]
    public PlayerNumber PlayerNumber;

    // Skill_0 is Throw
    internal bool skill_0_Down;
    internal bool skill_0_Pressed;
    internal bool skill_0_Up;
    internal bool skill_1_Down;
    internal bool skill_1_Pressed;
    internal bool skill_1_Up;

    public void Initialize(PlayerNumber playerNumber)
    {
        PlayerNumber = playerNumber;
        ActorSkinHelper.Initialize(playerNumber);
    }

    protected override void FixedUpdate()
    {
        Vector2 movement = ControlManager.Instance.Battle_Move[(int) PlayerNumber] * Time.fixedDeltaTime * Accelerate;
        CurMoveAttempt = new Vector3(movement.x, 0, movement.y);

        base.FixedUpdate();

        #region Skill

        skill_0_Down = ControlManager.Instance.Battle_Skill[(int) PlayerNumber, 0].Down;
        skill_0_Pressed = ControlManager.Instance.Battle_Skill[(int) PlayerNumber, 0].Pressed;
        skill_0_Up = ControlManager.Instance.Battle_Skill[(int) PlayerNumber, 0].Up;
        skill_1_Down = ControlManager.Instance.Battle_Skill[(int) PlayerNumber, 1].Down;
        skill_1_Pressed = ControlManager.Instance.Battle_Skill[(int) PlayerNumber, 1].Pressed;
        skill_1_Up = ControlManager.Instance.Battle_Skill[(int) PlayerNumber, 1].Up;

        if (skill_0_Down) Lift();

        if (skill_0_Pressed) ThrowCharge();

        if (skill_0_Up) Throw();

        #endregion
    }

    protected override void InternalFixedUpdate()
    {
        base.InternalFixedUpdate();

        // 推的过程中再次按下对应方向键，则踢出箱子
        bool kick = false;
        if (!CurMoveAttempt.x.Equals(0) && LastMoveAttempt.x.Equals(0))
        {
            if (CurMoveAttempt.x * CurForward.x > 0)
            {
                kick = true;
            }
        }

        if (!CurMoveAttempt.z.Equals(0) && LastMoveAttempt.z.Equals(0))
        {
            if (CurMoveAttempt.z * CurForward.z > 0)
            {
                kick = true;
            }
        }

        if (PushState == PushStates.Pushing && kick)
        {
            //Kick();
        }
    }
}

public enum PlayerNumber
{
    Player1 = 0,
    Player2 = 1
}