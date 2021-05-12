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
                    ""name"": ""Skill_2_Player1"",
                    ""type"": ""Button"",
                    ""id"": ""56c0a96f-5fdd-4977-a34c-cb8d08314868"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""Skill_3_Player1"",
                    ""type"": ""Button"",
                    ""id"": ""f3ab6494-6327-40d1-bce5-c244d4718670"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""Skill_4_Player1"",
                    ""type"": ""Button"",
                    ""id"": ""e35f817b-28af-4fc8-aa2c-fb0cb6e5a65f"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""Skill_5_Player1"",
                    ""type"": ""Button"",
                    ""id"": ""8a0f370f-ffce-474c-9fcd-8d988b7e0fb0"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""Skill_6_Player1"",
                    ""type"": ""Button"",
                    ""id"": ""53becb5b-a8ee-4368-8952-2432e1d63610"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""Skill_7_Player1"",
                    ""type"": ""Button"",
                    ""id"": ""d855dc9c-abcd-410a-9e78-2d49126a218d"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""Skill_8_Player1"",
                    ""type"": ""Button"",
                    ""id"": ""73cd1365-8f31-4295-ba0a-617d20d4f425"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""Skill_9_Player1"",
                    ""type"": ""Button"",
                    ""id"": ""8efa629a-35cf-4cde-a2fc-18c13759f640"",
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
                    ""name"": ""Skill_2_Player2"",
                    ""type"": ""Button"",
                    ""id"": ""f7ea5b2e-833b-4a59-830b-a0433cf111ba"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""Skill_3_Player2"",
                    ""type"": ""Button"",
                    ""id"": ""dbe2b26f-d074-4dc0-9ecf-76019d3c0a63"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""Skill_4_Player2"",
                    ""type"": ""Button"",
                    ""id"": ""04b493f3-2ff4-4924-be4e-c7bedbe130d3"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""Skill_5_Player2"",
                    ""type"": ""Button"",
                    ""id"": ""f82f503d-3265-4198-b09c-7e2484872305"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""Skill_6_Player2"",
                    ""type"": ""Button"",
                    ""id"": ""1ef5ea9d-a594-4b09-a103-c0e45c69be8c"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""Skill_7_Player2"",
                    ""type"": ""Button"",
                    ""id"": ""b93337c7-ac97-4889-9698-68ee4ed80901"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""Skill_8_Player2"",
                    ""type"": ""Button"",
                    ""id"": ""03069b32-8d95-4b82-b5bb-db9942b25fd2"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""Skill_9_Player2"",
                    ""type"": ""Button"",
                    ""id"": ""b16cdbc7-9775-4f0f-b9bc-13b7dc382cce"",
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
                    ""name"": ""LeftSwitch"",
                    ""type"": ""Button"",
                    ""id"": ""4c0ad541-70b9-4323-b90a-f89bd96489f9"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""RightSwitch"",
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
                    ""id"": ""f9a3469e-bf26-401d-bd90-5b657d01ce7e"",
                    ""path"": ""<Gamepad>/leftTrigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Skill_0_Player1"",
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
                    ""path"": ""<Keyboard>/shift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Skill_1_Player1"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e989287b-4522-41b7-b1be-0fdef8cccdcf"",
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
                    ""id"": ""89174a94-8917-436b-af54-0e7a36d0e671"",
                    ""path"": ""<Gamepad>/leftStick/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Player1_Move_Up"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""794a8a21-9ea7-4190-8fc8-faf5f2839308"",
                    ""path"": ""<Gamepad>/dpad/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
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
                    ""id"": ""f8bef0d8-d1b8-4ff4-b083-fa066e08017e"",
                    ""path"": ""<Gamepad>/leftStick/down"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Player1_Move_Down"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""9530f8cd-b483-4712-aec1-193cd59b4b14"",
                    ""path"": ""<Gamepad>/dpad/down"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Player1_Move_Down"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""f837923d-decb-48c4-ba0a-c2b56eb0f97b"",
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
                    ""id"": ""6f1c57bd-e840-4c8e-8849-1d75c830c95b"",
                    ""path"": ""<Gamepad>/leftStick/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Player1_Move_Left"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""0404f8b0-d031-4c65-b77c-93eec5159c21"",
                    ""path"": ""<Gamepad>/dpad/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Player1_Move_Left"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""adbaaff8-f511-49c9-ac33-9a9087e34102"",
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
                    ""id"": ""99b96bb6-5383-44e5-b22c-ef8a09e61d43"",
                    ""path"": ""<Gamepad>/leftStick/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Player1_Move_Right"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""dc1356af-dcb5-4ca1-94f8-297f9fe4c976"",
                    ""path"": ""<Gamepad>/dpad/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
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
                    ""path"": ""<Keyboard>/upArrow"",
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
                    ""path"": ""<Keyboard>/downArrow"",
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
                    ""path"": ""<Keyboard>/leftArrow"",
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
                    ""path"": ""<Keyboard>/rightArrow"",
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
                    ""path"": ""<Keyboard>/d"",
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
                    ""name"": ""GamePadLeftStick"",
                    ""id"": ""f9011cab-874b-4d49-8173-82bd461e5a32"",
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
                    ""id"": ""cffdfc70-5dfe-4530-ba72-c5701d3668b3"",
                    ""path"": ""<Gamepad>/leftStick/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Player1_Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""e6962ea0-690c-4465-9b98-f4f44f254116"",
                    ""path"": ""<Gamepad>/leftStick/down"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Player1_Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""1d4eb31a-b9ba-4699-8867-7a7fc4d7a643"",
                    ""path"": ""<Gamepad>/leftStick/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Player1_Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""11cbde51-2061-40db-989b-4685b61618b8"",
                    ""path"": ""<Gamepad>/leftStick/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Player1_Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""GamePadDPad"",
                    ""id"": ""52bc9b2d-4666-49d8-af83-1e986d7ad84a"",
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
                    ""id"": ""15be6eae-76b5-440f-bf06-8260de56e6ce"",
                    ""path"": ""<Gamepad>/dpad/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Player1_Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""d09b9478-a630-4ff2-b8f8-492d74190a37"",
                    ""path"": ""<Gamepad>/dpad/down"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Player1_Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""4fc9b8bf-d15c-4a28-8ecc-8ca2c1d372bd"",
                    ""path"": ""<Gamepad>/dpad/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Player1_Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""08ca0041-7fd1-487e-92e9-f2cb95924c31"",
                    ""path"": ""<Gamepad>/dpad/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Player1_Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""bb7a45e9-abe3-4220-bd4a-d31e251e0abc"",
                    ""path"": ""<Keyboard>/#(Q)"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""LeftSwitch"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""dc807174-4bc3-4cbf-a080-cbb23c674455"",
                    ""path"": ""<Gamepad>/leftShoulder"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""LeftSwitch"",
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
                    ""action"": ""RightSwitch"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""0dde1976-62bd-4118-9e7b-12e0cf3b0b3c"",
                    ""path"": ""<Gamepad>/rightShoulder"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""RightSwitch"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""5b02d28f-f1a0-4ef3-9507-eec407c0ce55"",
                    ""path"": ""<Keyboard>/h"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Skill_2_Player1"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""dde48212-40ab-410c-8a61-41c61bbb979f"",
                    ""path"": ""<Gamepad>/leftStick/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Player1_Move_Up_M"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""31d9b27b-e6e2-44e6-9a36-c3e0dba0302f"",
                    ""path"": ""<Gamepad>/dpad/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Player1_Move_Up_M"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""686a5268-dc75-42c8-8fec-85a4c3fd2759"",
                    ""path"": ""<Gamepad>/leftStick/down"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Player1_Move_Down_M"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""28580cdc-3e9c-4265-b977-9847673506b9"",
                    ""path"": ""<Gamepad>/dpad/down"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Player1_Move_Down_M"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d7d435dd-4666-44de-b50d-1ba261c4b8e3"",
                    ""path"": ""<Gamepad>/leftStick/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Player1_Move_Left_M"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""6d55e847-44d2-432b-a6cc-ed90198061df"",
                    ""path"": ""<Gamepad>/dpad/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Player1_Move_Left_M"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""f93d5740-c987-4e90-b15f-157509e043a7"",
                    ""path"": ""<Gamepad>/leftStick/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Player1_Move_Right_M"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""be0298fc-6e71-492e-b935-5a64bc612766"",
                    ""path"": ""<Gamepad>/dpad/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Player1_Move_Right_M"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""045f97e4-150e-4a25-b7a3-4684bbe95a18"",
                    ""path"": ""<Keyboard>/j"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Skill_3_Player1"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e8838878-13ab-429b-b78f-d6b7ebdb196c"",
                    ""path"": ""<Gamepad>/buttonWest"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Skill_3_Player1"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d2e019b6-c4b5-420c-98d3-5148a59ac8ac"",
                    ""path"": ""<Keyboard>/k"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Skill_4_Player1"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""599fcc54-119c-4666-bc60-6572744f05c4"",
                    ""path"": ""<Gamepad>/buttonNorth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Skill_4_Player1"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a1139f59-ee57-4e63-9661-a469d3488e8d"",
                    ""path"": ""<Keyboard>/l"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Skill_5_Player1"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""c91b635c-363f-474f-a8e1-8fedb1fe46a5"",
                    ""path"": ""<Gamepad>/buttonEast"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Skill_5_Player1"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""ec90f968-9f44-44fb-83b5-eb6f1b4edfb2"",
                    ""path"": ""<Gamepad>/rightTrigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Skill_1_Player1"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""faee6af6-42c7-49be-9396-f94adc7e69db"",
                    ""path"": ""<Keyboard>/1"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Skill_6_Player1"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""842354ce-6ef4-4665-9cca-69f6d73fe0de"",
                    ""path"": ""<Keyboard>/2"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Skill_7_Player1"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""8e028770-d8c9-4b6f-91b6-08d44b461ba1"",
                    ""path"": ""<Keyboard>/3"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Skill_8_Player1"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""220bc1ef-37d0-41a7-929e-54ea83468db4"",
                    ""path"": ""<Keyboard>/4"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Skill_9_Player1"",
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
                    ""name"": ""Confirm"",
                    ""type"": ""Button"",
                    ""id"": ""9c95a13c-9036-4cae-9675-0ddef4b52926"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
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
                    ""name"": ""Cancel"",
                    ""type"": ""Button"",
                    ""id"": ""8d4f3577-d55d-4fac-a439-94225b57b9ad"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=1)""
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
                    ""name"": ""DebugConsole"",
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
                    ""name"": ""ReloadGame"",
                    ""type"": ""Button"",
                    ""id"": ""f08b4971-aebb-4143-b619-4cc77d25c2bb"",
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
                },
                {
                    ""name"": ""ToggleUI"",
                    ""type"": ""Button"",
                    ""id"": ""64044141-2d58-4858-a8df-455957fe23ea"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""ToggleDebugPanel"",
                    ""type"": ""Button"",
                    ""id"": ""25917861-bf33-45b7-906b-5b8ef997638c"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""InteractiveKey"",
                    ""type"": ""Button"",
                    ""id"": ""2a2737b3-6adb-4d9c-ab7c-96f34dd805db"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""SlowDownGame"",
                    ""type"": ""Button"",
                    ""id"": ""6b643af7-1749-4735-8ca9-f26b532e5816"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""ReturnToOpenWorld"",
                    ""type"": ""Button"",
                    ""id"": ""ec437b7c-4a56-412f-baae-d2f1fefb8239"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""SkillPreviewPanel"",
                    ""type"": ""Button"",
                    ""id"": ""c70aeac8-43cc-456c-b1e7-d3feabe67bc6"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""UINavigation"",
                    ""type"": ""Button"",
                    ""id"": ""bc02dd05-0578-4b0a-8f60-84da6c6b0a02"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press""
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""2349d29d-84fe-419b-b518-7447fc786c1d"",
                    ""path"": ""<Gamepad>/start"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Exit"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""0c377215-e8a4-41bd-9d41-3ce4171cdb58"",
                    ""path"": ""<Keyboard>/escape"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
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
                    ""id"": ""e143e42c-3e22-47ce-b879-1baa35066785"",
                    ""path"": ""<Gamepad>/select"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Tab"",
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
                    ""action"": ""DebugConsole"",
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
                    ""action"": ""RestartGame"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""50c4cd9c-6198-4d5d-b7b7-807142622972"",
                    ""path"": ""<Keyboard>/f3"",
                    ""interactions"": ""Press(behavior=2)"",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""ToggleUI"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""31728538-e785-458b-a6f4-15c2524dcb7b"",
                    ""path"": ""<Keyboard>/f4"",
                    ""interactions"": ""Press(behavior=2)"",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""ToggleDebugPanel"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""07a95fc5-987a-4f13-a436-67b3a7c561cc"",
                    ""path"": ""<Keyboard>/f"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""InteractiveKey"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d745739c-0960-4a12-9791-8253378e2546"",
                    ""path"": ""<Gamepad>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""InteractiveKey"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""ca04e5db-4ab9-41d3-ae12-6cccc327ba41"",
                    ""path"": ""<Keyboard>/equals"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""SlowDownGame"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""39a93977-ae39-4a82-976d-66c6ae2b3b47"",
                    ""path"": ""<Keyboard>/F10"",
                    ""interactions"": ""Press(behavior=2)"",
                    ""processors"": ""Clamp(max=1)"",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""ReloadGame"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d27e1062-395a-4e29-ab68-d66b2445373d"",
                    ""path"": ""<Keyboard>/b"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""ReturnToOpenWorld"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""05598b18-1fe4-4a74-86ce-59a8b88f126f"",
                    ""path"": ""<Keyboard>/n"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""SkillPreviewPanel"",
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
                    ""id"": ""90c64eb1-f2c1-40d0-9f13-1330c2807e89"",
                    ""path"": ""*/{Cancel}"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad;KeyboardMouse"",
                    ""action"": ""Cancel"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""DPad"",
                    ""id"": ""ab5cb213-d49b-40c4-9c69-08bd686fd274"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""UINavigation"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""f47bc717-2ecf-4528-a054-7adfdf2b0967"",
                    ""path"": ""<Gamepad>/dpad/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""UINavigation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""a7c57673-acf4-442d-9bd1-6c4f9b52a862"",
                    ""path"": ""<Gamepad>/dpad/down"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""UINavigation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""3858350b-3118-48d1-b4fc-2718511a1fb5"",
                    ""path"": ""<Gamepad>/dpad/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""UINavigation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""fc251bd0-a65f-466c-afa9-77c9ae21cbc3"",
                    ""path"": ""<Gamepad>/dpad/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""UINavigation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
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
        m_BattleInput_Skill_2_Player1 = m_BattleInput.FindAction("Skill_2_Player1", throwIfNotFound: true);
        m_BattleInput_Skill_3_Player1 = m_BattleInput.FindAction("Skill_3_Player1", throwIfNotFound: true);
        m_BattleInput_Skill_4_Player1 = m_BattleInput.FindAction("Skill_4_Player1", throwIfNotFound: true);
        m_BattleInput_Skill_5_Player1 = m_BattleInput.FindAction("Skill_5_Player1", throwIfNotFound: true);
        m_BattleInput_Skill_6_Player1 = m_BattleInput.FindAction("Skill_6_Player1", throwIfNotFound: true);
        m_BattleInput_Skill_7_Player1 = m_BattleInput.FindAction("Skill_7_Player1", throwIfNotFound: true);
        m_BattleInput_Skill_8_Player1 = m_BattleInput.FindAction("Skill_8_Player1", throwIfNotFound: true);
        m_BattleInput_Skill_9_Player1 = m_BattleInput.FindAction("Skill_9_Player1", throwIfNotFound: true);
        m_BattleInput_Skill_0_Player2 = m_BattleInput.FindAction("Skill_0_Player2", throwIfNotFound: true);
        m_BattleInput_Skill_1_Player2 = m_BattleInput.FindAction("Skill_1_Player2", throwIfNotFound: true);
        m_BattleInput_Skill_2_Player2 = m_BattleInput.FindAction("Skill_2_Player2", throwIfNotFound: true);
        m_BattleInput_Skill_3_Player2 = m_BattleInput.FindAction("Skill_3_Player2", throwIfNotFound: true);
        m_BattleInput_Skill_4_Player2 = m_BattleInput.FindAction("Skill_4_Player2", throwIfNotFound: true);
        m_BattleInput_Skill_5_Player2 = m_BattleInput.FindAction("Skill_5_Player2", throwIfNotFound: true);
        m_BattleInput_Skill_6_Player2 = m_BattleInput.FindAction("Skill_6_Player2", throwIfNotFound: true);
        m_BattleInput_Skill_7_Player2 = m_BattleInput.FindAction("Skill_7_Player2", throwIfNotFound: true);
        m_BattleInput_Skill_8_Player2 = m_BattleInput.FindAction("Skill_8_Player2", throwIfNotFound: true);
        m_BattleInput_Skill_9_Player2 = m_BattleInput.FindAction("Skill_9_Player2", throwIfNotFound: true);
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
        m_BattleInput_LeftSwitch = m_BattleInput.FindAction("LeftSwitch", throwIfNotFound: true);
        m_BattleInput_RightSwitch = m_BattleInput.FindAction("RightSwitch", throwIfNotFound: true);
        // Common
        m_Common = asset.FindActionMap("Common", throwIfNotFound: true);
        m_Common_MouseLeftClick = m_Common.FindAction("MouseLeftClick", throwIfNotFound: true);
        m_Common_MouseRightClick = m_Common.FindAction("MouseRightClick", throwIfNotFound: true);
        m_Common_MouseMiddleClick = m_Common.FindAction("MouseMiddleClick", throwIfNotFound: true);
        m_Common_MousePosition = m_Common.FindAction("MousePosition", throwIfNotFound: true);
        m_Common_MouseWheel = m_Common.FindAction("MouseWheel", throwIfNotFound: true);
        m_Common_Confirm = m_Common.FindAction("Confirm", throwIfNotFound: true);
        m_Common_Exit = m_Common.FindAction("Exit", throwIfNotFound: true);
        m_Common_Cancel = m_Common.FindAction("Cancel", throwIfNotFound: true);
        m_Common_Tab = m_Common.FindAction("Tab", throwIfNotFound: true);
        m_Common_DebugConsole = m_Common.FindAction("DebugConsole", throwIfNotFound: true);
        m_Common_RestartGame = m_Common.FindAction("RestartGame", throwIfNotFound: true);
        m_Common_ReloadGame = m_Common.FindAction("ReloadGame", throwIfNotFound: true);
        m_Common_PauseGame = m_Common.FindAction("PauseGame", throwIfNotFound: true);
        m_Common_ToggleUI = m_Common.FindAction("ToggleUI", throwIfNotFound: true);
        m_Common_ToggleDebugPanel = m_Common.FindAction("ToggleDebugPanel", throwIfNotFound: true);
        m_Common_InteractiveKey = m_Common.FindAction("InteractiveKey", throwIfNotFound: true);
        m_Common_SlowDownGame = m_Common.FindAction("SlowDownGame", throwIfNotFound: true);
        m_Common_ReturnToOpenWorld = m_Common.FindAction("ReturnToOpenWorld", throwIfNotFound: true);
        m_Common_SkillPreviewPanel = m_Common.FindAction("SkillPreviewPanel", throwIfNotFound: true);
        m_Common_UINavigation = m_Common.FindAction("UINavigation", throwIfNotFound: true);
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
    private readonly InputAction m_BattleInput_Skill_2_Player1;
    private readonly InputAction m_BattleInput_Skill_3_Player1;
    private readonly InputAction m_BattleInput_Skill_4_Player1;
    private readonly InputAction m_BattleInput_Skill_5_Player1;
    private readonly InputAction m_BattleInput_Skill_6_Player1;
    private readonly InputAction m_BattleInput_Skill_7_Player1;
    private readonly InputAction m_BattleInput_Skill_8_Player1;
    private readonly InputAction m_BattleInput_Skill_9_Player1;
    private readonly InputAction m_BattleInput_Skill_0_Player2;
    private readonly InputAction m_BattleInput_Skill_1_Player2;
    private readonly InputAction m_BattleInput_Skill_2_Player2;
    private readonly InputAction m_BattleInput_Skill_3_Player2;
    private readonly InputAction m_BattleInput_Skill_4_Player2;
    private readonly InputAction m_BattleInput_Skill_5_Player2;
    private readonly InputAction m_BattleInput_Skill_6_Player2;
    private readonly InputAction m_BattleInput_Skill_7_Player2;
    private readonly InputAction m_BattleInput_Skill_8_Player2;
    private readonly InputAction m_BattleInput_Skill_9_Player2;
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
    private readonly InputAction m_BattleInput_LeftSwitch;
    private readonly InputAction m_BattleInput_RightSwitch;
    public struct BattleInputActions
    {
        private @PlayerInput m_Wrapper;
        public BattleInputActions(@PlayerInput wrapper) { m_Wrapper = wrapper; }
        public InputAction @MouseLeftClick => m_Wrapper.m_BattleInput_MouseLeftClick;
        public InputAction @MouseRightClick => m_Wrapper.m_BattleInput_MouseRightClick;
        public InputAction @MouseMiddleClick => m_Wrapper.m_BattleInput_MouseMiddleClick;
        public InputAction @Skill_0_Player1 => m_Wrapper.m_BattleInput_Skill_0_Player1;
        public InputAction @Skill_1_Player1 => m_Wrapper.m_BattleInput_Skill_1_Player1;
        public InputAction @Skill_2_Player1 => m_Wrapper.m_BattleInput_Skill_2_Player1;
        public InputAction @Skill_3_Player1 => m_Wrapper.m_BattleInput_Skill_3_Player1;
        public InputAction @Skill_4_Player1 => m_Wrapper.m_BattleInput_Skill_4_Player1;
        public InputAction @Skill_5_Player1 => m_Wrapper.m_BattleInput_Skill_5_Player1;
        public InputAction @Skill_6_Player1 => m_Wrapper.m_BattleInput_Skill_6_Player1;
        public InputAction @Skill_7_Player1 => m_Wrapper.m_BattleInput_Skill_7_Player1;
        public InputAction @Skill_8_Player1 => m_Wrapper.m_BattleInput_Skill_8_Player1;
        public InputAction @Skill_9_Player1 => m_Wrapper.m_BattleInput_Skill_9_Player1;
        public InputAction @Skill_0_Player2 => m_Wrapper.m_BattleInput_Skill_0_Player2;
        public InputAction @Skill_1_Player2 => m_Wrapper.m_BattleInput_Skill_1_Player2;
        public InputAction @Skill_2_Player2 => m_Wrapper.m_BattleInput_Skill_2_Player2;
        public InputAction @Skill_3_Player2 => m_Wrapper.m_BattleInput_Skill_3_Player2;
        public InputAction @Skill_4_Player2 => m_Wrapper.m_BattleInput_Skill_4_Player2;
        public InputAction @Skill_5_Player2 => m_Wrapper.m_BattleInput_Skill_5_Player2;
        public InputAction @Skill_6_Player2 => m_Wrapper.m_BattleInput_Skill_6_Player2;
        public InputAction @Skill_7_Player2 => m_Wrapper.m_BattleInput_Skill_7_Player2;
        public InputAction @Skill_8_Player2 => m_Wrapper.m_BattleInput_Skill_8_Player2;
        public InputAction @Skill_9_Player2 => m_Wrapper.m_BattleInput_Skill_9_Player2;
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
        public InputAction @LeftSwitch => m_Wrapper.m_BattleInput_LeftSwitch;
        public InputAction @RightSwitch => m_Wrapper.m_BattleInput_RightSwitch;
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
                @Skill_2_Player1.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_2_Player1;
                @Skill_2_Player1.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_2_Player1;
                @Skill_2_Player1.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_2_Player1;
                @Skill_3_Player1.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_3_Player1;
                @Skill_3_Player1.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_3_Player1;
                @Skill_3_Player1.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_3_Player1;
                @Skill_4_Player1.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_4_Player1;
                @Skill_4_Player1.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_4_Player1;
                @Skill_4_Player1.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_4_Player1;
                @Skill_5_Player1.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_5_Player1;
                @Skill_5_Player1.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_5_Player1;
                @Skill_5_Player1.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_5_Player1;
                @Skill_6_Player1.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_6_Player1;
                @Skill_6_Player1.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_6_Player1;
                @Skill_6_Player1.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_6_Player1;
                @Skill_7_Player1.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_7_Player1;
                @Skill_7_Player1.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_7_Player1;
                @Skill_7_Player1.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_7_Player1;
                @Skill_8_Player1.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_8_Player1;
                @Skill_8_Player1.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_8_Player1;
                @Skill_8_Player1.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_8_Player1;
                @Skill_9_Player1.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_9_Player1;
                @Skill_9_Player1.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_9_Player1;
                @Skill_9_Player1.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_9_Player1;
                @Skill_0_Player2.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_0_Player2;
                @Skill_0_Player2.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_0_Player2;
                @Skill_0_Player2.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_0_Player2;
                @Skill_1_Player2.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_1_Player2;
                @Skill_1_Player2.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_1_Player2;
                @Skill_1_Player2.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_1_Player2;
                @Skill_2_Player2.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_2_Player2;
                @Skill_2_Player2.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_2_Player2;
                @Skill_2_Player2.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_2_Player2;
                @Skill_3_Player2.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_3_Player2;
                @Skill_3_Player2.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_3_Player2;
                @Skill_3_Player2.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_3_Player2;
                @Skill_4_Player2.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_4_Player2;
                @Skill_4_Player2.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_4_Player2;
                @Skill_4_Player2.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_4_Player2;
                @Skill_5_Player2.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_5_Player2;
                @Skill_5_Player2.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_5_Player2;
                @Skill_5_Player2.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_5_Player2;
                @Skill_6_Player2.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_6_Player2;
                @Skill_6_Player2.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_6_Player2;
                @Skill_6_Player2.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_6_Player2;
                @Skill_7_Player2.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_7_Player2;
                @Skill_7_Player2.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_7_Player2;
                @Skill_7_Player2.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_7_Player2;
                @Skill_8_Player2.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_8_Player2;
                @Skill_8_Player2.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_8_Player2;
                @Skill_8_Player2.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_8_Player2;
                @Skill_9_Player2.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_9_Player2;
                @Skill_9_Player2.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_9_Player2;
                @Skill_9_Player2.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnSkill_9_Player2;
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
                @LeftSwitch.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnLeftSwitch;
                @LeftSwitch.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnLeftSwitch;
                @LeftSwitch.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnLeftSwitch;
                @RightSwitch.started -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnRightSwitch;
                @RightSwitch.performed -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnRightSwitch;
                @RightSwitch.canceled -= m_Wrapper.m_BattleInputActionsCallbackInterface.OnRightSwitch;
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
                @Skill_2_Player1.started += instance.OnSkill_2_Player1;
                @Skill_2_Player1.performed += instance.OnSkill_2_Player1;
                @Skill_2_Player1.canceled += instance.OnSkill_2_Player1;
                @Skill_3_Player1.started += instance.OnSkill_3_Player1;
                @Skill_3_Player1.performed += instance.OnSkill_3_Player1;
                @Skill_3_Player1.canceled += instance.OnSkill_3_Player1;
                @Skill_4_Player1.started += instance.OnSkill_4_Player1;
                @Skill_4_Player1.performed += instance.OnSkill_4_Player1;
                @Skill_4_Player1.canceled += instance.OnSkill_4_Player1;
                @Skill_5_Player1.started += instance.OnSkill_5_Player1;
                @Skill_5_Player1.performed += instance.OnSkill_5_Player1;
                @Skill_5_Player1.canceled += instance.OnSkill_5_Player1;
                @Skill_6_Player1.started += instance.OnSkill_6_Player1;
                @Skill_6_Player1.performed += instance.OnSkill_6_Player1;
                @Skill_6_Player1.canceled += instance.OnSkill_6_Player1;
                @Skill_7_Player1.started += instance.OnSkill_7_Player1;
                @Skill_7_Player1.performed += instance.OnSkill_7_Player1;
                @Skill_7_Player1.canceled += instance.OnSkill_7_Player1;
                @Skill_8_Player1.started += instance.OnSkill_8_Player1;
                @Skill_8_Player1.performed += instance.OnSkill_8_Player1;
                @Skill_8_Player1.canceled += instance.OnSkill_8_Player1;
                @Skill_9_Player1.started += instance.OnSkill_9_Player1;
                @Skill_9_Player1.performed += instance.OnSkill_9_Player1;
                @Skill_9_Player1.canceled += instance.OnSkill_9_Player1;
                @Skill_0_Player2.started += instance.OnSkill_0_Player2;
                @Skill_0_Player2.performed += instance.OnSkill_0_Player2;
                @Skill_0_Player2.canceled += instance.OnSkill_0_Player2;
                @Skill_1_Player2.started += instance.OnSkill_1_Player2;
                @Skill_1_Player2.performed += instance.OnSkill_1_Player2;
                @Skill_1_Player2.canceled += instance.OnSkill_1_Player2;
                @Skill_2_Player2.started += instance.OnSkill_2_Player2;
                @Skill_2_Player2.performed += instance.OnSkill_2_Player2;
                @Skill_2_Player2.canceled += instance.OnSkill_2_Player2;
                @Skill_3_Player2.started += instance.OnSkill_3_Player2;
                @Skill_3_Player2.performed += instance.OnSkill_3_Player2;
                @Skill_3_Player2.canceled += instance.OnSkill_3_Player2;
                @Skill_4_Player2.started += instance.OnSkill_4_Player2;
                @Skill_4_Player2.performed += instance.OnSkill_4_Player2;
                @Skill_4_Player2.canceled += instance.OnSkill_4_Player2;
                @Skill_5_Player2.started += instance.OnSkill_5_Player2;
                @Skill_5_Player2.performed += instance.OnSkill_5_Player2;
                @Skill_5_Player2.canceled += instance.OnSkill_5_Player2;
                @Skill_6_Player2.started += instance.OnSkill_6_Player2;
                @Skill_6_Player2.performed += instance.OnSkill_6_Player2;
                @Skill_6_Player2.canceled += instance.OnSkill_6_Player2;
                @Skill_7_Player2.started += instance.OnSkill_7_Player2;
                @Skill_7_Player2.performed += instance.OnSkill_7_Player2;
                @Skill_7_Player2.canceled += instance.OnSkill_7_Player2;
                @Skill_8_Player2.started += instance.OnSkill_8_Player2;
                @Skill_8_Player2.performed += instance.OnSkill_8_Player2;
                @Skill_8_Player2.canceled += instance.OnSkill_8_Player2;
                @Skill_9_Player2.started += instance.OnSkill_9_Player2;
                @Skill_9_Player2.performed += instance.OnSkill_9_Player2;
                @Skill_9_Player2.canceled += instance.OnSkill_9_Player2;
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
                @LeftSwitch.started += instance.OnLeftSwitch;
                @LeftSwitch.performed += instance.OnLeftSwitch;
                @LeftSwitch.canceled += instance.OnLeftSwitch;
                @RightSwitch.started += instance.OnRightSwitch;
                @RightSwitch.performed += instance.OnRightSwitch;
                @RightSwitch.canceled += instance.OnRightSwitch;
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
    private readonly InputAction m_Common_Confirm;
    private readonly InputAction m_Common_Exit;
    private readonly InputAction m_Common_Cancel;
    private readonly InputAction m_Common_Tab;
    private readonly InputAction m_Common_DebugConsole;
    private readonly InputAction m_Common_RestartGame;
    private readonly InputAction m_Common_ReloadGame;
    private readonly InputAction m_Common_PauseGame;
    private readonly InputAction m_Common_ToggleUI;
    private readonly InputAction m_Common_ToggleDebugPanel;
    private readonly InputAction m_Common_InteractiveKey;
    private readonly InputAction m_Common_SlowDownGame;
    private readonly InputAction m_Common_ReturnToOpenWorld;
    private readonly InputAction m_Common_SkillPreviewPanel;
    private readonly InputAction m_Common_UINavigation;
    public struct CommonActions
    {
        private @PlayerInput m_Wrapper;
        public CommonActions(@PlayerInput wrapper) { m_Wrapper = wrapper; }
        public InputAction @MouseLeftClick => m_Wrapper.m_Common_MouseLeftClick;
        public InputAction @MouseRightClick => m_Wrapper.m_Common_MouseRightClick;
        public InputAction @MouseMiddleClick => m_Wrapper.m_Common_MouseMiddleClick;
        public InputAction @MousePosition => m_Wrapper.m_Common_MousePosition;
        public InputAction @MouseWheel => m_Wrapper.m_Common_MouseWheel;
        public InputAction @Confirm => m_Wrapper.m_Common_Confirm;
        public InputAction @Exit => m_Wrapper.m_Common_Exit;
        public InputAction @Cancel => m_Wrapper.m_Common_Cancel;
        public InputAction @Tab => m_Wrapper.m_Common_Tab;
        public InputAction @DebugConsole => m_Wrapper.m_Common_DebugConsole;
        public InputAction @RestartGame => m_Wrapper.m_Common_RestartGame;
        public InputAction @ReloadGame => m_Wrapper.m_Common_ReloadGame;
        public InputAction @PauseGame => m_Wrapper.m_Common_PauseGame;
        public InputAction @ToggleUI => m_Wrapper.m_Common_ToggleUI;
        public InputAction @ToggleDebugPanel => m_Wrapper.m_Common_ToggleDebugPanel;
        public InputAction @InteractiveKey => m_Wrapper.m_Common_InteractiveKey;
        public InputAction @SlowDownGame => m_Wrapper.m_Common_SlowDownGame;
        public InputAction @ReturnToOpenWorld => m_Wrapper.m_Common_ReturnToOpenWorld;
        public InputAction @SkillPreviewPanel => m_Wrapper.m_Common_SkillPreviewPanel;
        public InputAction @UINavigation => m_Wrapper.m_Common_UINavigation;
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
                @Confirm.started -= m_Wrapper.m_CommonActionsCallbackInterface.OnConfirm;
                @Confirm.performed -= m_Wrapper.m_CommonActionsCallbackInterface.OnConfirm;
                @Confirm.canceled -= m_Wrapper.m_CommonActionsCallbackInterface.OnConfirm;
                @Exit.started -= m_Wrapper.m_CommonActionsCallbackInterface.OnExit;
                @Exit.performed -= m_Wrapper.m_CommonActionsCallbackInterface.OnExit;
                @Exit.canceled -= m_Wrapper.m_CommonActionsCallbackInterface.OnExit;
                @Cancel.started -= m_Wrapper.m_CommonActionsCallbackInterface.OnCancel;
                @Cancel.performed -= m_Wrapper.m_CommonActionsCallbackInterface.OnCancel;
                @Cancel.canceled -= m_Wrapper.m_CommonActionsCallbackInterface.OnCancel;
                @Tab.started -= m_Wrapper.m_CommonActionsCallbackInterface.OnTab;
                @Tab.performed -= m_Wrapper.m_CommonActionsCallbackInterface.OnTab;
                @Tab.canceled -= m_Wrapper.m_CommonActionsCallbackInterface.OnTab;
                @DebugConsole.started -= m_Wrapper.m_CommonActionsCallbackInterface.OnDebugConsole;
                @DebugConsole.performed -= m_Wrapper.m_CommonActionsCallbackInterface.OnDebugConsole;
                @DebugConsole.canceled -= m_Wrapper.m_CommonActionsCallbackInterface.OnDebugConsole;
                @RestartGame.started -= m_Wrapper.m_CommonActionsCallbackInterface.OnRestartGame;
                @RestartGame.performed -= m_Wrapper.m_CommonActionsCallbackInterface.OnRestartGame;
                @RestartGame.canceled -= m_Wrapper.m_CommonActionsCallbackInterface.OnRestartGame;
                @ReloadGame.started -= m_Wrapper.m_CommonActionsCallbackInterface.OnReloadGame;
                @ReloadGame.performed -= m_Wrapper.m_CommonActionsCallbackInterface.OnReloadGame;
                @ReloadGame.canceled -= m_Wrapper.m_CommonActionsCallbackInterface.OnReloadGame;
                @PauseGame.started -= m_Wrapper.m_CommonActionsCallbackInterface.OnPauseGame;
                @PauseGame.performed -= m_Wrapper.m_CommonActionsCallbackInterface.OnPauseGame;
                @PauseGame.canceled -= m_Wrapper.m_CommonActionsCallbackInterface.OnPauseGame;
                @ToggleUI.started -= m_Wrapper.m_CommonActionsCallbackInterface.OnToggleUI;
                @ToggleUI.performed -= m_Wrapper.m_CommonActionsCallbackInterface.OnToggleUI;
                @ToggleUI.canceled -= m_Wrapper.m_CommonActionsCallbackInterface.OnToggleUI;
                @ToggleDebugPanel.started -= m_Wrapper.m_CommonActionsCallbackInterface.OnToggleDebugPanel;
                @ToggleDebugPanel.performed -= m_Wrapper.m_CommonActionsCallbackInterface.OnToggleDebugPanel;
                @ToggleDebugPanel.canceled -= m_Wrapper.m_CommonActionsCallbackInterface.OnToggleDebugPanel;
                @InteractiveKey.started -= m_Wrapper.m_CommonActionsCallbackInterface.OnInteractiveKey;
                @InteractiveKey.performed -= m_Wrapper.m_CommonActionsCallbackInterface.OnInteractiveKey;
                @InteractiveKey.canceled -= m_Wrapper.m_CommonActionsCallbackInterface.OnInteractiveKey;
                @SlowDownGame.started -= m_Wrapper.m_CommonActionsCallbackInterface.OnSlowDownGame;
                @SlowDownGame.performed -= m_Wrapper.m_CommonActionsCallbackInterface.OnSlowDownGame;
                @SlowDownGame.canceled -= m_Wrapper.m_CommonActionsCallbackInterface.OnSlowDownGame;
                @ReturnToOpenWorld.started -= m_Wrapper.m_CommonActionsCallbackInterface.OnReturnToOpenWorld;
                @ReturnToOpenWorld.performed -= m_Wrapper.m_CommonActionsCallbackInterface.OnReturnToOpenWorld;
                @ReturnToOpenWorld.canceled -= m_Wrapper.m_CommonActionsCallbackInterface.OnReturnToOpenWorld;
                @SkillPreviewPanel.started -= m_Wrapper.m_CommonActionsCallbackInterface.OnSkillPreviewPanel;
                @SkillPreviewPanel.performed -= m_Wrapper.m_CommonActionsCallbackInterface.OnSkillPreviewPanel;
                @SkillPreviewPanel.canceled -= m_Wrapper.m_CommonActionsCallbackInterface.OnSkillPreviewPanel;
                @UINavigation.started -= m_Wrapper.m_CommonActionsCallbackInterface.OnUINavigation;
                @UINavigation.performed -= m_Wrapper.m_CommonActionsCallbackInterface.OnUINavigation;
                @UINavigation.canceled -= m_Wrapper.m_CommonActionsCallbackInterface.OnUINavigation;
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
                @Confirm.started += instance.OnConfirm;
                @Confirm.performed += instance.OnConfirm;
                @Confirm.canceled += instance.OnConfirm;
                @Exit.started += instance.OnExit;
                @Exit.performed += instance.OnExit;
                @Exit.canceled += instance.OnExit;
                @Cancel.started += instance.OnCancel;
                @Cancel.performed += instance.OnCancel;
                @Cancel.canceled += instance.OnCancel;
                @Tab.started += instance.OnTab;
                @Tab.performed += instance.OnTab;
                @Tab.canceled += instance.OnTab;
                @DebugConsole.started += instance.OnDebugConsole;
                @DebugConsole.performed += instance.OnDebugConsole;
                @DebugConsole.canceled += instance.OnDebugConsole;
                @RestartGame.started += instance.OnRestartGame;
                @RestartGame.performed += instance.OnRestartGame;
                @RestartGame.canceled += instance.OnRestartGame;
                @ReloadGame.started += instance.OnReloadGame;
                @ReloadGame.performed += instance.OnReloadGame;
                @ReloadGame.canceled += instance.OnReloadGame;
                @PauseGame.started += instance.OnPauseGame;
                @PauseGame.performed += instance.OnPauseGame;
                @PauseGame.canceled += instance.OnPauseGame;
                @ToggleUI.started += instance.OnToggleUI;
                @ToggleUI.performed += instance.OnToggleUI;
                @ToggleUI.canceled += instance.OnToggleUI;
                @ToggleDebugPanel.started += instance.OnToggleDebugPanel;
                @ToggleDebugPanel.performed += instance.OnToggleDebugPanel;
                @ToggleDebugPanel.canceled += instance.OnToggleDebugPanel;
                @InteractiveKey.started += instance.OnInteractiveKey;
                @InteractiveKey.performed += instance.OnInteractiveKey;
                @InteractiveKey.canceled += instance.OnInteractiveKey;
                @SlowDownGame.started += instance.OnSlowDownGame;
                @SlowDownGame.performed += instance.OnSlowDownGame;
                @SlowDownGame.canceled += instance.OnSlowDownGame;
                @ReturnToOpenWorld.started += instance.OnReturnToOpenWorld;
                @ReturnToOpenWorld.performed += instance.OnReturnToOpenWorld;
                @ReturnToOpenWorld.canceled += instance.OnReturnToOpenWorld;
                @SkillPreviewPanel.started += instance.OnSkillPreviewPanel;
                @SkillPreviewPanel.performed += instance.OnSkillPreviewPanel;
                @SkillPreviewPanel.canceled += instance.OnSkillPreviewPanel;
                @UINavigation.started += instance.OnUINavigation;
                @UINavigation.performed += instance.OnUINavigation;
                @UINavigation.canceled += instance.OnUINavigation;
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
        void OnSkill_2_Player1(InputAction.CallbackContext context);
        void OnSkill_3_Player1(InputAction.CallbackContext context);
        void OnSkill_4_Player1(InputAction.CallbackContext context);
        void OnSkill_5_Player1(InputAction.CallbackContext context);
        void OnSkill_6_Player1(InputAction.CallbackContext context);
        void OnSkill_7_Player1(InputAction.CallbackContext context);
        void OnSkill_8_Player1(InputAction.CallbackContext context);
        void OnSkill_9_Player1(InputAction.CallbackContext context);
        void OnSkill_0_Player2(InputAction.CallbackContext context);
        void OnSkill_1_Player2(InputAction.CallbackContext context);
        void OnSkill_2_Player2(InputAction.CallbackContext context);
        void OnSkill_3_Player2(InputAction.CallbackContext context);
        void OnSkill_4_Player2(InputAction.CallbackContext context);
        void OnSkill_5_Player2(InputAction.CallbackContext context);
        void OnSkill_6_Player2(InputAction.CallbackContext context);
        void OnSkill_7_Player2(InputAction.CallbackContext context);
        void OnSkill_8_Player2(InputAction.CallbackContext context);
        void OnSkill_9_Player2(InputAction.CallbackContext context);
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
        void OnLeftSwitch(InputAction.CallbackContext context);
        void OnRightSwitch(InputAction.CallbackContext context);
    }
    public interface ICommonActions
    {
        void OnMouseLeftClick(InputAction.CallbackContext context);
        void OnMouseRightClick(InputAction.CallbackContext context);
        void OnMouseMiddleClick(InputAction.CallbackContext context);
        void OnMousePosition(InputAction.CallbackContext context);
        void OnMouseWheel(InputAction.CallbackContext context);
        void OnConfirm(InputAction.CallbackContext context);
        void OnExit(InputAction.CallbackContext context);
        void OnCancel(InputAction.CallbackContext context);
        void OnTab(InputAction.CallbackContext context);
        void OnDebugConsole(InputAction.CallbackContext context);
        void OnRestartGame(InputAction.CallbackContext context);
        void OnReloadGame(InputAction.CallbackContext context);
        void OnPauseGame(InputAction.CallbackContext context);
        void OnToggleUI(InputAction.CallbackContext context);
        void OnToggleDebugPanel(InputAction.CallbackContext context);
        void OnInteractiveKey(InputAction.CallbackContext context);
        void OnSlowDownGame(InputAction.CallbackContext context);
        void OnReturnToOpenWorld(InputAction.CallbackContext context);
        void OnSkillPreviewPanel(InputAction.CallbackContext context);
        void OnUINavigation(InputAction.CallbackContext context);
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
