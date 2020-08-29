using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class ButtonState
{
    [LabelText("按键")]
    public ButtonNames ButtonName;

    [LabelText("按下")]
    public bool Down;

    [LabelText("按住")]
    public bool Pressed;

    [LabelText("释放")]
    public bool Up;

    [HideInInspector]
    public bool LastPressed;

    public override string ToString()
    {
        if (!Down && !Up) return "";
        string res = ButtonName + (Down ? ",Down" : "") + (Up ? ",Up" : "");
        return res;
    }

    public void Reset()
    {
        Down = false;
        LastPressed = Pressed;
        Up = false;
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

    Battle_Skill_0,
    Battle_Skill_1,
    Battle_Skill_2,
    Battle_Skill_3,
    Battle_ToggleBattleTip,

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

    COMMON_MAX_FLAG = 600,
}