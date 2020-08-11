using UnityEngine;
using UnityEngine.InputSystem;

namespace Goldenwere.Unity.Controller
{
    /// <summary>
    /// Abstract class for the Goldenwere series of ManagementCamera, which each have different styles of motion
    /// </summary>
    public abstract class ManagementCamera : MonoBehaviour
    {
        #region Fields
#pragma warning disable 0649
        [Header("Game Settings")]               // useful exposed code for circumstances such as changes in game state
        /**************/ public bool            controlMotionEnabled;

        [Header("UX Settings")]                 // expose these via a settings/controls menu if possible rather than set them in Inspector
        /**************/ public bool            settingMouseMotionIsToggled;
        [Range(0.01f,5)] public float           settingMovementSensitivity = 1f;
        [Range(0.01f,5)] public float           settingRotationSensitivity = 1f;
        [Range(0.01f,5)] public float           settingZoomSensitivity = 1f;

        [Header("Core Components")]
        [Tooltip                                ("The attached PlayerInput class")]
        [SerializeField] protected PlayerInput  attachedInput;
        [Range(0.01f, 100)][Tooltip             ("Distance that must be kept between the camera and any object not under IgnoreRaycast with a collider")]
        [SerializeField] protected float        collisionPadding = 3f;
        [Tooltip                                ("Transform for horizontal rotation")]
        [SerializeField] protected Transform    transformPivot;
        [Tooltip                                ("Transform for vertical rotation")]
        [SerializeField] protected Transform    transformTilt;
        [Range(0.1f, 100)][Tooltip              ("How fast smooth motion takes place")]
        [SerializeField] protected float        smoothMotionSpeed = 10f;
        [Tooltip                                ("Whether to use smooth motion (via Lerp/Slerp) or not")]
        [SerializeField] protected bool         useCameraSmoothing = true;
        [Tooltip                                ("Sets the maximum angle at which the camera can rotate vertically (down and up respectively)")]
        [SerializeField] protected Vector2      verticalClamping = new Vector2(-50f, 50f);
#pragma warning restore 0649

        #region Sensitivty Constants (these shouldn't need tweaked, as they are for ensuring that different inputs result in similar sensitivity at base; use other settings instead)
        /**************/ protected const float    sensitivityScaleMovement = 0.35f;
        /**************/ protected const float    sensitivityScaleMovementMouse = 0.005f;
        /**************/ protected const float    sensitivityScaleRotation = 1f;
        /**************/ protected const float    sensitivityScaleRotationMouse = 0.01f;
        /**************/ protected const float    sensitivityScaleZoom = 1f;
        /**************/ protected const float    sensitivityScaleZoomMouse = 3f;
        #endregion
        #region Working Variables (these are used for the camera's functionality)
        /**************/ protected Vector3      workingDesiredPosition;
        /**************/ protected Quaternion   workingDesiredRotationHorizontal;
        /**************/ protected Quaternion   workingDesiredRotationVertical;
        /**************/ protected bool         workingInputActionMovement;
        /**************/ protected bool         workingInputActionRotation;
        /**************/ protected bool         workingInputActionZoom;
        /**************/ protected bool         workingInputMouseToggleMovement;
        /**************/ protected bool         workingInputMouseToggleRotation;
        /**************/ protected bool         workingInputMouseToggleZoom;
        #endregion
        #endregion

        #region Methods
        /// <summary>
        /// Sets working transform variables on Start
        /// </summary>
        protected void Start()
        {
            workingDesiredPosition = transform.position;
            workingDesiredRotationHorizontal = transformPivot.transform.localRotation;
            workingDesiredRotationVertical = transformTilt.transform.localRotation;
        }

