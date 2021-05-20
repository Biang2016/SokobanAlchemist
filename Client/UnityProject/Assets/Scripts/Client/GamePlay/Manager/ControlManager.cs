using System.Collections.Generic;
using BiangLibrary;
using BiangLibrary.GameDataFormat.Grid;
using BiangLibrary.Singleton;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class ControlManager : TSingletonBaseManager<ControlManager>
{
    #region ControlScheme

    public enum ControlScheme
    {
        KeyboardMouse,
        GamePad,
    }

    private ControlScheme currentControlScheme;

    public ControlScheme CurrentControlScheme
    {
        get { return currentControlScheme; }
        set
        {
            if (currentControlScheme != value)
            {
                OnControlSchemeChanged?.Invoke(currentControlScheme, value);
                currentControlScheme = value;
            }
        }
    }

    public UnityAction<ControlScheme, ControlScheme> OnControlSchemeChanged;

    #endregion

    private PlayerInput PlayerInput;
    private PlayerInput.CommonActions CommonActions;
    private PlayerInput.BattleActions BattleActions;
    private PlayerInput.MenuActions MenuActions;

    public Dictionary<ButtonNames, ButtonState> ButtonStateDict = new Dictionary<ButtonNames, ButtonState>();
    public Dictionary<ButtonNames, ButtonState> ButtonStateDict_LastFrame = new Dictionary<ButtonNames, ButtonState>();

    private Vector2 MousePosition => Mouse.current.position.ReadValue();
    private Vector2 MouseWheel => Mouse.current.scroll.ReadValue();

    #region Common

    public bool CommonActionsEnabled
    {
        get { return CommonActions.enabled; }
        set
        {
            if (value)
            {
                Debug.Log("CommonActions On");
                CommonActions.Enable();
            }
            else
            {
                Debug.Log("CommonActions Off");
                CommonActions.Disable();
            }
        }
    }

    public ButtonState Common_MouseLeft = new ButtonState() {ButtonName = ButtonNames.Common_MouseLeft};
    public ButtonState Common_MouseRight = new ButtonState() {ButtonName = ButtonNames.Common_MouseRight};
    public ButtonState Common_MouseMiddle = new ButtonState() {ButtonName = ButtonNames.Common_MouseMiddle};

    private Vector2 Last_Common_MousePosition = Vector2.zero;

    public Vector2 Common_MousePosition
    {
        get
        {
            if (CommonActions.enabled)
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
            if (CommonActions.enabled)
            {
                return MouseWheel;
            }
            else
            {
                return Vector2.zero;
            }
        }
    }

    public ButtonState Common_ReloadGame = new ButtonState() {ButtonName = ButtonNames.Common_ReloadGame};
    public ButtonState Common_PauseGame = new ButtonState() {ButtonName = ButtonNames.Common_PauseGame};
    public ButtonState Common_ToggleUI = new ButtonState() {ButtonName = ButtonNames.Common_ToggleUI};
    public ButtonState Common_ToggleDebugPanel = new ButtonState() {ButtonName = ButtonNames.Common_ToggleDebugPanel};
    public ButtonState Common_DebugConsole = new ButtonState() {ButtonName = ButtonNames.Common_DebugConsole};

    #endregion

    #region Battle

    public bool BattleActionEnabled
    {
        get { return BattleActions.enabled; }
        set
        {
            if (value)
            {
                Debug.Log("BattleActions On");
                BattleActions.Enable();
            }
            else
            {
                Debug.Log("BattleActions Off");
                BattleActions.Disable();
            }
        }
    }

    public ButtonState Battle_MouseLeft = new ButtonState() {ButtonName = ButtonNames.Battle_MouseLeft};
    public ButtonState Battle_MouseRight = new ButtonState() {ButtonName = ButtonNames.Battle_MouseRight};
    public ButtonState Battle_MouseMiddle = new ButtonState() {ButtonName = ButtonNames.Battle_MouseMiddle};

    public ButtonState Battle_LeftSwitch = new ButtonState() {ButtonName = ButtonNames.Battle_LeftSwitch};
    public ButtonState Battle_RightSwitch = new ButtonState() {ButtonName = ButtonNames.Battle_RightSwitch};
    public ButtonState Battle_InteractiveKey = new ButtonState() {ButtonName = ButtonNames.Battle_InteractiveKey};
    public ButtonState Battle_RestartGame = new ButtonState() {ButtonName = ButtonNames.Battle_RestartGame};
    public ButtonState Battle_SlowDownGame = new ButtonState() {ButtonName = ButtonNames.Battle_SlowDownGame};
    public ButtonState Battle_ReturnToOpenWorld = new ButtonState() {ButtonName = ButtonNames.Battle_ReturnToOpenWorld};
    public ButtonState Battle_ToggleBattleTip = new ButtonState() {ButtonName = ButtonNames.Battle_ToggleBattleTip};

    public Vector2[] Battle_Move = new Vector2[2];
    public ButtonState[,] Battle_MoveButtons = new ButtonState[2, 4];
    public ButtonState[,] Battle_MoveButtons_LastFrame = new ButtonState[2, 4];

    private Vector2 Last_Battle_MousePosition = Vector2.zero;

    public Vector2 Battle_MousePosition
    {
        get
        {
            if (BattleActions.enabled)
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
            if (BattleActions.enabled)
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

    #endregion

    #region Menu

    public bool MenuActionEnabled
    {
        get { return MenuActions.enabled; }
        set
        {
            if (value)
            {
                Debug.Log("MenuActions On");
                MenuActions.Enable();
            }
            else
            {
                Debug.Log("MenuActions Off");
                MenuActions.Disable();
            }
        }
    }

    public ButtonState Menu_MouseLeft = new ButtonState() {ButtonName = ButtonNames.Menu_MouseLeft};
    public ButtonState Menu_MouseRight = new ButtonState() {ButtonName = ButtonNames.Menu_MouseRight};
    public ButtonState Menu_MouseMiddle = new ButtonState() {ButtonName = ButtonNames.Menu_MouseMiddle};

    public ButtonState Menu_Confirm = new ButtonState() {ButtonName = ButtonNames.Menu_Confirm};
    public ButtonState Menu_Cancel = new ButtonState() {ButtonName = ButtonNames.Menu_Cancel};

    public ButtonState Menu_LeftSwitch = new ButtonState() {ButtonName = ButtonNames.Menu_LeftSwitch};
    public ButtonState Menu_RightSwitch = new ButtonState() {ButtonName = ButtonNames.Menu_RightSwitch};
    public ButtonState Menu_SkillPreviewPanel = new ButtonState() {ButtonName = ButtonNames.Menu_SkillPreviewPanel};
    public ButtonState Menu_ExitMenuPanel = new ButtonState() {ButtonName = ButtonNames.Menu_ExitMenuPanel};
    public ButtonState Menu_KeyBindPanel = new ButtonState() {ButtonName = ButtonNames.Menu_KeyBindPanel};

    #endregion

    public override void Awake()
    {
        ButtonStateDict.Clear();
        ButtonStateDict_LastFrame.Clear();

        InputSystem.onActionChange +=
            (obj, change) =>
            {
                if (change == InputActionChange.ActionPerformed)
                {
                    InputDevice lastDevice = ((InputAction) obj).activeControl.device;
                    string currentControlSchemeStr = lastDevice.displayName;
                    switch (currentControlSchemeStr)
                    {
                        case "Mouse":
                        {
                            CurrentControlScheme = ControlScheme.KeyboardMouse;
                            break;
                        }
                        case "Keyboard":
                        {
                            CurrentControlScheme = ControlScheme.KeyboardMouse;
                            break;
                        }
                        case "Xbox Controller":
                        {
                            CurrentControlScheme = ControlScheme.GamePad;
                            break;
                        }
                    }
                }
            };

        PlayerInput = new PlayerInput();
        CommonActions = new PlayerInput.CommonActions(PlayerInput);
        BattleActions = new PlayerInput.BattleActions(PlayerInput);
        MenuActions = new PlayerInput.MenuActions(PlayerInput);

        #region Common

        Common_MouseLeft.GetStateCallbackFromContext_UpDownPress(CommonActions.MouseLeftClick);
        Common_MouseRight.GetStateCallbackFromContext_UpDownPress(CommonActions.MouseRightClick);
        Common_MouseMiddle.GetStateCallbackFromContext_UpDownPress(CommonActions.MouseMiddleClick);

        Common_ReloadGame.GetStateCallbackFromContext_UpDownPress(CommonActions.ReloadGame);
        Common_PauseGame.GetStateCallbackFromContext_UpDownPress(CommonActions.PauseGame);
        Common_ToggleUI.GetStateCallbackFromContext_UpDownPress(CommonActions.ToggleUI);
        Common_ToggleDebugPanel.GetStateCallbackFromContext_UpDownPress(CommonActions.ToggleDebugPanel);
        Common_DebugConsole.GetStateCallbackFromContext_UpDownPress(CommonActions.DebugConsole);

        #endregion

        #region Battle

        Battle_MouseLeft.GetStateCallbackFromContext_UpDownPress(BattleActions.MouseLeftClick);
        Battle_MouseRight.GetStateCallbackFromContext_UpDownPress(BattleActions.MouseRightClick);
        Battle_MouseMiddle.GetStateCallbackFromContext_UpDownPress(BattleActions.MouseMiddleClick);

        Battle_InteractiveKey.GetStateCallbackFromContext_UpDownPress(BattleActions.InteractiveKey);
        Battle_LeftSwitch.GetStateCallbackFromContext_UpDownPress(BattleActions.LeftSwitch);
        Battle_RightSwitch.GetStateCallbackFromContext_UpDownPress(BattleActions.RightSwitch);
        Battle_ReturnToOpenWorld.GetStateCallbackFromContext_UpDownPress(BattleActions.ReturnToOpenWorld);
        Battle_RestartGame.GetStateCallbackFromContext_UpDownPress(BattleActions.RestartGame);
        Battle_SlowDownGame.GetStateCallbackFromContext_UpDownPress(BattleActions.SlowDownGame);
        Battle_ToggleBattleTip.GetStateCallbackFromContext_UpDownPress(BattleActions.ToggleBattleTip);

        // 移动组合向量
        BattleActions.Player1_Move.performed += context => Battle_Move[(int) PlayerNumber.Player1] = context.ReadValue<Vector2>();
        BattleActions.Player1_Move.canceled += context => Battle_Move[(int) PlayerNumber.Player1] = Vector2.zero;

        // 正常按键
        Battle_MoveButtons[(int) PlayerNumber.Player1, (int) GridPosR.Orientation.Up] = new ButtonState() {ButtonName = ButtonNames.Battle_Move_Up_Player1};
        Battle_MoveButtons[(int) PlayerNumber.Player1, (int) GridPosR.Orientation.Right] = new ButtonState() {ButtonName = ButtonNames.Battle_Move_Right_Player1};
        Battle_MoveButtons[(int) PlayerNumber.Player1, (int) GridPosR.Orientation.Down] = new ButtonState() {ButtonName = ButtonNames.Battle_Move_Down_Player1};
        Battle_MoveButtons[(int) PlayerNumber.Player1, (int) GridPosR.Orientation.Left] = new ButtonState() {ButtonName = ButtonNames.Battle_Move_Left_Player1};

        Battle_MoveButtons[(int) PlayerNumber.Player2, (int) GridPosR.Orientation.Up] = new ButtonState() {ButtonName = ButtonNames.Battle_Move_Up_Player2};
        Battle_MoveButtons[(int) PlayerNumber.Player2, (int) GridPosR.Orientation.Right] = new ButtonState() {ButtonName = ButtonNames.Battle_Move_Right_Player2};
        Battle_MoveButtons[(int) PlayerNumber.Player2, (int) GridPosR.Orientation.Down] = new ButtonState() {ButtonName = ButtonNames.Battle_Move_Down_Player2};
        Battle_MoveButtons[(int) PlayerNumber.Player2, (int) GridPosR.Orientation.Left] = new ButtonState() {ButtonName = ButtonNames.Battle_Move_Left_Player2};

        Battle_MoveButtons[(int) PlayerNumber.Player1, (int) GridPosR.Orientation.Up].GetStateCallbackFromContext_UpDownPress(BattleActions.Player1_Move_Up);
        Battle_MoveButtons[(int) PlayerNumber.Player1, (int) GridPosR.Orientation.Right].GetStateCallbackFromContext_UpDownPress(BattleActions.Player1_Move_Right);
        Battle_MoveButtons[(int) PlayerNumber.Player1, (int) GridPosR.Orientation.Down].GetStateCallbackFromContext_UpDownPress(BattleActions.Player1_Move_Down);
        Battle_MoveButtons[(int) PlayerNumber.Player1, (int) GridPosR.Orientation.Left].GetStateCallbackFromContext_UpDownPress(BattleActions.Player1_Move_Left);

        Battle_MoveButtons[(int) PlayerNumber.Player2, (int) GridPosR.Orientation.Up].GetStateCallbackFromContext_UpDownPress(BattleActions.Player2_Move_Up);
        Battle_MoveButtons[(int) PlayerNumber.Player2, (int) GridPosR.Orientation.Right].GetStateCallbackFromContext_UpDownPress(BattleActions.Player2_Move_Right);
        Battle_MoveButtons[(int) PlayerNumber.Player2, (int) GridPosR.Orientation.Down].GetStateCallbackFromContext_UpDownPress(BattleActions.Player2_Move_Down);
        Battle_MoveButtons[(int) PlayerNumber.Player2, (int) GridPosR.Orientation.Left].GetStateCallbackFromContext_UpDownPress(BattleActions.Player2_Move_Left);

        // 双击方向键
        Battle_MoveButtons[(int) PlayerNumber.Player1, (int) GridPosR.Orientation.Up].GetStateCallbackFromContext_MultipleClick(BattleActions.Player1_Move_Up_M);
        Battle_MoveButtons[(int) PlayerNumber.Player1, (int) GridPosR.Orientation.Right].GetStateCallbackFromContext_MultipleClick(BattleActions.Player1_Move_Right_M);
        Battle_MoveButtons[(int) PlayerNumber.Player1, (int) GridPosR.Orientation.Down].GetStateCallbackFromContext_MultipleClick(BattleActions.Player1_Move_Down_M);
        Battle_MoveButtons[(int) PlayerNumber.Player1, (int) GridPosR.Orientation.Left].GetStateCallbackFromContext_MultipleClick(BattleActions.Player1_Move_Left_M);

        Battle_MoveButtons[(int) PlayerNumber.Player2, (int) GridPosR.Orientation.Up].GetStateCallbackFromContext_MultipleClick(BattleActions.Player2_Move_Up_M);
        Battle_MoveButtons[(int) PlayerNumber.Player2, (int) GridPosR.Orientation.Right].GetStateCallbackFromContext_MultipleClick(BattleActions.Player2_Move_Right_M);
        Battle_MoveButtons[(int) PlayerNumber.Player2, (int) GridPosR.Orientation.Down].GetStateCallbackFromContext_MultipleClick(BattleActions.Player2_Move_Down_M);
        Battle_MoveButtons[(int) PlayerNumber.Player2, (int) GridPosR.Orientation.Left].GetStateCallbackFromContext_MultipleClick(BattleActions.Player2_Move_Left_M);

        Battle_MoveButtons_LastFrame[(int) PlayerNumber.Player1, (int) GridPosR.Orientation.Up] = ButtonStateDict_LastFrame[ButtonNames.Battle_Move_Up_Player1];
        Battle_MoveButtons_LastFrame[(int) PlayerNumber.Player1, (int) GridPosR.Orientation.Right] = ButtonStateDict_LastFrame[ButtonNames.Battle_Move_Right_Player1];
        Battle_MoveButtons_LastFrame[(int) PlayerNumber.Player1, (int) GridPosR.Orientation.Down] = ButtonStateDict_LastFrame[ButtonNames.Battle_Move_Down_Player1];
        Battle_MoveButtons_LastFrame[(int) PlayerNumber.Player1, (int) GridPosR.Orientation.Left] = ButtonStateDict_LastFrame[ButtonNames.Battle_Move_Left_Player1];

        Battle_MoveButtons_LastFrame[(int) PlayerNumber.Player2, (int) GridPosR.Orientation.Up] = ButtonStateDict_LastFrame[ButtonNames.Battle_Move_Up_Player2];
        Battle_MoveButtons_LastFrame[(int) PlayerNumber.Player2, (int) GridPosR.Orientation.Right] = ButtonStateDict_LastFrame[ButtonNames.Battle_Move_Right_Player2];
        Battle_MoveButtons_LastFrame[(int) PlayerNumber.Player2, (int) GridPosR.Orientation.Down] = ButtonStateDict_LastFrame[ButtonNames.Battle_Move_Down_Player2];
        Battle_MoveButtons_LastFrame[(int) PlayerNumber.Player2, (int) GridPosR.Orientation.Left] = ButtonStateDict_LastFrame[ButtonNames.Battle_Move_Left_Player2];

        // 技能
        Battle_Skill_0_Player1.GetStateCallbackFromContext_UpDownPress(BattleActions.Skill_0_Player1);
        Battle_Skill_1_Player1.GetStateCallbackFromContext_UpDownPress(BattleActions.Skill_1_Player1);
        Battle_Skill_2_Player1.GetStateCallbackFromContext_UpDownPress(BattleActions.Skill_2_Player1);
        Battle_Skill_3_Player1.GetStateCallbackFromContext_UpDownPress(BattleActions.Skill_3_Player1);
        Battle_Skill_4_Player1.GetStateCallbackFromContext_UpDownPress(BattleActions.Skill_4_Player1);
        Battle_Skill_5_Player1.GetStateCallbackFromContext_UpDownPress(BattleActions.Skill_5_Player1);
        Battle_Skill_6_Player1.GetStateCallbackFromContext_UpDownPress(BattleActions.Skill_6_Player1);
        Battle_Skill_7_Player1.GetStateCallbackFromContext_UpDownPress(BattleActions.Skill_7_Player1);
        Battle_Skill_8_Player1.GetStateCallbackFromContext_UpDownPress(BattleActions.Skill_8_Player1);
        Battle_Skill_9_Player1.GetStateCallbackFromContext_UpDownPress(BattleActions.Skill_9_Player1);
        Battle_Skill_0_Player2.GetStateCallbackFromContext_UpDownPress(BattleActions.Skill_0_Player2);
        Battle_Skill_1_Player2.GetStateCallbackFromContext_UpDownPress(BattleActions.Skill_1_Player2);
        Battle_Skill_2_Player2.GetStateCallbackFromContext_UpDownPress(BattleActions.Skill_2_Player2);
        Battle_Skill_3_Player2.GetStateCallbackFromContext_UpDownPress(BattleActions.Skill_3_Player2);
        Battle_Skill_4_Player2.GetStateCallbackFromContext_UpDownPress(BattleActions.Skill_4_Player2);
        Battle_Skill_5_Player2.GetStateCallbackFromContext_UpDownPress(BattleActions.Skill_5_Player2);
        Battle_Skill_6_Player2.GetStateCallbackFromContext_UpDownPress(BattleActions.Skill_6_Player2);
        Battle_Skill_7_Player2.GetStateCallbackFromContext_UpDownPress(BattleActions.Skill_7_Player2);
        Battle_Skill_8_Player2.GetStateCallbackFromContext_UpDownPress(BattleActions.Skill_8_Player2);
        Battle_Skill_9_Player2.GetStateCallbackFromContext_UpDownPress(BattleActions.Skill_9_Player2);

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

        #endregion

        #region Menu

        Menu_MouseLeft.GetStateCallbackFromContext_UpDownPress(MenuActions.MouseLeftClick);
        Menu_MouseRight.GetStateCallbackFromContext_UpDownPress(MenuActions.MouseRightClick);
        Menu_MouseMiddle.GetStateCallbackFromContext_UpDownPress(MenuActions.MouseMiddleClick);

        Menu_Confirm.GetStateCallbackFromContext_UpDownPress(MenuActions.Confirm);
        Menu_Cancel.GetStateCallbackFromContext_UpDownPress(MenuActions.Cancel);
        Menu_LeftSwitch.GetStateCallbackFromContext_UpDownPress(MenuActions.LeftSwitch);
        Menu_RightSwitch.GetStateCallbackFromContext_UpDownPress(MenuActions.RightSwitch);
        Menu_SkillPreviewPanel.GetStateCallbackFromContext_UpDownPress(MenuActions.SkillPreviewPanel);
        Menu_ExitMenuPanel.GetStateCallbackFromContext_UpDownPress(MenuActions.ExitMenuPanel);
        Menu_KeyBindPanel.GetStateCallbackFromContext_UpDownPress(MenuActions.KeyBindPanel);

        #endregion

        PlayerInput.Enable();
        CommonActions.Enable();
        BattleActions.Enable();
        MenuActions.Enable();

        InitControlDescDict();
    }

    public override void Update(float deltaTime)
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
            if (kv.Value.Pressed) kv.Value.PressedDuration += Time.deltaTime;
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

        base.Update(deltaTime);
    }

    public override void LateUpdate(float deltaTime)
    {
        base.LateUpdate(deltaTime);
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

    #region Control Description

    private Dictionary<ControlScheme, Dictionary<ButtonNames, string>> ControlDescDict = new Dictionary<ControlScheme, Dictionary<ButtonNames, string>>();

    private void InitControlDescDict()
    {
        ControlDescDict.Clear();

        #region KeyboardMouse

        Dictionary<ButtonNames, string> keyboardMouseDict = new Dictionary<ButtonNames, string>();
        ControlDescDict.Add(ControlScheme.KeyboardMouse, keyboardMouseDict);
        keyboardMouseDict.Add(ButtonNames.Fake_CharacterMove, "WASD");
        keyboardMouseDict.Add(ButtonNames.Battle_Skill_0_Player1, "Space");
        keyboardMouseDict.Add(ButtonNames.Battle_Skill_1_Player1, "Shift");
        keyboardMouseDict.Add(ButtonNames.Battle_Skill_2_Player1, "H");
        keyboardMouseDict.Add(ButtonNames.Battle_Skill_3_Player1, "J");
        keyboardMouseDict.Add(ButtonNames.Battle_Skill_4_Player1, "K");
        keyboardMouseDict.Add(ButtonNames.Battle_Skill_5_Player1, "L");
        keyboardMouseDict.Add(ButtonNames.Battle_LeftSwitch, "Q");
        keyboardMouseDict.Add(ButtonNames.Battle_RightSwitch, "E");
        keyboardMouseDict.Add(ButtonNames.Battle_InteractiveKey, "F");
        keyboardMouseDict.Add(ButtonNames.Menu_ExitMenuPanel, "ESC");
        keyboardMouseDict.Add(ButtonNames.Menu_KeyBindPanel, "TAB");
        keyboardMouseDict.Add(ButtonNames.Menu_Cancel, "ESC");
        keyboardMouseDict.Add(ButtonNames.Menu_Confirm, "Space");
        keyboardMouseDict.Add(ButtonNames.Menu_LeftSwitch, "Q");
        keyboardMouseDict.Add(ButtonNames.Menu_RightSwitch, "E");

        #endregion

        #region GamePad (XBox)

        Dictionary<ButtonNames, string> gamePadDict = new Dictionary<ButtonNames, string>();
        ControlDescDict.Add(ControlScheme.GamePad, gamePadDict);
        gamePadDict.Add(ButtonNames.Fake_CharacterMove, "LStick");
        gamePadDict.Add(ButtonNames.Battle_Skill_0_Player1, "LT");
        gamePadDict.Add(ButtonNames.Battle_Skill_1_Player1, "RT");
        gamePadDict.Add(ButtonNames.Battle_Skill_2_Player1, "");
        gamePadDict.Add(ButtonNames.Battle_Skill_3_Player1, "X");
        gamePadDict.Add(ButtonNames.Battle_Skill_4_Player1, "Y");
        gamePadDict.Add(ButtonNames.Battle_Skill_5_Player1, "B");
        gamePadDict.Add(ButtonNames.Battle_LeftSwitch, "LB");
        gamePadDict.Add(ButtonNames.Battle_RightSwitch, "RB");
        gamePadDict.Add(ButtonNames.Battle_InteractiveKey, "A");
        gamePadDict.Add(ButtonNames.Menu_ExitMenuPanel, "Start");
        gamePadDict.Add(ButtonNames.Menu_KeyBindPanel, "Select");
        gamePadDict.Add(ButtonNames.Menu_Cancel, "B");
        gamePadDict.Add(ButtonNames.Menu_Confirm, "A");
        gamePadDict.Add(ButtonNames.Menu_LeftSwitch, "LB");
        gamePadDict.Add(ButtonNames.Menu_RightSwitch, "RB");

        #endregion
    }

    public string GetControlDescText(ButtonNames buttonName, bool withColor = true)
    {
        if (ControlDescDict[CurrentControlScheme].TryGetValue(buttonName, out string desc))
        {
            if (withColor)
            {
                string colored_Desc = CommonUtils.AddHighLightColorToText(desc, "#f1ff52");
                return colored_Desc;
            }
            else
            {
                return desc;
            }
        }

        return buttonName.ToString();
    }

    public Dictionary<ButtonNames, string> GetCurrentButtonNamesForTips()
    {
        return ControlDescDict[CurrentControlScheme];
    }

    #endregion

    public override void ShutDown()
    {
        base.ShutDown();
        OnControlSchemeChanged = null;
    }
}