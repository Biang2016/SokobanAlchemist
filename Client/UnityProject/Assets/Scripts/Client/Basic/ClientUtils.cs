using BiangStudio.GameDataFormat.Grid;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Interactions;

public static class ClientUtils
{
    public static void GetStateCallbackFromContext_UpDownPress(this ButtonState state, InputAction action)
    {
        if (!ControlManager.Instance.ButtonStateDict.ContainsKey(state.ButtonName))
        {
            ControlManager.Instance.ButtonStateDict.Add(state.ButtonName, state);
            ButtonState lastFrameState = new ButtonState();
            state.ApplyTo(lastFrameState);
            ControlManager.Instance.ButtonStateDict_LastFrame.Add(state.ButtonName, lastFrameState);
        }
        else
        {
            Debug.Log($"ControlManager ButtonState {state.ButtonName} 重名");
            return;
        }

        action.performed += context =>
        {
            ButtonControl bc = (ButtonControl) context.control;
            state.Down = !state.LastPressed;
            state.Pressed = bc.isPressed;
            state.Up = bc.wasReleasedThisFrame;
            if (bc.wasReleasedThisFrame)
            {
                state.Down = false;
                state.Pressed = false;
            }
        };

        action.canceled += context => { state.Pressed = false; };
    }

    public static void GetStateCallbackFromContext_MultipleClick(this ButtonState state, InputAction action)
    {
        if (!ControlManager.Instance.ButtonStateDict.ContainsKey(state.ButtonName))
        {
            ControlManager.Instance.ButtonStateDict.Add(state.ButtonName, state);
            ButtonState lastFrameState = new ButtonState();
            state.ApplyTo(lastFrameState);
            ControlManager.Instance.ButtonStateDict_LastFrame.Add(state.ButtonName, lastFrameState);
        }
        else
        {
            return;
        }

        action.performed += context =>
        {
            if (context.interaction is MultiTapInteraction mt)
            {
                state.MultiClick = mt.tapCount;
            }
        };

        action.canceled += context => { state.MultiClick = 0; };
    }

    public static GridPos3D ToGridPos3D(this Vector3 vector3)
    {
        return new GridPos3D(Mathf.RoundToInt(vector3.x), Mathf.RoundToInt(vector3.y), Mathf.RoundToInt(vector3.z));
    }
}