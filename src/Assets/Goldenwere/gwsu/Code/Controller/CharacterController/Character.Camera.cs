/**
*** Copyright (C) 2020 Goldenwere
*** Part of the Goldenwere Standard Unity repository
*** The Goldenwere Standard Unity Repository is licensed under the MIT license
***
*** File Info:
***     Description - Contains the camera portion/module of the CharacterController
***     Pkg Name    - CharacterController
***     Pkg Ver     - 2.0.0
***     Pkg Req     - CoreAPI
**/

using System.Collections.Generic;
using UnityEngine;

namespace Goldenwere.Unity.Controller
{
    public partial class CharacterController : MonoBehaviour
    {
        protected delegate void RotationForm();

        /// <summary>
        /// Define the states the controller can have from inputs and assign numerical values to them for calculations
        /// </summary>
        protected enum SpeedState
        {
            idle    = 0,
            normal  = 1,
            walk    = 2,
            run     = 3,
            air     = 4,
        }

        /// <summary>
        /// Structure for defining field of view settings
        /// </summary>
        [System.Serializable]
        protected struct FOVSettings
        {
            [Tooltip                    ("Whether to modify the FOV or not")]
            public bool                 useFOV;

            [Tooltip                    ("The standard camera FOV, used when standing completely still")]
            public float                fovBase;

            [Tooltip                    ("FOV multiplier when walking (movement + walk modifier)")]
            public float                fovMultiplierWalk;
            
            [Tooltip                    ("FOV multiplier when at normal pace (movement + no speed modifiers)")]
            public float                fovMultiplierNorm;

            [Tooltip                    ("FOV multiplier when running (movement + run modifier)")]
            public float                fovMultiplierRun;

            [Tooltip                    ("FOV multiplier when in the air (jumping or falling)")]
            public float                fovMultiplierAir;

            [Tooltip                    ("Speed factor which affects how long FOV transitions last (higher = faster)")]
            public float                fovSpeed;
        }

        /// <summary>
        /// Structure for defining camera motion settings
        /// </summary>
        [System.Serializable]
        protected struct MotionSettings
        {
            public bool                 invertHorizontal;
            public bool                 invertVertical;
            public bool                 useMouseSmoothing;
            public float                mouseSmoothSpeed;
            public float                lookSensitivity;

            public float                turnMultiplierWalk;
            public float                turnMultiplierNorm;
            public float                turnMultiplierRun;
            public float                turnMultiplierAir;
        }

        /// <summary>
        /// Structure for defining camera settings; contains subset structures of settings
        /// </summary>
        [System.Serializable]
        protected struct CameraSettings
        {
            [Tooltip                    ("Array of cameras for the controller. Typically 1-2 (1 for scene, 1 if skybox is separate)")]
            public Camera[]             cameras;

            [Tooltip                    ("Array of transforms in which the camera is rotated up or down.\n" +
                                        "Note: The transforms must match in at least array length; the order doesn't necessarily need to match because the same operations are applied to all.\n" +
                                        "Note: If using a third person camera, make sure the joints are at the point in which you want the camera to be set.\n" +
                                        "Note: If intending to use animation (e.g. bobbing), make sure the rotation joint takes priority in hierarchy over the animation joint.")]
            public Transform[]          cameraRotationJoints;

            [Tooltip                    ("The min/max in euler angles that the camera can look up or down (typically this is just around -/+ 89-90); x = min, y = max")]
            public Vector2              verticalClamp;

            public FOVSettings          settingsFOV;
            public MotionSettings       settingsMotion;

            // TO-DO: third person support with distanceFromCenter; set via a mouse-wheel input / pgUp-pgDn input and UX property
            // Have clamp variable for min/max distance;
            // Have bool for whether to enable toggling from first person to third person when approaching min distance
            // Have bool for if third person distance setting is supported (this determines if the input is listened to) and a state bool (outside CameraSettings) for first versus third
            // Have setting whether to swap left or right (this essentially multiplies the x of the joint by -1 when changed) and clamp setting for shoulder factor, both also properties
        }
        
        private SpeedState                      currentSpeed;
        private Dictionary<SpeedState, float>   fovSpeedsToValues;              // for storing speeds to values without having to do conditionals
        private Dictionary<SpeedState, float>   turnMultipliersToValues;    // for storing speeds to values without having to do conditionals
        private PrioritizedControllerModule     fovModule;
        private Quaternion[]                    workingCameraRotations;         // for working with camera rotations before applying them to the cameras
        private Quaternion                      workingControllerRotation;      // for working with controller rotation before applying them to the controller
        private RotationForm                    workingRotationForm;            // for storing which form of rotation should be performed without checking the setting every frame
        // TODO: potentially wrap controller in another transform for separating y-axis rotation from potential gravity-rotation module

