using UnityEngine;
using System.Collections;

namespace Goldenwere.Unity.Demos
{
    public class CharacterControllerDemo : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private Controller.CharacterController controller;
#pragma warning restore 0649
        /**************/ private Vector3                        velocity;
        /**************/ private float                          velMag;

        private void Awake()
        {
            controller.Initialize(new Controller.CharacterPhysicsRigidbodyBased());
            StartCoroutine(UpdateValuesSlowly());
        }

        private void OnGUI()
        {
            GUI.Label(
                new Rect(20, 20, Screen.width - 20, Screen.height - 20),
                string.Format("Movement: {0}- {1}\nRotation: {2}- {3}\nJump + Crouch + Crawl: {4} + {5} + {6}\nWalk + Run: {7} + {8}\nLean: {9}- {10}\nInteract: {11}\nGravity: {12}\nGrounded?: {13}- {14}\nVelocity: {15} ({16:0.00}m/s)",
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
                controller.InputValueGravity,
                controller.Grounded,
                controller.GroundContactNormal,
                velocity,
                velMag)
            );
        }

        private IEnumerator UpdateValuesSlowly()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.25f);
                velocity = controller.System.Velocity;
                velMag = velocity.magnitude;
            }
        }
    }
}