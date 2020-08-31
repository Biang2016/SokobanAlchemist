using System.Collections.Generic;
using BiangStudio.Singleton;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControlManager : TSingletonBaseManager<ControlManager>
{
    private PlayerInput PlayerInput;
    private PlayerInput.CommonActions CommonInputActions;
    private PlayerInput.BattleInputActions BattleInputActions;
    private PlayerInput.BuildingInputActions BuildingInputActions;

    public Dictionary<ButtonNames, ButtonState> ButtonStateDict = new Dictionary<ButtonNames, ButtonState>();

    #region Building

    public bool BuildingInputActionEnabled => BuildingInputActions.enabled;

    public ButtonState Building_MouseLeft = new ButtonState() {ButtonName = ButtonNames.Building_MouseLeft};
    public ButtonState Building_MouseRight = new ButtonState() {ButtonName = ButtonNames.Building_MouseRight};
    public ButtonState Building_MouseMiddle = new ButtonState() {ButtonName = ButtonNames.Building_MouseMiddle};

    public Vector2 Building_Move;

    private Vector2 Last_Building_MousePosition = Vector2.zero;

    public Vector2 Building_MousePosition
    {
        get
        {
            if (BuildingInputActions.enabled)
            {
                Last_Building_MousePosition = MousePosition;
                return MousePosition;
            }
            else
            {
                return Last_Building_MousePosition;
            }
        }
    }

    public Vector2 Building_MouseWheel
    {
        get
        {
            if (BuildingInputActions.enabled)
            {
                return MouseWheel;
            }
            else
            {
                return Vector2.zero;
            }
        }
    }

    public ButtonState Building_RotateItem = new ButtonState() {ButtonName = ButtonNames.Building_RotateItem};
    public ButtonState Building_ToggleBackpack = new ButtonState() {ButtonName = ButtonNames.Building_ToggleBackpack};
    public ButtonState Building_ToggleWireLines = new ButtonState() {ButtonName = ButtonNames.Building_ToggleWireLines};

    #endregion

    #region Battle

    public bool BattleInputActionEnabled => BattleInputActions.enabled;

    public ButtonState Battle_MouseLeft = new ButtonState() {ButtonName = ButtonNames.Battle_MouseLeft};
    public ButtonState Battle_MouseRight = new ButtonState() {ButtonName = ButtonNames.Battle_MouseRight};
    public ButtonState Battle_MouseMiddle = new ButtonState() {ButtonName = ButtonNames.Battle_MouseMiddle};

    public Vector2[] Battle_Move = new Vector2[2];

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
        PlayerInput = new PlayerInput();
        CommonInputActions = new PlayerInput.CommonActions(PlayerInput);
        BattleInputActions = new PlayerInput.BattleInputActions(PlayerInput);
        BuildingInputActions = new PlayerInput.BuildingInputActions(PlayerInput);

        Building_MouseLeft.GetStateCallbackFromContext(BuildingInputActions.MouseLeftClick);
        Building_MouseRight.GetStateCallbackFromContext(BuildingInputActions.MouseRightClick);
        Building_MouseMiddle.GetStateCallbackFromContext(BuildingInputActions.MouseMiddleClick);

        BuildingInputActions.Move.performed += context => Building_Move = context.ReadValue<Vector2>();
        BuildingInputActions.Move.canceled += context => Building_Move = Vector2.zero;

        Building_RotateItem.GetStateCallbackFromContext(BuildingInputActions.RotateItem);
        Building_ToggleBackpack.GetStateCallbackFromContext(BuildingInputActions.ToggleBackpack);
        Building_ToggleWireLines.GetStateCallbackFromContext(BuildingInputActions.ToggleWireLines);

        Battle_MouseLeft.GetStateCallbackFromContext(BattleInputActions.MouseLeftClick);
        Battle_MouseRight.GetStateCallbackFromContext(BattleInputActions.MouseRightClick);
        Battle_MouseMiddle.GetStateCallbackFromContext(BattleInputActions.MouseMiddleClick);

        BattleInputActions.Player1Move.performed += context => Battle_Move[(int)PlayerNumber.Player1] = context.ReadValue<Vector2>();
        BattleInputActions.Player1Move.canceled += context => Battle_Move[(int)PlayerNumber.Player1] = Vector2.zero;

        BattleInputActions.Player2Move.performed += context => Battle_Move[(int)PlayerNumber.Player2] = context.ReadValue<Vector2>();
        BattleInputActions.Player2Move.canceled += context => Battle_Move[(int)PlayerNumber.Player2] = Vector2.zero;

        Battle_Skill_0_Player1.GetStateCallbackFromContext(BattleInputActions.Skill_0_Player1);
        Battle_Skill_1_Player1.GetStateCallbackFromContext(BattleInputActions.Skill_1_Player1);
        Battle_Skill_0_Player2.GetStateCallbackFromContext(BattleInputActions.Skill_0_Player2);
        Battle_Skill_1_Player2.GetStateCallbackFromContext(BattleInputActions.Skill_1_Player2);

        Battle_Skill[(int) PlayerNumber.Player1, 0] = Battle_Skill_0_Player1;
        Battle_Skill[(int) PlayerNumber.Player1, 1] = Battle_Skill_1_Player1;
        Battle_Skill[(int) PlayerNumber.Player2, 0] = Battle_Skill_0_Player2;
        Battle_Skill[(int) PlayerNumber.Player2, 1] = Battle_Skill_1_Player2;

        Battle_ToggleBattleTip.GetStateCallbackFromContext(BattleInputActions.ToggleBattleTip);

        Common_MouseLeft.GetStateCallbackFromContext(CommonInputActions.MouseLeftClick);
        Common_MouseRight.GetStateCallbackFromContext(CommonInputActions.MouseRightClick);
        Common_MouseMiddle.GetStateCallbackFromContext(CommonInputActions.MouseMiddleClick);

        Common_Confirm.GetStateCallbackFromContext(CommonInputActions.Confirm);
        Common_Debug.GetStateCallbackFromContext(CommonInputActions.Debug);
        Common_Exit.GetStateCallbackFromContext(CommonInputActions.Exit);
        Common_Tab.GetStateCallbackFromContext(CommonInputActions.Tab);
        Common_RestartGame.GetStateCallbackFromContext(CommonInputActions.RestartGame);

        PlayerInput.Enable();
        CommonInputActions.Enable();
        BattleInputActions.Enable();
        BuildingInputActions.Disable();
    }

    public override void FixedUpdate(float deltaTime)
    {
        foreach (KeyValuePair<ButtonNames, ButtonState> kv in ButtonStateDict)
        {
            kv.Value.Reset();
        }

        InputSystem.Update();
        if (true)
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

    public void EnableBuildingInputActions(bool enable)
    {
        if (enable)
        {
            BuildingInputActions.Enable();
        }
        else
        {
            BuildingInputActions.Disable();
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