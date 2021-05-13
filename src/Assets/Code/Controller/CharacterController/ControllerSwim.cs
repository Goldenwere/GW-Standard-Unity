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
        }

#pragma warning disable 0649
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
            {
                controller.GravityModifier = 0;
                controller.StickToGroundModifier = 0.1f;
            }
        }

        public void OnFluidExit(BodyOfFluid fluid)
        {
            controller.RemoveModuleFromFixedUpdate(swimModule);
            trackedFluid = null;
            controller.GravityModifier = 1;
            controller.StickToGroundModifier = 1;
            if (!controller.InputActiveLean)
                controller.IsMovementBlocked = false;
            controller.IsHeightBlocked = false;
        }

        private void FixedUpdate_Swim()
        {
            controller.IsMovementBlocked = true;
            if (swimSettings.controllerSinks)
                controller.System.AddForce(-controller.System.VerticalVelocity * trackedFluid.VelocityDampening * swimSettings.sinkSpeedModifier, ForceMode.Impulse);
            controller.System.AddForce(-controller.System.Velocity * trackedFluid.Friction, ForceMode.Force);
        }
    }
}