using System.Collections;
using UnityEngine;

namespace Goldenwere.Unity.Controller
{
/// <summary>
    /// Optional module to handle the controller's jumping,
    /// with the ability to perform tap-based, held-based, or tap-then-held-based jumps,
    /// which is separated from the controller
    /// </summary>
    /// <remarks>
    /// While this is a very generic module with multiple modes of jumping,
    /// this is separate to follow the optional modules pattern
    /// </remarks>
    [RequireComponent(typeof(CharacterController))]
    public partial class ControllerJump : MonoBehaviour
    {
        protected delegate void JumpForm();

        /// <summary>
        /// Defines what mode of jumping to perform
        /// </summary>
        protected enum JumpMode : int
        {
            /// <summary>
            /// simply not assigning in inspector would achieve the effect of JumpMode.none,
            /// however this exists in case of situations such as prefab variants
            /// </summary>
            none                    = 0,
            j_tap                   = 1,
            j_held                  = 2,
            j_tapThenHold           = 3,
            // TODO: j_tapThenBoostOrHold = 4
        }

        /// <summary>
        /// Structure for defining jump settings
        /// </summary>
        [System.Serializable]
        protected struct JumpSettings
        {
            [Tooltip                        ("What form of jumping is used for the controller")]
            public JumpMode                 mode;

            [Tooltip                        ("[mode: tap] How many jumps can be performed")]
            public int                      jumpCount;

            [Tooltip                        ("[mode: tap] Whether to reset vertical velocity before applying jump" +
                                            "This is useful in a multi-tap setup in preventing bugs.")]
            public bool                     resetVerticalForTap;

            [Tooltip                        ("[mode: tap or tapThenHold] The delay between jumps")]
            public float                    delayBetweenJumps;

            [Tooltip                        ("[mode: tap or tapThenHold] The tap force of a jump")]
            public float                    tapJumpForce;

            [Tooltip                        ("[mode: held or tapThenHold] The hold force of a jump")]
            public float                    heldJumpForce;

            [Tooltip                        ("[mode: any] Useful for bypassing potential bugginess caused by grounding")]
            public float                    shellJumpOffset;
        }
        
#pragma warning disable 0649
        [SerializeField] private JumpSettings               jumpSettings;
#pragma warning restore 0649
        /**************/ private CharacterController        controller;
        /**************/ private bool                       controllerGrounded;
        /**************/ private bool                       controllerJump;
        /**************/ private bool                       controllerJumpReleased;
        /**************/ private JumpForm                   jumpForm;
        /**************/ private PrioritizedOptionalModule  jumpModule;
        /**************/ private int                        jumpsLeft;                  // only applies for j_multiTap
        /**************/ private float                      jumpTimer;
        /**************/ private bool                       preventHold;
        /**************/ private bool                       preventTap;
        
        public float    TapJumpForceMultiplier  { get; set; }

        public float    HeldJumpForceMultiplier { get; set; }

        public float    DelayBetweenJumps
        {
            get => jumpSettings.delayBetweenJumps;
            set => jumpSettings.delayBetweenJumps = value;
        }

        public int      JumpCount
        {
            get => jumpSettings.jumpCount;
            set => jumpSettings.jumpCount = value;
        }

        public bool     PreventHold
        {
            get { return preventHold; }
            set
            {
                preventHold = value;
                UpdateModuleState();
            }
        }

        public bool     PreventTap
        {
            get { return preventTap; }
            set
            {
                preventTap = value;
                UpdateModuleState();
            }
        }

