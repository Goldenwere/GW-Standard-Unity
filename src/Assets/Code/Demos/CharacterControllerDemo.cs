﻿using UnityEngine;

namespace Goldenwere.Unity.Demos
{
    public class CharacterControllerDemo : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private Controller.CharacterController controller;
#pragma warning restore 0649

        private void Awake()
        {
            controller.Initialize(new Controller.CharacterPhysicsRigidbodyBased());
        }

        private void OnGUI()
        {
            GUI.Label(
                new Rect(20, 20, Screen.width - 20, Screen.height - 20),
                string.Format("Movement: {0}- {1}\nRotation: {2}- {3}\nJump + Crouch + Crawl: {4} + {5} + {6}\nWalk + Run: {7} + {8}\nLean: {9}- {10}\nInteract: {11}\nGravity: {12}",
                controller.InputActiveMovement,
                controller.InputValueMovement,
                controller.InputActiveRotation,
                controller.InputValueRotation,
                controller.InputValueJump,
                controller.InputValueCrouch,
                controller.InputValueCrawl,
                controller.InputValueWalk,
                controller.InputValueRun,
                controller.InputActiveLean,
                controller.InputValueLean,
                controller.InputValueInteract,
                controller.InputValueGravity)
            );
        }
    }
}