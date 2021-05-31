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
    public class ControllerJump : MonoBehaviour
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
            // TODO: j_tapThenHover = 4
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

            [Tooltip                        ("[mode: tap] Whether to reset vertical velocity before applying jump\n" +
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
        [SerializeField] private JumpSettings                   jumpSettings;
#pragma warning restore 0649
        /**************/ private CharacterController            controller;                 // the controller of which this module is attached

        /**************/ private bool                           controllerGrounded;         // listen to when the controller groundstate changes
        /**************/ private bool                           controllerJump;             // listen to when the jump input is called
        // TODO: apply to tap
        // TODO: UX/accessibility property to determine whether this should be respected
        /**************/ private bool                           controllerJumpReleased;     // only applies for tapThenHeld: track when jump is released

        /**************/ private JumpForm                       jumpForm;                   // which jump function to run under FixedUpdate
        /**************/ private PrioritizedControllerModule    jumpModule;                 // the instance of this jump module, sent to the controller to call under its update

        /**************/ private int                            jumpsLeft;                  // only applies for tap: how many jumps are left before the controller must fall
        /**************/ private float                          jumpTimer;                  // only applies for tap or tapThenHeld: how long since a jump was called

        // these are private fields so that one must use the properties in order to optimize the controller with an active/inactive state
        // TODO: implement these fields
        /**************/ private bool                           preventHeld;
        /**************/ private bool                           preventTap;
        
        /// <summary>
        /// Multiplier to be applied to the tap force
        /// </summary>
        /// <remarks>Rather than setting this to 0, for optimization sake, instead set PreventTap to false</remarks>
        public float    TapJumpForceMultiplier  { get; set; }

        /// <summary>
        /// Multiplier to be applied to the held force
        /// </summary>
        /// <remarks>Rather than setting this to 0, for optimization sake, instead set PreventHeld to false</remarks>
        public float    HeldJumpForceMultiplier { get; set; }

        /// <summary>
        /// The delay [for tap] between tap jumps, or [for tapThenHeld] between the tap jump and held jump
        /// </summary>
        public float    DelayBetweenJumps
        {
            get => jumpSettings.delayBetweenJumps;
            set => jumpSettings.delayBetweenJumps = value;
        }

        /// <summary>
        /// The amount [for tap] of jumps the controller has
        /// </summary>
        public int      JumpCount
        {
            get => jumpSettings.jumpCount;
            set => jumpSettings.jumpCount = value;
        }

        /// <summary>
        /// Whether to block held inputs or not
        /// </summary>
        /// <remarks>(useful for temporary effects or temporarily disabling the module if applicable)</remarks>
        public bool     PreventHeld
        {
            get { return preventHeld; }
            set
            {
                preventHeld = value;
                UpdateModuleState();
            }
        }

        /// <summary>
        /// Whether to block tap inputs or not
        /// </summary>
        /// /// <remarks>(useful for temporary effects or temporarily disabling the module if applicable)</remarks>
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
            jumpModule = new PrioritizedControllerModule(1, FixedUpdate_Jump);

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
                        controller.AddModuleToFixedUpdate(jumpModule);
                        jumpForm = JumpModeTapThenHeld;
                        break;
                    case JumpMode.j_held:
                        controller.AddModuleToFixedUpdate(jumpModule);
                        jumpForm = JumpModeHeld;
                        break;
                    case JumpMode.j_tap:
                        controller.AddModuleToFixedUpdate(jumpModule);
                        jumpForm = JumpModeTap;
                        break;
                    case JumpMode.none:
                    default:
                        controller.RemoveModuleFromFixedUpdate(jumpModule);
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
                if (!val || !controller.IsMovementBlocked)
                {
                    controllerJump = val;
                    if (!val && !controllerGrounded && !controllerJumpReleased)
                        controllerJumpReleased = true;
                }
            };
        }

        /// <summary>
        /// [Performed Under: FixedUpdate()] Updates the jump module
        /// </summary>
        private void FixedUpdate_Jump()
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
            if (preventTap && preventHeld ||
                preventHeld && jumpSettings.mode == JumpMode.j_held ||
                preventTap && jumpSettings.mode == JumpMode.j_tap)
                controller.RemoveModuleFromFixedUpdate(jumpModule);

            // module should be added under the following conditions:
            // 1. both tap and hold are not prevented and mode isn't none
            // 2. if mode is held and held isn't prevented
            // 3. if mode is tap and tap isn't prevented
            else if (!preventTap && !preventHeld ||
                !preventHeld && jumpSettings.mode == JumpMode.j_held ||
                !preventTap && jumpSettings.mode == JumpMode.j_tap)
                controller.AddModuleToFixedUpdate(jumpModule);
        }

        /// <summary>
        /// Function which runs under FixedUpdate_Jump when mode = tap
        /// </summary>
        private void JumpModeTap()
        {
            // 1. increment timer each fixed frame
            jumpTimer += Time.fixedDeltaTime;

            // 2. perform jump if: input active, timer is higher than delay, and there's jumps left
            if (controllerJump && jumpTimer >= DelayBetweenJumps && jumpsLeft > 0)
            {
                // A. fix the controller getting stuck from grounding force/checker
                if (controllerGrounded)
                {
                    // i. jumpsLeft needs reset
                    jumpsLeft = JumpCount;

                    // ii. add a physics-less ungrounding assist using shellJumpOffset
                    transform.position += transform.up * jumpSettings.shellJumpOffset;
                }

                // B. reset vertical velocity if set to do so before the next tap
                if (jumpSettings.resetVerticalForTap)
                {
                    Vector3 vel = controller.System.HorizontalVelocity;
                    controller.System.Velocity = vel;
                }

                // C. add impulse force (using transform's up times tap forces), reset timer, and decrement jump counter
                controller.System.AddForce(transform.up * jumpSettings.tapJumpForce * TapJumpForceMultiplier, ForceMode.Impulse);
                jumpTimer = 0;
                jumpsLeft--;
            }
        }

        /// <summary>
        /// Function which runs under FixedUpdate_Jump when mode = held
        /// </summary>
        private void JumpModeHeld()
        {
            // 1. perform jump if: input active
            // TODO: jumpTimer <= HeldJumpLimit
            if (controllerJump)
            {
                // A. fix the controller getting stuck from grounding force/checker
                if(controllerGrounded)
                    transform.position += transform.up * jumpSettings.shellJumpOffset * Time.fixedDeltaTime;
                // B. add regular force (using transform's up times held forces)
                controller.System.AddForce(transform.up * jumpSettings.heldJumpForce * HeldJumpForceMultiplier, ForceMode.Force);
            }
        }

        /// <summary>
        /// Function which runs under FixedUpdate_Jump when mode = tapThenHeld
        /// </summary>
        private void JumpModeTapThenHeld()
        {
            // 1. start incrementing timer when not grounded
            if (!controllerGrounded)
                jumpTimer += Time.fixedDeltaTime;
            // 2. perform jump if: input active
            if (controllerJump)
            {
                // A. perform tap force if: grounded
                if (controllerGrounded)
                {
                    // i. add a physics-less ungrounding assist using shellJumpOffset
                    transform.position += transform.up * jumpSettings.shellJumpOffset;
                    // ii. add impulse force (using transform's up times tap forces)
                    controller.System.AddForce(transform.up * jumpSettings.tapJumpForce * TapJumpForceMultiplier, ForceMode.Impulse);
                    // iii. reset jump timer
                    jumpTimer = 0;
                }

                // B. perform held force if: jump was initially released and timer >= delay
                // TODO: jumpTimer <= HeldJumpLimit
                else if (controllerJumpReleased && jumpTimer >= DelayBetweenJumps)
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