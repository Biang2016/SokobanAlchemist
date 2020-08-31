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
                    ""name"": ""Player1Move"",
                    ""type"": ""Value"",
                    ""id"": ""bd466e74-41ff-4ec2-9bc2-c86fdc792344"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Player2Move"",
                    ""type"": ""Value"",
                    ""id"": ""35c1c381-d25b-4e4d-b776-f4747eace5e0"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
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
                    ""path"": ""<Keyboard>/rightCtrl"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
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
                    ""id"": ""aa9bcc24-85ce-491a-ad30-60acc3a6e5b5"",
                    ""path"": ""<Gamepad>/leftStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Player1Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""WASD"",
                    ""id"": ""6b95673d-8a12-412a-9390-8184253550b8"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Player1Move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""b0ca1bb8-34d0-457b-8918-390c38c82bf3"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Player1Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""5faf8d5b-04de-42a3-80e9-df1147142717"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Player1Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""e9718a4c-1f96-49cf-8f1f-0f1f9488a89f"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Player1Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""5e2988c1-cf9c-4a5c-aaeb-3d177349c4e8"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Player1Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
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
                    ""id"": ""d52fa57f-3fae-4b12-b993-c66604d86f81"",
                    ""path"": ""<Gamepad>/leftStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Player2Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""Arrows"",
                    ""id"": ""196d71d6-e36e-4eca-999d-fd636c364557"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Player2Move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""7a711c63-0ded-4971-bfd5-6763efa9cc99"",
                    ""path"": ""<Keyboard>/upArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Player2Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""dffaee49-73b3-4701-afe0-60c62257a353"",
                    ""path"": ""<Keyboard>/downArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Player2Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""c1ba0d01-1c82-4f84-8abd-a22dcbbd2d1b"",
                    ""path"": ""<Keyboard>/leftArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Player2Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""db40706c-de41-411a-a957-a2c14e5ac15e"",
                    ""path"": ""<Keyboard>/rightArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Player2Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""dd014788-5e27-4daf-9256-adfce84e4ba7"",
                    ""path"": ""<Keyboard>/j"",
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
                }
            ]
        },
        {
            ""name"": ""BuildingInput"",
            ""id"": ""e3b1181f-7121-4f94-89e7-addd0d7b9803"",
            ""actions"": [
                {
                    ""name"": ""MouseLeftClick"",
                    ""type"": ""Button"",
                    ""id"": ""015b758b-31e4-4e14-ab9d-383d30922006"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""Move"",
                    ""type"": ""Value"",
                    ""id"": ""3e484d28-18ca-4c8e-a899-7cdaff5fab1d"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""MouseRightClick"",
                    ""type"": ""Button"",
                    ""id"": ""dfc6415d-0fa1-4f02-897c-0a00d4f2c106"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""MouseMiddleClick"",
                    ""type"": ""Button"",
                    ""id"": ""91a6bbaa-0bcc-4eb3-b451-59fa66970d2d"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""RotateItem"",
                    ""type"": ""Button"",
                    ""id"": ""097c04ba-5832-435d-8a3d-368f89c5f76b"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""ToggleBackpack"",
                    ""type"": ""Button"",
                    ""id"": ""9c4bb76c-4707-4e5c-a7f3-74e9d4fe4aa9"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""ToggleWireLines"",
                    ""type"": ""Button"",
                    ""id"": ""ab7dc26b-d210-49a5-bc1e-93e822716dbc"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""ToggleDebug"",
                    ""type"": ""Button"",
                    ""id"": ""ff7e40b2-232a-4a3e-bd3c-5911d1a7078e"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""631bf7b5-0411-4e1c-a8bb-042821cc53d0"",
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
                    ""id"": ""55efd318-f9f9-4475-b5a8-b4074461a8a9"",
                    ""path"": ""<Keyboard>/r"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""RotateItem"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""f36765b4-70ff-4ede-b1f7-71db8bd78fd1"",
                    ""path"": ""<Keyboard>/tab"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""ToggleBackpack"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""b23b49ab-92c7-4c71-9d25-26e508838519"",
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
                    ""id"": ""8f46ccae-7a7e-4634-b2f9-1e1afb241b88"",
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
                    ""id"": ""da559815-77f3-4f03-aa50-6e7dfd6bc3e7"",
                    ""path"": ""<Keyboard>/U"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""ToggleDebug"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""18410dfb-989a-4efe-b6ca-8529847e146b"",
                    ""path"": ""<Keyboard>/g"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""ToggleWireLines"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""054689b7-60de-4224-8ea7-a74cd32b3c3c"",
                    ""path"": ""<Gamepad>/leftStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""WASD"",
                    ""id"": ""17d857f9-cc87-416c-b57c-72e8d84ae6a1"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""3b986b9f-f881-43da-9d46-24b198c0411a"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""cc3cfbc1-9f05-4f9a-a2e8-4d31cf4b5ba6"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""978feba3-2e79-49c8-9001-7ee444c4500f"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""853fe531-968f-48f1-b307-1e18488081a9"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
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
                    ""path"": ""*/{Point}"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse;Gamepad"",
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
        m_BattleInput_Player1Move = m_BattleInput.FindAction("Player1Move", throwIfNotFound: true);
        m_BattleInput_Player2Move = m_BattleInput.FindAction("Player2Move", throwIfNotFound: true);
        m_BattleInput_Skill_0_Player1 = m_BattleInput.FindAction("Skill_0_Player1", throwIfNotFound: true);
        m_BattleInput_Skill_1_Player1 = m_BattleInput.FindAction("Skill_1_Player1", throwIfNotFound: true);
        m_BattleInput_Skill_0_Player2 = m_BattleInput.FindAction("Skill_0_Player2", throwIfNotFound: true);
        m_BattleInput_Skill_1_Player2 = m_BattleInput.FindAction("Skill_1_Player2", throwIfNotFound: true);
        m_BattleInput_ToggleBattleTip = m_BattleInput.FindAction("ToggleBattleTip", throwIfNotFound: true);
        // BuildingInput
        m_BuildingInput = asset.FindActionMap("BuildingInput", throwIfNotFound: true);
        m_BuildingInput_MouseLeftClick = m_BuildingInput.FindAction("MouseLeftClick", throwIfNotFound: true);
        m_BuildingInput_Move = m_BuildingInput.FindAction("Move", throwIfNotFound: true);
        m_BuildingInput_MouseRightClick = m_BuildingInput.FindAction("MouseRightClick", throwIfNotFound: true);
        m_BuildingInput_MouseMiddleClick = m_BuildingInput.FindAction("MouseMiddleClick", throwIfNotFound: true);
        m_BuildingInput_RotateItem = m_BuildingInput.FindAction("RotateItem", throwIfNotFound: true);
        m_BuildingInput_ToggleBackpack = m_BuildingInput.FindAction("ToggleBackpack", throwIfNotFound: true);
        m_BuildingInput_ToggleWireLines = m_BuildingInput.FindAction("ToggleWireLines", throwIfNotFound: true);
        m_BuildingInput_ToggleDebug = m_BuildingInput.FindAction("ToggleDebug", throwIfNotFound: true);
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
    private readonly InputAction m_BattleInput_Player1Move;
    private readonly InputAction m_BattleInput_Player2Move;
    private readonly InputAction m_BattleInput_Skill_0_Player1;
    private readonly InputAction m_BattleInput_Skill_1_Player1;
    private readonly InputAction m_BattleInput_Skill_0_Player2;
    private readonly InputAction m_BattleInput_Skill_1_Player2;
    private readonly InputAction m_BattleInput_ToggleBattleTip;
    public struct BattleInputActions
    {
        private @PlayerInput m_Wrapper;
        public BattleInputActions(@PlayerInput wrapper) { m_Wrapper = wrapper; }
        public InputAction @MouseLeftClick => m_Wrapper.m_BattleInput_MouseLeftClick;
        public InputAction @MouseRightClick => m_Wrapper.m_BattleInput_MouseRightClick;
        public InputAction @MouseMiddleClick => m_Wrapper.m_BattleInput_MouseMiddleClick;
        public InputAction @Player1Move => m_Wrapper.m_BattleInput_Player1Move;
        public InputAction @Player2Move => m_Wrapper.m_BattleInput_Player2Move;
        public InputAction @Skill_0_Player1 => m_Wrapper.m_BattleInput_Skill_0_Player1;
        public InputAction @Skill_1_Player1 => m_Wrapper.m_BattleInput_Skill_1_Player1;
        public InputAction @Skill_0_Player2 => m_Wrapper.m_BattleInput_Skill_0_Player2;
        public InputAction @Skill_1_Player2 => m_Wrapper.m_BattleInput_Skill_1_Player2;
        public InputAction @ToggleBattleTip => m_Wrapper.m_BattleInput_ToggleBattleTip;
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
                @Player1Move.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer1Move;
                @Player1Move.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer1Move;
                @Player1Move.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer1Move;
                @Player2Move.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer2Move;
                @Player2Move.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer2Move;
                @Player2Move.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnPlayer2Move;
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
                @Player1Move.started += instance.OnPlayer1Move;
                @Player1Move.performed += instance.OnPlayer1Move;
                @Player1Move.canceled += instance.OnPlayer1Move;
                @Player2Move.started += instance.OnPlayer2Move;
                @Player2Move.performed += instance.OnPlayer2Move;
                @Player2Move.canceled += instance.OnPlayer2Move;
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
            }
        }
    }
    public BattleInputActions @BattleInput => new BattleInputActions(this);

    // BuildingInput
    private readonly InputActionMap m_BuildingInput;
    private IBuildingInputActions m_BuildingInputActionsCallbackInterface;
    private readonly InputAction m_BuildingInput_MouseLeftClick;
    private readonly InputAction m_BuildingInput_Move;
    private readonly InputAction m_BuildingInput_MouseRightClick;
    private readonly InputAction m_BuildingInput_MouseMiddleClick;
    private readonly InputAction m_BuildingInput_RotateItem;
    private readonly InputAction m_BuildingInput_ToggleBackpack;
    private readonly InputAction m_BuildingInput_ToggleWireLines;
    private readonly InputAction m_BuildingInput_ToggleDebug;
    public struct BuildingInputActions
    {
        private @PlayerInput m_Wrapper;
        public BuildingInputActions(@PlayerInput wrapper) { m_Wrapper = wrapper; }
        public InputAction @MouseLeftClick => m_Wrapper.m_BuildingInput_MouseLeftClick;
        public InputAction @Move => m_Wrapper.m_BuildingInput_Move;
        public InputAction @MouseRightClick => m_Wrapper.m_BuildingInput_MouseRightClick;
        public InputAction @MouseMiddleClick => m_Wrapper.m_BuildingInput_MouseMiddleClick;
        public InputAction @RotateItem => m_Wrapper.m_BuildingInput_RotateItem;
        public InputAction @ToggleBackpack => m_Wrapper.m_BuildingInput_ToggleBackpack;
        public InputAction @ToggleWireLines => m_Wrapper.m_BuildingInput_ToggleWireLines;
        public InputAction @ToggleDebug => m_Wrapper.m_BuildingInput_ToggleDebug;
        public InputActionMap Get() { return m_Wrapper.m_BuildingInput; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(BuildingInputActions set) { return set.Get(); }
        public void SetCallbacks(IBuildingInputActions instance)
        {
            if (m_Wrapper.m_BuildingInputActionsCallbackInterface != null)
            {
                @MouseLeftClick.started -= m_Wrapper.m_BuildingInputActionsCallbackInterface.OnMouseLeftClick;
                @MouseLeftClick.performed -= m_Wrapper.m_BuildingInputActionsCallbackInterface.OnMouseLeftClick;
                @MouseLeftClick.canceled -= m_Wrapper.m_BuildingInputActionsCallbackInterface.OnMouseLeftClick;
                @Move.started -= m_Wrapper.m_BuildingInputActionsCallbackInterface.OnMove;
                @Move.performed -= m_Wrapper.m_BuildingInputActionsCallbackInterface.OnMove;
                @Move.canceled -= m_Wrapper.m_BuildingInputActionsCallbackInterface.OnMove;
                @MouseRightClick.started -= m_Wrapper.m_BuildingInputActionsCallbackInterface.OnMouseRightClick;
                @MouseRightClick.performed -= m_Wrapper.m_BuildingInputActionsCallbackInterface.OnMouseRightClick;
                @MouseRightClick.canceled -= m_Wrapper.m_BuildingInputActionsCallbackInterface.OnMouseRightClick;
                @MouseMiddleClick.started -= m_Wrapper.m_BuildingInputActionsCallbackInterface.OnMouseMiddleClick;
                @MouseMiddleClick.performed -= m_Wrapper.m_BuildingInputActionsCallbackInterface.OnMouseMiddleClick;
                @MouseMiddleClick.canceled -= m_Wrapper.m_BuildingInputActionsCallbackInterface.OnMouseMiddleClick;
                @RotateItem.started -= m_Wrapper.m_BuildingInputActionsCallbackInterface.OnRotateItem;
                @RotateItem.performed -= m_Wrapper.m_BuildingInputActionsCallbackInterface.OnRotateItem;
                @RotateItem.canceled -= m_Wrapper.m_BuildingInputActionsCallbackInterface.OnRotateItem;
                @ToggleBackpack.started -= m_Wrapper.m_BuildingInputActionsCallbackInterface.OnToggleBackpack;
                @ToggleBackpack.performed -= m_Wrapper.m_BuildingInputActionsCallbackInterface.OnToggleBackpack;
                @ToggleBackpack.canceled -= m_Wrapper.m_BuildingInputActionsCallbackInterface.OnToggleBackpack;
                @ToggleWireLines.started -= m_Wrapper.m_BuildingInputActionsCallbackInterface.OnToggleWireLines;
                @ToggleWireLines.performed -= m_Wrapper.m_BuildingInputActionsCallbackInterface.OnToggleWireLines;
                @ToggleWireLines.canceled -= m_Wrapper.m_BuildingInputActionsCallbackInterface.OnToggleWireLines;
                @ToggleDebug.started -= m_Wrapper.m_BuildingInputActionsCallbackInterface.OnToggleDebug;
                @ToggleDebug.performed -= m_Wrapper.m_BuildingInputActionsCallbackInterface.OnToggleDebug;
                @ToggleDebug.canceled -= m_Wrapper.m_BuildingInputActionsCallbackInterface.OnToggleDebug;
            }
            m_Wrapper.m_BuildingInputActionsCallbackInterface = instance;
            if (instance != null)
            {
                @MouseLeftClick.started += instance.OnMouseLeftClick;
                @MouseLeftClick.performed += instance.OnMouseLeftClick;
                @MouseLeftClick.canceled += instance.OnMouseLeftClick;
                @Move.started += instance.OnMove;
                @Move.performed += instance.OnMove;
                @Move.canceled += instance.OnMove;
                @MouseRightClick.started += instance.OnMouseRightClick;
                @MouseRightClick.performed += instance.OnMouseRightClick;
                @MouseRightClick.canceled += instance.OnMouseRightClick;
                @MouseMiddleClick.started += instance.OnMouseMiddleClick;
                @MouseMiddleClick.performed += instance.OnMouseMiddleClick;
                @MouseMiddleClick.canceled += instance.OnMouseMiddleClick;
                @RotateItem.started += instance.OnRotateItem;
                @RotateItem.performed += instance.OnRotateItem;
                @RotateItem.canceled += instance.OnRotateItem;
                @ToggleBackpack.started += instance.OnToggleBackpack;
                @ToggleBackpack.performed += instance.OnToggleBackpack;
                @ToggleBackpack.canceled += instance.OnToggleBackpack;
                @ToggleWireLines.started += instance.OnToggleWireLines;
                @ToggleWireLines.performed += instance.OnToggleWireLines;
                @ToggleWireLines.canceled += instance.OnToggleWireLines;
                @ToggleDebug.started += instance.OnToggleDebug;
                @ToggleDebug.performed += instance.OnToggleDebug;
                @ToggleDebug.canceled += instance.OnToggleDebug;
            }
        }
    }
    public BuildingInputActions @BuildingInput => new BuildingInputActions(this);

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
            }
        }
    }
    public CommonActions @Common => new CommonActions(this);
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
        void OnPlayer1Move(InputAction.CallbackContext context);
        void OnPlayer2Move(InputAction.CallbackContext context);
        void OnSkill_0_Player1(InputAction.CallbackContext context);
        void OnSkill_1_Player1(InputAction.CallbackContext context);
        void OnSkill_0_Player2(InputAction.CallbackContext context);
        void OnSkill_1_Player2(InputAction.CallbackContext context);
        void OnToggleBattleTip(InputAction.CallbackContext context);
    }
    public interface IBuildingInputActions
    {
        void OnMouseLeftClick(InputAction.CallbackContext context);
        void OnMove(InputAction.CallbackContext context);
        void OnMouseRightClick(InputAction.CallbackContext context);
        void OnMouseMiddleClick(InputAction.CallbackContext context);
        void OnRotateItem(InputAction.CallbackContext context);
        void OnToggleBackpack(InputAction.CallbackContext context);
        void OnToggleWireLines(InputAction.CallbackContext context);
        void OnToggleDebug(InputAction.CallbackContext context);
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
    }
}
