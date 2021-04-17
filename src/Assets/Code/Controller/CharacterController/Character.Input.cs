using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Goldenwere.Unity.Controller
{
    public delegate void inputBtn(bool val);
    public delegate void input1d(float val);
    public delegate void input2d(Vector2 val);
    public delegate void inputVoid();

    public partial class CharacterController : MonoBehaviour
    {
        /// <summary>
        /// Structure for defining input-related settings in inspector
        /// </summary>
        [System.Serializable]
        protected struct InputSettings
        {
            public PlayerInput      playerInput;
            public bool             modifiersAreToggled;

            #region Input action names: these must match the name of the InputAction as defined in the PlayerInput being used
            public string           actionNameMovement;
            public string           actionNameRotation;
            public string           actionNameJump;
            public string           actionNameCrouch;
            public string           actionNameCrawl;
            public string           actionNameWalk;
            public string           actionNameRun;
            public string           actionNameGravity;
            public string           actionNameLean;
            public string           actionNameInteract;
            #endregion

            public ControllerInputs InitializeInputsFromSettings(PlayerInput input, CharacterController parent)
            {
                return new ControllerInputs()
                {
                    movement = new InputContainer(actionNameMovement, input, parent, false),
                    rotation = new InputContainer(actionNameRotation, input, parent, false),
                    jump = new InputContainer(actionNameJump, input, parent, false),
                    crouch = new InputContainer(actionNameCrouch, input, parent, true),
                    crawl = new InputContainer(actionNameCrawl, input, parent, true),
                    walk = new InputContainer(actionNameWalk, input, parent, true),
                    run = new InputContainer(actionNameRun, input, parent, true),
                    gravity = new InputContainer(actionNameGravity, input, parent, false),
                    lean = new InputContainer(actionNameLean, input, parent, true),
                    interact = new InputContainer(actionNameInteract, input, parent, false),
                };
            }
        }

        /// <summary>
        /// Container for input module's inputs
        /// </summary>
        protected class ControllerInputs
        {
            public InputContainer       movement;
            public InputContainer       rotation;
            public InputContainer       jump;
            public InputContainer       crouch;
            public InputContainer       crawl;
            public InputContainer       walk;
            public InputContainer       run;
            public InputContainer       gravity;
            public InputContainer       lean;
            public InputContainer       interact;
        }

        /// <summary>
        /// Wrapper for the InputSystem.InputAction to setup handlers and maintain state information
        /// </summary>
        protected class InputContainer
        {
            public InputAction          action;
            public CharacterController  parent;
            public bool                 isActive;
            public bool                 isModifier;
            public event inputVoid      Input;

            public T GetValue<T>() where T: struct
            {
                return action.ReadValue<T>();
            }

            /// <summary>
            /// Constructor for an InputContainer
            /// </summary>
            /// <param name="name">The name/key for the InputAction</param>
            /// <param name="input">The PlayerInput the InputAction is under</param>
            /// <param name="_parent">The controller, in order to read whether modifiers are toggled or not</param>
            /// <param name="_isModifier">Whether the input is a modifier or not (which can either be toggled or held)</param>
            public InputContainer(string name, PlayerInput input, CharacterController _parent, bool _isModifier = false)
            {
                isActive = false;
                isModifier = _isModifier;
                parent = _parent;
                action = input.actions.FindAction(name);
                if (action == null)
                    throw new System.NullReferenceException("[gw-std-unity] Controller encountered null when searching for " + name + " on the provided PlayerInput. " +
                    "If the action is unused, this can be disregarded; otherwise, this input will not function properly");
                else
                {
                    action.performed += ctx =>
                    {
                        if (isModifier && parent.settingsForInput.modifiersAreToggled)
                            isActive = !isActive;
                        else
                            isActive = true;
                        Input?.Invoke();
                    };
                    action.canceled += ctx =>
                    {
                        if (!isModifier)
                            isActive = false;
                        Input?.Invoke();
                    };
                }
            }
        }

        /**************/ private ControllerInputs           inputs;
        /**************/ private List<InputContainer>       activeInputs;

        public event input2d                                Movement;
        public event input2d                                Rotation;
        public event inputBtn                               Jump;
        public event inputBtn                               Crouch;
        public event inputBtn                               Crawl;
        public event inputBtn                               Walk;
        public event inputBtn                               Run;
        public event inputBtn                               Gravity;
        public event input1d                                Lean;
        public event inputBtn                               Interact;

        /// <summary>
        /// Initialize the controller's input module
        /// </summary>
        private void InitializeInput()
        {
            inputs = settingsForInput.InitializeInputsFromSettings(settingsForInput.playerInput, this);
            activeInputs = new List<InputContainer>(10);
        }
    }
}