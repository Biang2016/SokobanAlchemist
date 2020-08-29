using UnityEngine;

namespace BiangStudio.Controller
{
    public class XBoxController : Controller
    {
        public const float JoyStickThreshold = 0.8f;

        public override void Init(ControllerIndex controllerIndex)
        {
            base.Init(controllerIndex);
            AddPlayerButton = ControlButtons.A;
        }

        public override void Update()
        {
            ButtonPressed[ControlButtons.StartButton] = Active && Input.GetButtonDown("Start_" + ControllerIndex_Int);
            ButtonPressed[ControlButtons.A] = Active && Input.GetButtonDown("A_" + ControllerIndex_Int);
            ButtonPressed[ControlButtons.B] = Active && Input.GetButtonDown("B_" + ControllerIndex_Int);
            ButtonPressed[ControlButtons.X] = Active && Input.GetButtonDown("X_" + ControllerIndex_Int);
            ButtonPressed[ControlButtons.Y] = Active && Input.GetButtonDown("Y_" + ControllerIndex_Int);
            ButtonPressed[ControlButtons.LeftBumper] = Active && Input.GetButtonDown("LB_" + ControllerIndex_Int);
            ButtonPressed[ControlButtons.RightBumper] = Active && Input.GetButtonDown("RB_" + ControllerIndex_Int);

            float leftTrigger = Active ? (Input.GetAxis("LT_" + ControllerIndex_Int)) : 0;
            float rightTrigger = Active ? (Input.GetAxis("RT_" + ControllerIndex_Int)) : 0;

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        // leftTrigger = (leftTrigger + 1) / 2f;
        // rightTrigger = (rightTrigger + 1) / 2f;
#endif
            ButtonPressed[ControlButtons.LeftTrigger] = Active && (leftTrigger > JoyStickThreshold || leftTrigger < -JoyStickThreshold);
            ButtonPressed[ControlButtons.RightTrigger] = Active && (rightTrigger > JoyStickThreshold || rightTrigger < -JoyStickThreshold);

            float leftHorizontal = Active ? Input.GetAxis("LH_" + ControllerIndex_Int) : 0;
            float rightHorizontal = (Active || Active_RightStick_OR) ? Input.GetAxis("RH_" + ControllerIndex_Int) : 0;
            float leftVertical = Active ? Input.GetAxis("LV_" + ControllerIndex_Int) : 0;
            float rightVertical = (Active || Active_RightStick_OR) ? Input.GetAxis("RV_" + ControllerIndex_Int) : 0;

            Axises[ControlAxis.LeftStick_H] = Active ? (Mathf.Abs(leftHorizontal) > JoyStickThreshold ? leftHorizontal : 0) : 0;
            Axises[ControlAxis.RightStick_H] = (Active || Active_RightStick_OR) ? (Mathf.Abs(rightHorizontal) > JoyStickThreshold ? rightHorizontal : 0) : 0;
            Axises[ControlAxis.LeftStick_V] = Active ? (Mathf.Abs(leftVertical) > JoyStickThreshold ? leftVertical : 0) : 0;
            Axises[ControlAxis.RightStick_V] = (Active || Active_RightStick_OR) ? (Mathf.Abs(rightVertical) > JoyStickThreshold ? rightVertical : 0) : 0;

            ButtonPressed[ControlButtons.LeftStickRight] = Active && leftHorizontal > JoyStickThreshold;
            ButtonPressed[ControlButtons.RightStickRight] = (Active || Active_RightStick_OR) && rightHorizontal > JoyStickThreshold;
            ButtonPressed[ControlButtons.LeftStickLeft] = Active && leftHorizontal < -JoyStickThreshold;
            ButtonPressed[ControlButtons.RightStickLeft] = (Active || Active_RightStick_OR) && rightHorizontal < -JoyStickThreshold;

            ButtonPressed[ControlButtons.LeftStickUp] = Active && leftVertical < -JoyStickThreshold;
            ButtonPressed[ControlButtons.RightStickUp] = (Active || Active_RightStick_OR) && rightVertical < -JoyStickThreshold;
            ButtonPressed[ControlButtons.LeftStickDown] = Active && leftVertical > JoyStickThreshold;
            ButtonPressed[ControlButtons.RightStickDown] = (Active || Active_RightStick_OR) && rightVertical > JoyStickThreshold;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            float dpad_h = Active ? (Input.GetAxis("DPAD_H_" + ControllerIndex_Int)) : 0;
            float dpad_v = Active ? (Input.GetAxis("DPAD_V_" + ControllerIndex_Int)) : 0;

            ButtonPressed[ControlButtons.DPAD_Up] = Active && dpad_v > JoyStickThreshold;
            ButtonPressed[ControlButtons.DPAD_Down] = Active && dpad_v < -JoyStickThreshold;
            ButtonPressed[ControlButtons.DPAD_Left] = Active && dpad_h < -JoyStickThreshold;
            ButtonPressed[ControlButtons.DPAD_Right] = Active && dpad_h > JoyStickThreshold;
#endif
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        ButtonPressed[ControlButtons.DPAD_Up] = Active && Input.GetButton("DPAD_Up_" + ControllerIndex_Int);
        ButtonPressed[ControlButtons.DPAD_Down] = Active && Input.GetButton("DPAD_Down_" + ControllerIndex_Int);
        ButtonPressed[ControlButtons.DPAD_Left] = Active && Input.GetButton("DPAD_Left_" + ControllerIndex_Int);
        ButtonPressed[ControlButtons.DPAD_Right] = Active && Input.GetButton("DPAD_Right_" + ControllerIndex_Int);
#endif

            base.Update();
        }
    }
}