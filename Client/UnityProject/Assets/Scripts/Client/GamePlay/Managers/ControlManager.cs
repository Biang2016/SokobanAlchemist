using System.Collections.Generic;
using BiangStudio.Singleton;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControlManager : TSingletonBaseManager<ControlManager>
{
    private PlayerInput PlayerInput;
    private PlayerInput.CommonActions CommonInputActions;
    private PlayerInput.BattleInputActions MechaBattleInputActions;
    private PlayerInput.BuildingInputActions MechaBuildingInputActions;

    public Dictionary<ButtonNames, ButtonState> ButtonStateDict = new Dictionary<ButtonNames, ButtonState>();

    #region Building

    public bool BuildingInputActionEnabled => MechaBuildingInputActions.enabled;

    public ButtonState Building_MouseLeft = new ButtonState() {ButtonName = ButtonNames.Building_MouseLeft};
    public ButtonState Building_MouseRight = new ButtonState() {ButtonName = ButtonNames.Building_MouseRight};
    public ButtonState Building_MouseMiddle = new ButtonState() {ButtonName = ButtonNames.Building_MouseMiddle};

    public Vector2 Building_Move;

    private Vector2 Last_Building_MousePosition = Vector2.zero;

    public Vector2 Building_MousePosition
    {
        get
        {
            if (MechaBuildingInputActions.enabled)
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
            if (MechaBuildingInputActions.enabled)
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

    public bool BattleInputActionEnabled => MechaBattleInputActions.enabled;

    public ButtonState Battle_MouseLeft = new ButtonState() {ButtonName = ButtonNames.Battle_MouseLeft};
    public ButtonState Battle_MouseRight = new ButtonState() {ButtonName = ButtonNames.Battle_MouseRight};
    public ButtonState Battle_MouseMiddle = new ButtonState() {ButtonName = ButtonNames.Battle_MouseMiddle};

    public Vector2 Battle_Move;

    private Vector2 Last_Battle_MousePosition = Vector2.zero;

    public Vector2 Battle_MousePosition
    {
        get
        {
            if (MechaBattleInputActions.enabled)
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
            if (MechaBattleInputActions.enabled)
            {
                return MouseWheel;
            }
            else
            {
                return Vector2.zero;
            }
        }
    }

    public ButtonState Battle_Skill_0 = new ButtonState() {ButtonName = ButtonNames.Battle_Skill_0};
    public ButtonState Battle_Skill_1 = new ButtonState() {ButtonName = ButtonNames.Battle_Skill_1};
    public ButtonState Battle_Skill_2 = new ButtonState() {ButtonName = ButtonNames.Battle_Skill_2};
    public ButtonState Battle_Skill_3 = new ButtonState() {ButtonName = ButtonNames.Battle_Skill_3};

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
        MechaBattleInputActions = new PlayerInput.BattleInputActions(PlayerInput);
        MechaBuildingInputActions = new PlayerInput.BuildingInputActions(PlayerInput);

        Building_MouseLeft.GetStateCallbackFromContext(MechaBuildingInputActions.MouseLeftClick);
        Building_MouseRight.GetStateCallbackFromContext(MechaBuildingInputActions.MouseRightClick);
        Building_MouseMiddle.GetStateCallbackFromContext(MechaBuildingInputActions.MouseMiddleClick);

        MechaBuildingInputActions.Move.performed += context => Building_Move = context.ReadValue<Vector2>();
        MechaBuildingInputActions.Move.canceled += context => Building_Move = Vector2.zero;

        Building_RotateItem.GetStateCallbackFromContext(MechaBuildingInputActions.RotateItem);
        Building_ToggleBackpack.GetStateCallbackFromContext(MechaBuildingInputActions.ToggleBackpack);
        Building_ToggleWireLines.GetStateCallbackFromContext(MechaBuildingInputActions.ToggleWireLines);

        Battle_MouseLeft.GetStateCallbackFromContext(MechaBattleInputActions.MouseLeftClick);
        Battle_MouseRight.GetStateCallbackFromContext(MechaBattleInputActions.MouseRightClick);
        Battle_MouseMiddle.GetStateCallbackFromContext(MechaBattleInputActions.MouseMiddleClick);

        MechaBattleInputActions.Move.performed += context => Battle_Move = context.ReadValue<Vector2>();
        MechaBattleInputActions.Move.canceled += context => Battle_Move = Vector2.zero;

        Battle_Skill_0.GetStateCallbackFromContext(MechaBattleInputActions.Skill_0);
        Battle_Skill_1.GetStateCallbackFromContext(MechaBattleInputActions.Skill_1);
        Battle_Skill_2.GetStateCallbackFromContext(MechaBattleInputActions.Skill_2);
        Battle_Skill_3.GetStateCallbackFromContext(MechaBattleInputActions.Skill_3);
        Battle_ToggleBattleTip.GetStateCallbackFromContext(MechaBattleInputActions.ToggleBattleTip);

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
        MechaBattleInputActions.Enable();
        MechaBuildingInputActions.Disable();
    }

    public override void Update(float deltaTime)
    {
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
    }

    public override void LateUpdate(float deltaTime)
    {
        foreach (KeyValuePair<ButtonNames, ButtonState> kv in ButtonStateDict)
        {
            kv.Value.Reset();
        }
    }

    public void EnableBattleInputActions(bool enable)
    {
        if (enable)
        {
            MechaBattleInputActions.Enable();
        }
        else
        {
            MechaBattleInputActions.Disable();
        }
    }

    public void EnableBuildingInputActions(bool enable)
    {
        if (enable)
        {
            MechaBuildingInputActions.Enable();
        }
        else
        {
            MechaBuildingInputActions.Disable();
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