﻿using System.Collections;
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
        [SerializeField] private PhysicSettings             settingsForPhysics;
        [SerializeField] private CameraSettings             settingsForCamera;
#pragma warning restore
        /**************/ private bool                       initialized;
        /**************/ public ControllerLoadedDelegate    controllerLoaded;
        
        /// <summary>
        /// Handles initialization of the controller's various modules
        /// </summary>
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
        }

        private void FixedUpdate()
        {
        
        }
    }
}