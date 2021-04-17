using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Goldenwere.Unity.Controller
{
    public delegate void inputActive(bool val);

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
            public event inputActive    updated;

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
                        updated?.Invoke(isActive);
                    };
                    action.canceled += ctx =>
                    {
                        if (!isModifier)
                        { 
                            isActive = false;
                            updated?.Invoke(isActive);
                        }
                    };
                }
            }
        }

        /**************/ private ControllerInputs       inputs;
        /**************/ private List<InputContainer>   activeInputs;

        public event inputActive                        Movement;
        public event inputActive                        Rotation;
        public event inputActive                        Jump;
        public event inputActive                        Crouch;
        public event inputActive                        Crawl;
        public event inputActive                        Walk;
        public event inputActive                        Run;
        public event inputActive                        Gravity;
        public event inputActive                        Lean;
        public event inputActive                        Interact;

        public Vector2                                  ValMovement { get; private set; }
        public Vector2                                  ValRotation { get; private set; }
        public bool                                     ValJump     { get; private set; }
        public bool                                     ValCrouch   { get; private set;}
        public bool                                     ValCrawl    { get; private set; }
        public bool                                     ValWalk     { get; private set;}
        public bool                                     ValRun      { get; private set; }
        public bool                                     ValGravity  { get; private set; }
        public float                                    ValLean     { get; private set; }
        public bool                                     ValInteract { get; private set; }

        /// <summary>
        /// Initialize the controller's input module
        /// </summary>
        private void InitializeInput()
        {
            inputs = settingsForInput.InitializeInputsFromSettings(settingsForInput.playerInput, this);
            activeInputs = new List<InputContainer>(10);

            inputs.movement.updated += (bool val) =>
            {
                Movement?.Invoke(val);
                ValMovement = inputs.movement.GetValue<Vector2>();
            };
            inputs.rotation.updated += (bool val) =>
            {
                Rotation?.Invoke(val);
                ValRotation = inputs.rotation.GetValue<Vector2>();
            };
            inputs.jump.updated     += (bool val) =>
            {
                Jump?.Invoke(val);
                ValJump = val;
            };
            inputs.crouch.updated   += (bool val) =>
            {
                Crouch?.Invoke(val);
                ValCrouch = val;
            };
            inputs.crawl.updated    += (bool val) =>
            {
                Crawl?.Invoke(val);
                ValCrawl = val;
            };
            inputs.walk.updated     += (bool val) =>
            {
                Walk?.Invoke(val);
                ValWalk = val;
            };
            inputs.run.updated      += (bool val) =>
            {
                Run?.Invoke(val);
                ValRun = val;
            };
            inputs.gravity.updated  += (bool val) =>
            {
                Gravity?.Invoke(val);
                ValGravity = val;
            };
            inputs.lean.updated     += (bool val) =>
            {
                Lean?.Invoke(val);
                ValLean = inputs.lean.GetValue<float>();
            };
            inputs.interact.updated += (bool val) =>
            {
                Interact?.Invoke(val);
                ValInteract = val;
            };
        }
    }
}