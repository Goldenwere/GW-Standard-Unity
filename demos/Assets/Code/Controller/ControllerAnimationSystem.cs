/**
*** Copyright (C) 2020 Goldenwere
*** Part of the Goldenwere Standard Unity repository
*** The Goldenwere Standard Unity Repository is licensed under the MIT license
***
*** File Info:
***     Description - Contains the ControllerAnimationSystem
***     Pkg Name    - FirstPersonController
***     Pkg Ver     - 1.0.0
***     Pkg Req     - CoreAPI
**/

using UnityEngine;

namespace Goldenwere.Unity.Controller
{
    [RequireComponent(typeof(FirstPersonController))]
    public class ControllerAnimationSystem : MonoBehaviour
    {
        #region Fields
#pragma warning disable 0649
        [Tooltip                                        ("Attach all animators you plan on calling animations on. " +
                                                        "Ensure that you use the same strings as those defined in here.")]
        [SerializeField] private Animator[]             animatorsToCall;
#pragma warning restore 0649
        /**************/ private FirstPersonController  attachedController;
        #endregion

        #region Methods
        /// <summary>
        /// Reference the controller on Monobehaviour.Awake()
        /// </summary>
        private void Awake()
        {
            attachedController = GetComponent<FirstPersonController>();
        }

        /// <summary>
        /// Subscribe to the UpdateMovementState event of the controller on Monobehaviour.OnEnable()
        /// </summary>
        private void OnEnable()
        {
            attachedController.UpdateMovementState += OnUpdateMovementState;
        }

        /// <summary>
        /// Unsubscribe from the UpdateMovementState event of the controller on Monobehaviour.OnDisable()
        /// </summary>
        private void OnDisable()
        {
            attachedController.UpdateMovementState -= OnUpdateMovementState;
        }

        /// <summary>
        /// Handler for the UpdateMovementState event which sets animator parameters for the animators to determine which animation to play
        /// </summary>
        /// <param name="state">The current MovementState</param>
        private void OnUpdateMovementState(MovementState state)
        {
            foreach(Animator a in animatorsToCall)
            {
                // Determine grounded
                switch (state)
                {
                    case MovementState.falling:
                    case MovementState.jumping:
                        a.SetBool("IsGrounded", false);
                        break;
                    default:
                        a.SetBool("IsGrounded", true);
                        break;
                }

                // Determine crouched
                switch (state)
                {
                    // idle-crouched is treated as an idle state rather than a crouch state so that an idle animation plays
                    case MovementState.fast_crouched:
                    case MovementState.norm_crouched:
                    case MovementState.slow_crouched:
                        a.SetBool("IsCrouched", true);
                        break;
                    default:
                        a.SetBool("IsCrouched", false);
                        break;
                }

                // Determine movement setting
                switch(state)
                {
                    case MovementState.fast:
                        a.SetInteger("MovementSetting", 3);
                        break;
                    case MovementState.norm:
                        a.SetInteger("MovementSetting", 2);
                        break;
                    case MovementState.slow:
                        a.SetInteger("MovementSetting", 1);
                        break;
                    // includes idle/idle_crouched, all others are overwritten anyways
                    default:
                        a.SetInteger("MovementSetting", 0);
                        break;
                }
            }
        }
        #endregion
    }
}
