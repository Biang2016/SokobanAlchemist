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

// UI提示中的按键绑定依赖此处的枚举名称，名称慎重修改
public enum ButtonNames
{
    None = 0,

    Fake_CharacterMove,

    BATTLE_MIN_FLAG = 300,

    Battle_MouseLeft,
    Battle_MouseRight,
    Battle_MouseMiddle,

    Battle_LeftSwitch,
    Battle_RightSwitch,
    Battle_InteractiveKey,
    Battle_ReturnToOpenWorld,
    Battle_RestartGame,
    Battle_SlowDownGame,

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
    Battle_Skill_3_Player1,
    Battle_Skill_4_Player1,
    Battle_Skill_5_Player1,
    Battle_Skill_6_Player1,
    Battle_Skill_7_Player1,
    Battle_Skill_8_Player1,
    Battle_Skill_9_Player1,
    Battle_Skill_0_Player2,
    Battle_Skill_1_Player2,
    Battle_Skill_2_Player2,
    Battle_Skill_3_Player2,
    Battle_Skill_4_Player2,
    Battle_Skill_5_Player2,
    Battle_Skill_6_Player2,
    Battle_Skill_7_Player2,
    Battle_Skill_8_Player2,
    Battle_Skill_9_Player2,
    Battle_ToggleBattleTip,

    BATTLE_MAX_FLAG = 400,

    COMMON_MIN_FLAG = 500,

    Common_MouseLeft,
    Common_MouseRight,
    Common_MouseMiddle,

    Common_ReloadGame,
    Common_PauseGame,
    Common_ToggleUI,
    Common_ToggleDebugPanel,
    Common_DebugConsole,

    COMMON_MAX_FLAG = 600,

    MENU_MIN_FLAG = 700,

    Menu_MouseLeft,
    Menu_MouseRight,
    Menu_MouseMiddle,

    Menu_Cancel,
    Menu_Confirm,
    Menu_LeftSwitch,
    Menu_RightSwitch,
    Menu_SkillPreviewPanel,
    Menu_ExitMenuPanel,
    Menu_KeyBindPanel,

    MENU_MAX_FLAG = 800,
}