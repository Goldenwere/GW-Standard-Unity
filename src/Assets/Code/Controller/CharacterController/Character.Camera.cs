using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Goldenwere.Unity.Controller
{
    public partial class CharacterController : MonoBehaviour
    {
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

            [Tooltip                    ("Speed (in seconds) which FOV transitions last")]
            public float                fovDuration;

            [Tooltip                    ("Curve on which FOV transitions are performed")]
            public AnimationCurve       fovCurve;
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

            [Tooltip                    ("The min/max in euler angles that the camera can look up or down (typically this is just around -/+ 89-90)")]
            public ValueClamp<float>    verticalClamp;

            public FOVSettings          settingsFOV;
            public MotionSettings       settingsMotion;

            // TO-DO: third person support with distanceFromCenter; set via a mouse-wheel input / pgUp-pgDn input and UX property
            // Have clamp variable for min/max distance;
            // Have bool for whether to enable toggling from first person to third person when approaching min distance
            // Have bool for if third person distance setting is supported (this determines if the input is listened to) and a state bool (outside CameraSettings) for first versus third
            // Have setting whether to swap left or right (this essentially multiplies the x of the joint by -1 when changed) and clamp setting for shoulder factor, both also properties
        }

        #region UX exposed properties
        /// <summary>
        /// Sets whether FOV shifting is enabled or not
        /// </summary>
        /// <remarks>Intended for use with a game's UX/UI e.g. a settings menu</remarks>
        public bool FOVShiftingEnabled
        {
            get => settingsForCamera.settingsFOV.useFOV;
            set => settingsForCamera.settingsFOV.useFOV = value;
        }

        /// <summary>
        /// Sets the camera's base FOV
        /// </summary>
        /// <remarks>Intended for use with a game's UX/UI e.g. a settings menu</remarks>
        public float FOV
        {
            get => settingsForCamera.settingsFOV.fovBase;
            set => settingsForCamera.settingsFOV.fovBase = value;
        }

        /// <summary>
        /// Sets whether camera smoothing is enabled or not
        /// </summary>
        /// <remarks>Intended for use with a game's UX/UI e.g. a settings menu</remarks>
        public bool CameraSmoothingEnabled
        {
            get => settingsForCamera.settingsMotion.useMouseSmoothing;
            set => settingsForCamera.settingsMotion.useMouseSmoothing = value;
        }

        /// <summary>
        /// Sets the camera's smoothing speed
        /// </summary>
        /// <remarks>Intended for use with a game's UX/UI e.g. a settings menu</remarks>
        public bool CameraSmoothingSpeed
        {
            get => settingsForCamera.settingsMotion.useMouseSmoothing;
            set => settingsForCamera.settingsMotion.useMouseSmoothing = value;
        }
        #endregion

        /// <summary>
        /// Initialize the controller's camera module
        /// </summary>
        private void InitializeCamera()
        {
            if (settingsForCamera.cameras.Length < 1)
                Debug.LogException(new System.Exception("[gw-std-unity] Controller has no cameras assigned. Assign them under settingsForCamera"));
            else
                foreach(Camera c in settingsForCamera.cameras)
                    if (c == null)
                        Debug.LogException(new System.Exception("[gw-std-unity] Controller has a null camera assigned. Ensure all slots of settingsForCamera.cameras are assigned"));

            if (settingsForCamera.cameraRotationJoints.Length < 1)
                Debug.LogException(new System.Exception("[gw-std-unity] Controller has no rotation joints assigned. Assign them under settingsForCamera"));
            else if (settingsForCamera.cameraRotationJoints.Length != settingsForCamera.cameraRotationJoints.Length)
                Debug.LogException(new System.Exception("[gw-std-unity] Controller doesn't have the same number of rotation joints as cameras. Ensure these are assigned properly under settingsForCamera."));
            else
                foreach(Transform t in settingsForCamera.cameraRotationJoints)
                    if (t == null)
                        Debug.LogException(new System.Exception("[gw-std-unity] Controller has a null rotation joint assigned. Ensure all slots of settingsForCamera.cameraRotationJoints are assigned"));
        }
    }
}