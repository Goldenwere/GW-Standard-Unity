/**
*** Copyright (C) 2020 Goldenwere
*** Part of the Goldenwere Standard Unity repository
*** The Goldenwere Standard Unity Repository is licensed under the MIT license
***
*** File Info:
***     Description - Contains the main declaration of the CharacterController
***     Pkg Name    - CharacterController
***     Pkg Ver     - 2.0.0
***     Pkg Req     - CoreAPI
**/

using System.Collections.Generic;
using UnityEngine;

namespace Goldenwere.Unity.Controllers.CharacterController
{
    public delegate void ControllerLoadedDelegate(GWCharacterController loaded);
    public delegate void ControllerModuleDelegate();

    /// <summary>
    /// Container for referencing a module's Update/FixedUpdate method and assigning it a priority
    /// </summary>
    public class PrioritizedControllerModule
    {
        public readonly int                         priority;
        public readonly ControllerModuleDelegate    method;

        /// <summary>
        /// Creates an instance of PrioritizedControllerModule with priority and method to assign
        /// </summary>
        /// <param name="_priority">The priority of the method when adding it to a module collection; the lower the number, the higher its priority (think of it as Unity script execution order)</param>
        /// <param name="_method"></param>
        public PrioritizedControllerModule(int _priority, ControllerModuleDelegate _method)
        {
            method = _method;
            priority = _priority;
        }
    }

    /// <summary>
    /// The Goldenwere CharacterController is a highly generalized rigidbody-based character controller based on a system of modules.<br/>
    /// Certain modules on the controller are required in order for the controller to function correctly (i.e. physics, input, movement, and camera).<br/>
    /// Its camera runs under MonoBehaviour.Update and its physics and movement run under MonoBehaviour.FixedUpdate.<br/>
    /// Its input is event-based through the use of UnityEngine.InputSystem.
    /// </summary>
    public partial class GWCharacterController : MonoBehaviour
    {
#pragma warning disable 0649
        [Tooltip                                                    ("(Default: true) Automatically initializes the controller on Start; can be disabled for manually calling Initialize")]
        [SerializeField] private bool                               initializeOnStart           = true;
        [SerializeField] private InputSettings                      settingsForInput;
        [SerializeField] private PhysicSettings                     settingsForPhysics;
        [SerializeField] private MovementSettings                   settingsForMovement;
        [SerializeField] private CameraSettings                     settingsForCamera;
#pragma warning restore
        /**************/ private bool                               initialized;
        /**************/ private List<PrioritizedControllerModule>  modulesUnderFixedUpdate;
        /**************/ private List<PrioritizedControllerModule>  modulesUnderUpdate;

        /**************/ public ControllerLoadedDelegate            ControllerLoaded;
        /**************/ public bool                                IsHeightBlocked             { get; set; }
        /**************/ public bool                                IsMovementBlocked           { get; set; }
        /**************/ public bool                                IsPhysicsBlocked            { get; set; }
        
        /// <summary>
        /// Handles initialization of the controller's various modules
        /// </summary>
        /// <param name="physicSystem">The character physics system to use</param>
        public void Initialize(ICharacterPhysics physicSystem)
        {
            if (!initialized)
            {
                modulesUnderFixedUpdate = new List<PrioritizedControllerModule>(8);
                modulesUnderUpdate = new List<PrioritizedControllerModule>(8);

                // ensure input can be initialized
                if (settingsForInput.playerInput == null)
                    Debug.LogException(new System.NullReferenceException("[gw-std-unity] Controller's PlayerInput is null; " +
                        "this must be assigned in order for the controller to work properly. Either assign one in inspector or do so in code before the first frame."));
                else
                    InitializeInput();

                InitializeCamera();
                InitializePhysics(physicSystem);
                InitializeMovement();
                
                initialized = true;
                ControllerLoaded?.Invoke(this);
            }
        }

        /// <summary>
        /// Initializes the controller if initializeOnStart is set to true
        /// </summary>
        private void Start()
        {
            if (initializeOnStart)
                Initialize(new CharacterPhysicsRigidbodyBased());
        }

        /// <summary>
        /// On Unity Update, update camera (required) and any additional modules (modulesUnderUpdate)
        /// </summary>
        private void Update()
        {
            Update_Camera();
            foreach(PrioritizedControllerModule module in modulesUnderUpdate)
                module.method();
        }

        /// <summary>
        /// On Unity FixedUpdate, update physics and movement (required) and any additional modules (modulesUnderFixedUpdate)
        /// </summary>
        private void FixedUpdate()
        {
            FixedUpdate_Movement();
            FixedUpdate_Physics();
            foreach(PrioritizedControllerModule module in modulesUnderFixedUpdate)
                module.method();
        }

        /// <summary>
        /// Adds a module (if not already added) to the modulesUnderUpdate collection
        /// </summary>
        /// <param name="module">The PrioritizedControllerModule to add</param>
        public void AddModuleToUpdate(PrioritizedControllerModule module)
        {
            if (!modulesUnderUpdate.Contains(module))
            {
                modulesUnderUpdate.Add(module);
                modulesUnderUpdate.Sort((x, y) => {
                    if (x.priority < y.priority) return 1;
                    if (x.priority > y.priority) return -1;
                    return -x.method.ToString().CompareTo(y.method.ToString());
                });
            }
        }

        /// <summary>
        /// Adds a module (if not already added) to the modulesUnderFixedUpdate collection
        /// </summary>
        /// <param name="module">The PrioritizedControllerModule to add</param>
        public void AddModuleToFixedUpdate(PrioritizedControllerModule module)
        {
            if (!modulesUnderFixedUpdate.Contains(module))
            {
                modulesUnderFixedUpdate.Add(module);
                modulesUnderFixedUpdate.Sort((x, y) => {
                    if (x.priority < y.priority) return 1;
                    if (x.priority > y.priority) return -1;
                    return -x.method.ToString().CompareTo(y.method.ToString());
                });
            }
        }

        /// <summary>
        /// Removes a module from the modulesUnderUpdate collection
        /// </summary>
        /// <param name="module">The PrioritizedControllerModule to remove</param>
        public void RemoveModuleFromUpdate(PrioritizedControllerModule module)
        {
            modulesUnderUpdate.Remove(module);
        }

        /// <summary>
        /// Removes a module from the modulesUnderFixedUpdate collection
        /// </summary>
        /// <param name="module">The PrioritizedControllerModule to remove</param>
        public void RemoveModuleFromFixedUpdate(PrioritizedControllerModule module)
        {
            modulesUnderFixedUpdate.Remove(module);
        }
    }
}