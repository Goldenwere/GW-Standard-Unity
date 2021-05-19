using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Goldenwere.Unity.PhysicsUtil;

namespace Goldenwere.Unity.Controller
{
    /// <summary>
    /// Optional module to handle the controller's swimming,
    /// which is separated from the controller
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class ControllerSwim : MonoBehaviour, ISwimmable
    {
        [System.Serializable]
        protected struct SwimSettings
        {
            [Tooltip                                        ("Whether the controller sinks or not")]
            public bool                                     controllerSinks;
            [Tooltip                                        ("The point above water in which the controller can no longer swim up")]
            public float                                    heightAboveWater;
            [Tooltip                                        ("The amount in which mmovement speed is modified (movement is handled a bit differently from gravity)")]
            public float                                    moveSpeedModifier;
            [Tooltip                                        ("The amount in which sink speed is modified (gravity is handled a bit differently from regular movement speed)")]
            public float                                    sinkSpeedModifier;

            public float                                    speedSwimUp;
            public float                                    speedSwimDown;
            public float                                    speedSwimMovement;
        }

#pragma warning disable 0649
        [Tooltip                                            ("(Optional) If provided, the controller will move in the direction of the camera's forward " +
                                                            "rather than the transform's forward (which doesn't rotate)")]
        [SerializeField] private Transform                  cameraForForward;
        [SerializeField] private SwimSettings               swimSettings;
        [SerializeField] private PrioritizedOptionalModule  swimModule;
#pragma warning restore 0649
        /**************/ private CharacterController        controller;
        /**************/ private BodyOfFluid                trackedFluid;

        private void Awake()
        {
            swimModule = new PrioritizedOptionalModule(10, FixedUpdate_Swim);
            controller = GetComponent<CharacterController>();
        }

        public void OnFluidEnter(BodyOfFluid fluid)
        {
            controller.AddModuleToFixedUpdate(swimModule);
            trackedFluid = fluid;
            controller.IsMovementBlocked = true;
            controller.IsHeightBlocked = true;
            if (!swimSettings.controllerSinks)
                controller.IsPhysicsBlocked = true;
        }

        public void OnFluidExit(BodyOfFluid fluid)
        {
            controller.RemoveModuleFromFixedUpdate(swimModule);
            trackedFluid = null;
            controller.IsPhysicsBlocked = false;
            if (!controller.InputActiveLean)
                controller.IsMovementBlocked = false;
            controller.IsHeightBlocked = false;
        }

        private void FixedUpdate_Swim()
        {
            controller.IsMovementBlocked = true;

            if (controller.InputValueJump)
                controller.System.AddForce(transform.up * swimSettings.speedSwimUp, ForceMode.VelocityChange);
            else if (controller.InputValueCrouch || controller.InputValueCrawl)
                controller.System.AddForce(-transform.up * swimSettings.speedSwimDown, ForceMode.VelocityChange);
            if (controller.InputActiveMovement)
            {
                Vector3 intendedDirection = cameraForForward != null ?
                    cameraForForward.forward * controller.InputValueMovement.y + cameraForForward.right * controller.InputValueMovement.x :
                    transform.forward * controller.InputValueMovement.y + transform.right * controller.InputValueMovement.x;
                controller.System.AddForce(intendedDirection * swimSettings.speedSwimMovement, ForceMode.VelocityChange);
            }

            if (swimSettings.controllerSinks)
                controller.System.AddForce(Physics.gravity * swimSettings.sinkSpeedModifier, ForceMode.Acceleration);
            controller.System.AddForce(-controller.System.Velocity * trackedFluid.Friction, ForceMode.Force);
        }
    }
}