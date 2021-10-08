// GENERATED AUTOMATICALLY FROM 'Assets/Input/PlayerInput.inputactions'

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
            ""name"": ""Base"",
            ""id"": ""31695ec3-fc94-411b-a9d6-fb5334cfc298"",
            ""actions"": [
                {
                    ""name"": ""Look"",
                    ""type"": ""Value"",
                    ""id"": ""f5c73b8c-5052-4567-8b98-2d06aee0d3c0"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Move"",
                    ""type"": ""Value"",
                    ""id"": ""77b58d58-09e2-477d-ac09-6004de49db03"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""ToggleLights"",
                    ""type"": ""Button"",
                    ""id"": ""1b590f05-6bd0-448a-88e0-4be8e6d5a02a"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""GearUp"",
                    ""type"": ""Button"",
                    ""id"": ""2e75ff40-12ba-4b41-816b-1188c4419be2"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""GearDown"",
                    ""type"": ""Button"",
                    ""id"": ""c40a5e99-7fa1-455b-a297-d69084bf5c1d"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Tractor"",
                    ""type"": ""Button"",
                    ""id"": ""4a916176-2bca-4d49-bcba-842be1f5d907"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""GameStart"",
                    ""type"": ""Button"",
                    ""id"": ""33148024-d4da-4d80-a1be-ef4853086b82"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""72f6dd76-5e08-4983-928b-145f1a18bb30"",
                    ""path"": ""<Mouse>/delta"",
                    ""interactions"": """",
                    ""processors"": ""ScaleVector2(x=0.05,y=0.05)"",
                    ""groups"": """",
                    ""action"": ""Look"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""29f3b6f1-9658-4b51-ab9a-18fcfe3e67af"",
                    ""path"": ""<Gamepad>/rightStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Look"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""2D Vector"",
                    ""id"": ""0d8d5298-917f-446d-a944-cfae8a5c6e6d"",
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
                    ""id"": ""7fc84afe-17be-4d04-9a75-c7f973d73ff8"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""f26f4629-3230-4e0c-9b66-bad462304bdf"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""7f8be81b-a5e0-4526-81e8-d7863768672f"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""487ef46d-e7ae-4283-8f21-13d70e87d714"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""2D Vector"",
                    ""id"": ""73f4e3b8-ecab-419f-8d8c-4c700e540a62"",
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
                    ""id"": ""d2388eeb-8009-499d-848a-d544973f50fe"",
                    ""path"": ""<Keyboard>/upArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""3df45a22-312c-4705-8df0-68df53efa04d"",
                    ""path"": ""<Keyboard>/downArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""170a52f4-aa7c-4ba4-875b-948aacb01062"",
                    ""path"": ""<Keyboard>/leftArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""b6bdc15a-64c1-4443-8d0d-63aaae7af606"",
                    ""path"": ""<Keyboard>/rightArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""c610eb3b-819d-465e-b5b6-7fba5c03538f"",
                    ""path"": ""<Gamepad>/leftStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""04f471f5-8de8-4b20-848a-60c529c2d6d9"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ToggleLights"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3d64d4e4-0dde-42eb-a031-d290ed122d19"",
                    ""path"": ""<Gamepad>/buttonWest"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ToggleLights"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""21534fd4-bc8c-4fc1-9e92-21c9726e99ba"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""GearUp"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""f33a44c1-0f58-4434-b441-30059366898e"",
                    ""path"": ""<Gamepad>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""GearUp"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""994e1158-f6d6-4c57-bf26-81dbf89af5ad"",
                    ""path"": ""<Keyboard>/ctrl"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""GearDown"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""ea135d1b-3655-48cd-88a7-8d36ab45df0a"",
                    ""path"": ""<Gamepad>/buttonEast"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""GearDown"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""70bd34fa-b9a7-46b7-9e52-9909eeda5eef"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Tractor"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""639d2a0b-d193-4b10-8e54-9075b7c54a52"",
                    ""path"": ""<Gamepad>/rightTrigger"",
                    ""interactions"": ""Press"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Tractor"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""beed35f2-79cc-4c12-80d6-78a290a6017b"",
                    ""path"": ""<Keyboard>/anyKey"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""GameStart"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""f904488c-b744-448b-bb4b-45d55f5b406d"",
                    ""path"": ""<Gamepad>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""GameStart"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3d1deb8a-b173-445a-99a5-d8baec838046"",
                    ""path"": ""<Gamepad>/start"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""GameStart"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""7b88bbac-98ea-4ab8-b4ea-0f3c8467fc51"",
                    ""path"": ""<Mouse>/press"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""GameStart"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Base
        m_Base = asset.FindActionMap("Base", throwIfNotFound: true);
        m_Base_Look = m_Base.FindAction("Look", throwIfNotFound: true);
        m_Base_Move = m_Base.FindAction("Move", throwIfNotFound: true);
        m_Base_ToggleLights = m_Base.FindAction("ToggleLights", throwIfNotFound: true);
        m_Base_GearUp = m_Base.FindAction("GearUp", throwIfNotFound: true);
        m_Base_GearDown = m_Base.FindAction("GearDown", throwIfNotFound: true);
        m_Base_Tractor = m_Base.FindAction("Tractor", throwIfNotFound: true);
        m_Base_GameStart = m_Base.FindAction("GameStart", throwIfNotFound: true);
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

    // Base
    private readonly InputActionMap m_Base;
    private IBaseActions m_BaseActionsCallbackInterface;
    private readonly InputAction m_Base_Look;
    private readonly InputAction m_Base_Move;
    private readonly InputAction m_Base_ToggleLights;
    private readonly InputAction m_Base_GearUp;
    private readonly InputAction m_Base_GearDown;
    private readonly InputAction m_Base_Tractor;
    private readonly InputAction m_Base_GameStart;
    public struct BaseActions
    {
        private @PlayerInput m_Wrapper;
        public BaseActions(@PlayerInput wrapper) { m_Wrapper = wrapper; }
        public InputAction @Look => m_Wrapper.m_Base_Look;
        public InputAction @Move => m_Wrapper.m_Base_Move;
        public InputAction @ToggleLights => m_Wrapper.m_Base_ToggleLights;
        public InputAction @GearUp => m_Wrapper.m_Base_GearUp;
        public InputAction @GearDown => m_Wrapper.m_Base_GearDown;
        public InputAction @Tractor => m_Wrapper.m_Base_Tractor;
        public InputAction @GameStart => m_Wrapper.m_Base_GameStart;
        public InputActionMap Get() { return m_Wrapper.m_Base; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(BaseActions set) { return set.Get(); }
        public void SetCallbacks(IBaseActions instance)
        {
            if (m_Wrapper.m_BaseActionsCallbackInterface != null)
            {
                @Look.started -= m_Wrapper.m_BaseActionsCallbackInterface.OnLook;
                @Look.performed -= m_Wrapper.m_BaseActionsCallbackInterface.OnLook;
                @Look.canceled -= m_Wrapper.m_BaseActionsCallbackInterface.OnLook;
                @Move.started -= m_Wrapper.m_BaseActionsCallbackInterface.OnMove;
                @Move.performed -= m_Wrapper.m_BaseActionsCallbackInterface.OnMove;
                @Move.canceled -= m_Wrapper.m_BaseActionsCallbackInterface.OnMove;
                @ToggleLights.started -= m_Wrapper.m_BaseActionsCallbackInterface.OnToggleLights;
                @ToggleLights.performed -= m_Wrapper.m_BaseActionsCallbackInterface.OnToggleLights;
                @ToggleLights.canceled -= m_Wrapper.m_BaseActionsCallbackInterface.OnToggleLights;
                @GearUp.started -= m_Wrapper.m_BaseActionsCallbackInterface.OnGearUp;
                @GearUp.performed -= m_Wrapper.m_BaseActionsCallbackInterface.OnGearUp;
                @GearUp.canceled -= m_Wrapper.m_BaseActionsCallbackInterface.OnGearUp;
                @GearDown.started -= m_Wrapper.m_BaseActionsCallbackInterface.OnGearDown;
                @GearDown.performed -= m_Wrapper.m_BaseActionsCallbackInterface.OnGearDown;
                @GearDown.canceled -= m_Wrapper.m_BaseActionsCallbackInterface.OnGearDown;
                @Tractor.started -= m_Wrapper.m_BaseActionsCallbackInterface.OnTractor;
                @Tractor.performed -= m_Wrapper.m_BaseActionsCallbackInterface.OnTractor;
                @Tractor.canceled -= m_Wrapper.m_BaseActionsCallbackInterface.OnTractor;
                @GameStart.started -= m_Wrapper.m_BaseActionsCallbackInterface.OnGameStart;
                @GameStart.performed -= m_Wrapper.m_BaseActionsCallbackInterface.OnGameStart;
                @GameStart.canceled -= m_Wrapper.m_BaseActionsCallbackInterface.OnGameStart;
            }
            m_Wrapper.m_BaseActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Look.started += instance.OnLook;
                @Look.performed += instance.OnLook;
                @Look.canceled += instance.OnLook;
                @Move.started += instance.OnMove;
                @Move.performed += instance.OnMove;
                @Move.canceled += instance.OnMove;
                @ToggleLights.started += instance.OnToggleLights;
                @ToggleLights.performed += instance.OnToggleLights;
                @ToggleLights.canceled += instance.OnToggleLights;
                @GearUp.started += instance.OnGearUp;
                @GearUp.performed += instance.OnGearUp;
                @GearUp.canceled += instance.OnGearUp;
                @GearDown.started += instance.OnGearDown;
                @GearDown.performed += instance.OnGearDown;
                @GearDown.canceled += instance.OnGearDown;
                @Tractor.started += instance.OnTractor;
                @Tractor.performed += instance.OnTractor;
                @Tractor.canceled += instance.OnTractor;
                @GameStart.started += instance.OnGameStart;
                @GameStart.performed += instance.OnGameStart;
                @GameStart.canceled += instance.OnGameStart;
            }
        }
    }
    public BaseActions @Base => new BaseActions(this);
    public interface IBaseActions
    {
        void OnLook(InputAction.CallbackContext context);
        void OnMove(InputAction.CallbackContext context);
        void OnToggleLights(InputAction.CallbackContext context);
        void OnGearUp(InputAction.CallbackContext context);
        void OnGearDown(InputAction.CallbackContext context);
        void OnTractor(InputAction.CallbackContext context);
        void OnGameStart(InputAction.CallbackContext context);
    }
}
