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
            if (!rotationPointSet)
            {
                if (workingInputActionMovement || workingInputActionRotation || workingInputMouseToggleMovement || workingInputMouseToggleRotation)
                    SetRotationPoint();
            }

            else
            {
                if (!workingInputActionMovement && !workingInputActionRotation && !workingInputMouseToggleMovement && !workingInputMouseToggleRotation)
                    rotationPointSet = false;
            }

            base.Update();
        }

        /// <summary>
        /// Performs camera rotation based on input
        /// </summary>
        /// <remarks>This method performs rotation around a raycasted or default point</remarks>
        /// <param name="input">The current input (modified to account for device sensitivity scaling)</param>
        protected override void PerformRotation(Vector2 input)
        {
            Vector3 eulerAngles = (transformTilt.right * -input.y) + (Vector3.up * input.x);
            Vector3 newPos = workingDesiredPosition.RotateSelfAroundPoint(rotationPoint, eulerAngles);

            if (!WillCollideAtNewPosition(newPos, workingDesiredPosition - newPos))
            {
                workingDesiredRotationHorizontal *= Quaternion.Euler(0, input.x * settingRotationSensitivity, 0);
                workingDesiredRotationVertical *= Quaternion.Euler(-input.y * settingRotationSensitivity, 0, 0);
                workingDesiredRotationVertical = workingDesiredRotationVertical.VerticalClampEuler(verticalClamping.x, verticalClamping.y);

                workingDesiredPosition = newPos;
            }
        }

        /// <summary>
        /// Sets the point at which the camera rotates around
        /// </summary>
        private void SetRotationPoint()
        {
            if (Physics.Raycast(new Ray(transformTilt.position, transformTilt.forward), out RaycastHit hit, maxDistance))
                rotationPoint = hit.point;
            else
                rotationPoint = transformTilt.forward * defaultPointDistance;

            rotationPointSet = true;
        }
        #endregion
    }
}