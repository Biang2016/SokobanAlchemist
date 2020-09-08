using System.Collections.Generic;
using BiangStudio.GameDataFormat.Grid;
using BiangStudio.Singleton;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControlManager : TSingletonBaseManager<ControlManager>
{
    private PlayerInput PlayerInput;
    private PlayerInput.CommonActions CommonInputActions;
    private PlayerInput.BattleInputActions BattleInputActions;

    public Dictionary<ButtonNames, ButtonState> ButtonStateDict = new Dictionary<ButtonNames, ButtonState>();
    public Dictionary<ButtonNames, ButtonState> ButtonStateDict_LastFrame = new Dictionary<ButtonNames, ButtonState>();

    #region Battle

    public bool BattleInputActionEnabled => BattleInputActions.enabled;

    public ButtonState Battle_MouseLeft = new ButtonState() {ButtonName = ButtonNames.Battle_MouseLeft};
    public ButtonState Battle_MouseRight = new ButtonState() {ButtonName = ButtonNames.Battle_MouseRight};
    public ButtonState Battle_MouseMiddle = new ButtonState() {ButtonName = ButtonNames.Battle_MouseMiddle};

    public Vector2[] Battle_Move = new Vector2[2];
    public Vector2 Player2_RightStick = new Vector2();
    public ButtonState[,] Battle_MoveButtons = new ButtonState[2,4];
    public ButtonState[,] Battle_MoveButtons_LastFrame = new ButtonState[2,4];

    private Vector2 Last_Battle_MousePosition = Vector2.zero;

    public Vector2 Battle_MousePosition
    {
        get
        {
            if (BattleInputActions.enabled)
            {
                Last_Battle_MousePosition = MousePosition;
                return MousePosition;
            }
            else
            {
                return Last_Battle_MousePosition;
            }
        }
    }

    public Vector2 Battle_MouseWheel
    {
        get
        {
            if (BattleInputActions.enabled)
            {
                return MouseWheel;
            }
            else
            {
                return Vector2.zero;
            }
        }
    }

    public ButtonState[,] Battle_Skill = new ButtonState[2, 2];

    public ButtonState Battle_Skill_0_Player1 = new ButtonState() {ButtonName = ButtonNames.Battle_Skill_0_Player1};
    public ButtonState Battle_Skill_1_Player1 = new ButtonState() {ButtonName = ButtonNames.Battle_Skill_1_Player1};
    public ButtonState Battle_Skill_0_Player2 = new ButtonState() {ButtonName = ButtonNames.Battle_Skill_0_Player2};
    public ButtonState Battle_Skill_1_Player2 = new ButtonState() {ButtonName = ButtonNames.Battle_Skill_1_Player2};

    public ButtonState Battle_ToggleBattleTip = new ButtonState() {ButtonName = ButtonNames.Battle_ToggleBattleTip};

    #endregion

    #region Common

    public bool CommonInputActionsEnabled => CommonInputActions.enabled;

    public ButtonState Common_MouseLeft = new ButtonState() {ButtonName = ButtonNames.Common_MouseLeft};
    public ButtonState Common_MouseRight = new ButtonState() {ButtonName = ButtonNames.Common_MouseRight};
    public ButtonState Common_MouseMiddle = new ButtonState() {ButtonName = ButtonNames.Common_MouseMiddle};

    private Vector2 Last_Common_MousePosition = Vector2.zero;

    public Vector2 Common_MousePosition
    {
        get
        {
            if (CommonInputActions.enabled)
            {
                Last_Common_MousePosition = MousePosition;
                return MousePosition;
            }
            else
            {
                return Last_Common_MousePosition;
            }
        }
    }

    public Vector2 Common_MouseWheel
    {
        get
        {
            if (CommonInputActions.enabled)
            {
                return MouseWheel;
            }
            else
            {
                return Vector2.zero;
            }
        }
    }

    public ButtonState Common_Confirm = new ButtonState() {ButtonName = ButtonNames.Common_Confirm};
    public ButtonState Common_Debug = new ButtonState() {ButtonName = ButtonNames.Common_Debug};
    public ButtonState Common_Exit = new ButtonState() {ButtonName = ButtonNames.Common_Exit};
    public ButtonState Common_Tab = new ButtonState() {ButtonName = ButtonNames.Common_Tab};
    public ButtonState Common_RestartGame = new ButtonState() {ButtonName = ButtonNames.Common_RestartGame};

    #endregion

    private Vector2 MousePosition => Mouse.current.position.ReadValue();
    private Vector2 MouseWheel => Mouse.current.scroll.ReadValue();

    public override void Awake()
    {
        ButtonStateDict.Clear();
        ButtonStateDict_LastFrame.Clear();

        PlayerInput = new PlayerInput();
        CommonInputActions = new PlayerInput.CommonActions(PlayerInput);
        BattleInputActions = new PlayerInput.BattleInputActions(PlayerInput);

        Battle_MouseLeft.GetStateCallbackFromContext_UpDownPress(BattleInputActions.MouseLeftClick);
        Battle_MouseRight.GetStateCallbackFromContext_UpDownPress(BattleInputActions.MouseRightClick);
        Battle_MouseMiddle.GetStateCallbackFromContext_UpDownPress(BattleInputActions.MouseMiddleClick);

        // 移动组合向量
        BattleInputActions.Player1_Move.performed += context => Battle_Move[(int)PlayerNumber.Player1] = context.ReadValue<Vector2>();
        BattleInputActions.Player1_Move.canceled += context => Battle_Move[(int)PlayerNumber.Player1] = Vector2.zero;

        BattleInputActions.Player2_Move.performed += context => Battle_Move[(int)PlayerNumber.Player2] = context.ReadValue<Vector2>();
        BattleInputActions.Player2_Move.canceled += context => Battle_Move[(int)PlayerNumber.Player2] = Vector2.zero;

        BattleInputActions.Player2_RightStick.performed += context => Player2_RightStick = context.ReadValue<Vector2>();
        BattleInputActions.Player2_RightStick.canceled += context => Player2_RightStick = Vector2.zero;

        // 正常按键
        Battle_MoveButtons[(int)PlayerNumber.Player1, (int)GridPosR.Orientation.Up] = new ButtonState() { ButtonName = ButtonNames.Battle_Move_Up_Player1 };
        Battle_MoveButtons[(int)PlayerNumber.Player1, (int)GridPosR.Orientation.Right] = new ButtonState() { ButtonName = ButtonNames.Battle_Move_Right_Player1 };
        Battle_MoveButtons[(int)PlayerNumber.Player1, (int)GridPosR.Orientation.Down] = new ButtonState() { ButtonName = ButtonNames.Battle_Move_Down_Player1 };
        Battle_MoveButtons[(int)PlayerNumber.Player1, (int)GridPosR.Orientation.Left] = new ButtonState() { ButtonName = ButtonNames.Battle_Move_Left_Player1 };

        Battle_MoveButtons[(int)PlayerNumber.Player2, (int)GridPosR.Orientation.Up] = new ButtonState() { ButtonName = ButtonNames.Battle_Move_Up_Player2 };
        Battle_MoveButtons[(int)PlayerNumber.Player2, (int)GridPosR.Orientation.Right] = new ButtonState() { ButtonName = ButtonNames.Battle_Move_Right_Player2 };
        Battle_MoveButtons[(int)PlayerNumber.Player2, (int)GridPosR.Orientation.Down] = new ButtonState() { ButtonName = ButtonNames.Battle_Move_Down_Player2 };
        Battle_MoveButtons[(int)PlayerNumber.Player2, (int)GridPosR.Orientation.Left] = new ButtonState() { ButtonName = ButtonNames.Battle_Move_Left_Player2 };

        Battle_MoveButtons[(int)PlayerNumber.Player1, (int)GridPosR.Orientation.Up].GetStateCallbackFromContext_UpDownPress(BattleInputActions.Player1_Move_Up);
        Battle_MoveButtons[(int)PlayerNumber.Player1, (int)GridPosR.Orientation.Right].GetStateCallbackFromContext_UpDownPress(BattleInputActions.Player1_Move_Right);
        Battle_MoveButtons[(int)PlayerNumber.Player1, (int)GridPosR.Orientation.Down].GetStateCallbackFromContext_UpDownPress(BattleInputActions.Player1_Move_Down);
        Battle_MoveButtons[(int)PlayerNumber.Player1, (int)GridPosR.Orientation.Left].GetStateCallbackFromContext_UpDownPress(BattleInputActions.Player1_Move_Left);

        Battle_MoveButtons[(int)PlayerNumber.Player2, (int)GridPosR.Orientation.Up].GetStateCallbackFromContext_UpDownPress(BattleInputActions.Player2_Move_Up);
        Battle_MoveButtons[(int)PlayerNumber.Player2, (int)GridPosR.Orientation.Right].GetStateCallbackFromContext_UpDownPress(BattleInputActions.Player2_Move_Right);
        Battle_MoveButtons[(int)PlayerNumber.Player2, (int)GridPosR.Orientation.Down].GetStateCallbackFromContext_UpDownPress(BattleInputActions.Player2_Move_Down);
        Battle_MoveButtons[(int)PlayerNumber.Player2, (int)GridPosR.Orientation.Left].GetStateCallbackFromContext_UpDownPress(BattleInputActions.Player2_Move_Left);

        // 双击方向键
        Battle_MoveButtons[(int)PlayerNumber.Player1, (int)GridPosR.Orientation.Up].GetStateCallbackFromContext_MultipleClick(BattleInputActions.Player1_Move_Up_M);
        Battle_MoveButtons[(int)PlayerNumber.Player1, (int)GridPosR.Orientation.Right].GetStateCallbackFromContext_MultipleClick(BattleInputActions.Player1_Move_Right_M);
        Battle_MoveButtons[(int)PlayerNumber.Player1, (int)GridPosR.Orientation.Down].GetStateCallbackFromContext_MultipleClick(BattleInputActions.Player1_Move_Down_M);
        Battle_MoveButtons[(int)PlayerNumber.Player1, (int)GridPosR.Orientation.Left].GetStateCallbackFromContext_MultipleClick(BattleInputActions.Player1_Move_Left_M);

        Battle_MoveButtons[(int)PlayerNumber.Player2, (int)GridPosR.Orientation.Up].GetStateCallbackFromContext_MultipleClick(BattleInputActions.Player2_Move_Up_M);
        Battle_MoveButtons[(int)PlayerNumber.Player2, (int)GridPosR.Orientation.Right].GetStateCallbackFromContext_MultipleClick(BattleInputActions.Player2_Move_Right_M);
        Battle_MoveButtons[(int)PlayerNumber.Player2, (int)GridPosR.Orientation.Down].GetStateCallbackFromContext_MultipleClick(BattleInputActions.Player2_Move_Down_M);
        Battle_MoveButtons[(int)PlayerNumber.Player2, (int)GridPosR.Orientation.Left].GetStateCallbackFromContext_MultipleClick(BattleInputActions.Player2_Move_Left_M);

        Battle_MoveButtons_LastFrame[(int) PlayerNumber.Player1, (int) GridPosR.Orientation.Up] = ButtonStateDict_LastFrame[ButtonNames.Battle_Move_Up_Player1];
        Battle_MoveButtons_LastFrame[(int) PlayerNumber.Player1, (int) GridPosR.Orientation.Right] = ButtonStateDict_LastFrame[ButtonNames.Battle_Move_Right_Player1];
        Battle_MoveButtons_LastFrame[(int) PlayerNumber.Player1, (int) GridPosR.Orientation.Down] = ButtonStateDict_LastFrame[ButtonNames.Battle_Move_Down_Player1];
        Battle_MoveButtons_LastFrame[(int) PlayerNumber.Player1, (int) GridPosR.Orientation.Left] = ButtonStateDict_LastFrame[ButtonNames.Battle_Move_Left_Player1];

        Battle_MoveButtons_LastFrame[(int) PlayerNumber.Player2, (int) GridPosR.Orientation.Up] = ButtonStateDict_LastFrame[ButtonNames.Battle_Move_Up_Player2];
        Battle_MoveButtons_LastFrame[(int) PlayerNumber.Player2, (int) GridPosR.Orientation.Right] = ButtonStateDict_LastFrame[ButtonNames.Battle_Move_Right_Player2];
        Battle_MoveButtons_LastFrame[(int) PlayerNumber.Player2, (int) GridPosR.Orientation.Down] = ButtonStateDict_LastFrame[ButtonNames.Battle_Move_Down_Player2];
        Battle_MoveButtons_LastFrame[(int) PlayerNumber.Player2, (int) GridPosR.Orientation.Left] = ButtonStateDict_LastFrame[ButtonNames.Battle_Move_Left_Player2];

        // 技能
        Battle_Skill_0_Player1.GetStateCallbackFromContext_UpDownPress(BattleInputActions.Skill_0_Player1);
        Battle_Skill_1_Player1.GetStateCallbackFromContext_UpDownPress(BattleInputActions.Skill_1_Player1);
        Battle_Skill_0_Player2.GetStateCallbackFromContext_UpDownPress(BattleInputActions.Skill_0_Player2);
        Battle_Skill_1_Player2.GetStateCallbackFromContext_UpDownPress(BattleInputActions.Skill_1_Player2);

        Battle_Skill[(int) PlayerNumber.Player1, 0] = Battle_Skill_0_Player1;
        Battle_Skill[(int) PlayerNumber.Player1, 1] = Battle_Skill_1_Player1;
        Battle_Skill[(int) PlayerNumber.Player2, 0] = Battle_Skill_0_Player2;
        Battle_Skill[(int) PlayerNumber.Player2, 1] = Battle_Skill_1_Player2;

        Battle_ToggleBattleTip.GetStateCallbackFromContext_UpDownPress(BattleInputActions.ToggleBattleTip);

        Common_MouseLeft.GetStateCallbackFromContext_UpDownPress(CommonInputActions.MouseLeftClick);
        Common_MouseRight.GetStateCallbackFromContext_UpDownPress(CommonInputActions.MouseRightClick);
        Common_MouseMiddle.GetStateCallbackFromContext_UpDownPress(CommonInputActions.MouseMiddleClick);

        Common_Confirm.GetStateCallbackFromContext_UpDownPress(CommonInputActions.Confirm);
        Common_Debug.GetStateCallbackFromContext_UpDownPress(CommonInputActions.Debug);
        Common_Exit.GetStateCallbackFromContext_UpDownPress(CommonInputActions.Exit);
        Common_Tab.GetStateCallbackFromContext_UpDownPress(CommonInputActions.Tab);
        Common_RestartGame.GetStateCallbackFromContext_UpDownPress(CommonInputActions.RestartGame);

        PlayerInput.Enable();
        CommonInputActions.Enable();
        BattleInputActions.Enable();
    }

    public override void FixedUpdate(float deltaTime)
    {
        foreach (KeyValuePair<ButtonNames, ButtonState> kv in ButtonStateDict_LastFrame)
        {
            ButtonStateDict[kv.Key].ApplyTo(kv.Value);
        }

        foreach (KeyValuePair<ButtonNames, ButtonState> kv in ButtonStateDict)
        {
            kv.Value.Reset();
        }

        InputSystem.Update();
        if (false)
        {
            foreach (KeyValuePair<ButtonNames, ButtonState> kv in ButtonStateDict)
            {
                string input = kv.Value.ToString();
                if (!string.IsNullOrWhiteSpace(input))
                {
                    Debug.Log(input);
                }
            }
        }

        base.FixedUpdate(deltaTime);
    }

    public override void LateUpdate(float deltaTime)
    {
        base.LateUpdate(deltaTime);
    }

    public void EnableBattleInputActions(bool enable)
    {
        if (enable)
        {
            BattleInputActions.Enable();
        }
        else
        {
            BattleInputActions.Disable();
        }
    }

    public bool CheckButtonAction(ButtonState buttonState)
    {
        if (ButtonStateDict.TryGetValue(buttonState.ButtonName, out ButtonState myButtonState))
        {
            return (buttonState.Down && myButtonState.Down) || (buttonState.Up && myButtonState.Up) || (buttonState.Pressed && myButtonState.Pressed);
        }
        else
        {
            return false;
        }
    }

    public bool CheckButtonAction_Instantaneously(ButtonState buttonState)
    {
        if (ButtonStateDict.TryGetValue(buttonState.ButtonName, out ButtonState myButtonState))
        {
            return (buttonState.Down && myButtonState.Down) || (buttonState.Up && myButtonState.Up);
        }
        else
        {
            return false;
        }
    }

    public bool CheckButtonAction_Continuously(ButtonState buttonState)
    {
        if (ButtonStateDict.TryGetValue(buttonState.ButtonName, out ButtonState myButtonState))
        {
            return (buttonState.Pressed && myButtonState.Pressed);
        }
        else
        {
            return false;
        }
    }

    public bool CheckButtonAction(ButtonNames buttonName, bool down, bool up, bool pressed)
    {
        if (ButtonStateDict.TryGetValue(buttonName, out ButtonState myButtonState))
        {
            return (down && myButtonState.Down) || (up && myButtonState.Up) || (pressed && myButtonState.Pressed);
        }
        else
        {
            return false;
        }
    }
}