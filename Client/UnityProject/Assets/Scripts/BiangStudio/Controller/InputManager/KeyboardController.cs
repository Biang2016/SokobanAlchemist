using UnityEngine;

namespace BiangStudio.Controller
{
    public class KeyBoardController : Controller
    {
        public const float KeyboardThreshold = 0.8f;

        public override void Init(ControllerIndex controllerIndex)
        {
            base.Init(controllerIndex);
            AddPlayerButton = ControlButtons.StartButton;
        }

        public override void Update()
        {
            ButtonPressed[ControlButtons.StartButton] = Active && Input.GetButtonDown("Start_" + ControllerIndex_Int);

            ButtonPressed[ControlButtons.LeftTrigger] = Active && Input.GetMouseButton(0);
            ButtonPressed[ControlButtons.RightTrigger] = Active && Input.GetMouseButton(1);

            float leftHorizontal = Active ? Input.GetAxis("LH_" + ControllerIndex_Int) : 0;
            float leftVertical = Active ? Input.GetAxis("LV_" + ControllerIndex_Int) : 0;

            Axises[ControlAxis.LeftStick_H] = Active ? (Mathf.Abs(leftHorizontal) > KeyboardThreshold ? leftHorizontal : 0) : 0;
            Axises[ControlAxis.LeftStick_V] = Active ? (Mathf.Abs(leftVertical) > KeyboardThreshold ? leftVertical : 0) : 0;

            ButtonPressed[ControlButtons.LeftStickRight] = Active && leftHorizontal > KeyboardThreshold;
            ButtonPressed[ControlButtons.LeftStickLeft] = Active && leftHorizontal < -KeyboardThreshold;

            ButtonPressed[ControlButtons.LeftStickUp] = Active && leftVertical < -KeyboardThreshold;
            ButtonPressed[ControlButtons.LeftStickDown] = Active && leftVertical > KeyboardThreshold;

#if UNITY_STANDALONE_OSX
        ButtonPressed[ControlButtons.DPAD_Left] = Active && Input.GetButton("DPAD_Left_5");
        ButtonPressed[ControlButtons.DPAD_Right] = Active && Input.GetButton("DPAD_Right_5");
        ButtonPressed[ControlButtons.DPAD_Up] = Active && Input.GetButton("DPAD_Up_5");
        ButtonPressed[ControlButtons.DPAD_Down] = Active && Input.GetButton("DPAD_Down_5");
#elif UNITY_STANDALONE_WIN
            float dpad_horizontal = Active ? Input.GetAxis("DPAD_H_" + ControllerIndex_Int) : 0;
            float dpad_vertical = Active ? Input.GetAxis("DPAD_V_" + ControllerIndex_Int) : 0;

            ButtonPressed[ControlButtons.DPAD_Left] = Active && dpad_horizontal < 0;
            ButtonPressed[ControlButtons.DPAD_Right] = Active && dpad_horizontal > 0;
            ButtonPressed[ControlButtons.DPAD_Up] = Active && dpad_vertical > 0;
            ButtonPressed[ControlButtons.DPAD_Down] = Active && dpad_vertical < 0;
#endif

            base.Update();
        }
    }
}