        #region UX exposed properties
        /// <summary>
        /// Sets whether FOV shifting is enabled or not
        /// </summary>
        /// <remarks>Intended for use with a game's UX/UI e.g. a settings menu</remarks>
        public bool     FOVShiftingEnabled
        {
            get => settingsForCamera.settingsFOV.useFOV;
            set 
            {
                settingsForCamera.settingsFOV.useFOV = value;
                if (!value)
                    modulesUnderUpdate.Remove(fovModule);
                else if (!modulesUnderUpdate.Contains(fovModule))
                    modulesUnderUpdate.Add(fovModule);
                foreach(Camera c in settingsForCamera.cameras)
                    c.fieldOfView = FOV;
            }
        }

        /// <summary>
        /// Sets the camera's base FOV
        /// </summary>
        /// <remarks>Intended for use with a game's UX/UI e.g. a settings menu</remarks>
        public float    FOV
        {
            get => settingsForCamera.settingsFOV.fovBase;
            set => settingsForCamera.settingsFOV.fovBase = value;
        }

        /// <summary>
        /// Sets whether camera smoothing is enabled or not
        /// </summary>
        /// <remarks>Intended for use with a game's UX/UI e.g. a settings menu</remarks>
        public bool     CameraSmoothingEnabled
        {
            get => settingsForCamera.settingsMotion.useMouseSmoothing;
            set
            {
                settingsForCamera.settingsMotion.useMouseSmoothing = value;
                workingRotationForm = CameraSmoothingEnabled ? (RotationForm)RotateWithSmoothing : (RotationForm)RotateWithoutSmoothing;
            }
        }

        /// <summary>
        /// Sets the camera's smoothing speed
        /// </summary>
        /// <remarks>Intended for use with a game's UX/UI e.g. a settings menu</remarks>
        public float    CameraSmoothingSpeed
        {
            get => settingsForCamera.settingsMotion.mouseSmoothSpeed;
            set => settingsForCamera.settingsMotion.mouseSmoothSpeed = value;
        }

        /// <summary>
        /// Sets the camera's look sensitivity
        /// </summary>
        /// <remarks>Intended for use with a game's UX/UI e.g. a settings menu</remarks>
        public float    CameraLookSensitivity
        {
            get => settingsForCamera.settingsMotion.lookSensitivity;
            set => settingsForCamera.settingsMotion.lookSensitivity = value;
        }

        /// <summary>
        /// Sets the camera's horizontal inversion setting
        /// </summary>
        /// <remarks>Intended for use with a game's UX/UI e.g. a settings menu</remarks>
        public bool     CameraInvertHorizontal
        {
            get => settingsForCamera.settingsMotion.invertHorizontal;
            set => settingsForCamera.settingsMotion.invertHorizontal = value;
        }

        /// <summary>
        /// Sets the camera's vertical inversion setting
        /// </summary>
        /// <remarks>Intended for use with a game's UX/UI e.g. a settings menu</remarks>
        public bool     CameraInvertVertical
        {
            get => settingsForCamera.settingsMotion.invertVertical;
            set => settingsForCamera.settingsMotion.invertVertical = value;
        }
        #endregion

        /// <summary>
        /// Initialize the controller's camera module
        /// </summary>
        private void InitializeCamera()
        {
            #region Verification Code
            if (settingsForCamera.cameras.Length < 1)
                Debug.LogException(new System.Exception("[gw-std-unity] Controller has no cameras assigned. Assign them under settingsForCamera"));
            else
                foreach(Camera c in settingsForCamera.cameras)
                    if (c == null)
                        Debug.LogException(new System.Exception("[gw-std-unity] Controller has a null camera assigned. Ensure all slots of settingsForCamera.cameras are assigned"));
                    else
                        c.fieldOfView = FOV;

            if (settingsForCamera.cameraRotationJoints.Length < 1)
                Debug.LogException(new System.Exception("[gw-std-unity] Controller has no rotation joints assigned. Assign them under settingsForCamera"));
            else if (settingsForCamera.cameraRotationJoints.Length != settingsForCamera.cameraRotationJoints.Length)
                Debug.LogException(new System.Exception("[gw-std-unity] Controller doesn't have the same number of rotation joints as cameras. Ensure these are assigned properly under settingsForCamera."));
            else
                foreach(Transform t in settingsForCamera.cameraRotationJoints)
                    if (t == null)
                        Debug.LogException(new System.Exception("[gw-std-unity] Controller has a null rotation joint assigned. Ensure all slots of settingsForCamera.cameraRotationJoints are assigned"));
                    else
                    {
                        workingCameraRotations = new Quaternion[settingsForCamera.cameraRotationJoints.Length];
                        for (int i = 0; i < workingCameraRotations.Length; i++)
                            workingCameraRotations[i] = settingsForCamera.cameraRotationJoints[i].localRotation;
                        workingControllerRotation = transform.localRotation;
                    }
            #endregion

            // set what method to use for camera smoothing based on settings
            workingRotationForm = CameraSmoothingEnabled ? (RotationForm)RotateWithSmoothing : (RotationForm)RotateWithoutSmoothing;
            // and assign fov settings to their corresponding speed states for clean reading
            fovSpeedsToValues = new Dictionary<SpeedState, float>()
            {
                { SpeedState.idle,      FOV },
                { SpeedState.normal,    settingsForCamera.settingsFOV.fovMultiplierNorm * FOV },
                { SpeedState.air,       settingsForCamera.settingsFOV.fovMultiplierAir * FOV },
                { SpeedState.walk,      settingsForCamera.settingsFOV.fovMultiplierWalk * FOV },
                { SpeedState.run,       settingsForCamera.settingsFOV.fovMultiplierRun * FOV },
            };
            turnMultipliersToValues = new Dictionary<SpeedState, float>()
            {
                { SpeedState.idle,      1 },
                { SpeedState.normal,    settingsForCamera.settingsMotion.turnMultiplierNorm },
                { SpeedState.air,       settingsForCamera.settingsMotion.turnMultiplierAir },
                { SpeedState.walk,      settingsForCamera.settingsMotion.turnMultiplierWalk },
                { SpeedState.run,       settingsForCamera.settingsMotion.turnMultiplierRun },
            };

            // create and assign the module
            fovModule = new PrioritizedControllerModule(99, UpdateFOV);
            if (FOVShiftingEnabled)
                AddModuleToUpdate(fovModule);
        }

