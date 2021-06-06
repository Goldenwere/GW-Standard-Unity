/**
*** Copyright (C) 2020 Goldenwere
*** Part of the Goldenwere Standard Unity repository
*** The Goldenwere Standard Unity Repository is licensed under the MIT license
***
*** File Info:
***     Description - Contains the movement portion/module of the CharacterController
***     Pkg Name    - CharacterController
***     Pkg Ver     - 2.0.0
***     Pkg Req     - CoreAPI
**/

using UnityEngine;

namespace Goldenwere.Unity.Controllers.CharacterController
{ 
    public partial class GWCharacterController : MonoBehaviour
    {
        /// <summary>
        /// Structure for defining controller movement settings
        /// </summary>
        [System.Serializable]
        protected struct MovementSettings
        {
            public bool             allowWalk;
            public bool             allowRun;
            public bool             allowCrouch;
            public bool             allowCrawl;
            public bool             allowAirMovement;

            public float            speedNormal;
            public float            speedMultiplierWalk;
            public float            speedMultiplierRun;
            public float            speedMultiplierCrouch;
            public float            speedMultiplierCrawl;

            public AnimationCurve   speedSlopeModifier;
        }

        /// <summary>
        /// Exposed "final" multiplier to apply to the controller
        /// </summary>
        /// <remarks>Practical example: a status effect is affecting the speed of the player</remarks>
        public float                SpeedMultiplier     { get; set; }
        
        public bool                 AllowWalk           => settingsForMovement.allowWalk;
        public bool                 AllowRun            => settingsForMovement.allowRun;
        public bool                 AllowCrouch         => settingsForMovement.allowCrouch;
        public bool                 AllowCrawl          => settingsForMovement.allowCrawl;
        public bool                 AllowAirMovement    => settingsForMovement.allowAirMovement;

        public Vector3              IntendedDirection   { get; private set; }

        /// <summary>
        /// Initializes the movement module
        /// </summary>
        private void InitializeMovement()
        {
            SpeedMultiplier = 1;
        }

        /// <summary>
        /// [Performed Under: FixedUpdate()] Updates the controller's basic movement (horizontal + gravity)
        /// </summary>
        /// <remarks>TODO: Jumping is a separate module</remarks>
        private void FixedUpdate_Movement()
        {
            if (InputActiveMovement && !IsMovementBlocked)
            {
                if (settingsForMovement.allowAirMovement || Grounded)
                {
                    // 1. get movement values
                    Vector3 val = InputValueMovement;
                    IntendedDirection = transform.forward * val.y + transform.right * val.x;
                    float speed = GetSpeed();

                    // 2. create force, project it on the current surface normal
                    Vector3 force = IntendedDirection * speed;
                    force = Vector3.ProjectOnPlane(force, GroundContactNormal);

                    // 3. get horizontal velocity and add force if vel.mag < speed
                    if (system.HorizontalVelocity.sqrMagnitude < force.sqrMagnitude)
                        system.AddForce(force, ForceMode.VelocityChange);
                }
            }
        }

        /// <summary>
        /// Gets the controller's current speed
        /// </summary>
        /// <returns>Float of default speed * modifiers * final multiplier</returns>
        private float GetSpeed()
        {
            float f = settingsForMovement.speedNormal;

            // prioritize run over walk
            if (InputValueRun && settingsForMovement.allowRun)
                f *= settingsForMovement.speedMultiplierRun;
            else if (InputValueWalk && settingsForMovement.allowWalk)
                f *= settingsForMovement.speedMultiplierWalk;

            // prioritize crawl over crouch
            if (InputValueCrawl && settingsForMovement.allowCrawl)
                f *= settingsForMovement.speedMultiplierCrawl;
            else if (InputValueCrouch && settingsForMovement.allowCrouch)
                f *= settingsForMovement.speedMultiplierCrouch;

            return f * SpeedMultiplier * settingsForMovement.speedSlopeModifier.Evaluate(SlopeAngle);
        }
    }
}