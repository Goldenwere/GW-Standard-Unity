/**
*** Copyright (C) 2020 Goldenwere
*** Part of the Goldenwere Standard Unity repository
*** The Goldenwere Standard Unity Repository is licensed under the MIT license
***
*** File Info:
***     Description - Contains the ManagementCamDemo class
***     Pkg Name    - ManagementCamera_Demo
***     Pkg Ver     - 1.1.0
***     Pkg Req     - ManagementCamera
**/

using UnityEngine;
using UnityEngine.InputSystem;
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
        [SerializeField] private ManagementCamera   cam;
        [Range(0.01f,10f)][Tooltip                  ("The cursor only gets hidden when the squared velocity of the camera reaches this value.\n" +
                                                    "Note that this value is directly compared against the velocity's sqrMagnitude for performance reasons," +
                                                    "meaning it is not itself squared.")]
        [SerializeField] private float              cursorHideSqrVelocityThreshold = 1.0f;
        [Tooltip                                    ("Whether to restore the cursor after unhiding it to the position it was originally at before hiding.\n" +
                                                    "Normally, CursorLockMode.Locked centers the cursor, which is good for things like first person controllers" +
                                                    "but may actually be frustrating to the player for controllers like the management cameras.")]
        [SerializeField] private bool               restoreCursorPositionAfterShown = true;
#pragma warning restore 0649
        /**************/ private bool               isMouseBeingUsed;
        /**************/ private Vector2            prevMousePos;
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
        /// On Update, check to see if the cursor needs hidden
        /// </summary>
        private void Update()
        {
            // First, check if the camera is using mouse and the cursor is still visible
            if (isMouseBeingUsed && Cursor.visible)
            {
                // Second, only hide the cursor if the velocity is great enough
                if (cam.CurrentCameraVelocity.sqrMagnitude >= cursorHideSqrVelocityThreshold)
                {
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                    prevMousePos = Mouse.current.position.ReadValue();
                }
            }
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
        private void OnCameraMouseStateChanged(bool _isMouseBeingUsed)
        {
            if (isMouseBeingUsed && !_isMouseBeingUsed)
            {
                Cursor.lockState = CursorLockMode.None;
                if (restoreCursorPositionAfterShown && !Cursor.visible)
                    Mouse.current.WarpCursorPosition(prevMousePos);
                Cursor.visible = true;
            }
            isMouseBeingUsed = _isMouseBeingUsed;
        }
        #endregion
    }
}