        /// <summary>
        /// [Performed Under: Update()] Updates the camera and controller rotation
        /// </summary>
        private void Update_Camera()
        {
            // 0. ensure speed is updated
            UpdateSpeed();

            if (InputActiveRotation)
            {
                // 1. get input and invert if set in UX
                Vector2 val = InputValueRotation;
                if (CameraInvertHorizontal)
                    val.x *= -1;
                if (CameraInvertVertical)
                    val.y *= -1;

                // 2. set camera rotations
                for (int i = 0; i < workingCameraRotations.Length; i++)
                {
                    workingCameraRotations[i] *= Quaternion.Euler(-val.y * CameraLookSensitivity, 0, 0);
                    workingCameraRotations[i] = workingCameraRotations[i].VerticalClampEuler(settingsForCamera.verticalClamp.x, settingsForCamera.verticalClamp.y);
                }
                workingControllerRotation *= Quaternion.Euler(0, val.x * CameraLookSensitivity * turnMultipliersToValues[currentSpeed], 0);
            }

            // 3. perform camera rotation
            workingRotationForm();
        }

        /// <summary>
        /// Method for rotating the controller with smoothing
        /// </summary>
        private void RotateWithSmoothing()
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, workingControllerRotation, CameraSmoothingSpeed * Time.deltaTime);
            for (int i = 0; i < workingCameraRotations.Length; i++)
                settingsForCamera.cameraRotationJoints[i].transform.localRotation = Quaternion.Slerp(
                    settingsForCamera.cameraRotationJoints[i].transform.localRotation,
                    workingCameraRotations[i],
                    CameraSmoothingSpeed * Time.deltaTime);
        }

        /// <summary>
        /// Method for rotating the controller without smoothing
        /// </summary>
        private void RotateWithoutSmoothing()
        {
            transform.localRotation = workingControllerRotation;
            for (int i = 0; i < workingCameraRotations.Length; i++)
                settingsForCamera.cameraRotationJoints[i].transform.localRotation = workingCameraRotations[i];
        }

        /// <summary>
        /// Method which updates the current speed state
        /// </summary>
        /// TODO: optimize to be input-based and move into different module (likely movement)
        private void UpdateSpeed()
        {
            // if controller moving, set currentSpeed based on other inputs
            if (InputActiveMovement && !IsMovementBlocked)
            {
                if (InputValueRun)
                    currentSpeed = SpeedState.run;
                else if (InputValueWalk)
                    currentSpeed = SpeedState.walk;
                else
                    currentSpeed = SpeedState.normal;
            }
            // otherwise, set as idle/air where appropriate
            else
                if (!Grounded)
                    currentSpeed = SpeedState.air;
                else
                    currentSpeed = SpeedState.idle;
        }

        /// <summary>
        /// Method which contains the FOV shifting module
        /// </summary>
        private void UpdateFOV()
        {
            // Lerp fov based on current state
            InterpolateFOV(currentSpeed);
        }

        /// <summary>
        /// Method for interpolating FOV
        /// </summary>
        /// <param name="state">The speed state to read speed value from</param>
        private void InterpolateFOV(SpeedState state)
        {
            foreach(Camera c in settingsForCamera.cameras)
                c.fieldOfView = Mathf.Lerp(c.fieldOfView, fovSpeedsToValues[state], Time.deltaTime * settingsForCamera.settingsFOV.fovSpeed);
        }
    }
}