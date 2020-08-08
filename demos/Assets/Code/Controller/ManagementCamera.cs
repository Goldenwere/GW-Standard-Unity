using UnityEngine;
using UnityEngine.InputSystem;

namespace Goldenwere.Unity.Controller
{
    /// <summary>
    /// Abstract class for the Goldenwere series of ManagementCamera, which each have different styles of motion
    /// </summary>
    public abstract class ManagementCamera
    {
        /**************/ public  bool           controlMotionEnabled;
        /**************/ public  float          settingMovementSensitivity = 1f;
        /**************/ public  float          settingRotationSensitivity = 1f;
        /**************/ public  float          settingZoomSensitivity = 1f;
        /**************/ public  bool           settingMouseMotionIsToggled;
        /**************/ private const float    sensitivityScaleMovement = 0.35f;
        /**************/ private const float    sensitivityScaleMovementMouse = 0.005f;
        /**************/ private const float    sensitivityScaleRotation = 1f;
        /**************/ private const float    sensitivityScaleRotationMouse = 0.01f;
        /**************/ private const float    sensitivityScaleZoom = 1f;
        /**************/ private const float    sensitivityScaleZoomMouse = 3f;
        /**************/ private bool           workingInputActionMovement;
        /**************/ private bool           workingInputActionRotation;
        /**************/ private bool           workingInputActionZoom;
        /**************/ private Vector2        workingInputMouseDelta;
        /**************/ private bool           workingInputMouseToggleMovement;
        /**************/ private bool           workingInputMouseToggleRotation;
        /**************/ private bool           workingInputMouseToggleZoom;
        /**************/ private float          workingInputMouseZoom;

        #region Input Handlers
        /// <summary>
        /// Handler for ActionMovement from PlayerInput
        /// </summary>
        /// <param name="context">Holds bool regarding performed or cancelled</param>
        public void OnInput_ActionMovement(InputAction.CallbackContext context)
        {
            workingInputActionMovement = context.performed;
        }

        /// <summary>
        /// Handler for ActionRotation from PlayerInput
        /// </summary>
        /// <param name="context">Holds bool regarding performed or cancelled</param>
        public void OnInput_ActionRotation(InputAction.CallbackContext context)
        {
            workingInputActionRotation = context.performed;
        }

        /// <summary>
        /// Handler for ActionZoom from PlayerInput
        /// </summary>
        /// <param name="context">Holds bool regarding performed or cancelled</param>
        public void OnInput_ActionZoom(InputAction.CallbackContext context)
        {
            workingInputActionZoom = context.performed;
        }

        /// <summary>
        /// Handler for MouseDelta from PlayerInput
        /// </summary>
        /// <param name="context">Holds Vector2 containing input value</param>
        public void OnInput_MouseDelta(InputAction.CallbackContext context)
        {
            workingInputMouseDelta = context.ReadValue<Vector2>() * sensitivityScaleRotationMouse;
        }

        /// <summary>
        /// Handler for MouseScroll from PlayerInput
        /// </summary>
        /// <param name="context">Holds float containing input value</param>
        public void OnInput_MouseScroll(InputAction.CallbackContext context)
        {
            workingInputMouseZoom = context.ReadValue<float>() * sensitivityScaleZoomMouse;
        }

        /// <summary>
        /// Handler for MouseMotion from PlayerInput
        /// </summary>
        /// <param name="context">Holds bool regarding performed or cancelled</param>
        public void OnInput_MouseToggleMovement(InputAction.CallbackContext context)
        {
            if (settingMouseMotionIsToggled)
                workingInputMouseToggleMovement = !workingInputMouseToggleMovement;
            else
                workingInputMouseToggleMovement = context.performed;
        }

        /// <summary>
        /// Handler for MouseMotion from PlayerInput
        /// </summary>
        /// <param name="context">Holds bool regarding performed or cancelled</param>
        public void OnInput_MouseToggleRotation(InputAction.CallbackContext context)
        {
            if (settingMouseMotionIsToggled)
                workingInputMouseToggleRotation = !workingInputMouseToggleRotation;
            else
                workingInputMouseToggleRotation = context.performed;
        }

        /// <summary>
        /// Handler for MouseMotion from PlayerInput
        /// </summary>
        /// <param name="context">Holds bool regarding performed or cancelled</param>
        public void OnInput_MouseToggleZoom(InputAction.CallbackContext context)
        {
            if (settingMouseMotionIsToggled)
                workingInputMouseToggleZoom = !workingInputMouseToggleZoom;
            else
                workingInputMouseToggleZoom = context.performed;

        }
        #endregion
    }
}