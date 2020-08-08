using UnityEngine;
using Goldenwere.Unity.Controller;

namespace Goldenwere.Unity.Demos
{
    /// <summary>
    /// Class for managing the GodGameCam demo
    /// </summary>
    public class GodGameCamDemo : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private GodGameCamera cam;
#pragma warning restore 0649

        /// <summary>
        /// On Start, enable cam modifiers
        /// </summary>
        private void Start()
        {
            cam.CameraModifiersAreEnabled = true;
        }
    }
}