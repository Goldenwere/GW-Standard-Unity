using System.Collections.Generic;
using UnityEngine;

namespace Goldenwere.Unity.Controller
{
    /// <summary>
    /// Optional module to handle animations for the controller
    /// which is separated from the controller.
    /// </summary>
    /// <remarks>
    /// This is separate because there may be other ways one may want to handle animations.
    /// For example, one may want to use coroutines rather than animators, or get velocity to handle animation playback speed.
    /// </remarks>
    public class ControllerAnimation : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private Animator[]             animators;
        [SerializeField] private CharacterController    controller;
#pragma warning restore 0649

        /// <summary>
        /// Set up animation module on start
        /// </summary>
        private void Start()
        {
            // log exceptions if any relevant
            if (controller == null)
                Debug.LogException(new System.NullReferenceException("[gw-std-unity] Controller's animation module's controller is unassigned."));
            else if (animators.Length == 0)
                Debug.LogException(new System.NullReferenceException("[gw-std-unity] Controller's animation module has no animators assigned."));

            // there's no reason to continue initialization without controller or animators assigned, thus the else
            else
            {
                // warn about null animators, which can cause exceptions later on
                foreach(Animator a in animators)
                    if (a == null)
                        Debug.LogException(new System.NullReferenceException("[gw-std-unity] Controller's animation module has unassigned animator slots in inspector."));

                // handle ControllerLoaded event to listen to input events to set animator state
                controller.ControllerLoaded += (c) =>
                {
                    controller.Crawl += (val) => SetAnimatorState("crawl", val);
                    controller.Crouch += (val) => SetAnimatorState("crouch", val);
                    controller.Gravity += (val) => SetAnimatorState("gravity", val);
                    controller.Interact += (val) => SetAnimatorState("interact", val);
                    controller.Jump += (val) => SetAnimatorState("jump", val);
                    controller.Lean += (val) => SetAnimatorState("lean", val);
                    controller.Movement += (val) => SetAnimatorState("movement", val);
                    controller.Rotation += (val) => SetAnimatorState("rotation", val);
                    controller.Run += (val) => SetAnimatorState("run", val);
                    controller.Walk += (val) => SetAnimatorState("walk", val);
                };
            }
        }

        private void SetAnimatorState(string id, bool value)
        {
            foreach(Animator a in animators)
                a.SetBool(id, value);
        }
    }
}