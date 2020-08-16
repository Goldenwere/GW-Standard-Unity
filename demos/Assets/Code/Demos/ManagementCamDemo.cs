/**
*** Copyright (C) 2020 Goldenwere
*** Part of the Goldenwere Standard Unity repository
*** The Goldenwere Standard Unity Repository is licensed under the MIT license
***
*** File Info:
***     Description - Contains the ManagementCamDemo class
***     Bundle Name - ManagementCamera_Demo
***     Bundle Ver  - 1.0.0
***     Bundle Req  - ManagementCamera
**/

using UnityEngine;
using Goldenwere.Unity.Controller;

namespace Goldenwere.Unity.Demos
{
    /// <summary>
    /// Class for managing the GodGameCam demo
    /// </summary>
    public class ManagementCamDemo : MonoBehaviour
    {
        #region Fields
#pragma warning disable 0649
        [SerializeField] private ManagementCamera cam;
#pragma warning restore 0649
        #endregion

        #region Methods
        /// <summary>
        /// On Start, enable cam
        /// </summary>
        private void Start()
        {
            cam.controlMotionEnabled = true;
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }

        /// <summary>
        /// Subscribe to events on Enable
        /// </summary>
        private void OnEnable()
        {
            cam.CameraMouseStateChanged += OnCameraMouseStateChanged;
        }

        /// <summary>
        /// Unsubscribe from events on Disable
        /// </summary>
        private void OnDisable()
        {
            cam.CameraMouseStateChanged -= OnCameraMouseStateChanged;
        }

        /// <summary>
        /// Handler for the CameraMouseStateChanged event which toggles cursor state
        /// </summary>
        /// <param name="isMouseBeingUsed">Whether the mouse is currently being used for camera motion</param>
        private void OnCameraMouseStateChanged(bool isMouseBeingUsed)
        {
            if (isMouseBeingUsed)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        #endregion
    }
}