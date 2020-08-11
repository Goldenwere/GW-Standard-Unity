using UnityEngine;

namespace Goldenwere.Unity.Controller
{
    /// <summary>
    /// Variant of ManagementCamera which depends on raycasting from the camera to a point in the scene
    /// </summary>
    public class ManagementCamera_Raycasted : ManagementCamera
    {
        #region Fields
        [Header("Raycasted Settings")]
        [Tooltip                        ("The camera to raycast from")]
        [SerializeField] private Camera attachedCamera;
        [Range(0.1f,100f)][Tooltip      ("When the raycast doesn't find a collider, an invisible point will be used instead defaultPointDistance units away from the camera")]
        [SerializeField] private float  defaultPointDistance;
        [Range(0.1f,1000f)][Tooltip     ("The maximum distance to raycast")]
        [SerializeField] private float  maxDistance;
        #endregion
        #region Methods
        /// <summary>
        /// Performs camera rotation based on input
        /// </summary>
        /// <remarks>This method performs rotation around a raycasted or default point</remarks>
        /// <param name="input">The current input (modified to account for device sensitivity scaling)</param>
        protected override void PerformRotation(Vector2 input)
        {
            throw new System.NotImplementedException();
        }
        #endregion
    }
}