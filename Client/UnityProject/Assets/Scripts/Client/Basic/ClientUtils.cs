﻿using BiangStudio.GameDataFormat.Grid;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public static class ClientUtils
{
    public static void GetStateCallbackFromContext(this ButtonState state, InputAction action)
    {
        ControlManager.Instance.ButtonStateDict.Add(state.ButtonName, state);
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
}