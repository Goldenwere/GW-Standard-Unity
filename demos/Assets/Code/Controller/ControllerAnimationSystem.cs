using UnityEngine;

namespace Goldenwere.Unity.Controller
{
    [RequireComponent(typeof(FirstPersonController))]
    public class ControllerAnimationSystem : MonoBehaviour
    {
#pragma warning disable 0649
        [Tooltip("Attach all animators you plan on calling animations on. Ensure that you use the same strings as those defined in here.")]
        [SerializeField]    private Animator[]              animatorsToCall;
        /**************/    private FirstPersonController   attachedController;
#pragma warning restore 0649

        private void Awake()
        {
            attachedController = GetComponent<FirstPersonController>();
        }

        private void OnEnable()
        {
            attachedController.UpdateMovementState += OnUpdateMovementState;
        }

        private void OnDisable()
        {
            attachedController.UpdateMovementState -= OnUpdateMovementState;
        }

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
    }
}