        /// <summary>
        /// Setup jump module on awake amd controller initialization
        /// </summary>
        private void Awake()
        {
            // setup main variables
            controller = GetComponent<CharacterController>();
            jumpModule = new PrioritizedOptionalModule(1, Update_Jump);

            // ensure settings from inspector are valid
            if (jumpSettings.jumpCount < 1)
                jumpSettings.jumpCount = 1;
            if (jumpSettings.delayBetweenJumps < 0)
                jumpSettings.delayBetweenJumps = 0;

            // assign working variables
            ResetWorkingVariables();
            TapJumpForceMultiplier = 1;
            HeldJumpForceMultiplier = 1;

            // add module when controller is loaded
            controller.ControllerLoaded += (c) =>
            {
                switch (jumpSettings.mode)
                {
                    case JumpMode.j_tapThenHold:
                        controller.AddModuleToUpdate(jumpModule);
                        jumpForm = JumpModeTapThenHeld;
                        break;
                    case JumpMode.j_held:
                        controller.AddModuleToUpdate(jumpModule);
                        jumpForm = JumpModeHeld;
                        break;
                    case JumpMode.j_tap:
                        controller.AddModuleToUpdate(jumpModule);
                        jumpForm = JumpModeTap;
                        break;
                    case JumpMode.none:
                    default:
                        controller.RemoveModuleFromUpdate(jumpModule);
                        jumpForm = JumpModeTap;
                        break;
                }
            };

            // listen to groundstate and reset working variables
            controller.GroundStateChanged += (val) =>
            {
                controllerGrounded = val;
                if (val)
                    ResetWorkingVariables();
            };

            // listen to jumpstate
            controller.Jump += (val) => 
            {
                controllerJump = val;
                if (!val && !controllerGrounded && !controllerJumpReleased)
                    controllerJumpReleased = true;
            };
        }

        /// <summary>
        /// [Performed Under: Update()] Updates the jump module
        /// </summary>
        private void Update_Jump()
        {
            jumpForm();
        }

        /// <summary>
        /// Updates the module state when a prevent bool is toggled
        /// </summary>
        private void UpdateModuleState()
        {
            // module should be removed under the following conditions:
            // 1. both tap and hold are prevented, regardless of mode
            // 2. if mode is held and held is prevented
            // 3. if mode is tap and tap is prevented
            if (preventTap && preventHold ||
                preventHold && jumpSettings.mode == JumpMode.j_held ||
                preventTap && jumpSettings.mode == JumpMode.j_tap)
                controller.RemoveModuleFromUpdate(jumpModule);

            // module should be added under the following conditions:
            // 1. both tap and hold are not prevented and mode isn't none
            // 2. if mode is held and held isn't prevented
            // 3. if mode is tap and tap isn't prevented
            else if (!preventTap && !preventHold ||
                !preventHold && jumpSettings.mode == JumpMode.j_held ||
                !preventTap && jumpSettings.mode == JumpMode.j_tap)
                controller.AddModuleToUpdate(jumpModule);
        }

        private void JumpModeTap()
        {
            jumpTimer += Time.deltaTime;
            if (controllerJump && jumpTimer >= DelayBetweenJumps && jumpsLeft > 0)
            {
                if (controllerGrounded)
                {
                    jumpsLeft = JumpCount;
                    transform.position += transform.up * jumpSettings.shellJumpOffset;
                }
                if (jumpSettings.resetVerticalForTap)
                {
                    Vector3 vel = controller.System.HorizontalVelocity;
                    controller.System.Velocity = vel;
                }
                controller.System.AddForce(transform.up * jumpSettings.tapJumpForce * TapJumpForceMultiplier, ForceMode.Impulse);
                jumpTimer = 0;
                jumpsLeft--;
            }
        }

        private void JumpModeHeld()
        {
            if (controllerJump)
            {
                if(controllerGrounded)
                    transform.position += transform.up * jumpSettings.shellJumpOffset * Time.deltaTime;
                controller.System.AddForce(transform.up * jumpSettings.heldJumpForce * HeldJumpForceMultiplier, ForceMode.Force);
            }
        }

        private void JumpModeTapThenHeld()
        {
            if (!controllerGrounded)
                jumpTimer += Time.deltaTime;
            if (controllerJump)
            {
                if (controllerGrounded)
                {
                    transform.position += transform.up * jumpSettings.shellJumpOffset;
                    controller.System.AddForce(transform.up * jumpSettings.tapJumpForce * TapJumpForceMultiplier, ForceMode.Impulse);
                }
                else if (jumpTimer >= DelayBetweenJumps && controllerJumpReleased)
                    controller.System.AddForce(transform.up * jumpSettings.heldJumpForce * HeldJumpForceMultiplier, ForceMode.Force);
            }
        }

        /// <summary>
        /// Utility function for resetting updated values
        /// </summary>
        private void ResetWorkingVariables()
        {
            jumpsLeft = jumpSettings.jumpCount;
            jumpTimer = jumpSettings.delayBetweenJumps;
            controllerJumpReleased = false;
        }
    }
}