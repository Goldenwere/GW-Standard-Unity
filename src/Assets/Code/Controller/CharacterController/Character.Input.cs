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

            /// <summary>
            /// Creates ControllerInputs from the inspector-defined action names for the input module to use
            /// </summary>
            /// <param name="parent">The parent CharacterController, needed for modifier-based input containers to read if modifiers are toggled</param>
            /// <returns>The initialized ControllerInputs from InputContainers' action names</returns>
            public ControllerInputs InitializeInputsFromSettings(CharacterController parent)
            {
                return new ControllerInputs()
                {
                    movement = new InputContainer(actionNameMovement, playerInput, parent, false),
                    rotation = new InputContainer(actionNameRotation, playerInput, parent, false),
                    jump = new InputContainer(actionNameJump, playerInput, parent, false),
                    crouch = new InputContainer(actionNameCrouch, playerInput, parent, true),
                    crawl = new InputContainer(actionNameCrawl, playerInput, parent, true),
                    walk = new InputContainer(actionNameWalk, playerInput, parent, true),
                    run = new InputContainer(actionNameRun, playerInput, parent, true),
                    gravity = new InputContainer(actionNameGravity, playerInput, parent, false),
                    lean = new InputContainer(actionNameLean, playerInput, parent, true),
                    interact = new InputContainer(actionNameInteract, playerInput, parent, false),
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
            public event inputActive    Updated;

            /// <summary>
            /// Calls action.ReadValue
            /// </summary>
            /// <typeparam name="T">The value-type associated with the input</typeparam>
            /// <returns>The value stored in the input</returns>
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
                    Debug.LogException(new System.NullReferenceException("[gw-std-unity] Controller encountered null when searching for " + name + " on the provided PlayerInput. " +
                    "If the action is unused, this can be disregarded; otherwise, this input will not function properly"));

                else
                {
                    action.performed += ctx =>
                    {
                        if (isModifier && parent.settingsForInput.modifiersAreToggled)
                            isActive = !isActive;
                        else
                            isActive = true;
                        Updated?.Invoke(isActive);
                    };
                    action.canceled += ctx =>
                    {
                        if (!isModifier)
                        { 
                            isActive = false;
                            Updated?.Invoke(isActive);
                        }
                    };
                }
            }
        }

        private ControllerInputs       inputs;

        #region State events for custom systems to optionally subscribe to without having to read values every frame
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
        #endregion

        #region Properties of current input values
        public Vector2                                  ValMovement         { get => inputs.movement.GetValue<Vector2>(); }
        public bool                                     ValMovementActive   { get => inputs.movement.isActive; }
        public Vector2                                  ValRotation         { get => inputs.rotation.GetValue<Vector2>(); }
        public bool                                     ValRotationActive   { get => inputs.rotation.isActive; }
        public bool                                     ValJump             { get => inputs.jump.isActive; }
        public bool                                     ValCrouch           { get => inputs.crouch.isActive; }
        public bool                                     ValCrawl            { get => inputs.crawl.isActive; }
        public bool                                     ValWalk             { get => inputs.walk.isActive; }
        public bool                                     ValRun              { get => inputs.run.isActive; }
        public bool                                     ValGravity          { get => inputs.gravity.isActive; }
        public float                                    ValLean             { get => inputs.lean.GetValue<float>(); }
        public bool                                     ValLeanActive       { get => inputs.lean.isActive; }
        public bool                                     ValInteract         { get => inputs.interact.isActive; }
        #endregion

        /// <summary>
        /// Initialize the controller's input module
        /// </summary>
        private void InitializeInput()
        {
            inputs = settingsForInput.InitializeInputsFromSettings(this);

            inputs.movement.Updated += (bool val) =>
            {
                Movement?.Invoke(val);
            };
            inputs.rotation.Updated += (bool val) =>
            {
                Rotation?.Invoke(val);
            };
            inputs.jump.Updated     += (bool val) =>
            {
                Jump?.Invoke(val);
            };
            inputs.crouch.Updated   += (bool val) =>
            {
                Crouch?.Invoke(val);
            };
            inputs.crawl.Updated    += (bool val) =>
            {
                Crawl?.Invoke(val);
            };
            inputs.walk.Updated     += (bool val) =>
            {
                Walk?.Invoke(val);
            };
            inputs.run.Updated      += (bool val) =>
            {
                Run?.Invoke(val);
            };
            inputs.gravity.Updated  += (bool val) =>
            {
                Gravity?.Invoke(val);
            };
            inputs.lean.Updated     += (bool val) =>
            {
                Lean?.Invoke(val);
            };
            inputs.interact.Updated += (bool val) =>
            {
                Interact?.Invoke(val);
            };
        }
    }
}