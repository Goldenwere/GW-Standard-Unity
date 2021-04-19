using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Goldenwere.Unity.Controller
{
    public delegate void ControllerLoadedDelegate(CharacterController loaded);
    public delegate void ControllerModuleDelegate();

    public struct PrioritizedOptionalModule
    {
        public readonly int                         priority;
        public readonly ControllerModuleDelegate    method;

        public PrioritizedOptionalModule(int _priority, ControllerModuleDelegate _method)
        {
            method = _method;
            priority = _priority;
        }
    }

    public partial class CharacterController : MonoBehaviour
    {
#pragma warning disable 0649
        [Tooltip                                                    ("(Default: true) Automatically initializes the controller on Start; can be disabled for manually calling Initialize")]
        [SerializeField] private bool                               initializeOnStart = true;
        [SerializeField] private InputSettings                      settingsForInput;
        [SerializeField] private PhysicSettings                     settingsForPhysics;
        [SerializeField] private MovementSettings                   settingsForMovement;
        [SerializeField] private CameraSettings                     settingsForCamera;
#pragma warning restore
        /**************/ private bool                               initialized;
        /**************/ public ControllerLoadedDelegate            controllerLoaded;
        /**************/ private List<PrioritizedOptionalModule>    modulesUnderFixedUpdate;
        /**************/ private List<PrioritizedOptionalModule>    modulesUnderUpdate;
        
        /// <summary>
        /// Handles initialization of the controller's various modules
        /// </summary>
        /// <param name="physicSystem">The character physics system to use</param>
        public void Initialize(ICharacterPhysics physicSystem)
        {
            if (!initialized)
            { 
                if (settingsForInput.playerInput == null)
                    Debug.LogException(new System.NullReferenceException("[gw-std-unity] Controller's PlayerInput is null; " +
                        "this must be assigned in order for the controller to work properly. Either assign one in inspector or do so in code before the first frame."));
                else
                    InitializeInput();

                InitializeCamera();
                InitializePhysics(physicSystem);
                InitializeMovement();

                modulesUnderFixedUpdate = new List<PrioritizedOptionalModule>(8);
                modulesUnderUpdate = new List<PrioritizedOptionalModule>(8);
                
                initialized = true;
                controllerLoaded?.Invoke(this);
            }
        }

        private void Start()
        {
            if (initializeOnStart)
                Initialize(new CharacterPhysicsRigidbodyBased());
        }

        private void Update()
        {
            Update_Camera();
            foreach(PrioritizedOptionalModule module in modulesUnderUpdate)
                module.method();
        }

        private void FixedUpdate()
        {
            FixedUpdate_Movement();
            FixedUpdate_Physics();
            foreach(PrioritizedOptionalModule module in modulesUnderFixedUpdate)
                module.method();
        }

        private void AddModuleToUpdate(PrioritizedOptionalModule module)
        {
            modulesUnderUpdate.Add(module);
            modulesUnderUpdate.Sort((x, y) => {
                if (x.priority > y.priority) return 1;
                if (x.priority < y.priority) return -1;
                return x.method.ToString().CompareTo(y.method.ToString());
            });
        }

        private void AddModuleToFixedUpdate(PrioritizedOptionalModule module)
        {
            modulesUnderFixedUpdate.Add(module);
            modulesUnderFixedUpdate.Sort((x, y) => {
                if (x.priority > y.priority) return 1;
                if (x.priority < y.priority) return -1;
                return x.method.ToString().CompareTo(y.method.ToString());
            });
        }
    }
}