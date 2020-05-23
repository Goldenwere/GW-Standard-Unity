using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Goldenwere.Unity.Controller
{
    /// <summary>
    /// Defines the current movement state of the controller for use in animation events
    /// </summary>
    public enum MovementState
    {
        fast,
        fast_crouched,
        norm,
        norm_crouched,
        slow,
        slow_crouched,
        idle,
        idle_crouched,
        falling,
        jumping
    }

    public delegate void MovementStateHandler(MovementState state);

    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public class FirstPersonController : MonoBehaviour
    {
#pragma warning disable 0649
        /// <summary>
        /// Variables related to physical/directional movement 
        /// </summary>
        [Serializable] public class MovementSettings
        {
            #region Force settings
            [Header("Force Settings")]

            [Tooltip            ("The magnitude of the force applied to air movement (the opposite of current velocity)")]
            [SerializeField]    private float           forceFrictionAir;
            [Tooltip            ("The magnitude of the force applied to ground movement while moving (the opposite of current velocity)")]
            [SerializeField]    private float           forceFrictionMotion;
            [Tooltip            ("The magnitude of the force applied to ground movement while receiving no input and while grounded (the opposite of current velocity)")]
            [SerializeField]    private float           forceFrictionStationary;
            [Tooltip            ("The force magnitude to apply for jumping")]
            [SerializeField]    private float           forceJump;
            [Tooltip            ("Tendency to stick to ground (typically below 1, ideally around 0.1 or 0.05 for higher, more 'realistic' masses)")]
            [SerializeField]    private float           forceStickToGround;
            [Tooltip            ("Multiplier for gravity")]
            [SerializeField]    private float           forceGravityMultiplier;
            #endregion

            #region Generic settings
            [Header("Generic Settings")]

            [Tooltip            ("Whether the player can jump while crouched")]
            [SerializeField]    private bool            settingCanJumpWhileCrouched;
            [Tooltip            ("Whether the player can control movement while not grounded")]
            [SerializeField]    private bool            settingControlAirMovement;
            [Tooltip            ("Mass to set the rigidbody to")]
            [SerializeField]    private float           settingControllerMass;
            [Tooltip            ("The height to set the controller to while crouched")]
            [SerializeField]    private float           settingCrouchHeight;
            [Tooltip            ("The height to set the controller to while not crouched (will override whatever is already defined in attached CapsuleCollider)")]
            [SerializeField]    private float           settingNormalHeight;
            [Tooltip            ("Reduces radius by one minus this value to avoid getting stuck in a wall (ideally set around 0.1f)")]
            [SerializeField]    private float           settingShellOffset;
            [Tooltip            ("Modifies speed on slopes")]
            [SerializeField]    private AnimationCurve  settingSlopeModifier = new AnimationCurve(new Keyframe(-90.0f, 1.0f), new Keyframe(0.0f, 1.0f), new Keyframe(90.0f, 0.0f));
            [Tooltip            ("The time in seconds to wait before considering the controller as \"falling\". (ideally set around 0.75-1f seconds)")]
            [SerializeField]    private float           settingWaitBeforeFallTime;
            #endregion

            #region Speed settings
            [Header("Speed Settings")]
            /* VVV NOTE THAT SPEEDS ARE AFFECTED BY MASS AND FRICTION - ADJUSTING THOSE AFFECTS THE RESULTING SPEED VVV */

            [Tooltip            ("Multiplier to speed based on whether crouched or not (ideally between 0-1 non-inclusive)")]
            [SerializeField]    private float           speedCrouchMultiplier;
            [Tooltip            ("Speed when a fast modifier is used (example: sprinting)")]
            [SerializeField]    private float           speedFast;
            [Tooltip            ("Speed when no modifier is used (example: jogging pace for action games, walking pace for scenic games)")]
            [SerializeField]    private float           speedNorm;
            [Tooltip            ("Speed when a slow modifier is used (example: crouching/sneaking, walking pace for action games)")]
            [SerializeField]    private float           speedSlow;
            #endregion

            #region Exposed settings
            [Header("Exposed Settings / Utility")]

            [Tooltip            ("Whether modifiers (fast, slow/crouch) are toggled or held")]
            /**************/    public  bool            areModifiersToggled;
            [Tooltip            ("Whether the player can crouch")]
            /**************/    public  bool            canCrouch;
            [Tooltip            ("Whether the player can move at all (best used for pausing)")]
            /**************/    public  bool            canMove;
            [Tooltip            ("Whether the player can use fast movement (example: when stamina runs out)")]
            /**************/    public  bool            canMoveFast;
            [Tooltip            ("Whether the player can use slow movement (example: when cannot crouch/sneak)")]
            /**************/    public  bool            canMoveSlow;
            [Tooltip            ("An exposed speed multipler (typically leave this at 1; example use: status effect that slows the player down")]
            /**************/    public  float           speedMultiplier;
            #endregion

            #region Properties
            public float            ForceFrictionAir            { get { return forceFrictionAir; } }
            public float            ForceFrictionMotion         { get { return forceFrictionMotion; } }
            public float            ForceFrictionStationary     { get { return forceFrictionStationary; } }
            public float            ForceStickToGround          { get { return forceStickToGround; } }
            public float            ForceJump                   { get { return forceJump; } }
            public float            ForceGravityMultiplier      { get { return forceGravityMultiplier; } }
            public bool             SettingCanJumpWhileCrouched { get { return settingCanJumpWhileCrouched; } }
            public bool             SettingControlAirMovement   { get { return settingControlAirMovement; } }
            public float            SettingControllerMass       { get { return settingControllerMass; } }
            public float            SettingCrouchHeight         { get { return settingCrouchHeight; } }
            public float            SettingNormalHeight         { get { return settingNormalHeight; } }
            public float            SettingShellOffset          { get { return settingShellOffset; } }
            public AnimationCurve   SettingSlopeModifier        { get { return settingSlopeModifier; } }
            public float            SettingWaitBeforeFallTime   { get { return settingWaitBeforeFallTime; } }
            public float            SpeedCrouchMultiplier       { get { return speedCrouchMultiplier; } }
            public float            SpeedFast                   { get { return speedFast; } }
            public float            SpeedNorm                   { get { return speedNorm; } }
            public float            SpeedSlow                   { get { return speedSlow; } }
            #endregion
        }

        /// <summary>
        /// Variables related to camera/rotational movement
        /// </summary>
        [Serializable] public class CameraSettings
        {
            #region Internal Camera settings
            [Header             ("Internal Camera Settings")]

            [Tooltip            ("The attached camera joints that the controller can use for rotation (cameras or animated joints should be be children of these joints)")]
            [SerializeField]    private GameObject[]    attachedCameraJoints;
            [Tooltip            ("The attached cameras that the controller can animate FOV")]
            [SerializeField]    private Camera[]        attachedCameras;
            [Tooltip            ("The minimum (x) and maximum (y) rotation in degrees that the camera can rotate vertically (ideally a range within -90 and 90 degrees)")]
            [SerializeField]    private Vector2         cameraClampVertical;
            [Tooltip            ("The animation curve to use for camera FOV transitioning")]
            [SerializeField]    private AnimationCurve  cameraFOVCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            [Tooltip            ("The FOV difference to apply to attached cameras while crouched")]
            [SerializeField]    private float           cameraFOVDifferenceCrouched;
            [Tooltip            ("The FOV difference to apply to attached cameras while falling")]
            [SerializeField]    private float           cameraFOVDifferenceFalling;
            [Tooltip            ("The FOV difference to apply to attached cameras when moving with fast speed")]
            [SerializeField]    private float           cameraFOVDifferenceFast;
            [Tooltip            ("The FOV difference to apply to attached cameras when moving with norm speed")]
            [SerializeField]    private float           cameraFOVDifferenceNorm;
            [Tooltip            ("The FOV difference to apply to attached cameras when moving with slow speed")]
            [SerializeField]    private float           cameraFOVDifferenceSlow;
            [Tooltip            ("The length in seconds that the FOV transition occurs")]
            [SerializeField]    private float           cameraFOVTransitionDuration;
            [Tooltip            ("The camera will always be positioned this amount from the topmost vertex of the CapsuleCollider (ideally set around 0.2f)")]
            [SerializeField]    private float           settingCameraHeightOffset;
            #endregion

            #region Exposed  camera settings
            [Header("Exposed Camera Settings")]

            [Tooltip            ("The base FOV setting to apply to attached cameras before adding/subtracting the controller's difference settings " +
                "(this is the FOV used for idle; use the Difference settings for all other movement states)")]
            /**************/    public  float   cameraFOV;
            [Tooltip            ("Multiplier for camera sensitivity")]
            /**************/    public  float   cameraSensitivity;
            [Tooltip            ("Whether to use smoothing with camera movement")]
            /**************/    public  bool    smoothLook;
            [Tooltip            ("The speed at which camera smoothing is applied (the higher the number, the less time that the camera takes to rotate)")]
            /**************/    public  float   smoothSpeed;
            #endregion

            #region Properties
            public GameObject[]     AttachedCameraJoints        { get { return attachedCameraJoints; } }
            public Camera[]         AttachedCameras             { get { return attachedCameras; } }
            public Vector2          CameraClampVertical         { get { return cameraClampVertical; } }
            public AnimationCurve   CameraFOVCurve              { get { return cameraFOVCurve; } }
            public float            CameraFOVDifferenceCrouched { get { return cameraFOVDifferenceCrouched; } }
            public float            CameraFOVDifferenceFalling  { get { return cameraFOVDifferenceFalling; } }
            public float            CameraFOVDifferenceFast     { get { return cameraFOVDifferenceFast; } }
            public float            CameraFOVDifferenceNorm     { get { return cameraFOVDifferenceNorm; } }
            public float            CameraFOVDifferenceSlow     { get { return cameraFOVDifferenceSlow; } }
            public float            CameraFOVTransitionDuration { get { return cameraFOVTransitionDuration; } }
            public float            SettingCameraHeightOffset   { get { return settingCameraHeightOffset; } }
            #endregion
        }

        [SerializeField]    private PlayerInput         attachedControls;
        [SerializeField]    private CameraSettings      settingsCamera;
        [SerializeField]    private MovementSettings    settingsMovement;
        /**************/    private CapsuleCollider     attachedCollider;
        /**************/    private Rigidbody           attachedRigidbody;
        /**************/    private bool                workingControlActionDoMovement;
        /**************/    private bool                workingControlActionDoRotation;
        /**************/    private bool                workingControlActionModifierMoveFast;
        /**************/    private bool                workingControlActionModifierMoveSlow;
        /**************/    private Coroutine           workingFOVCoroutine;
        /**************/    private bool                workingFOVCoroutineRunning;
        /**************/    private bool                workingJumpDesired;
        /**************/    private bool                workingJumpIsJumping;
        /**************/    private bool                workingJumpIsJumpingCoroutineRunning;
        /**************/    private Vector3             workingGroundContactNormal;
        /**************/    private bool                workingGroundstateCurrent;
        /**************/    private bool                workingGroundstatePrevious;
        /**************/    private bool                workingIsCrouched;
        /**************/    private Quaternion[]        workingRotationCameraJoints;
        /**************/    private Quaternion          workingRotationController;

        public  event MovementStateHandler              UpdateMovementState;

        public  bool                MovementIsCrouched          { get { return workingIsCrouched; } }
        public  bool                MovementIsGrounded          { get { return workingGroundstateCurrent; } }
        public  bool                MovementIsMovingFast        { get { return !workingIsCrouched && workingControlActionModifierMoveFast; } }
        public  bool                MovementIsMovingFastCrouch  { get { return workingIsCrouched && workingControlActionModifierMoveFast; } }
        public  bool                MovementIsMovingSlow        { get { return !workingIsCrouched && workingControlActionModifierMoveSlow; } }
        public  bool                MovementIsMovingSlowCrouch  { get { return workingIsCrouched && workingControlActionModifierMoveSlow; } }
        public  CameraSettings      SettingsCamera              { get { return settingsCamera; } }
        public  MovementSettings    SettingsMovement            { get { return settingsMovement; } }
#pragma warning restore 0649

        private void Awake()
        {
            attachedCollider = gameObject.GetComponent<CapsuleCollider>();
            attachedRigidbody = gameObject.GetComponent<Rigidbody>();

            workingRotationCameraJoints = new Quaternion[settingsCamera.AttachedCameraJoints.Length];
            for (int i = 0; i < workingRotationCameraJoints.Length; i++)
                workingRotationCameraJoints[i] = settingsCamera.AttachedCameraJoints[i].transform.localRotation;
            workingRotationController = transform.localRotation;

            // For much of this controller to function as expected, rigidbody fields are overrode.
            attachedRigidbody.mass = settingsMovement.SettingControllerMass;
            attachedRigidbody.isKinematic = false;
            attachedRigidbody.useGravity = false;
            attachedRigidbody.drag = 0;
            attachedRigidbody.angularDrag = 0;
            attachedRigidbody.constraints = RigidbodyConstraints.FreezeRotation;

            foreach (Camera c in settingsCamera.AttachedCameras)
                c.fieldOfView = settingsCamera.cameraFOV;
        }

        private void FixedUpdate()
        {
            if (settingsMovement.canMove)
            {
                HandleRotation();
                HandleMovement();
                HandleGravity();
            }
        }

        /// <summary>
        /// Handler for the Crouch input event (defined in ControllerActions)
        /// </summary>
        /// <param name="context">The context associated with the input</param>
        public void OnCrouch(InputAction.CallbackContext context)
        {
            if (settingsMovement.canCrouch)
            {
                if (settingsMovement.areModifiersToggled)
                    workingIsCrouched = !workingIsCrouched;
                else
                    workingIsCrouched = context.performed;
            }
            DetermineMovementState();
        }

        /// <summary>
        /// Handler for the Jump input event (defined in ControllerActions)
        /// </summary>
        /// <param name="context">The context associated with the input (unused)</param>
        public void OnJump(InputAction.CallbackContext context)
        {
            if (!context.canceled && !workingIsCrouched || !context.canceled && settingsMovement.SettingCanJumpWhileCrouched)
            {
                workingJumpDesired = true;
                DetermineMovementState();
            }
        }

        /// <summary>
        /// Handler for the ModifierMovementFast input event (defined in ControllerActions)
        /// </summary>
        /// <param name="context">The context associated with the input</param>
        public void OnModifierMovementFast(InputAction.CallbackContext context)
        {
            if (settingsMovement.canMoveFast)
            {
                if (settingsMovement.areModifiersToggled)
                    workingControlActionModifierMoveFast = !workingControlActionModifierMoveFast;
                else
                    workingControlActionModifierMoveFast = context.performed;

                DetermineMovementState();
            }
        }

        /// <summary>
        /// Handler for the ModifierMovementSlow input event (defined in ControllerActions)
        /// </summary>
        /// <param name="context">The context associated with the input</param>
        public void OnModifierMovementSlow(InputAction.CallbackContext context)
        {
            if (settingsMovement.canMoveSlow)
            {
                if (settingsMovement.areModifiersToggled)
                    workingControlActionModifierMoveSlow = !workingControlActionModifierMoveSlow;
                else
                    workingControlActionModifierMoveSlow = context.performed;

                DetermineMovementState();
            }
        }

        /// <summary>
        /// Handler for the Movement input event (defined in ControllerActions)
        /// </summary>
        /// <param name="context">The context associated with the input</param>
        public void OnMovement(InputAction.CallbackContext context)
        {
            workingControlActionDoMovement = context.performed;
            DetermineMovementState();
        }

        /// <summary>
        /// Handler for the Rotation input event (defined in ControllerActions)
        /// </summary>
        /// <param name="context">The context associated with the input</param>
        public void OnRotation(InputAction.CallbackContext context)
        {
            workingControlActionDoRotation = context.performed;
        }

        /// <summary>
        /// Determines the current MovementState before notifying an animation/sound system of the current state
        /// </summary>
        private void DetermineMovementState()
        {
            if (workingJumpDesired || workingJumpIsJumping)
            {
                UpdateMovementState?.Invoke(MovementState.jumping);
                if (settingsCamera.AttachedCameras[0].fieldOfView != settingsCamera.cameraFOV + settingsCamera.CameraFOVDifferenceFalling)
                {
                    if (workingFOVCoroutineRunning)
                        StopCoroutine(workingFOVCoroutine);
                    workingFOVCoroutine = StartCoroutine(TransitionFOV(settingsCamera.cameraFOV + settingsCamera.CameraFOVDifferenceFalling));
                }
            }

            else if (!workingGroundstateCurrent)
            {
                if (!workingJumpIsJumping && !workingJumpDesired)
                {
                    StopCoroutine(WaitBeforeCallingFall());
                    StartCoroutine(WaitBeforeCallingFall());
                }
            }

            else
            {
                if (MovementIsCrouched)
                {
                    if (!workingControlActionDoMovement)
                    {
                        UpdateMovementState?.Invoke(MovementState.idle_crouched);
                        if (settingsCamera.AttachedCameras[0].fieldOfView != settingsCamera.cameraFOV)
                        {
                            if (workingFOVCoroutineRunning)
                                StopCoroutine(workingFOVCoroutine);
                            workingFOVCoroutine = StartCoroutine(TransitionFOV(settingsCamera.cameraFOV));
                        }
                    }

                    else
                    {
                        if (MovementIsMovingSlowCrouch)
                            UpdateMovementState?.Invoke(MovementState.slow_crouched);
                        else if (MovementIsMovingFastCrouch)
                            UpdateMovementState?.Invoke(MovementState.fast_crouched);
                        else
                            UpdateMovementState?.Invoke(MovementState.norm_crouched);

                        if (settingsCamera.AttachedCameras[0].fieldOfView != settingsCamera.cameraFOV + settingsCamera.CameraFOVDifferenceCrouched)
                        {
                            if (workingFOVCoroutineRunning)
                                StopCoroutine(workingFOVCoroutine);
                            workingFOVCoroutine = StartCoroutine(TransitionFOV(settingsCamera.cameraFOV + settingsCamera.CameraFOVDifferenceCrouched));
                        }
                    }
                }

                else
                {
                    if (!workingControlActionDoMovement)
                    {
                        UpdateMovementState?.Invoke(MovementState.idle);
                        if (settingsCamera.AttachedCameras[0].fieldOfView != settingsCamera.cameraFOV)
                        {
                            if (workingFOVCoroutineRunning)
                                StopCoroutine(workingFOVCoroutine);
                            workingFOVCoroutine = StartCoroutine(TransitionFOV(settingsCamera.cameraFOV));
                        }
                    }

                    else
                    {
                        if (MovementIsMovingSlow)
                        {
                            UpdateMovementState?.Invoke(MovementState.slow);
                            if (settingsCamera.AttachedCameras[0].fieldOfView != settingsCamera.cameraFOV + settingsCamera.CameraFOVDifferenceSlow)
                            {
                                if (workingFOVCoroutineRunning)
                                    StopCoroutine(workingFOVCoroutine);
                                workingFOVCoroutine = StartCoroutine(TransitionFOV(settingsCamera.cameraFOV + settingsCamera.CameraFOVDifferenceSlow));
                            }
                        }
                        else if (MovementIsMovingFast)
                        {
                            UpdateMovementState?.Invoke(MovementState.fast);
                            if (settingsCamera.AttachedCameras[0].fieldOfView != settingsCamera.cameraFOV + settingsCamera.CameraFOVDifferenceFast)
                            {
                                if (workingFOVCoroutineRunning)
                                    StopCoroutine(workingFOVCoroutine);
                                workingFOVCoroutine = StartCoroutine(TransitionFOV(settingsCamera.cameraFOV + settingsCamera.CameraFOVDifferenceFast));
                            }
                        }
                        else
                        {
                            UpdateMovementState?.Invoke(MovementState.norm);
                            if (settingsCamera.AttachedCameras[0].fieldOfView != settingsCamera.cameraFOV + settingsCamera.CameraFOVDifferenceNorm)
                            {
                                if (workingFOVCoroutineRunning)
                                    StopCoroutine(workingFOVCoroutine);
                                workingFOVCoroutine = StartCoroutine(TransitionFOV(settingsCamera.cameraFOV + settingsCamera.CameraFOVDifferenceNorm));
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Determines the current ground state of the controller
        /// </summary>
        private void DetermineGroundstate()
        {
            workingGroundstateCurrent = Physics.SphereCast(transform.position,
                    attachedCollider.radius * (1.0f - settingsMovement.SettingShellOffset),
                    Vector3.down,
                    out RaycastHit hit,
                    ((attachedCollider.height / 2f) - attachedCollider.radius) + settingsMovement.ForceStickToGround,
                    Physics.AllLayers, QueryTriggerInteraction.Ignore);

            if (workingGroundstateCurrent)
            {
                workingGroundContactNormal = hit.normal;
                if (workingJumpIsJumping && !workingJumpIsJumpingCoroutineRunning)
                {
                    StartCoroutine(WaitBeforeEndingIsJumping());
                    workingJumpIsJumpingCoroutineRunning = true;
                }

                else if (!workingGroundstatePrevious)
                {
                    workingGroundstatePrevious = true;
                    DetermineMovementState();
                }
            }

            else
            {
                workingGroundContactNormal = Vector3.up;
                if (workingGroundstatePrevious)
                {
                    workingGroundstatePrevious = false;
                    DetermineMovementState();
                }
            }
        }

        /// <summary>
        /// Determines current controller speed based on input
        /// </summary>
        /// <returns>The speed that corresponds to input and movement settings</returns>
        private float DetermineDesiredSpeed()
        {
            // Slow movement is preferred over fast movement if both modifiers are held

            if (workingControlActionModifierMoveSlow)
                if (workingIsCrouched)
                    return settingsMovement.SpeedSlow * settingsMovement.SpeedCrouchMultiplier * DetermineSlope() * settingsMovement.speedMultiplier;
                else
                    return settingsMovement.SpeedSlow * DetermineSlope() * settingsMovement.speedMultiplier;

            else if (workingControlActionModifierMoveFast)
                if (workingIsCrouched)
                    return settingsMovement.SpeedFast * settingsMovement.SpeedCrouchMultiplier * DetermineSlope() * settingsMovement.speedMultiplier;
                else
                    return settingsMovement.SpeedFast * DetermineSlope() * settingsMovement.speedMultiplier;

            else
                if (workingIsCrouched)
                    return settingsMovement.SpeedNorm * settingsMovement.SpeedCrouchMultiplier * DetermineSlope() * settingsMovement.speedMultiplier;
                else
                    return settingsMovement.SpeedNorm * DetermineSlope() * settingsMovement.speedMultiplier;
        }

        /// <summary>
        /// Determines what modifier to apply to movement speed depending on slopes
        /// </summary>
        /// <returns>The speed modifier based on the current slope angle</returns>
        private float DetermineSlope()
        {
            float angle = Vector3.Angle(workingGroundContactNormal, Vector3.up);
            return settingsMovement.SettingSlopeModifier.Evaluate(angle);
        }

        /// <summary>
        /// Moves the player along the y axis based on input and/or gravity
        /// </summary>
        private void HandleGravity()
        {
            // Move the controller down based on crouch before handling gravity
            if (workingIsCrouched && attachedCollider.height > settingsMovement.SettingCrouchHeight)
            {
                attachedCollider.height = settingsMovement.SettingCrouchHeight;
                transform.position = new Vector3(
                    transform.position.x, 
                    transform.position.y - ((settingsMovement.SettingNormalHeight - settingsMovement.SettingCrouchHeight) / 2), 
                    transform.position.z);
                for (int i = 0; i < settingsCamera.AttachedCameraJoints.Length; i++)
                    settingsCamera.AttachedCameraJoints[i].transform.localPosition = new Vector3(0, (settingsMovement.SettingCrouchHeight / 2) - settingsCamera.SettingCameraHeightOffset, 0);
            }

            else if (!workingIsCrouched && attachedCollider.height < settingsMovement.SettingNormalHeight)
            {
                transform.position = new Vector3(
                    transform.position.x,
                    transform.position.y + ((settingsMovement.SettingNormalHeight - settingsMovement.SettingCrouchHeight) / 2),
                    transform.position.z);
                attachedCollider.height = settingsMovement.SettingNormalHeight;
                for (int i = 0; i < settingsCamera.AttachedCameraJoints.Length; i++)
                    settingsCamera.AttachedCameraJoints[i].transform.localPosition = new Vector3(0, (settingsMovement.SettingNormalHeight / 2) - settingsCamera.SettingCameraHeightOffset, 0);
            }

            DetermineGroundstate();

            if (workingJumpDesired && workingGroundstateCurrent)
            {
                attachedRigidbody.AddForce(new Vector3(0f, settingsMovement.ForceJump * attachedRigidbody.mass, 0f), ForceMode.Impulse);
                workingJumpDesired = false;
                workingJumpIsJumping = true;
                DetermineMovementState();
            }

            else if (!workingGroundstateCurrent)
                attachedRigidbody.AddForce(Physics.gravity * settingsMovement.ForceGravityMultiplier, ForceMode.Acceleration);

            else
                attachedRigidbody.AddForce(Vector3.down * settingsMovement.ForceStickToGround, ForceMode.Impulse);
        }

        /// <summary>
        /// Moves the player along the xz (horizontal) plane based on input
        /// </summary>
        private void HandleMovement()
        {
            // Move controller based on input as long as either grounded or air movement is enabled
            if (workingControlActionDoMovement && workingGroundstateCurrent ||
                workingControlActionDoMovement && settingsMovement.SettingControlAirMovement)
            {
                Vector2 value = attachedControls.actions["Movement"].ReadValue<Vector2>().normalized;
                Vector3 dir = transform.forward * value.y + transform.right * value.x;

                float desiredSpeed = DetermineDesiredSpeed();
                Vector3 force = dir * desiredSpeed;
                force = Vector3.ProjectOnPlane(force, workingGroundContactNormal);

                if (attachedRigidbody.velocity.sqrMagnitude < Mathf.Pow(desiredSpeed, 2))
                    attachedRigidbody.AddForce(force, ForceMode.Impulse);
            }

            // Handle friction/drag
            if (attachedRigidbody.velocity.sqrMagnitude > 0)
            {
                Vector3 horizontalVelocity = new Vector3(attachedRigidbody.velocity.x, 0, attachedRigidbody.velocity.z);
                if (!workingGroundstateCurrent && !settingsMovement.SettingControlAirMovement)
                    attachedRigidbody.AddForce(-horizontalVelocity * settingsMovement.ForceFrictionAir, ForceMode.Impulse);
                else if (workingControlActionDoMovement)
                    attachedRigidbody.AddForce(-horizontalVelocity * settingsMovement.ForceFrictionMotion, ForceMode.Impulse);
                else
                    attachedRigidbody.AddForce(-horizontalVelocity * settingsMovement.ForceFrictionStationary, ForceMode.Impulse);
            }
        }

        /// <summary>
        /// Rotates the player camera vertically and the controller horizontally based on input
        /// </summary>
        private void HandleRotation()
        {
            if (workingControlActionDoRotation)
            {
                Vector2 value = attachedControls.actions["Rotation"].ReadValue<Vector2>();

                for (int i = 0; i < workingRotationCameraJoints.Length; i++)
                {
                    workingRotationCameraJoints[i] *= Quaternion.Euler(-value.y * settingsCamera.cameraSensitivity, 0, 0);
                    workingRotationCameraJoints[i] = workingRotationCameraJoints[i].VerticalClampEuler(settingsCamera.CameraClampVertical.x, settingsCamera.CameraClampVertical.y);
                }
                workingRotationController *= Quaternion.Euler(0, value.x * settingsCamera.cameraSensitivity, 0);
            }

            if (settingsCamera.smoothLook)
            {
                transform.localRotation = Quaternion.Slerp(transform.localRotation, workingRotationController, settingsCamera.smoothSpeed * Time.deltaTime);
                for (int i = 0; i < workingRotationCameraJoints.Length; i++)
                    settingsCamera.AttachedCameraJoints[i].transform.localRotation = Quaternion.Slerp(
                        settingsCamera.AttachedCameraJoints[i].transform.localRotation, 
                        workingRotationCameraJoints[i], 
                        settingsCamera.smoothSpeed * Time.deltaTime);
            }

            else
            {
                transform.localRotation = workingRotationController;
                for (int i = 0; i < workingRotationCameraJoints.Length; i++)
                    settingsCamera.AttachedCameraJoints[i].transform.localRotation = workingRotationCameraJoints[i];
            }
        }

        /// <summary>
        /// Transitions the FOV of each attached camera from their old values to the new value
        /// </summary>
        /// <param name="newFOV">The new FOV each camera should have at the end of the transition</param>
        private IEnumerator TransitionFOV(float newFOV)
        {
            workingFOVCoroutineRunning = true;
            float oldFOV = settingsCamera.AttachedCameras[0].fieldOfView;
            float t = 0;
            while (t <= settingsCamera.CameraFOVTransitionDuration)
            {
                foreach (Camera c in settingsCamera.AttachedCameras)
                    c.fieldOfView = Mathf.Lerp(oldFOV, newFOV, settingsCamera.CameraFOVCurve.Evaluate(t / settingsCamera.CameraFOVTransitionDuration));
                t += Time.deltaTime;
                yield return null;
            }
            // Ensure each FOV is exact by the end of the transition
            foreach (Camera c in settingsCamera.AttachedCameras)
                c.fieldOfView = newFOV;
        workingFOVCoroutineRunning = false;
        }

        /// <summary>
        /// To prevent false "falling" detection, such as for stairs or certain slopes while with a higher stick-to-ground,
        /// </summary>
        private IEnumerator WaitBeforeCallingFall()
        {
            yield return new WaitForSeconds(settingsMovement.SettingWaitBeforeFallTime);
            if (!workingGroundstateCurrent && !workingJumpDesired && !workingJumpIsJumping)
            {
                UpdateMovementState?.Invoke(MovementState.falling);
                if (settingsCamera.AttachedCameras[0].fieldOfView != settingsCamera.cameraFOV + settingsCamera.CameraFOVDifferenceFalling)
                {
                    if (workingFOVCoroutineRunning)
                        StopCoroutine(workingFOVCoroutine);
                    workingFOVCoroutine = StartCoroutine(TransitionFOV(settingsCamera.cameraFOV + settingsCamera.CameraFOVDifferenceFalling));
                }
            }
        }

        /// <summary>
        /// To prevent false detection of grounded while jumping
        /// </summary>
        private IEnumerator WaitBeforeEndingIsJumping()
        {
            yield return new WaitForSeconds(settingsMovement.SettingShellOffset * Time.deltaTime + Time.deltaTime);
            if (workingGroundstateCurrent)
            {
                workingJumpIsJumping = false;
                DetermineMovementState();
            }
            workingJumpIsJumpingCoroutineRunning = false;
        }
    }
}