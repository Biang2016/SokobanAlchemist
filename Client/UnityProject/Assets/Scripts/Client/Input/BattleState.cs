using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class ButtonState
{
    [LabelText("按键")]
    public ButtonNames ButtonName;

    [LabelText("上一次按下帧号")]
    public int LastDownFrame;

    [LabelText("按下")]
    public bool Down;

    [LabelText("按住")]
    public bool Pressed;

    [LabelText("上一次释放帧号")]
    public int LastUpFrame;

    [LabelText("释放")]
    public bool Up;

    [HideInInspector]
    public bool LastPressed;

    [LabelText("连击")]
    public int MultiClick;

    public float PressedDuration;

    public override string ToString()
    {
        if (!Down && !Up && MultiClick <= 1) return "";
        string res = ButtonName + (Down ? ",Down" : "") + (Up ? ",Up" : "") + (MultiClick > 1 ? "x" + MultiClick : "");
        return res;
    }

    public void Reset()
    {
        Down = false;
        LastPressed = Pressed;
        Up = false;
        MultiClick = 0;
    }

    public void ApplyTo(ButtonState target)
    {
        target.ButtonName = ButtonName;
        target.LastDownFrame = LastDownFrame;
        target.Down = Down;
        target.LastUpFrame = LastUpFrame;
        target.Up = Up;
        target.Pressed = Pressed;
        target.LastPressed = LastPressed;
        target.MultiClick = MultiClick;
        target.PressedDuration = PressedDuration;
    }

    public bool Before(ButtonState target)
    {
        if (Pressed && !target.Pressed) return true;
        if (Pressed && target.Pressed && LastDownFrame < target.LastDownFrame) return true;
        return false;
    }
}

public enum ButtonNames
{
    None = 0,

    BUILDING_MIN_FLAG = 100,

    Building_MouseLeft,
    Building_MouseRight,
    Building_MouseMiddle,

    Building_RotateItem,
    Building_ToggleBackpack,
    Building_ToggleWireLines,
    Building_ToggleDebug,

    BUILDING_MAX_FLAG = 200,

    BATTLE_MIN_FLAG = 300,

    Battle_MouseLeft,
    Battle_MouseRight,
    Battle_MouseMiddle,

    Battle_Move_Up_Player1,
    Battle_Move_Right_Player1,
    Battle_Move_Down_Player1,
    Battle_Move_Left_Player1,
    Battle_Move_Up_Player2,
    Battle_Move_Right_Player2,
    Battle_Move_Down_Player2,
    Battle_Move_Left_Player2,

    Battle_Skill_0_Player1,
    Battle_Skill_1_Player1,
    Battle_Skill_2_Player1,
    Battle_Skill_0_Player2,
    Battle_Skill_1_Player2,
    Battle_Skill_2_Player2,
    Battle_ToggleBattleTip,

    Battle_LeftRotateCamera,
    Battle_RightRotateCamera,

    BATTLE_MAX_FLAG = 400,

    COMMON_MIN_FLAG = 500,

    Common_MouseLeft,
    Common_MouseRight,
    Common_MouseMiddle,

    Common_Confirm,
    Common_Debug,
    Common_Exit,
    Common_Tab,
    Common_RestartGame,
    Common_Pause,

    COMMON_MAX_FLAG = 600,
}