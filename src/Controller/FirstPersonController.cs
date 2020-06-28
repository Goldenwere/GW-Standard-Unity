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

    /// <summary>
    /// Defines movement types that must be disabled in a safe manner to prevent bugs
    /// </summary>
    public enum MovementType
    {
        fast,
        slow,
        crouched
    }

    public delegate void MovementStateHandler(MovementState state);

    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public class FirstPersonController : MonoBehaviour
    {
        #region Fields & Properties
#pragma warning disable 0649
        /// <summary>
        /// Variables related to physical/directional movement 
        /// </summary>
        [Serializable] public class MovementSettings
        {
            #region Force settings
            [Header("Force Settings")]

            [Tooltip            ("The magnitude of the force applied to air movement (the opposite of current velocity)")]
            [SerializeField]    private float           forceFrictionAir = 0.1f;
            [Tooltip            ("The magnitude of the force applied to ground movement while moving (the opposite of current velocity)")]
            [SerializeField]    private float           forceFrictionMotion = 1;
            [Tooltip            ("The magnitude of the force applied to ground movement while receiving no input and while grounded (the opposite of current velocity)")]
            [SerializeField]    private float           forceFrictionStationary = 5;
            [Tooltip            ("The force magnitude to apply for jumping")]
            [SerializeField]    private float           forceJump = 8;
            [Tooltip            ("Tendency to stick to ground (typically below 1, ideally around 0.05-0.1)")]
            [SerializeField]    private float           forceStickToGround = 0.05f;
            [Tooltip            ("Multiplier for gravity")]
            [SerializeField]    private float           forceGravityMultiplier = 3;
            #endregion

            #region Generic settings
            [Header("Generic Settings")]

            [Tooltip            ("Whether the player can jump while crouched")]
            [SerializeField]    private bool            settingCanJumpWhileCrouched = false;
            [Tooltip            ("Whether the player can control movement while not grounded")]
            [SerializeField]    private bool            settingControlAirMovement = true;
            [Tooltip            ("Mass to set the rigidbody to")]
            [SerializeField]    private float           settingControllerMass = 5;
            [Tooltip            ("The height to set the controller to while crouched")]
            [SerializeField]    private float           settingCrouchHeight = 0.9f;
            [Tooltip            ("The distance to check for ground distance (ideally set around 0.1")]
            [SerializeField]    private float           settingGroundCheckDistance = 0.1f;
            [Tooltip            ("The height to set the controller to while not crouched (will override whatever is already defined in attached CapsuleCollider)")]
            [SerializeField]    private float           settingNormalHeight = 1.8f;
            [Tooltip            ("Reduces radius by one minus this value to avoid getting stuck in a wall (ideally set around 0.05-0.1)")]
            [SerializeField]    private float           settingShellOffset = 0.05f;
            [Tooltip            ("Modifies speed on slopes")]
            [SerializeField]    private AnimationCurve  settingSlopeModifier = new AnimationCurve(new Keyframe(-90.0f, 1.0f), new Keyframe(0.0f, 1.0f), new Keyframe(90.0f, 0.0f));
            [Tooltip            ("The time in seconds to wait before considering the controller as \"falling\". (ideally set around 0.75-1 seconds)")]
            [SerializeField]    private float           settingWaitBeforeFallTime = 0.75f;
            #endregion

            #region Speed settings
            [Header("Speed Settings")]
            /* VVV NOTE THAT SPEEDS ARE AFFECTED BY MASS AND FRICTION - ADJUSTING THOSE AFFECTS THE RESULTING SPEED VVV */

            [Tooltip            ("Multiplier to speed based on whether crouched or not (ideally between 0-1 non-inclusive)")]
            [SerializeField]    private float           speedCrouchMultiplier = 0.2f;
            [Tooltip            ("Speed when a fast modifier is used (example: sprinting)")]
            [SerializeField]    private float           speedFast = 10;
            [Tooltip            ("Speed when no modifier is used (example: jogging pace for action games, walking pace for scenic games)")]
            [SerializeField]    private float           speedNorm = 5;
            [Tooltip            ("Speed when a slow modifier is used (example: crouching/sneaking, walking pace for action games)")]
            [SerializeField]    private float           speedSlow = 2;
            #endregion

            #region Exposed settings
            [Header("Exposed Settings / Utility")]

            [Tooltip            ("Whether modifiers (fast, slow/crouch) are toggled or held")]
            /**************/    public  bool            areModifiersToggled = false;
            [Tooltip            ("Whether the player can crouch")]
            /**************/    public  bool            canCrouch = true;
            [Tooltip            ("Whether the player can move at all (best used for pausing)")]
            /**************/    public  bool            canMove = true;
            [Tooltip            ("Whether the player can use fast movement (example: when stamina runs out)")]
            /**************/    public  bool            canMoveFast = true;
            [Tooltip            ("Whether the player can use slow movement (example: when cannot crouch/sneak)")]
            /**************/    public  bool            canMoveSlow = true;
            [Tooltip            ("An exposed speed multipler (typically leave this at 1; example use: status effect that slows the player down")]
            /**************/    public  float           speedMultiplier = 1;
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
            public float            SettingGroundCheckDistance  { get { return settingGroundCheckDistance; } }
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
            [SerializeField]    private float           cameraFOVDifferenceCrouched = 0;
            [Tooltip            ("The FOV difference to apply to attached cameras while falling")]
            [SerializeField]    private float           cameraFOVDifferenceFalling = 10;
            [Tooltip            ("The FOV difference to apply to attached cameras when moving with fast speed")]
            [SerializeField]    private float           cameraFOVDifferenceFast = 10;
            [Tooltip            ("The FOV difference to apply to attached cameras when moving with norm speed")]
            [SerializeField]    private float           cameraFOVDifferenceNorm = 5;
            [Tooltip            ("The FOV difference to apply to attached cameras when moving with slow speed")]
            [SerializeField]    private float           cameraFOVDifferenceSlow = 0;
            [Tooltip            ("The length in seconds that the FOV transition occurs")]
            [SerializeField]    private float           cameraFOVTransitionDuration = 1;
            [Tooltip            ("The camera will always be positioned this amount from the topmost vertex of the CapsuleCollider (ideally set around 0.2f)")]
            [SerializeField]    private float           settingCameraHeightOffset = 0.2f;
            #endregion

            #region Exposed Camera settings
            [Header("Exposed Camera Settings")]

            [Tooltip            ("The base FOV setting to apply to attached cameras before adding/subtracting the controller's difference settings " +
                                "(this is the FOV used for idle; use the Difference settings for all other movement states)")]
            /**************/    public  float   cameraFOV = 80;
            [Tooltip            ("Toggle for enabling/disabling FOV shifting")]
            /**************/    public  bool    cameraFOVShiftingEnabled = true;
            [Tooltip            ("Toggle to enable inverted look for mouse only (useful if allowing separate bindings/settings for gamepad")]
            /**************/    public  bool    cameraLookInvertedMouseOnly = false;
            [Tooltip            ("Toggle for inverting camera look horizontally")]
            /**************/    public  bool    cameraLookInvertedHorizontal = false;
            [Tooltip            ("Toggle for inverting camera look vertically")]
            /**************/    public  bool    cameraLookInvertedVertical = false;
            [Tooltip            ("Multiplier for camera sensitivity")]
            /**************/    public  float   cameraSensitivity = 3;
            [Tooltip            ("Whether to use smoothing with camera movement")]
            /**************/    public  bool    smoothLook = true;
            [Tooltip            ("The speed at which camera smoothing is applied (the higher the number, the less time that the camera takes to rotate)")]
            /**************/    public  float   smoothSpeed = 20;
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
        /**************/    private bool                workingRotationFromMouse;

        public  event MovementStateHandler              UpdateMovementState;

        public  bool                MovementIsCrouched          { get { return workingIsCrouched; } }
        public  bool                MovementIsGrounded          { get { return workingGroundstateCurrent; } }
        public  bool                MovementIsMoving            { get { return workingControlActionDoMovement; } }
        public  bool                MovementIsMovingCrouch      { get { return workingControlActionDoMovement && workingIsCrouched; } }
        public  bool                MovementIsMovingFast        { get { return !workingIsCrouched && workingControlActionModifierMoveFast && workingControlActionDoMovement; } }
        public  bool                MovementIsMovingFastCrouch  { get { return workingIsCrouched && workingControlActionModifierMoveFast && workingControlActionDoMovement; } }
        public  bool                MovementIsMovingSlow        { get { return !workingIsCrouched && workingControlActionModifierMoveSlow && workingControlActionDoMovement; } }
        public  bool                MovementIsMovingSlowCrouch  { get { return workingIsCrouched && workingControlActionModifierMoveSlow && workingControlActionDoMovement; } }
        public  CameraSettings      SettingsCamera              { get { return settingsCamera; } }
        public  MovementSettings    SettingsMovement            { get { return settingsMovement; } }
#pragma warning restore 0649
        #endregion

        #region Methods
        /// <summary>
        /// Associates attached components and sets up said components on Monobehaviour.Awake()
        /// </summary>
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

        /// <summary>
        /// Handles controller movement and physics on Monobehaviour.FixedUpdate()
        /// </summary>
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
        /// A safe way to disable movement; directly manipulating certain types of movement will produce bugs if in the middle of said movement
        /// </summary>
        /// <param name="type">The type of movement to disable</param>
        public void DisableMovementSafely(MovementType type)
        {
            switch (type)
            {
                case MovementType.crouched:
                    settingsMovement.canCrouch = false;
                    workingIsCrouched = false;
                    break;
                case MovementType.slow:
                    settingsMovement.canMoveSlow = false;
                    workingControlActionModifierMoveSlow = false;
                    break;
                case MovementType.fast:
                default:
                    settingsMovement.canMoveFast = false;
                    workingControlActionModifierMoveFast = false;
                    break;
            }
            DetermineMovementState();
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
            workingRotationFromMouse = context.performed && context.control.device.path.Contains("Mouse");
        }

        /// <summary>
        /// Determines the current MovementState before notifying an animation/sound system of the current state
        /// </summary>
        private void DetermineMovementState()
        {
            if (workingJumpDesired || workingJumpIsJumping)
            {
                UpdateMovementState?.Invoke(MovementState.jumping);
                if (settingsCamera.cameraFOVShiftingEnabled && settingsCamera.AttachedCameras[0].fieldOfView != settingsCamera.cameraFOV + settingsCamera.CameraFOVDifferenceFalling)
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
                        if (settingsCamera.cameraFOVShiftingEnabled && settingsCamera.AttachedCameras[0].fieldOfView != settingsCamera.cameraFOV)
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

                        if (settingsCamera.cameraFOVShiftingEnabled && settingsCamera.AttachedCameras[0].fieldOfView != settingsCamera.cameraFOV + settingsCamera.CameraFOVDifferenceCrouched)
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
                        if (settingsCamera.cameraFOVShiftingEnabled && settingsCamera.AttachedCameras[0].fieldOfView != settingsCamera.cameraFOV)
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
                            if (settingsCamera.cameraFOVShiftingEnabled && settingsCamera.AttachedCameras[0].fieldOfView != settingsCamera.cameraFOV + settingsCamera.CameraFOVDifferenceSlow)
                            {
                                if (workingFOVCoroutineRunning)
                                    StopCoroutine(workingFOVCoroutine);
                                workingFOVCoroutine = StartCoroutine(TransitionFOV(settingsCamera.cameraFOV + settingsCamera.CameraFOVDifferenceSlow));
                            }
                        }
                        else if (MovementIsMovingFast)
                        {
                            UpdateMovementState?.Invoke(MovementState.fast);
                            if (settingsCamera.cameraFOVShiftingEnabled && settingsCamera.AttachedCameras[0].fieldOfView != settingsCamera.cameraFOV + settingsCamera.CameraFOVDifferenceFast)
                            {
                                if (workingFOVCoroutineRunning)
                                    StopCoroutine(workingFOVCoroutine);
                                workingFOVCoroutine = StartCoroutine(TransitionFOV(settingsCamera.cameraFOV + settingsCamera.CameraFOVDifferenceFast));
                            }
                        }
                        else
                        {
                            UpdateMovementState?.Invoke(MovementState.norm);
                            if (settingsCamera.cameraFOVShiftingEnabled && settingsCamera.AttachedCameras[0].fieldOfView != settingsCamera.cameraFOV + settingsCamera.CameraFOVDifferenceNorm)
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
            if (Physics.SphereCast(transform.position,
                    attachedCollider.radius * (1.0f - settingsMovement.SettingShellOffset),
                    Vector3.down,
                    out RaycastHit stickHit,
                    ((attachedCollider.height / 2f) - attachedCollider.radius) + settingsMovement.ForceStickToGround,
                    Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                if (Mathf.Abs(Vector3.Angle(stickHit.normal, Vector3.up)) < 85f)
                    attachedRigidbody.velocity = Vector3.ProjectOnPlane(attachedRigidbody.velocity, stickHit.normal);
            }

            workingGroundstateCurrent = Physics.SphereCast(transform.position,
                attachedCollider.radius * (1.0f - settingsMovement.SettingShellOffset),
                Vector3.down,
                out RaycastHit hit,
                ((attachedCollider.height / 2f) - attachedCollider.radius) + settingsMovement.SettingGroundCheckDistance,
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

                Vector3 horizontalVelocity = new Vector3(attachedRigidbody.velocity.x, 0, attachedRigidbody.velocity.z);
                if (horizontalVelocity.sqrMagnitude < Mathf.Pow(desiredSpeed, 2))
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
                if (settingsCamera.cameraLookInvertedVertical)
                    if (!settingsCamera.cameraLookInvertedMouseOnly || settingsCamera.cameraLookInvertedMouseOnly && workingRotationFromMouse)
                        value.y *= -1;
                if (settingsCamera.cameraLookInvertedHorizontal)
                    if (!settingsCamera.cameraLookInvertedMouseOnly || settingsCamera.cameraLookInvertedMouseOnly && workingRotationFromMouse)
                        value.x *= -1;

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
                if (settingsCamera.cameraFOVShiftingEnabled && settingsCamera.AttachedCameras[0].fieldOfView != settingsCamera.cameraFOV + settingsCamera.CameraFOVDifferenceFalling)
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
        #endregion
    }
}