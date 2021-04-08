using BiangLibrary.GameDataFormat.Grid;
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
            if (state.Up) state.LastUpFrame = Time.frameCount;

            if (bc.wasReleasedThisFrame)
            {
                state.Down = false;
                state.Pressed = false;
            }

            if (state.Down)
            {
                state.PressedDuration = 0;
                state.LastDownFrame = Time.frameCount;
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

    public static bool InsideModule(this GridPos3D localGP)
    {
        return localGP.x >= 0 && localGP.x < WorldModule.MODULE_SIZE
                              && localGP.y >= 0 && localGP.y < WorldModule.MODULE_SIZE
                              && localGP.z >= 0 && localGP.z < WorldModule.MODULE_SIZE;
    }

    public static void InGameUIFaceToCamera(Transform transform)
    {
        Vector3 diff = transform.position - CameraManager.Instance.MainCamera.transform.position;
        Ray ray = CameraManager.Instance.MainCamera.ScreenPointToRay(new Vector2(Screen.width / 2f, Screen.height / 2f));
        float distance = Vector3.Dot(ray.direction, diff);
        Vector3 cameraCenter = CameraManager.Instance.MainCamera.transform.position + ray.direction * distance;
        Vector3 offset = transform.position - cameraCenter;
        transform.rotation = Quaternion.LookRotation(transform.position - (CameraManager.Instance.MainCamera.transform.position + offset));
    }

    public static ConfigManager.TypeStartIndex ConvertToTypeStartIndex(this ushort index)
    {
        if (index >= (ushort) ConfigManager.TypeStartIndex.BattleIndicator) return ConfigManager.TypeStartIndex.BattleIndicator;
        if (index >= (ushort) ConfigManager.TypeStartIndex.FX) return ConfigManager.TypeStartIndex.FX;
        if (index >= (ushort) ConfigManager.TypeStartIndex.World) return ConfigManager.TypeStartIndex.World;
        if (index >= (ushort) ConfigManager.TypeStartIndex.StaticLayout) return ConfigManager.TypeStartIndex.StaticLayout;
        if (index >= (ushort) ConfigManager.TypeStartIndex.WorldModule) return ConfigManager.TypeStartIndex.WorldModule;
        if (index >= (ushort) ConfigManager.TypeStartIndex.LevelTrigger) return ConfigManager.TypeStartIndex.LevelTrigger;
        if (index >= (ushort) ConfigManager.TypeStartIndex.Actor) return ConfigManager.TypeStartIndex.Actor;
        if (index >= (ushort) ConfigManager.TypeStartIndex.BoxIcon) return ConfigManager.TypeStartIndex.BoxIcon;
        if (index >= (ushort) ConfigManager.TypeStartIndex.Box) return ConfigManager.TypeStartIndex.Box;
        return ConfigManager.TypeStartIndex.None;
    }
}