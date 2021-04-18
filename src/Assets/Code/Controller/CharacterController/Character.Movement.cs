using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Goldenwere.Unity.Controller
{ 
    public partial class CharacterController : MonoBehaviour
    {
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
        }

        /// <summary>
        /// Exposed "final" multiplier to apply to the controller
        /// </summary>
        /// <remarks>E.G.: a status effect is affecting the player</remarks>
        public float                SpeedMultiplier { get; set; }

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
            if (InputActiveMovement)
            {
                if (settingsForMovement.allowAirMovement || Grounded)
                {
                    // 1. get movement values
                    Vector3 val = InputValueMovement;
                    Vector3 dir = transform.forward * val.y + transform.right * val.x;
                    float speed = GetSpeed();

                    // 2. create force, project it on the current surface normal
                    Vector3 force = dir * speed;
                    force = Vector3.ProjectOnPlane(force, GroundContactNormal);

                    // 3. get horizontal velocity and add force if vel.mag < speed
                    if (system.HorizontalVelocity.sqrMagnitude < Mathf.Pow(speed, 2))
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

            return f * SpeedMultiplier;
        }
    }
}