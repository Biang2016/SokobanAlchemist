// GENERATED AUTOMATICALLY FROM 'Assets/Inputs/PlayerInput.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @PlayerInput : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @PlayerInput()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerInput"",
    ""maps"": [
        {
            ""name"": ""BattleInput"",
            ""id"": ""1ece4267-24e1-4d81-a6b1-405a9d7c51cb"",
            ""actions"": [
                {
                    ""name"": ""MouseLeftClick"",
                    ""type"": ""Button"",
                    ""id"": ""5a64d0f6-9a46-49fe-834c-30f69d7569b0"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""MouseRightClick"",
                    ""type"": ""Button"",
                    ""id"": ""78be1988-5a9e-4d77-a087-54214b8a8a85"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""MouseMiddleClick"",
                    ""type"": ""Button"",
                    ""id"": ""dcdf1bc6-6254-491a-874a-b162a7d558d8"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""Skill_0_Player1"",
                    ""type"": ""Button"",
                    ""id"": ""797408c5-59fe-4154-b3ea-9afca0591bb8"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""Skill_1_Player1"",
                    ""type"": ""Button"",
                    ""id"": ""cca62478-03c7-43fd-bedb-6ab2a816094e"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""Skill_0_Player2"",
                    ""type"": ""Button"",
                    ""id"": ""f027c168-eca3-48e2-be31-6edf44e526b2"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""Skill_1_Player2"",
                    ""type"": ""Button"",
                    ""id"": ""5bf27678-969d-408c-b4ee-0d8e232e54f6"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""ToggleBattleTip"",
                    ""type"": ""Button"",
                    ""id"": ""bc2fed5a-0d95-47bd-b476-ece2648b9499"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""Player1_Move_Up"",
                    ""type"": ""Button"",
                    ""id"": ""37b67fb2-d4f3-414a-8eba-ca6625406613"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""Player1_Move_Up_M"",
                    ""type"": ""Button"",
                    ""id"": ""1d291fd4-50d2-4a04-8020-518dda2ad6f4"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""MultiTap""
                },
                {
                    ""name"": ""Player1_Move_Down"",
                    ""type"": ""Button"",
                    ""id"": ""9b2beb4f-36de-4ad9-8de9-6172873ea4cd"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""Player1_Move_Down_M"",
                    ""type"": ""Button"",
                    ""id"": ""da302877-7216-4e98-a9b0-b8ff9c551565"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""MultiTap""
                },
                {
                    ""name"": ""Player1_Move_Left"",
                    ""type"": ""Button"",
                    ""id"": ""da1a7bb8-6527-4fc3-97ec-289b7797b1b7"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""Player1_Move_Left_M"",
                    ""type"": ""Button"",
                    ""id"": ""aff66ec5-9ea3-4960-b5ee-a6923de63a3e"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""MultiTap""
                },
                {
                    ""name"": ""Player1_Move_Right"",
                    ""type"": ""Button"",
                    ""id"": ""fc9e0a5e-1efc-4b8f-8944-89fadc0f0fa9"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""Player1_Move_Right_M"",
                    ""type"": ""Button"",
                    ""id"": ""020592f2-636d-4f98-bedc-267c92874100"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""MultiTap""
                },
                {
                    ""name"": ""Player2_Move_Up"",
                    ""type"": ""Button"",
                    ""id"": ""5486c412-9038-4eaf-9c47-6eb902436113"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""Player2_Move_Up_M"",
                    ""type"": ""Button"",
                    ""id"": ""82f42399-2104-49fe-9f8e-1704256ea076"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""MultiTap""
                },
                {
                    ""name"": ""Player2_Move_Down"",
                    ""type"": ""Button"",
                    ""id"": ""a106b680-7852-4c43-a6a1-64d0564cbded"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""Player2_Move_Down_M"",
                    ""type"": ""Button"",
                    ""id"": ""f41c3794-cc2b-4da6-b4bc-41db85a969d8"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""MultiTap""
                },
                {
                    ""name"": ""Player2_Move_Left"",
                    ""type"": ""Button"",
                    ""id"": ""410f507e-4816-470f-88a5-d34dae660bf8"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""Player2_Move_Left_M"",
                    ""type"": ""Button"",
                    ""id"": ""457c66ed-2668-48f0-9759-cb7a71264b99"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""MultiTap""
                },
                {
                    ""name"": ""Player2_Move_Right"",
                    ""type"": ""Button"",
                    ""id"": ""a8b1f2be-4766-438a-adcd-b22ab2f8daca"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""Player2_Move_Right_M"",
                    ""type"": ""Button"",
                    ""id"": ""d3980ffd-68e0-4487-97a9-cdbdbdb2a428"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""MultiTap""
                },
                {
                    ""name"": ""Player1_Move"",
                    ""type"": ""Value"",
                    ""id"": ""0b215f27-ebbe-472c-975b-47da80fb2f2e"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Player2_Move"",
                    ""type"": ""Value"",
                    ""id"": ""aa33bbb9-0b58-4323-9720-decb690a7265"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Player2_RightStick"",
                    ""type"": ""Value"",
                    ""id"": ""83a1b127-8522-46ed-9cf7-b8e16d0651d6"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""LeftRotateCamera"",
                    ""type"": ""Button"",
                    ""id"": ""4c0ad541-70b9-4323-b90a-f89bd96489f9"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""RightRotateCamera"",
                    ""type"": ""Button"",
                    ""id"": ""7ecf879f-d5c0-432e-b67c-46f4c52d0d68"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""da13915c-fa95-4a85-9caf-88322ccb72bf"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Skill_0_Player1"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""c6bae345-756c-4b79-9f20-2a60951a487a"",
                    ""path"": ""<Keyboard>/enter"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Skill_0_Player2"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3817b7ce-ae5b-4aba-b3bd-082722b4cd9a"",
                    ""path"": ""<XInputController>/leftShoulder"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Skill_0_Player2"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""1badfae2-bf1a-445a-bfee-f4de44f3323c"",
                    ""path"": ""<Mouse>/middleButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""MouseMiddleClick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""637e2b3c-750a-46c5-b9fd-c4a1cc798878"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""MouseLeftClick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""772f6319-2147-4b31-9912-a5af4efc436d"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""MouseRightClick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""27cf3453-9af4-41fd-9ab4-deae6e3b6533"",
                    ""path"": ""<Keyboard>/t"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""ToggleBattleTip"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""dd014788-5e27-4daf-9256-adfce84e4ba7"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Skill_1_Player1"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""c1deb210-99ec-4ab0-886c-aa4c3b44578c"",
                    ""path"": ""<Keyboard>/rightShift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Skill_1_Player2"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e3d5732d-a9b3-4198-9978-5c29a5a30341"",
                    ""path"": ""<XInputController>/rightShoulder"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Skill_1_Player2"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""7c7e96ad-7dd7-4f27-9721-298be7e8e6b0"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Player1_Move_Up"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""f812ec38-218d-4bdf-98f3-93eec323f367"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Player1_Move_Down"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""b764e9d7-7f57-4a3b-9311-3eec6c798df3"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Player1_Move_Left"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""4a3ddf94-d08a-4662-90fa-cc0153a30321"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Player1_Move_Right"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""8687155c-c902-46a1-a86a-a88f7caad4e4"",
                    ""path"": ""<Keyboard>/upArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Player2_Move_Up"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a6d2e747-b7f3-42f3-9f54-30af9e8f01e0"",
                    ""path"": ""<XInputController>/dpad/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Player2_Move_Up"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""1bde870d-a270-4b07-a0f3-78bad3f18908"",
                    ""path"": ""<Gamepad>/leftStick/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Player2_Move_Up"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a0f8ec31-4fc9-4f5f-87e2-e9d419a7f474"",
                    ""path"": ""<Keyboard>/downArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Player2_Move_Down"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""5342d7ae-7633-4f5a-8e4e-84a5f849f5d7"",
                    ""path"": ""<XInputController>/dpad/down"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Player2_Move_Down"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""b894efd5-5f72-48d9-9210-df5b571b6e81"",
                    ""path"": ""<Gamepad>/leftStick/down"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Player2_Move_Down"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a32f34a4-4861-439d-bc49-5f66cd390df3"",
                    ""path"": ""<Keyboard>/leftArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Player2_Move_Left"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3ae5518b-6b3a-416c-93ec-250fc135d98f"",
                    ""path"": ""<XInputController>/dpad/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Player2_Move_Left"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""ddf0dc4d-752b-414a-8096-1501dbedcd1a"",
                    ""path"": ""<Gamepad>/leftStick/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Player2_Move_Left"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d636d0f0-80a9-40e3-bb9f-02c7a3a4cbfc"",
                    ""path"": ""<Keyboard>/rightArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Player2_Move_Right"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""ac0020b5-c3a0-4bb3-8625-7ea547575212"",
                    ""path"": ""<XInputController>/dpad/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Player2_Move_Right"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""8271e681-3b5b-43a3-8490-a95da9ba2a94"",
                    ""path"": ""<Gamepad>/leftStick/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Player2_Move_Right"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e7536e91-91b8-4267-862f-d6a42d6ddf06"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Player1_Move_Up_M"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""4609c32c-99ae-442f-8a92-2f54ecc03b60"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Player1_Move_Down_M"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""ade7b06b-cde4-4786-b485-b6d79a7e8fbb"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Player1_Move_Left_M"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""bea9c54b-79d1-40ff-ab7e-63ef50e13009"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Player1_Move_Right_M"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""5da9a9fc-ab45-4f61-b06f-88a77debfc06"",
                    ""path"": ""<Keyboard>/upArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Player2_Move_Up_M"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3d463c90-1c5a-4c28-bc50-a70040a20947"",
                    ""path"": ""<Keyboard>/downArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Player2_Move_Down_M"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""66efcc29-b70a-4846-a662-fbaac0892bab"",
                    ""path"": ""<Keyboard>/leftArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Player2_Move_Left_M"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""01052686-3780-4d1f-906f-4280ba96308c"",
                    ""path"": ""<Keyboard>/rightArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Player2_Move_Right_M"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""WASD"",
                    ""id"": ""c57745e1-5ccf-4c28-83b2-e7436003a6ce"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Player1_Move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""22754c01-0164-461b-b5c1-4658db58f124"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Player1_Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""d3e8ef33-db4d-4881-a9fe-11a593c1bd2b"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Player1_Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""9eee0886-6ef0-49d8-b25b-1f831f7b1060"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Player1_Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""39ce92e4-447c-4714-8859-73dbebe5497b"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Player1_Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Arrows"",
                    ""id"": ""0848bc1d-c24c-443f-9c4e-ce8ab5fe91ba"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Player2_Move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""01f4dc9b-b62f-47ca-8a67-b8cb5ba9a22e"",
                    ""path"": ""<Keyboard>/upArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Player2_Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""2fd2d29e-3fe7-4d59-9bb3-d18157c814ce"",
                    ""path"": ""<Keyboard>/downArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Player2_Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""3f8b841e-dd08-4125-bb2e-79a129ad9859"",
                    ""path"": ""<Keyboard>/leftArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Player2_Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""35fe9c26-0e6b-4f7a-9973-a72593680417"",
                    ""path"": ""<Keyboard>/rightArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Player2_Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""1e809f72-9970-4fca-843a-dd747f335638"",
                    ""path"": ""<Gamepad>/leftStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Player2_Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""fe69d644-20d7-4d7c-adf2-8b8814f6ce4a"",
                    ""path"": ""<Gamepad>/rightStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Player2_RightStick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""bb7a45e9-abe3-4220-bd4a-d31e251e0abc"",
                    ""path"": ""<Keyboard>/#(Q)"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""LeftRotateCamera"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""ceb3501d-8804-4b43-83aa-7c167bdcf35a"",
                    ""path"": ""<Keyboard>/#(E)"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""RightRotateCamera"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""Common"",
            ""id"": ""7e89e9d1-f7e1-49a8-bcc4-018eff99152e"",
            ""actions"": [
                {
                    ""name"": ""MouseLeftClick"",
                    ""type"": ""Button"",
                    ""id"": ""1c84be6d-be71-459a-b015-48d29931e2a2"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""MouseRightClick"",
                    ""type"": ""Button"",
                    ""id"": ""01504029-9327-4d64-ae1b-ea8d51e2a146"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""MouseMiddleClick"",
                    ""type"": ""Button"",
                    ""id"": ""d3ecfc04-fec2-4614-8ff5-618873f00b28"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""MousePosition"",
                    ""type"": ""Value"",
                    ""id"": ""c83e4c16-d1ae-4077-a4aa-f05b0242551a"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""MouseWheel"",
                    ""type"": ""Value"",
                    ""id"": ""ba2cb947-1326-4fed-81cd-9f68513c63cc"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Exit"",
                    ""type"": ""Button"",
                    ""id"": ""c221a3f9-4221-4da1-b188-bf04e006da04"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""Tab"",
                    ""type"": ""Button"",
                    ""id"": ""21fa8e64-6108-4648-bbba-0381934760dd"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""Confirm"",
                    ""type"": ""Button"",
                    ""id"": ""9c95a13c-9036-4cae-9675-0ddef4b52926"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""Debug"",
                    ""type"": ""Button"",
                    ""id"": ""a3d49bae-abc7-4e11-8476-9ad2ac9ac463"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""RestartGame"",
                    ""type"": ""Button"",
                    ""id"": ""072a09ac-3b24-472b-aa42-f116b670abde"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""RestartGameR"",
                    ""type"": ""Button"",
                    ""id"": ""fe201fdf-66b4-42fc-afab-f0140f76e6ae"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""PauseGame"",
                    ""type"": ""Button"",
                    ""id"": ""c7c254db-3263-4037-a169-d65434e02f33"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""c2a2ef1b-7b38-4f7c-aabe-258fb695fb5b"",
                    ""path"": ""*/{Cancel}"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad;KeyboardMouse"",
                    ""action"": ""Exit"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""be9919a6-8c46-44ff-a0b2-449817606f7b"",
                    ""path"": ""<Keyboard>/tab"",
                    ""interactions"": ""Press(behavior=2)"",
                    ""processors"": ""Clamp(max=1)"",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Tab"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""dd493237-8c65-4d8b-961e-f9f1a409bb06"",
                    ""path"": ""*/{Submit}"",
                    ""interactions"": ""Press(behavior=2)"",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse;Gamepad"",
                    ""action"": ""Confirm"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3eed784b-c75c-4d15-8ec8-e96895d015b0"",
                    ""path"": ""<Keyboard>/BackQuote"",
                    ""interactions"": ""Press(behavior=2)"",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Debug"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a1a5710a-99a2-4033-8f47-46dab9355f0a"",
                    ""path"": ""<Mouse>/middleButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""MouseMiddleClick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""15e73d6e-7d92-42fa-afe9-d3536d156ea4"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""MouseRightClick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""9857762b-435f-433c-8c10-5693573aa119"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": ""Press(behavior=2)"",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""MouseLeftClick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""bf3217aa-e15c-4140-9082-aa2291a3acbf"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""MousePosition"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""5ae4b0b1-cf3f-49dd-b9bc-010914dccf00"",
                    ""path"": ""<Mouse>/scroll"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""MouseWheel"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""c5a85a49-e777-4c4a-b3f5-2ceb978fcb5f"",
                    ""path"": ""<Keyboard>/F10"",
                    ""interactions"": ""Press(behavior=2)"",
                    ""processors"": ""Clamp(max=1)"",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""RestartGame"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""73a94d58-4b5b-4d7d-a9fb-57918fb08e48"",
                    ""path"": ""<Keyboard>/#(P)"",
                    ""interactions"": ""Press(behavior=2)"",
                    ""processors"": ""Clamp(max=1)"",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""PauseGame"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""6eb7e6eb-7226-408c-ab10-841faa1f75dc"",
                    ""path"": ""<Keyboard>/#(R)"",
                    ""interactions"": ""Press(behavior=2)"",
                    ""processors"": ""Clamp(max=1)"",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""RestartGameR"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""Editor"",
            ""id"": ""8bcbd970-9a64-4c63-8582-30bf3a934dc8"",
            ""actions"": [
                {
                    ""name"": ""MousePosition"",
                    ""type"": ""Value"",
                    ""id"": ""9f4b06f8-c4f2-4b3e-9b0e-61e8c39ee7d0"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""MouseWheel"",
                    ""type"": ""Value"",
                    ""id"": ""d8359d80-c966-4fea-a049-6cf9a449b6e9"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""MouseMiddleClick"",
                    ""type"": ""Button"",
                    ""id"": ""8ee37cb6-d6a1-47a9-a18b-a5e101d610ab"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""MouseRightClick"",
                    ""type"": ""Button"",
                    ""id"": ""21ccacfa-737a-46b6-aaca-28c64c1397e2"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""MouseLeftClick"",
                    ""type"": ""Button"",
                    ""id"": ""d47754d1-b274-474d-88d2-e3f130dff8b8"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""ac607e74-38ef-48f4-9167-b122f949bb51"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": ""Press(behavior=2)"",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""MouseLeftClick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""44c9c46f-3545-4217-ab96-dc31ff218319"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""MouseRightClick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""76339cd0-e103-44f0-97d5-7c284bad2b7c"",
                    ""path"": ""<Mouse>/middleButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""MouseMiddleClick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""01cb0134-2578-4f2c-81a2-c41232a250f6"",
                    ""path"": ""<Mouse>/scroll"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""MouseWheel"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3e51ece5-1ac3-41fc-999d-192463550e19"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""MousePosition"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""KeyboardMouse"",
            ""bindingGroup"": ""KeyboardMouse"",
            ""devices"": []
        },
        {
            ""name"": ""Gamepad"",
            ""bindingGroup"": ""Gamepad"",
            ""devices"": []
        }
    ]
}");
        // BattleInput
        m_BattleInput = asset.FindActionMap("BattleInput", throwIfNotFound: true);
        m_BattleInput_MouseLeftClick = m_BattleInput.FindAction("MouseLeftClick", throwIfNotFound: true);
        m_BattleInput_MouseRightClick = m_BattleInput.FindAction("MouseRightClick", throwIfNotFound: true);
        m_BattleInput_MouseMiddleClick = m_BattleInput.FindAction("MouseMiddleClick", throwIfNotFound: true);
        m_BattleInput_Skill_0_Player1 = m_BattleInput.FindAction("Skill_0_Player1", throwIfNotFound: true);
        m_BattleInput_Skill_1_Player1 = m_BattleInput.FindAction("Skill_1_Player1", throwIfNotFound: true);
        m_BattleInput_Skill_0_Player2 = m_BattleInput.FindAction("Skill_0_Player2", throwIfNotFound: true);
        m_BattleInput_Skill_1_Player2 = m_BattleInput.FindAction("Skill_1_Player2", throwIfNotFound: true);
        m_BattleInput_ToggleBattleTip = m_BattleInput.FindAction("ToggleBattleTip", throwIfNotFound: true);
        m_BattleInput_Player1_Move_Up = m_BattleInput.FindAction("Player1_Move_Up", throwIfNotFound: true);
        m_BattleInput_Player1_Move_Up_M = m_BattleInput.FindAction("Player1_Move_Up_M", throwIfNotFound: true);
        m_BattleInput_Player1_Move_Down = m_BattleInput.FindAction("Player1_Move_Down", throwIfNotFound: true);
        m_BattleInput_Player1_Move_Down_M = m_BattleInput.FindAction("Player1_Move_Down_M", throwIfNotFound: true);
        m_BattleInput_Player1_Move_Left = m_BattleInput.FindAction("Player1_Move_Left", throwIfNotFound: true);
        m_BattleInput_Player1_Move_Left_M = m_BattleInput.FindAction("Player1_Move_Left_M", throwIfNotFound: true);
        m_BattleInput_Player1_Move_Right = m_BattleInput.FindAction("Player1_Move_Right", throwIfNotFound: true);
        m_BattleInput_Player1_Move_Right_M = m_BattleInput.FindAction("Player1_Move_Right_M", throwIfNotFound: true);
        m_BattleInput_Player2_Move_Up = m_BattleInput.FindAction("Player2_Move_Up", throwIfNotFound: true);
        m_BattleInput_Player2_Move_Up_M = m_BattleInput.FindAction("Player2_Move_Up_M", throwIfNotFound: true);
        m_BattleInput_Player2_Move_Down = m_BattleInput.FindAction("Player2_Move_Down", throwIfNotFound: true);
        m_BattleInput_Player2_Move_Down_M = m_BattleInput.FindAction("Player2_Move_Down_M", throwIfNotFound: true);
        m_BattleInput_Player2_Move_Left = m_BattleInput.FindAction("Player2_Move_Left", throwIfNotFound: true);
        m_BattleInput_Player2_Move_Left_M = m_BattleInput.FindAction("Player2_Move_Left_M", throwIfNotFound: true);
        m_BattleInput_Player2_Move_Right = m_BattleInput.FindAction("Player2_Move_Right", throwIfNotFound: true);
        m_BattleInput_Player2_Move_Right_M = m_BattleInput.FindAction("Player2_Move_Right_M", throwIfNotFound: true);
        m_BattleInput_Player1_Move = m_BattleInput.FindAction("Player1_Move", throwIfNotFound: true);
        m_BattleInput_Player2_Move = m_BattleInput.FindAction("Player2_Move", throwIfNotFound: true);
        m_BattleInput_Player2_RightStick = m_BattleInput.FindAction("Player2_RightStick", throwIfNotFound: true);
        m_BattleInput_LeftRotateCamera = m_BattleInput.FindAction("LeftRotateCamera", throwIfNotFound: true);
        m_BattleInput_RightRotateCamera = m_BattleInput.FindAction("RightRotateCamera", throwIfNotFound: true);
        // Common
        m_Common = asset.FindActionMap("Common", throwIfNotFound: true);
        m_Common_MouseLeftClick = m_Common.FindAction("MouseLeftClick", throwIfNotFound: true);
        m_Common_MouseRightClick = m_Common.FindAction("MouseRightClick", throwIfNotFound: true);
        m_Common_MouseMiddleClick = m_Common.FindAction("MouseMiddleClick", throwIfNotFound: true);
        m_Common_MousePosition = m_Common.FindAction("MousePosition", throwIfNotFound: true);
        m_Common_MouseWheel = m_Common.FindAction("MouseWheel", throwIfNotFound: true);
        m_Common_Exit = m_Common.FindAction("Exit", throwIfNotFound: true);
        m_Common_Tab = m_Common.FindAction("Tab", throwIfNotFound: true);
        m_Common_Confirm = m_Common.FindAction("Confirm", throwIfNotFound: true);
        m_Common_Debug = m_Common.FindAction("Debug", throwIfNotFound: true);
        m_Common_RestartGame = m_Common.FindAction("RestartGame", throwIfNotFound: true);
        m_Common_RestartGameR = m_Common.FindAction("RestartGameR", throwIfNotFound: true);
        m_Common_PauseGame = m_Common.FindAction("PauseGame", throwIfNotFound: true);
        // Editor
        m_Editor = asset.FindActionMap("Editor", throwIfNotFound: true);
        m_Editor_MousePosition = m_Editor.FindAction("MousePosition", throwIfNotFound: true);
        m_Editor_MouseWheel = m_Editor.FindAction("MouseWheel", throwIfNotFound: true);
        m_Editor_MouseMiddleClick = m_Editor.FindAction("MouseMiddleClick", throwIfNotFound: true);
        m_Editor_MouseRightClick = m_Editor.FindAction("MouseRightClick", throwIfNotFound: true);
        m_Editor_MouseLeftClick = m_Editor.FindAction("MouseLeftClick", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // BattleInput
    private readonly InputActionMap m_BattleInput;
    private IBattleInputActions m_BattleInputActionsCallbackInterface;
    private readonly InputAction m_BattleInput_MouseLeftClick;
    private readonly InputAction m_BattleInput_MouseRightClick;
    private readonly InputAction m_BattleInput_MouseMiddleClick;
    private readonly InputAction m_BattleInput_Skill_0_Player1;
    private readonly InputAction m_BattleInput_Skill_1_Player1;
    private readonly InputAction m_BattleInput_Skill_0_Player2;
    private readonly InputAction m_BattleInput_Skill_1_Player2;
    private readonly InputAction m_BattleInput_ToggleBattleTip;
    private readonly InputAction m_BattleInput_Player1_Move_Up;
    private readonly InputAction m_BattleInput_Player1_Move_Up_M;
    private readonly InputAction m_BattleInput_Player1_Move_Down;
    private readonly InputAction m_BattleInput_Player1_Move_Down_M;
    private readonly InputAction m_BattleInput_Player1_Move_Left;
    private readonly InputAction m_BattleInput_Player1_Move_Left_M;
    private readonly InputAction m_BattleInput_Player1_Move_Right;
    private readonly InputAction m_BattleInput_Player1_Move_Right_M;
    private readonly InputAction m_BattleInput_Player2_Move_Up;
    private readonly InputAction m_BattleInput_Player2_Move_Up_M;
    private readonly InputAction m_BattleInput_Player2_Move_Down;
    private readonly InputAction m_BattleInput_Player2_Move_Down_M;
    private readonly InputAction m_BattleInput_Player2_Move_Left;
    private readonly InputAction m_BattleInput_Player2_Move_Left_M;
    private readonly InputAction m_BattleInput_Player2_Move_Right;
    private readonly InputAction m_BattleInput_Player2_Move_Right_M;
    private readonly InputAction m_BattleInput_Player1_Move;
    private readonly InputAction m_BattleInput_Player2_Move;
    private readonly InputAction m_BattleInput_Player2_RightStick;
    private readonly InputAction m_BattleInput_LeftRotateCamera;
    private readonly InputAction m_BattleInput_RightRotateCamera;
    public struct BattleInputActions
    {
        private @PlayerInput m_Wrapper;
        public BattleInputActions(@PlayerInput wrapper) { m_Wrapper = wrapper; }
        public InputAction @MouseLeftClick => m_Wrapper.m_BattleInput_MouseLeftClick;
        public InputAction @MouseRightClick => m_Wrapper.m_BattleInput_MouseRightClick;
        public InputAction @MouseMiddleClick => m_Wrapper.m_BattleInput_MouseMiddleClick;
        public InputAction @Skill_0_Player1 => m_Wrapper.m_BattleInput_Skill_0_Player1;
        public InputAction @Skill_1_Player1 => m_Wrapper.m_BattleInput_Skill_1_Player1;
        public InputAction @Skill_0_Player2 => m_Wrapper.m_BattleInput_Skill_0_Player2;
        public InputAction @Skill_1_Player2 => m_Wrapper.m_BattleInput_Skill_1_Player2;
        public InputAction @ToggleBattleTip => m_Wrapper.m_BattleInput_ToggleBattleTip;
        public InputAction @Player1_Move_Up => m_Wrapper.m_BattleInput_Player1_Move_Up;
        public InputAction @Player1_Move_Up_M => m_Wrapper.m_BattleInput_Player1_Move_Up_M;
        public InputAction @Player1_Move_Down => m_Wrapper.m_BattleInput_Player1_Move_Down;
        public InputAction @Player1_Move_Down_M => m_Wrapper.m_BattleInput_Player1_Move_Down_M;
        public InputAction @Player1_Move_Left => m_Wrapper.m_BattleInput_Player1_Move_Left;
        public InputAction @Player1_Move_Left_M => m_Wrapper.m_BattleInput_Player1_Move_Left_M;
        public InputAction @Player1_Move_Right => m_Wrapper.m_BattleInput_Player1_Move_Right;
        public InputAction @Player1_Move_Right_M => m_Wrapper.m_BattleInput_Player1_Move_Right_M;
        public InputAction @Player2_Move_Up => m_Wrapper.m_BattleInput_Player2_Move_Up;
        public InputAction @Player2_Move_Up_M => m_Wrapper.m_BattleInput_Player2_Move_Up_M;
        public InputAction @Player2_Move_Down => m_Wrapper.m_BattleInput_Player2_Move_Down;
        public InputAction @Player2_Move_Down_M => m_Wrapper.m_BattleInput_Player2_Move_Down_M;
        public InputAction @Player2_Move_Left => m_Wrapper.m_BattleInput_Player2_Move_Left;
        public InputAction @Player2_Move_Left_M => m_Wrapper.m_BattleInput_Player2_Move_Left_M;
        public InputAction @Player2_Move_Right => m_Wrapper.m_BattleInput_Player2_Move_Right;
        public InputAction @Player2_Move_Right_M => m_Wrapper.m_BattleInput_Player2_Move_Right_M;
        public InputAction @Player1_Move => m_Wrapper.m_BattleInput_Player1_Move;
        public InputAction @Player2_Move => m_Wrapper.m_BattleInput_Player2_Move;
        public InputAction @Player2_RightStick => m_Wrapper.m_BattleInput_Player2_RightStick;
        public InputAction @LeftRotateCamera => m_Wrapper.m_BattleInput_LeftRotateCamera;
        public InputAction @RightRotateCamera => m_Wrapper.m_BattleInput_RightRotateCamera;
        public InputActionMap Get() { return m_Wrapper.m_BattleInput; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(BattleInputActions set) { return set.Get(); }
        public void SetCallbacks(IBattleInputActions instance)
        {
            if (m_Wrapper.m_BattleInputActionsCallbackInterface != null)
            {
                @MouseLeftClick.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnMouseLeftClick;
                @MouseLeftClick.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnMouseLeftClick;
                @MouseLeftClick.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnMouseLeftClick;
                @MouseRightClick.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnMouseRightClick;
                @MouseRightClick.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnMouseRightClick;
                @MouseRightClick.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnMouseRightClick;
                @MouseMiddleClick.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnMouseMiddleClick;
                @MouseMiddleClick.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnMouseMiddleClick;
                @MouseMiddleClick.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnMouseMiddleClick;
                @Skill_0_Player1.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_0_Player1;
                @Skill_0_Player1.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_0_Player1;
                @Skill_0_Player1.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_0_Player1;
                @Skill_1_Player1.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_1_Player1;
                @Skill_1_Player1.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_1_Player1;
                @Skill_1_Player1.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_1_Player1;
                @Skill_0_Player2.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_0_Player2;
                @Skill_0_Player2.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_0_Player2;
                @Skill_0_Player2.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_0_Player2;
                @Skill_1_Player2.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_1_Player2;
                @Skill_1_Player2.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_1_Player2;
                @Skill_1_Player2.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_1_Player2;
                @ToggleBattleTip.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnToggleBattleTip;
                @ToggleBattleTip.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnToggleBattleTip;
                @ToggleBattleTip.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnToggleBattleTip;
                @Player1_Move_Up.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer1_Move_Up;
                @Player1_Move_Up.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer1_Move_Up;
                @Player1_Move_Up.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer1_Move_Up;
                @Player1_Move_Up_M.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer1_Move_Up_M;
                @Player1_Move_Up_M.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer1_Move_Up_M;
                @Player1_Move_Up_M.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer1_Move_Up_M;
                @Player1_Move_Down.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer1_Move_Down;
                @Player1_Move_Down.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer1_Move_Down;
                @Player1_Move_Down.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer1_Move_Down;
                @Player1_Move_Down_M.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer1_Move_Down_M;
                @Player1_Move_Down_M.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer1_Move_Down_M;
                @Player1_Move_Down_M.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer1_Move_Down_M;
                @Player1_Move_Left.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer1_Move_Left;
                @Player1_Move_Left.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer1_Move_Left;
                @Player1_Move_Left.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer1_Move_Left;
                @Player1_Move_Left_M.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer1_Move_Left_M;
                @Player1_Move_Left_M.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer1_Move_Left_M;
                @Player1_Move_Left_M.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer1_Move_Left_M;
                @Player1_Move_Right.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer1_Move_Right;
                @Player1_Move_Right.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer1_Move_Right;
                @Player1_Move_Right.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer1_Move_Right;
                @Player1_Move_Right_M.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer1_Move_Right_M;
                @Player1_Move_Right_M.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer1_Move_Right_M;
                @Player1_Move_Right_M.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer1_Move_Right_M;
                @Player2_Move_Up.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer2_Move_Up;
                @Player2_Move_Up.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer2_Move_Up;
                @Player2_Move_Up.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer2_Move_Up;
                @Player2_Move_Up_M.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer2_Move_Up_M;
                @Player2_Move_Up_M.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer2_Move_Up_M;
                @Player2_Move_Up_M.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer2_Move_Up_M;
                @Player2_Move_Down.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer2_Move_Down;
                @Player2_Move_Down.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer2_Move_Down;
                @Player2_Move_Down.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer2_Move_Down;
                @Player2_Move_Down_M.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer2_Move_Down_M;
                @Player2_Move_Down_M.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer2_Move_Down_M;
                @Player2_Move_Down_M.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer2_Move_Down_M;
                @Player2_Move_Left.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer2_Move_Left;
                @Player2_Move_Left.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer2_Move_Left;
                @Player2_Move_Left.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer2_Move_Left;
                @Player2_Move_Left_M.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer2_Move_Left_M;
                @Player2_Move_Left_M.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer2_Move_Left_M;
                @Player2_Move_Left_M.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer2_Move_Left_M;
                @Player2_Move_Right.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer2_Move_Right;
                @Player2_Move_Right.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer2_Move_Right;
                @Player2_Move_Right.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer2_Move_Right;
                @Player2_Move_Right_M.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer2_Move_Right_M;
                @Player2_Move_Right_M.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer2_Move_Right_M;
                @Player2_Move_Right_M.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer2_Move_Right_M;
                @Player1_Move.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer1_Move;
                @Player1_Move.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer1_Move;
                @Player1_Move.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer1_Move;
                @Player2_Move.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer2_Move;
                @Player2_Move.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer2_Move;
                @Player2_Move.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer2_Move;
                @Player2_RightStick.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer2_RightStick;
                @Player2_RightStick.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer2_RightStick;
                @Player2_RightStick.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer2_RightStick;
                @LeftRotateCamera.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnLeftRotateCamera;
                @LeftRotateCamera.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnLeftRotateCamera;
                @LeftRotateCamera.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnLeftRotateCamera;
                @RightRotateCamera.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnRightRotateCamera;
                @RightRotateCamera.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnRightRotateCamera;
                @RightRotateCamera.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnRightRotateCamera;
            }
            m_Wrapper.m_BattleInputActionsCallbackInterface = instance;
            if (instance != null)
            {
                @MouseLeftClick.started += instance.OnMouseLeftClick;
                @MouseLeftClick.performed += instance.OnMouseLeftClick;
                @MouseLeftClick.canceled += instance.OnMouseLeftClick;
                @MouseRightClick.started += instance.OnMouseRightClick;
                @MouseRightClick.performed += instance.OnMouseRightClick;
                @MouseRightClick.canceled += instance.OnMouseRightClick;
                @MouseMiddleClick.started += instance.OnMouseMiddleClick;
                @MouseMiddleClick.performed += instance.OnMouseMiddleClick;
                @MouseMiddleClick.canceled += instance.OnMouseMiddleClick;
                @Skill_0_Player1.started += instance.OnSkill_0_Player1;
                @Skill_0_Player1.performed += instance.OnSkill_0_Player1;
                @Skill_0_Player1.canceled += instance.OnSkill_0_Player1;
                @Skill_1_Player1.started += instance.OnSkill_1_Player1;
                @Skill_1_Player1.performed += instance.OnSkill_1_Player1;
                @Skill_1_Player1.canceled += instance.OnSkill_1_Player1;
                @Skill_0_Player2.started += instance.OnSkill_0_Player2;
                @Skill_0_Player2.performed += instance.OnSkill_0_Player2;
                @Skill_0_Player2.canceled += instance.OnSkill_0_Player2;
                @Skill_1_Player2.started += instance.OnSkill_1_Player2;
                @Skill_1_Player2.performed += instance.OnSkill_1_Player2;
                @Skill_1_Player2.canceled += instance.OnSkill_1_Player2;
                @ToggleBattleTip.started += instance.OnToggleBattleTip;
                @ToggleBattleTip.performed += instance.OnToggleBattleTip;
                @ToggleBattleTip.canceled += instance.OnToggleBattleTip;
                @Player1_Move_Up.started += instance.OnPlayer1_Move_Up;
                @Player1_Move_Up.performed += instance.OnPlayer1_Move_Up;
                @Player1_Move_Up.canceled += instance.OnPlayer1_Move_Up;
                @Player1_Move_Up_M.started += instance.OnPlayer1_Move_Up_M;
                @Player1_Move_Up_M.performed += instance.OnPlayer1_Move_Up_M;
                @Player1_Move_Up_M.canceled += instance.OnPlayer1_Move_Up_M;
                @Player1_Move_Down.started += instance.OnPlayer1_Move_Down;
                @Player1_Move_Down.performed += instance.OnPlayer1_Move_Down;
                @Player1_Move_Down.canceled += instance.OnPlayer1_Move_Down;
                @Player1_Move_Down_M.started += instance.OnPlayer1_Move_Down_M;
                @Player1_Move_Down_M.performed += instance.OnPlayer1_Move_Down_M;
                @Player1_Move_Down_M.canceled += instance.OnPlayer1_Move_Down_M;
                @Player1_Move_Left.started += instance.OnPlayer1_Move_Left;
                @Player1_Move_Left.performed += instance.OnPlayer1_Move_Left;
                @Player1_Move_Left.canceled += instance.OnPlayer1_Move_Left;
                @Player1_Move_Left_M.started += instance.OnPlayer1_Move_Left_M;
                @Player1_Move_Left_M.performed += instance.OnPlayer1_Move_Left_M;
                @Player1_Move_Left_M.canceled += instance.OnPlayer1_Move_Left_M;
                @Player1_Move_Right.started += instance.OnPlayer1_Move_Right;
                @Player1_Move_Right.performed += instance.OnPlayer1_Move_Right;
                @Player1_Move_Right.canceled += instance.OnPlayer1_Move_Right;
                @Player1_Move_Right_M.started += instance.OnPlayer1_Move_Right_M;
                @Player1_Move_Right_M.performed += instance.OnPlayer1_Move_Right_M;
                @Player1_Move_Right_M.canceled += instance.OnPlayer1_Move_Right_M;
                @Player2_Move_Up.started += instance.OnPlayer2_Move_Up;
                @Player2_Move_Up.performed += instance.OnPlayer2_Move_Up;
                @Player2_Move_Up.canceled += instance.OnPlayer2_Move_Up;
                @Player2_Move_Up_M.started += instance.OnPlayer2_Move_Up_M;
                @Player2_Move_Up_M.performed += instance.OnPlayer2_Move_Up_M;
                @Player2_Move_Up_M.canceled += instance.OnPlayer2_Move_Up_M;
                @Player2_Move_Down.started += instance.OnPlayer2_Move_Down;
                @Player2_Move_Down.performed += instance.OnPlayer2_Move_Down;
                @Player2_Move_Down.canceled += instance.OnPlayer2_Move_Down;
                @Player2_Move_Down_M.started += instance.OnPlayer2_Move_Down_M;
                @Player2_Move_Down_M.performed += instance.OnPlayer2_Move_Down_M;
                @Player2_Move_Down_M.canceled += instance.OnPlayer2_Move_Down_M;
                @Player2_Move_Left.started += instance.OnPlayer2_Move_Left;
                @Player2_Move_Left.performed += instance.OnPlayer2_Move_Left;
                @Player2_Move_Left.canceled += instance.OnPlayer2_Move_Left;
                @Player2_Move_Left_M.started += instance.OnPlayer2_Move_Left_M;
                @Player2_Move_Left_M.performed += instance.OnPlayer2_Move_Left_M;
                @Player2_Move_Left_M.canceled += instance.OnPlayer2_Move_Left_M;
                @Player2_Move_Right.started += instance.OnPlayer2_Move_Right;
                @Player2_Move_Right.performed += instance.OnPlayer2_Move_Right;
                @Player2_Move_Right.canceled += instance.OnPlayer2_Move_Right;
                @Player2_Move_Right_M.started += instance.OnPlayer2_Move_Right_M;
                @Player2_Move_Right_M.performed += instance.OnPlayer2_Move_Right_M;
                @Player2_Move_Right_M.canceled += instance.OnPlayer2_Move_Right_M;
                @Player1_Move.started += instance.OnPlayer1_Move;
                @Player1_Move.performed += instance.OnPlayer1_Move;
                @Player1_Move.canceled += instance.OnPlayer1_Move;
                @Player2_Move.started += instance.OnPlayer2_Move;
                @Player2_Move.performed += instance.OnPlayer2_Move;
                @Player2_Move.canceled += instance.OnPlayer2_Move;
                @Player2_RightStick.started += instance.OnPlayer2_RightStick;
                @Player2_RightStick.performed += instance.OnPlayer2_RightStick;
                @Player2_RightStick.canceled += instance.OnPlayer2_RightStick;
                @LeftRotateCamera.started += instance.OnLeftRotateCamera;
                @LeftRotateCamera.performed += instance.OnLeftRotateCamera;
                @LeftRotateCamera.canceled += instance.OnLeftRotateCamera;
                @RightRotateCamera.started += instance.OnRightRotateCamera;
                @RightRotateCamera.performed += instance.OnRightRotateCamera;
                @RightRotateCamera.canceled += instance.OnRightRotateCamera;
            }
        }
    }
    public BattleInputActions @BattleInput => new BattleInputActions(this);

    // Common
    private readonly InputActionMap m_Common;
    private ICommonActions m_CommonActionsCallbackInterface;
    private readonly InputAction m_Common_MouseLeftClick;
    private readonly InputAction m_Common_MouseRightClick;
    private readonly InputAction m_Common_MouseMiddleClick;
    private readonly InputAction m_Common_MousePosition;
    private readonly InputAction m_Common_MouseWheel;
    private readonly InputAction m_Common_Exit;
    private readonly InputAction m_Common_Tab;
    private readonly InputAction m_Common_Confirm;
    private readonly InputAction m_Common_Debug;
    private readonly InputAction m_Common_RestartGame;
    private readonly InputAction m_Common_RestartGameR;
    private readonly InputAction m_Common_PauseGame;
    public struct CommonActions
    {
        private @PlayerInput m_Wrapper;
        public CommonActions(@PlayerInput wrapper) { m_Wrapper = wrapper; }
        public InputAction @MouseLeftClick => m_Wrapper.m_Common_MouseLeftClick;
        public InputAction @MouseRightClick => m_Wrapper.m_Common_MouseRightClick;
        public InputAction @MouseMiddleClick => m_Wrapper.m_Common_MouseMiddleClick;
        public InputAction @MousePosition => m_Wrapper.m_Common_MousePosition;
        public InputAction @MouseWheel => m_Wrapper.m_Common_MouseWheel;
        public InputAction @Exit => m_Wrapper.m_Common_Exit;
        public InputAction @Tab => m_Wrapper.m_Common_Tab;
        public InputAction @Confirm => m_Wrapper.m_Common_Confirm;
        public InputAction @Debug => m_Wrapper.m_Common_Debug;
        public InputAction @RestartGame => m_Wrapper.m_Common_RestartGame;
        public InputAction @RestartGameR => m_Wrapper.m_Common_RestartGameR;
        public InputAction @PauseGame => m_Wrapper.m_Common_PauseGame;
        public InputActionMap Get() { return m_Wrapper.m_Common; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(CommonActions set) { return set.Get(); }
        public void SetCallbacks(ICommonActions instance)
        {
            if (m_Wrapper.m_CommonActionsCallbackInterface != null)
            {
                @MouseLeftClick.started -= m_Wrapper.m_CommonActionsCallbackInterface.OnMouseLeftClick;
                @MouseLeftClick.performed -= m_Wrapper.m_CommonActionsCallbackInterface.OnMouseLeftClick;
                @MouseLeftClick.canceled -= m_Wrapper.m_CommonActionsCallbackInterface.OnMouseLeftClick;
                @MouseRightClick.started -= m_Wrapper.m_CommonActionsCallbackInterface.OnMouseRightClick;
                @MouseRightClick.performed -= m_Wrapper.m_CommonActionsCallbackInterface.OnMouseRightClick;
                @MouseRightClick.canceled -= m_Wrapper.m_CommonActionsCallbackInterface.OnMouseRightClick;
                @MouseMiddleClick.started -= m_Wrapper.m_CommonActionsCallbackInterface.OnMouseMiddleClick;
                @MouseMiddleClick.performed -= m_Wrapper.m_CommonActionsCallbackInterface.OnMouseMiddleClick;
                @MouseMiddleClick.canceled -= m_Wrapper.m_CommonActionsCallbackInterface.OnMouseMiddleClick;
                @MousePosition.started -= m_Wrapper.m_CommonActionsCallbackInterface.OnMousePosition;
                @MousePosition.performed -= m_Wrapper.m_CommonActionsCallbackInterface.OnMousePosition;
                @MousePosition.canceled -= m_Wrapper.m_CommonActionsCallbackInterface.OnMousePosition;
                @MouseWheel.started -= m_Wrapper.m_CommonActionsCallbackInterface.OnMouseWheel;
                @MouseWheel.performed -= m_Wrapper.m_CommonActionsCallbackInterface.OnMouseWheel;
                @MouseWheel.canceled -= m_Wrapper.m_CommonActionsCallbackInterface.OnMouseWheel;
                @Exit.started -= m_Wrapper.m_CommonActionsCallbackInterface.OnExit;
                @Exit.performed -= m_Wrapper.m_CommonActionsCallbackInterface.OnExit;
                @Exit.canceled -= m_Wrapper.m_CommonActionsCallbackInterface.OnExit;
                @Tab.started -= m_Wrapper.m_CommonActionsCallbackInterface.OnTab;
                @Tab.performed -= m_Wrapper.m_CommonActionsCallbackInterface.OnTab;
                @Tab.canceled -= m_Wrapper.m_CommonActionsCallbackInterface.OnTab;
                @Confirm.started -= m_Wrapper.m_CommonActionsCallbackInterface.OnConfirm;
                @Confirm.performed -= m_Wrapper.m_CommonActionsCallbackInterface.OnConfirm;
                @Confirm.canceled -= m_Wrapper.m_CommonActionsCallbackInterface.OnConfirm;
                @Debug.started -= m_Wrapper.m_CommonActionsCallbackInterface.OnDebug;
                @Debug.performed -= m_Wrapper.m_CommonActionsCallbackInterface.OnDebug;
                @Debug.canceled -= m_Wrapper.m_CommonActionsCallbackInterface.OnDebug;
                @RestartGame.started -= m_Wrapper.m_CommonActionsCallbackInterface.OnRestartGame;
                @RestartGame.performed -= m_Wrapper.m_CommonActionsCallbackInterface.OnRestartGame;
                @RestartGame.canceled -= m_Wrapper.m_CommonActionsCallbackInterface.OnRestartGame;
                @RestartGameR.started -= m_Wrapper.m_CommonActionsCallbackInterface.OnRestartGameR;
                @RestartGameR.performed -= m_Wrapper.m_CommonActionsCallbackInterface.OnRestartGameR;
                @RestartGameR.canceled -= m_Wrapper.m_CommonActionsCallbackInterface.OnRestartGameR;
                @PauseGame.started -= m_Wrapper.m_CommonActionsCallbackInterface.OnPauseGame;
                @PauseGame.performed -= m_Wrapper.m_CommonActionsCallbackInterface.OnPauseGame;
                @PauseGame.canceled -= m_Wrapper.m_CommonActionsCallbackInterface.OnPauseGame;
            }
            m_Wrapper.m_CommonActionsCallbackInterface = instance;
            if (instance != null)
            {
                @MouseLeftClick.started += instance.OnMouseLeftClick;
                @MouseLeftClick.performed += instance.OnMouseLeftClick;
                @MouseLeftClick.canceled += instance.OnMouseLeftClick;
                @MouseRightClick.started += instance.OnMouseRightClick;
                @MouseRightClick.performed += instance.OnMouseRightClick;
                @MouseRightClick.canceled += instance.OnMouseRightClick;
                @MouseMiddleClick.started += instance.OnMouseMiddleClick;
                @MouseMiddleClick.performed += instance.OnMouseMiddleClick;
                @MouseMiddleClick.canceled += instance.OnMouseMiddleClick;
                @MousePosition.started += instance.OnMousePosition;
                @MousePosition.performed += instance.OnMousePosition;
                @MousePosition.canceled += instance.OnMousePosition;
                @MouseWheel.started += instance.OnMouseWheel;
                @MouseWheel.performed += instance.OnMouseWheel;
                @MouseWheel.canceled += instance.OnMouseWheel;
                @Exit.started += instance.OnExit;
                @Exit.performed += instance.OnExit;
                @Exit.canceled += instance.OnExit;
                @Tab.started += instance.OnTab;
                @Tab.performed += instance.OnTab;
                @Tab.canceled += instance.OnTab;
                @Confirm.started += instance.OnConfirm;
                @Confirm.performed += instance.OnConfirm;
                @Confirm.canceled += instance.OnConfirm;
                @Debug.started += instance.OnDebug;
                @Debug.performed += instance.OnDebug;
                @Debug.canceled += instance.OnDebug;
                @RestartGame.started += instance.OnRestartGame;
                @RestartGame.performed += instance.OnRestartGame;
                @RestartGame.canceled += instance.OnRestartGame;
                @RestartGameR.started += instance.OnRestartGameR;
                @RestartGameR.performed += instance.OnRestartGameR;
                @RestartGameR.canceled += instance.OnRestartGameR;
                @PauseGame.started += instance.OnPauseGame;
                @PauseGame.performed += instance.OnPauseGame;
                @PauseGame.canceled += instance.OnPauseGame;
            }
        }
    }
    public CommonActions @Common => new CommonActions(this);

    // Editor
    private readonly InputActionMap m_Editor;
    private IEditorActions m_EditorActionsCallbackInterface;
    private readonly InputAction m_Editor_MousePosition;
    private readonly InputAction m_Editor_MouseWheel;
    private readonly InputAction m_Editor_MouseMiddleClick;
    private readonly InputAction m_Editor_MouseRightClick;
    private readonly InputAction m_Editor_MouseLeftClick;
    public struct EditorActions
    {
        private @PlayerInput m_Wrapper;
        public EditorActions(@PlayerInput wrapper) { m_Wrapper = wrapper; }
        public InputAction @MousePosition => m_Wrapper.m_Editor_MousePosition;
        public InputAction @MouseWheel => m_Wrapper.m_Editor_MouseWheel;
        public InputAction @MouseMiddleClick => m_Wrapper.m_Editor_MouseMiddleClick;
        public InputAction @MouseRightClick => m_Wrapper.m_Editor_MouseRightClick;
        public InputAction @MouseLeftClick => m_Wrapper.m_Editor_MouseLeftClick;
        public InputActionMap Get() { return m_Wrapper.m_Editor; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(EditorActions set) { return set.Get(); }
        public void SetCallbacks(IEditorActions instance)
        {
            if (m_Wrapper.m_EditorActionsCallbackInterface != null)
            {
                @MousePosition.started -= m_Wrapper.m_EditorActionsCallbackInterface.OnMousePosition;
                @MousePosition.performed -= m_Wrapper.m_EditorActionsCallbackInterface.OnMousePosition;
                @MousePosition.canceled -= m_Wrapper.m_EditorActionsCallbackInterface.OnMousePosition;
                @MouseWheel.started -= m_Wrapper.m_EditorActionsCallbackInterface.OnMouseWheel;
                @MouseWheel.performed -= m_Wrapper.m_EditorActionsCallbackInterface.OnMouseWheel;
                @MouseWheel.canceled -= m_Wrapper.m_EditorActionsCallbackInterface.OnMouseWheel;
                @MouseMiddleClick.started -= m_Wrapper.m_EditorActionsCallbackInterface.OnMouseMiddleClick;
                @MouseMiddleClick.performed -= m_Wrapper.m_EditorActionsCallbackInterface.OnMouseMiddleClick;
                @MouseMiddleClick.canceled -= m_Wrapper.m_EditorActionsCallbackInterface.OnMouseMiddleClick;
                @MouseRightClick.started -= m_Wrapper.m_EditorActionsCallbackInterface.OnMouseRightClick;
                @MouseRightClick.performed -= m_Wrapper.m_EditorActionsCallbackInterface.OnMouseRightClick;
                @MouseRightClick.canceled -= m_Wrapper.m_EditorActionsCallbackInterface.OnMouseRightClick;
                @MouseLeftClick.started -= m_Wrapper.m_EditorActionsCallbackInterface.OnMouseLeftClick;
                @MouseLeftClick.performed -= m_Wrapper.m_EditorActionsCallbackInterface.OnMouseLeftClick;
                @MouseLeftClick.canceled -= m_Wrapper.m_EditorActionsCallbackInterface.OnMouseLeftClick;
            }
            m_Wrapper.m_EditorActionsCallbackInterface = instance;
            if (instance != null)
            {
                @MousePosition.started += instance.OnMousePosition;
                @MousePosition.performed += instance.OnMousePosition;
                @MousePosition.canceled += instance.OnMousePosition;
                @MouseWheel.started += instance.OnMouseWheel;
                @MouseWheel.performed += instance.OnMouseWheel;
                @MouseWheel.canceled += instance.OnMouseWheel;
                @MouseMiddleClick.started += instance.OnMouseMiddleClick;
                @MouseMiddleClick.performed += instance.OnMouseMiddleClick;
                @MouseMiddleClick.canceled += instance.OnMouseMiddleClick;
                @MouseRightClick.started += instance.OnMouseRightClick;
                @MouseRightClick.performed += instance.OnMouseRightClick;
                @MouseRightClick.canceled += instance.OnMouseRightClick;
                @MouseLeftClick.started += instance.OnMouseLeftClick;
                @MouseLeftClick.performed += instance.OnMouseLeftClick;
                @MouseLeftClick.canceled += instance.OnMouseLeftClick;
            }
        }
    }
    public EditorActions @Editor => new EditorActions(this);
    private int m_KeyboardMouseSchemeIndex = -1;
    public InputControlScheme KeyboardMouseScheme
    {
        get
        {
            if (m_KeyboardMouseSchemeIndex == -1) m_KeyboardMouseSchemeIndex = asset.FindControlSchemeIndex("KeyboardMouse");
            return asset.controlSchemes[m_KeyboardMouseSchemeIndex];
        }
    }
    private int m_GamepadSchemeIndex = -1;
    public InputControlScheme GamepadScheme
    {
        get
        {
            if (m_GamepadSchemeIndex == -1) m_GamepadSchemeIndex = asset.FindControlSchemeIndex("Gamepad");
            return asset.controlSchemes[m_GamepadSchemeIndex];
        }
    }
    public interface IBattleInputActions
    {
        void OnMouseLeftClick(InputAction.CallbackContext context);
        void OnMouseRightClick(InputAction.CallbackContext context);
        void OnMouseMiddleClick(InputAction.CallbackContext context);
        void OnSkill_0_Player1(InputAction.CallbackContext context);
        void OnSkill_1_Player1(InputAction.CallbackContext context);
        void OnSkill_0_Player2(InputAction.CallbackContext context);
        void OnSkill_1_Player2(InputAction.CallbackContext context);
        void OnToggleBattleTip(InputAction.CallbackContext context);
        void OnPlayer1_Move_Up(InputAction.CallbackContext context);
        void OnPlayer1_Move_Up_M(InputAction.CallbackContext context);
        void OnPlayer1_Move_Down(InputAction.CallbackContext context);
        void OnPlayer1_Move_Down_M(InputAction.CallbackContext context);
        void OnPlayer1_Move_Left(InputAction.CallbackContext context);
        void OnPlayer1_Move_Left_M(InputAction.CallbackContext context);
        void OnPlayer1_Move_Right(InputAction.CallbackContext context);
        void OnPlayer1_Move_Right_M(InputAction.CallbackContext context);
        void OnPlayer2_Move_Up(InputAction.CallbackContext context);
        void OnPlayer2_Move_Up_M(InputAction.CallbackContext context);
        void OnPlayer2_Move_Down(InputAction.CallbackContext context);
        void OnPlayer2_Move_Down_M(InputAction.CallbackContext context);
        void OnPlayer2_Move_Left(InputAction.CallbackContext context);
        void OnPlayer2_Move_Left_M(InputAction.CallbackContext context);
        void OnPlayer2_Move_Right(InputAction.CallbackContext context);
        void OnPlayer2_Move_Right_M(InputAction.CallbackContext context);
        void OnPlayer1_Move(InputAction.CallbackContext context);
        void OnPlayer2_Move(InputAction.CallbackContext context);
        void OnPlayer2_RightStick(InputAction.CallbackContext context);
        void OnLeftRotateCamera(InputAction.CallbackContext context);
        void OnRightRotateCamera(InputAction.CallbackContext context);
    }
    public interface ICommonActions
    {
        void OnMouseLeftClick(InputAction.CallbackContext context);
        void OnMouseRightClick(InputAction.CallbackContext context);
        void OnMouseMiddleClick(InputAction.CallbackContext context);
        void OnMousePosition(InputAction.CallbackContext context);
        void OnMouseWheel(InputAction.CallbackContext context);
        void OnExit(InputAction.CallbackContext context);
        void OnTab(InputAction.CallbackContext context);
        void OnConfirm(InputAction.CallbackContext context);
        void OnDebug(InputAction.CallbackContext context);
        void OnRestartGame(InputAction.CallbackContext context);
        void OnRestartGameR(InputAction.CallbackContext context);
        void OnPauseGame(InputAction.CallbackContext context);
    }
    public interface IEditorActions
    {
        void OnMousePosition(InputAction.CallbackContext context);
        void OnMouseWheel(InputAction.CallbackContext context);
        void OnMouseMiddleClick(InputAction.CallbackContext context);
        void OnMouseRightClick(InputAction.CallbackContext context);
        void OnMouseLeftClick(InputAction.CallbackContext context);
    }
}