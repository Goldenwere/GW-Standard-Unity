using UnityEngine;
using Goldenwere.Unity.Controller;

namespace Goldenwere.Unity.Demos
{
    /// <summary>
    /// Class for managing the GodGameCam demo
    /// </summary>
    public class GodGameCamDemo : MonoBehaviour
    {
        [SerializeField] private GodGameCamera cam;

        /// <summary>
        /// On Start, enable cam modifiers
        /// </summary>
        private void Start()
        {
            cam.CameraModifiersAreEnabled = true;
        }
    }
}