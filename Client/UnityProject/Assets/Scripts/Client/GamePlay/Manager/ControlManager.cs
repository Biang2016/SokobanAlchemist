using System.Collections.Generic;
using BiangLibrary.GameDataFormat.Grid;
using BiangLibrary.Singleton;
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
    public ButtonState[,] Battle_MoveButtons = new ButtonState[2, 4];
    public ButtonState[,] Battle_MoveButtons_LastFrame = new ButtonState[2, 4];

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

    public ButtonState[,] Battle_Skill = new ButtonState[2, 10];

    public ButtonState Battle_Skill_0_Player1 = new ButtonState() {ButtonName = ButtonNames.Battle_Skill_0_Player1};
    public ButtonState Battle_Skill_1_Player1 = new ButtonState() {ButtonName = ButtonNames.Battle_Skill_1_Player1};
    public ButtonState Battle_Skill_2_Player1 = new ButtonState() {ButtonName = ButtonNames.Battle_Skill_2_Player1};
    public ButtonState Battle_Skill_3_Player1 = new ButtonState() {ButtonName = ButtonNames.Battle_Skill_3_Player1};
    public ButtonState Battle_Skill_4_Player1 = new ButtonState() {ButtonName = ButtonNames.Battle_Skill_4_Player1};
    public ButtonState Battle_Skill_5_Player1 = new ButtonState() {ButtonName = ButtonNames.Battle_Skill_5_Player1};
    public ButtonState Battle_Skill_6_Player1 = new ButtonState() {ButtonName = ButtonNames.Battle_Skill_6_Player1};
    public ButtonState Battle_Skill_7_Player1 = new ButtonState() {ButtonName = ButtonNames.Battle_Skill_7_Player1};
    public ButtonState Battle_Skill_8_Player1 = new ButtonState() {ButtonName = ButtonNames.Battle_Skill_8_Player1};
    public ButtonState Battle_Skill_9_Player1 = new ButtonState() {ButtonName = ButtonNames.Battle_Skill_9_Player1};
    public ButtonState Battle_Skill_0_Player2 = new ButtonState() {ButtonName = ButtonNames.Battle_Skill_0_Player2};
    public ButtonState Battle_Skill_1_Player2 = new ButtonState() {ButtonName = ButtonNames.Battle_Skill_1_Player2};
    public ButtonState Battle_Skill_2_Player2 = new ButtonState() {ButtonName = ButtonNames.Battle_Skill_2_Player2};
    public ButtonState Battle_Skill_3_Player2 = new ButtonState() {ButtonName = ButtonNames.Battle_Skill_3_Player2};
    public ButtonState Battle_Skill_4_Player2 = new ButtonState() {ButtonName = ButtonNames.Battle_Skill_4_Player2};
    public ButtonState Battle_Skill_5_Player2 = new ButtonState() {ButtonName = ButtonNames.Battle_Skill_5_Player2};
    public ButtonState Battle_Skill_6_Player2 = new ButtonState() {ButtonName = ButtonNames.Battle_Skill_6_Player2};
    public ButtonState Battle_Skill_7_Player2 = new ButtonState() {ButtonName = ButtonNames.Battle_Skill_7_Player2};
    public ButtonState Battle_Skill_8_Player2 = new ButtonState() {ButtonName = ButtonNames.Battle_Skill_8_Player2};
    public ButtonState Battle_Skill_9_Player2 = new ButtonState() {ButtonName = ButtonNames.Battle_Skill_9_Player2};

    public ButtonState Battle_ToggleBattleTip = new ButtonState() {ButtonName = ButtonNames.Battle_ToggleBattleTip};

    public ButtonState Battle_LeftRotateCamera = new ButtonState() {ButtonName = ButtonNames.Battle_LeftRotateCamera};
    public ButtonState Battle_RightRotateCamera = new ButtonState() {ButtonName = ButtonNames.Battle_RightRotateCamera};

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
    public ButtonState Common_PauseGame = new ButtonState() {ButtonName = ButtonNames.Common_Pause};
    public ButtonState Common_ToggleUI = new ButtonState() {ButtonName = ButtonNames.Common_ToggleUI};
    public ButtonState Common_ToggleDebugButton = new ButtonState() {ButtonName = ButtonNames.Common_ToggleDebugButton};
    public ButtonState Common_InteractiveKey = new ButtonState() {ButtonName = ButtonNames.Common_InteractiveKey};

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
        BattleInputActions.Player1_Move.performed += context => Battle_Move[(int) PlayerNumber.Player1] = context.ReadValue<Vector2>();
        BattleInputActions.Player1_Move.canceled += context => Battle_Move[(int) PlayerNumber.Player1] = Vector2.zero;

        // 正常按键
        Battle_MoveButtons[(int) PlayerNumber.Player1, (int) GridPosR.Orientation.Up] = new ButtonState() {ButtonName = ButtonNames.Battle_Move_Up_Player1};
        Battle_MoveButtons[(int) PlayerNumber.Player1, (int) GridPosR.Orientation.Right] = new ButtonState() {ButtonName = ButtonNames.Battle_Move_Right_Player1};
        Battle_MoveButtons[(int) PlayerNumber.Player1, (int) GridPosR.Orientation.Down] = new ButtonState() {ButtonName = ButtonNames.Battle_Move_Down_Player1};
        Battle_MoveButtons[(int) PlayerNumber.Player1, (int) GridPosR.Orientation.Left] = new ButtonState() {ButtonName = ButtonNames.Battle_Move_Left_Player1};

        Battle_MoveButtons[(int) PlayerNumber.Player2, (int) GridPosR.Orientation.Up] = new ButtonState() {ButtonName = ButtonNames.Battle_Move_Up_Player2};
        Battle_MoveButtons[(int) PlayerNumber.Player2, (int) GridPosR.Orientation.Right] = new ButtonState() {ButtonName = ButtonNames.Battle_Move_Right_Player2};
        Battle_MoveButtons[(int) PlayerNumber.Player2, (int) GridPosR.Orientation.Down] = new ButtonState() {ButtonName = ButtonNames.Battle_Move_Down_Player2};
        Battle_MoveButtons[(int) PlayerNumber.Player2, (int) GridPosR.Orientation.Left] = new ButtonState() {ButtonName = ButtonNames.Battle_Move_Left_Player2};

        Battle_MoveButtons[(int) PlayerNumber.Player1, (int) GridPosR.Orientation.Up].GetStateCallbackFromContext_UpDownPress(BattleInputActions.Player1_Move_Up);
        Battle_MoveButtons[(int) PlayerNumber.Player1, (int) GridPosR.Orientation.Right].GetStateCallbackFromContext_UpDownPress(BattleInputActions.Player1_Move_Right);
        Battle_MoveButtons[(int) PlayerNumber.Player1, (int) GridPosR.Orientation.Down].GetStateCallbackFromContext_UpDownPress(BattleInputActions.Player1_Move_Down);
        Battle_MoveButtons[(int) PlayerNumber.Player1, (int) GridPosR.Orientation.Left].GetStateCallbackFromContext_UpDownPress(BattleInputActions.Player1_Move_Left);

        Battle_MoveButtons[(int) PlayerNumber.Player2, (int) GridPosR.Orientation.Up].GetStateCallbackFromContext_UpDownPress(BattleInputActions.Player2_Move_Up);
        Battle_MoveButtons[(int) PlayerNumber.Player2, (int) GridPosR.Orientation.Right].GetStateCallbackFromContext_UpDownPress(BattleInputActions.Player2_Move_Right);
        Battle_MoveButtons[(int) PlayerNumber.Player2, (int) GridPosR.Orientation.Down].GetStateCallbackFromContext_UpDownPress(BattleInputActions.Player2_Move_Down);
        Battle_MoveButtons[(int) PlayerNumber.Player2, (int) GridPosR.Orientation.Left].GetStateCallbackFromContext_UpDownPress(BattleInputActions.Player2_Move_Left);

        // 双击方向键
        Battle_MoveButtons[(int) PlayerNumber.Player1, (int) GridPosR.Orientation.Up].GetStateCallbackFromContext_MultipleClick(BattleInputActions.Player1_Move_Up_M);
        Battle_MoveButtons[(int) PlayerNumber.Player1, (int) GridPosR.Orientation.Right].GetStateCallbackFromContext_MultipleClick(BattleInputActions.Player1_Move_Right_M);
        Battle_MoveButtons[(int) PlayerNumber.Player1, (int) GridPosR.Orientation.Down].GetStateCallbackFromContext_MultipleClick(BattleInputActions.Player1_Move_Down_M);
        Battle_MoveButtons[(int) PlayerNumber.Player1, (int) GridPosR.Orientation.Left].GetStateCallbackFromContext_MultipleClick(BattleInputActions.Player1_Move_Left_M);

        Battle_MoveButtons[(int) PlayerNumber.Player2, (int) GridPosR.Orientation.Up].GetStateCallbackFromContext_MultipleClick(BattleInputActions.Player2_Move_Up_M);
        Battle_MoveButtons[(int) PlayerNumber.Player2, (int) GridPosR.Orientation.Right].GetStateCallbackFromContext_MultipleClick(BattleInputActions.Player2_Move_Right_M);
        Battle_MoveButtons[(int) PlayerNumber.Player2, (int) GridPosR.Orientation.Down].GetStateCallbackFromContext_MultipleClick(BattleInputActions.Player2_Move_Down_M);
        Battle_MoveButtons[(int) PlayerNumber.Player2, (int) GridPosR.Orientation.Left].GetStateCallbackFromContext_MultipleClick(BattleInputActions.Player2_Move_Left_M);

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
        Battle_Skill_2_Player1.GetStateCallbackFromContext_UpDownPress(BattleInputActions.Skill_2_Player1);
        Battle_Skill_3_Player1.GetStateCallbackFromContext_UpDownPress(BattleInputActions.Skill_3_Player1);
        Battle_Skill_4_Player1.GetStateCallbackFromContext_UpDownPress(BattleInputActions.Skill_4_Player1);
        Battle_Skill_5_Player1.GetStateCallbackFromContext_UpDownPress(BattleInputActions.Skill_5_Player1);
        Battle_Skill_6_Player1.GetStateCallbackFromContext_UpDownPress(BattleInputActions.Skill_6_Player1);
        Battle_Skill_7_Player1.GetStateCallbackFromContext_UpDownPress(BattleInputActions.Skill_7_Player1);
        Battle_Skill_8_Player1.GetStateCallbackFromContext_UpDownPress(BattleInputActions.Skill_8_Player1);
        Battle_Skill_9_Player1.GetStateCallbackFromContext_UpDownPress(BattleInputActions.Skill_9_Player1);
        Battle_Skill_0_Player2.GetStateCallbackFromContext_UpDownPress(BattleInputActions.Skill_0_Player2);
        Battle_Skill_1_Player2.GetStateCallbackFromContext_UpDownPress(BattleInputActions.Skill_1_Player2);
        Battle_Skill_2_Player2.GetStateCallbackFromContext_UpDownPress(BattleInputActions.Skill_2_Player2);
        Battle_Skill_3_Player2.GetStateCallbackFromContext_UpDownPress(BattleInputActions.Skill_3_Player2);
        Battle_Skill_4_Player2.GetStateCallbackFromContext_UpDownPress(BattleInputActions.Skill_4_Player2);
        Battle_Skill_5_Player2.GetStateCallbackFromContext_UpDownPress(BattleInputActions.Skill_5_Player2);
        Battle_Skill_6_Player2.GetStateCallbackFromContext_UpDownPress(BattleInputActions.Skill_6_Player2);
        Battle_Skill_7_Player2.GetStateCallbackFromContext_UpDownPress(BattleInputActions.Skill_7_Player2);
        Battle_Skill_8_Player2.GetStateCallbackFromContext_UpDownPress(BattleInputActions.Skill_8_Player2);
        Battle_Skill_9_Player2.GetStateCallbackFromContext_UpDownPress(BattleInputActions.Skill_9_Player2);

        Battle_Skill[(int) PlayerNumber.Player1, 0] = Battle_Skill_0_Player1;
        Battle_Skill[(int) PlayerNumber.Player1, 1] = Battle_Skill_1_Player1;
        Battle_Skill[(int) PlayerNumber.Player1, 2] = Battle_Skill_2_Player1;
        Battle_Skill[(int) PlayerNumber.Player1, 3] = Battle_Skill_3_Player1;
        Battle_Skill[(int) PlayerNumber.Player1, 4] = Battle_Skill_4_Player1;
        Battle_Skill[(int) PlayerNumber.Player1, 5] = Battle_Skill_5_Player1;
        Battle_Skill[(int) PlayerNumber.Player1, 6] = Battle_Skill_6_Player1;
        Battle_Skill[(int) PlayerNumber.Player1, 7] = Battle_Skill_7_Player1;
        Battle_Skill[(int) PlayerNumber.Player1, 8] = Battle_Skill_8_Player1;
        Battle_Skill[(int) PlayerNumber.Player1, 9] = Battle_Skill_9_Player1;

        Battle_Skill[(int) PlayerNumber.Player2, 0] = Battle_Skill_0_Player2;
        Battle_Skill[(int) PlayerNumber.Player2, 1] = Battle_Skill_1_Player2;
        Battle_Skill[(int) PlayerNumber.Player2, 2] = Battle_Skill_2_Player2;
        Battle_Skill[(int) PlayerNumber.Player2, 3] = Battle_Skill_3_Player2;
        Battle_Skill[(int) PlayerNumber.Player2, 4] = Battle_Skill_4_Player2;
        Battle_Skill[(int) PlayerNumber.Player2, 5] = Battle_Skill_5_Player2;
        Battle_Skill[(int) PlayerNumber.Player2, 6] = Battle_Skill_6_Player2;
        Battle_Skill[(int) PlayerNumber.Player2, 7] = Battle_Skill_7_Player2;
        Battle_Skill[(int) PlayerNumber.Player2, 8] = Battle_Skill_8_Player2;
        Battle_Skill[(int) PlayerNumber.Player2, 9] = Battle_Skill_9_Player2;

        Battle_ToggleBattleTip.GetStateCallbackFromContext_UpDownPress(BattleInputActions.ToggleBattleTip);
        Battle_LeftRotateCamera.GetStateCallbackFromContext_UpDownPress(BattleInputActions.LeftRotateCamera);
        Battle_RightRotateCamera.GetStateCallbackFromContext_UpDownPress(BattleInputActions.RightRotateCamera);

        Common_MouseLeft.GetStateCallbackFromContext_UpDownPress(CommonInputActions.MouseLeftClick);
        Common_MouseRight.GetStateCallbackFromContext_UpDownPress(CommonInputActions.MouseRightClick);
        Common_MouseMiddle.GetStateCallbackFromContext_UpDownPress(CommonInputActions.MouseMiddleClick);

        Common_Confirm.GetStateCallbackFromContext_UpDownPress(CommonInputActions.Confirm);
        Common_Debug.GetStateCallbackFromContext_UpDownPress(CommonInputActions.Debug);
        Common_Exit.GetStateCallbackFromContext_UpDownPress(CommonInputActions.Exit);
        Common_Tab.GetStateCallbackFromContext_UpDownPress(CommonInputActions.Tab);
        Common_RestartGame.GetStateCallbackFromContext_UpDownPress(CommonInputActions.RestartGame);
        Common_PauseGame.GetStateCallbackFromContext_UpDownPress(CommonInputActions.PauseGame);
        Common_ToggleUI.GetStateCallbackFromContext_UpDownPress(CommonInputActions.ToggleUI);
        Common_ToggleDebugButton.GetStateCallbackFromContext_UpDownPress(CommonInputActions.ToggleDebugButton);
        Common_InteractiveKey.GetStateCallbackFromContext_UpDownPress(CommonInputActions.InteractiveKey);

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

        foreach (KeyValuePair<ButtonNames, ButtonState> kv in ButtonStateDict)
        {
            if (kv.Value.Pressed) kv.Value.PressedDuration += Time.fixedDeltaTime;
        }

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