        /// <summary>
        /// Sets transform on Update
        /// </summary>
        protected virtual void Update()
        {
            if (controlMotionEnabled)
            {
                if (workingInputActionMovement)
                    PerformMovement(attachedInput.actions["ActionMovement"].ReadValue<Vector2>().normalized * sensitivityScaleMovement);

                if (workingInputActionRotation)
                    PerformRotation(attachedInput.actions["ActionRotation"].ReadValue<Vector2>().normalized * sensitivityScaleRotation);

                if (workingInputActionZoom)
                    PerformZoom(attachedInput.actions["ActionZoom"].ReadValue<float>() * sensitivityScaleZoom);

                if (useCameraSmoothing)
                {
                    transform.position = Vector3.Lerp(transform.position, workingDesiredPosition, Time.deltaTime * smoothMotionSpeed);
                    transformPivot.localRotation = Quaternion.Slerp(transformPivot.localRotation, workingDesiredRotationHorizontal, Time.deltaTime * smoothMotionSpeed);
                    transformTilt.localRotation = Quaternion.Slerp(transformTilt.localRotation, workingDesiredRotationVertical, Time.deltaTime * smoothMotionSpeed);
                }

                else
                {
                    transform.position = workingDesiredPosition;
                    transformPivot.localRotation = workingDesiredRotationHorizontal;
                    transformTilt.localRotation = workingDesiredRotationVertical;
                }
            }
        }

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
            Vector2 workingInputMouseDelta = context.ReadValue<Vector2>();

            if (controlMotionEnabled) 
            {
                if (workingInputMouseToggleMovement)
                    PerformMovement(workingInputMouseDelta * sensitivityScaleMovementMouse);
                if (workingInputMouseToggleRotation)
                    PerformRotation(workingInputMouseDelta * sensitivityScaleRotationMouse);
            }
        }

        /// <summary>
        /// Handler for MouseScroll from PlayerInput
        /// </summary>
        /// <param name="context">Holds float containing input value</param>
        public void OnInput_MouseScroll(InputAction.CallbackContext context)
        {
            float workingInputMouseZoom = context.ReadValue<float>();

            if (controlMotionEnabled)
            {
                if (workingInputMouseToggleZoom)
                    PerformZoom(workingInputMouseZoom * sensitivityScaleZoomMouse);
            }
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

        /// <summary>
        /// Performs camera movement on the horizontal plane based on input
        /// </summary>
        /// <param name="input">The current input (modified to account for device sensitivity scaling)</param>
        protected void PerformMovement(Vector2 input)
        {
            Vector3 add = (transform.forward * input.y * settingMovementSensitivity) + (transform.right * input.x * settingMovementSensitivity);
            if (!WillCollideAtNewPosition(workingDesiredPosition + add, add))
                workingDesiredPosition += add;
        }

        /// <summary>
        /// Performs camera rotation based on input
        /// </summary>
        /// <param name="input">The current input (modified to account for device sensitivity scaling)</param>
        protected abstract void PerformRotation(Vector2 input);

        /// <summary>
        /// Performs camera zooming in/out to where camera is looking based on input
        /// </summary>
        /// <param name="input">The current input (modified to account for device sensitivity scaling)</param>
        protected void PerformZoom(float input)
        {
            Vector3 add = transformTilt.forward * input * settingZoomSensitivity;
            if (!WillCollideAtNewPosition(workingDesiredPosition + add, add))
                workingDesiredPosition += add;
        }

        /// <summary>
        /// Utility for determining if camera will collide when moving in a certain direction to a new position
        /// </summary>
        /// <param name="newPosition">The position the camera will be after motion</param>
        /// <param name="direction">The direction of motion</param>
        /// <returns>Whether the camera will collide or not</returns>
        protected bool WillCollideAtNewPosition(Vector3 newPosition, Vector3 direction)
        {
            Collider[] cols = Physics.OverlapSphere(newPosition, collisionPadding);

            bool willCollide = false;
            foreach(Collider c in cols)
            {
                if (!willCollide && c.gameObject.layer != 2)
                {
                    Vector3 headingCurrent = c.transform.position - newPosition - direction;
                    float dotCurr = Vector3.Dot(headingCurrent, newPosition - direction);
                    Vector3 headingNew = c.transform.position - newPosition;
                    float dotNew = Vector3.Dot(headingNew, newPosition);
                    willCollide = Mathf.Abs(dotNew) - Mathf.Abs(dotCurr) > 0;
                }
            }
            return willCollide;
        }
        #endregion
    }
}