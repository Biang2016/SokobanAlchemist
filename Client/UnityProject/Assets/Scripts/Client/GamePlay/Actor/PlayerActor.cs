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
    internal bool skill1_Pressed;
    internal bool skill1_Up;

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

        skill0_Down = ControlManager.Instance.Battle_Skill[(int) PlayerNumber, 0].Down;
        skill0_Up = ControlManager.Instance.Battle_Skill[(int) PlayerNumber, 0].Up;
        skill1_Down = ControlManager.Instance.Battle_Skill[(int) PlayerNumber, 1].Down;
        skill1_Pressed = ControlManager.Instance.Battle_Skill[(int) PlayerNumber, 1].Pressed;
        skill1_Up = ControlManager.Instance.Battle_Skill[(int) PlayerNumber, 1].Up;

        if (skill0_Up) Kick();

        if (skill1_Down) Lift();

        if (skill1_Pressed) ThrowCharge();

        if (skill1_Up) Throw();

        #endregion
    }
}

public enum PlayerNumber
{
    Player1 = 0,
    Player2 = 1
}