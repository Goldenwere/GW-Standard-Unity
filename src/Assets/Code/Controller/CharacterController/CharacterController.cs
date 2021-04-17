using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Goldenwere.Unity.Controller
{
    public delegate void ControllerLoadedDelegate(CharacterController loaded);

    public partial class CharacterController : MonoBehaviour
    {
#pragma warning disable 0649
        [Tooltip                                            ("(Default: true) Automatically initializes the controller on Start; can be disabled for manually calling Initialize")]
        [SerializeField] private bool                       initializeOnStart = true;
        [SerializeField] private InputSettings              settingsForInput;
#pragma warning restore
        /**************/ private bool                       initialized;
        /**************/ public ControllerLoadedDelegate    controllerLoaded;
        
        public void Initialize()
        {
            if (!initialized)
            { 
                if (settingsForInput.playerInput == null)
                    throw new System.NullReferenceException("[gw-std-unity] Controller's PlayerInput is null; " +
                        "this must be assigned in order for the controller to work properly. Either assign one in inspector or do so in code before the first frame.");
                else
                    InitializeInput();
                
                initialized = true;
                controllerLoaded?.Invoke(this);
            }
        }

        private void Start()
        {
            if (initializeOnStart)
                Initialize();
        }

        private void Update()
        {
        
        }

        private void FixedUpdate()
        {
        
        }
    }
}