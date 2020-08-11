using UnityEngine;
using UnityEngine.InputSystem;

namespace Goldenwere.Unity.Controller
{
    /// <summary>
    /// Variant of ManagementCamera which depends on raycasting from the camera to a point in the scene
    /// </summary>
    public class ManagementCamera_Raycasted : ManagementCamera
    {
        #region Fields
        [Header("Raycasted Settings")]
#pragma warning disable 0649
        [Tooltip                            ("The camera to raycast from")]
        [SerializeField] private Camera     attachedCamera;
        [Range(0.1f,100f)][Tooltip          ("When the raycast doesn't find a collider, an invisible point will be used instead defaultPointDistance units away from the camera")]
        [SerializeField] private float      defaultPointDistance;
        [Range(0.1f,1000f)][Tooltip         ("The maximum distance to raycast")]
        [SerializeField] private float      maxDistance;
#pragma warning restore 0649

        /**************/ private Vector3    rotationPoint;
        /**************/ private bool       rotationPointSet;
        #endregion
        #region Methods
        protected override void Update()
        {
            if (workingInputActionMovement && !rotationPointSet || workingInputActionRotation && !rotationPointSet)
                SetRotationPoint();

            else if (!workingInputActionMovement && !workingInputActionRotation && rotationPointSet)
                rotationPointSet = false;

            base.Update();
        }

        /// <summary>
        /// Performs camera rotation based on input
        /// </summary>
        /// <remarks>This method performs rotation around a raycasted or default point</remarks>
        /// <param name="input">The current input (modified to account for device sensitivity scaling)</param>
        protected override void PerformRotation(Vector2 input)
        {
            workingDesiredRotationHorizontal *= Quaternion.Euler(0, input.x * settingRotationSensitivity, 0);
            workingDesiredRotationVertical *= Quaternion.Euler(-input.y * settingRotationSensitivity, 0, 0);
            workingDesiredRotationVertical = workingDesiredRotationVertical.VerticalClampEuler(verticalClamping.x, verticalClamping.y);
        }

        /// <summary>
        /// Sets the point at which the camera rotates around
        /// </summary>
        private void SetRotationPoint()
        {
            if (Physics.Raycast(attachedCamera.ScreenPointToRay(Mouse.current.position.ReadValue()), out RaycastHit hit, maxDistance))
                rotationPoint = hit.point;
            else
                rotationPoint = attachedCamera.transform.forward * defaultPointDistance;

            rotationPointSet = true;
        }
        #endregion
    }
}