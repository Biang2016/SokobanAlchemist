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

    public float DoubleMoveKickInterval = 0.3f;
    private bool forwardLastUp;
    private float forwardTick;
    private bool backwardLastUp;
    private float backwardTick;
    private bool leftwardLastUp;
    private float leftwardTick;
    private bool rightwardLastUp;
    private float rightwardTick;

    protected override void InternalFixedUpdate()
    {
        base.InternalFixedUpdate();

        // 双击方向键踢箱子
        if (LastMoveAttempt.x > 0 && CurMoveAttempt.x.Equals(0))
        {
            rightwardLastUp = true;
            rightwardTick = DoubleMoveKickInterval;
        }

        if (LastMoveAttempt.x < 0 && CurMoveAttempt.x.Equals(0))
        {
            leftwardLastUp = true;
            leftwardTick = DoubleMoveKickInterval;
        }

        if (LastMoveAttempt.z > 0 && CurMoveAttempt.z.Equals(0))
        {
            forwardLastUp = true;
            forwardTick = DoubleMoveKickInterval;
        }

        if (LastMoveAttempt.z < 0 && CurMoveAttempt.z.Equals(0))
        {
            backwardLastUp = true;
            backwardTick = DoubleMoveKickInterval;
        }

        rightwardTick -= Time.fixedDeltaTime;
        if (rightwardTick <= 0) rightwardLastUp = false;
        leftwardTick -= Time.fixedDeltaTime;
        if (leftwardTick <= 0) leftwardLastUp = false;
        forwardTick -= Time.fixedDeltaTime;
        if (forwardTick <= 0) forwardLastUp = false;
        backwardTick -= Time.fixedDeltaTime;
        if (backwardTick <= 0) backwardLastUp = false;

        if (CurMoveAttempt.x > 0 && LastMoveAttempt.x.Equals(0) && rightwardLastUp)
        {
            Kick();
        }
        else if (CurMoveAttempt.x < 0 && LastMoveAttempt.x.Equals(0) && leftwardLastUp)
        {
            Kick();
        }
        else if (CurMoveAttempt.z > 0 && LastMoveAttempt.z.Equals(0) && forwardLastUp)
        {
            Kick();
        }
        else if (CurMoveAttempt.z < 0 && LastMoveAttempt.z.Equals(0) && backwardLastUp)
        {
            Kick();
        }
    }
}

public enum PlayerNumber
{
    Player1 = 0,
    Player2 = 1
}