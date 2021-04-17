using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Goldenwere.Unity.Demos
{
    public class CharacterControllerDemo : MonoBehaviour
    {
        [SerializeField] private Controller.CharacterController controller;

        private void Awake()
        {
            controller.Initialize();
        }

        private void OnGUI()
        {
            
        }
    }
}