/**
*** Copyright (C) 2020 Goldenwere
*** Part of the Goldenwere Standard Unity repository
*** The Goldenwere Standard Unity Repository is licensed under the MIT license
***
*** File Info:
***     Description - Contains the optional swim module of the CharacterController
***     Pkg Name    - CharacterController
***     Pkg Ver     - 2.0.0
***     Pkg Req     - CoreAPI
**/

using UnityEngine;
using Goldenwere.Unity.PhysicsUtil;

namespace Goldenwere.Unity.Controllers.CharacterController
{
    /// <summary>
    /// Optional module to handle the controller's swimming,
    /// which is separated from the controller
    /// </summary>
    /// <remarks>This module assumes gravity or down is always (0,-1,0) in direction</remarks>
    [RequireComponent(typeof(GWCharacterController))]
    public class ControllerSwim : MonoBehaviour, ISwimmable
    {
        /// <summary>
        /// Structure for defining swim settings
        /// </summary>
        [System.Serializable]
        protected struct SwimSettings
        {
            [Tooltip                                            ("Whether the controller sinks or not")]
            public bool                                         controllerSinks;
            [Tooltip                                            ("The point above water in which the controller can no longer swim up")]
            public float                                        heightAboveWater;
            [Tooltip                                            ("The amount in which sink speed is modified (gravity is handled a bit differently from regular movement speed)")]
            public float                                        sinkSpeedModifier;

            public float                                        speedSwimUp;
            public float                                        speedSwimDown;
            public float                                        speedSwimMovement;
        }

#pragma warning disable 0649
        [Tooltip                                                ("(Optional) If provided, the controller will move in the direction of the camera's forward " +
                                                                "rather than the transform's forward (which doesn't rotate)")]
        [SerializeField] private Transform                      cameraForForward;
        [SerializeField] private SwimSettings                   swimSettings;
        [SerializeField] private PrioritizedControllerModule    swimModule;
#pragma warning restore 0649
        /**************/ private GWCharacterController          controller;
        /**************/ private BodyOfFluid                    trackedFluid;

        /// <summary>
        /// Sets up the module on Unity Awake
        /// </summary>
        private void Awake()
        {
            swimModule = new PrioritizedControllerModule(10, FixedUpdate_Swim);
            controller = GetComponent<GWCharacterController>();
        }

        /// <summary>
        /// Handles activating the module when entering a fluid
        /// </summary>
        /// <param name="fluid">The fluid that was entered</param>
        public void OnFluidEnter(BodyOfFluid fluid)
        {
            controller.AddModuleToFixedUpdate(swimModule);
            trackedFluid = fluid;
            controller.IsMovementBlocked = true;
            controller.IsHeightBlocked = true;
            if (!swimSettings.controllerSinks)
                controller.IsPhysicsBlocked = true;
        }

        /// <summary>
        /// Handles sleeping the module when leaving a fluid
        /// </summary>
        /// <param name="fluid">The fluid that was left</param>
        public void OnFluidExit(BodyOfFluid fluid)
        {
            controller.RemoveModuleFromFixedUpdate(swimModule);
            trackedFluid = null;
            controller.IsPhysicsBlocked = false;
            if (!controller.InputActiveLean)
                controller.IsMovementBlocked = false;
            controller.IsHeightBlocked = false;
        }

        /// <summary>
        /// The FixedUpdate loop for swimming
        /// </summary>
        private void FixedUpdate_Swim()
        {
            // ensure regular movement is completely blocked
            controller.IsMovementBlocked = true;

            // determine vertical movement
            // (note that vertical is the controller's up/down)
            if (controller.InputValueJump)
                controller.System.AddForce(transform.up * swimSettings.speedSwimUp, ForceMode.VelocityChange);
            else if (controller.InputValueCrouch || controller.InputValueCrawl)
                controller.System.AddForce(-transform.up * swimSettings.speedSwimDown, ForceMode.VelocityChange);

            // determine horizontal movement
            // (note that directional movement is based off of the camera's/controller's (if no camera) forward/right
            if (controller.InputActiveMovement)
            {
                Vector3 intendedDirection = cameraForForward != null ?
                    cameraForForward.forward * controller.InputValueMovement.y + cameraForForward.right * controller.InputValueMovement.x :
                    transform.forward * controller.InputValueMovement.y + transform.right * controller.InputValueMovement.x;
                controller.System.AddForce(intendedDirection * swimSettings.speedSwimMovement, ForceMode.VelocityChange);
            }

            // handle if the controller should sink or not
            if (swimSettings.controllerSinks)
                controller.System.AddForce(Physics.gravity * swimSettings.sinkSpeedModifier, ForceMode.Acceleration);
            controller.System.AddForce(-controller.System.Velocity * trackedFluid.Friction, ForceMode.Force);

            // handle "sinking" the controller if its out of water and not grounded
            // to make it so the controller can't exit the water when there's no ground to stand on
            if (IsOutOfWater() && !controller.Grounded)
                controller.System.AddForce(-transform.up * swimSettings.sinkSpeedModifier, ForceMode.Acceleration);
        }

        /// <summary>
        /// Determines whether the controller is out of water or not
        /// </summary>
        /// <returns>position + height - fluid SurfaceLevel > settings heightAboveWater</returns>
        private bool IsOutOfWater()
        {
            return transform.position.y + controller.Height - trackedFluid.SurfaceLevel > swimSettings.heightAboveWater;
        }
    }
}