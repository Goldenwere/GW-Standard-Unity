﻿using UnityEngine;
using UnityEngine.InputSystem;

namespace Goldenwere.Unity.Controller
{
    /// <summary>
    /// Optional module which allows for swapping between ManagementCamera modes
    /// </summary>
    public class CameraModeModule : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private ManagementCamera[] cameraModes;
#pragma warning restore 0649
        /**************/ private int selectedCamera;

        /// <summary>
        /// Setting selectedCamera in Start rather than inline allows this class to be enabled/disabled in inspector to disable event
        /// </summary>
        private void Start()
        {
            selectedCamera = 0;
        }

        /// <summary>
        /// Handler for SwapCamera from PlayerInput
        /// </summary>
        /// <param name="context">Holds bool regarding performed or cancelled</param>
        public void OnInput_SwapCamera(InputAction.CallbackContext context)
        {
            if (context.performed && cameraModes.Length > 0)
            {
                selectedCamera++;
                if (selectedCamera >= cameraModes.Length)
                    selectedCamera = 0;

                for (int i = 0; i < cameraModes.Length; i++)
                {
                    if (i == selectedCamera)
                        cameraModes[i].enabled = true;
                    else
                        cameraModes[i].enabled = false;
                }
            }
        }
    }
}