﻿/**
*** Copyright (C) 2020 Goldenwere
*** Part of the Goldenwere Standard Unity repository
*** The Goldenwere Standard Unity Repository is licensed under the MIT license
***
*** File Info:
***     Description - Contains the optional lean module of the CharacterController
***     Pkg Name    - CharacterController
***     Pkg Ver     - 2.0.0
***     Pkg Req     - CoreAPI
**/

using System.Collections;
using UnityEngine;

namespace Goldenwere.Unity.Controllers.CharacterController
{
    /// <summary>
    /// Optional module to handle the controller's leaning,
    /// which is separated from the controller
    /// </summary>
    [RequireComponent(typeof(GWCharacterController))]
    public class CharacterLean : MonoBehaviour
    {
        [System.Serializable]
        protected struct LeanSettings
        {
            [Tooltip                                            ("The maximum angle the character can lean")]
            public float                                        angleMaxLean;
            [Tooltip                                            ("Whether to block the ability for the controller to move")]
            public bool                                         preventMovement;
            [Tooltip                                            ("The speed at which the character leans/unleans")]
            public float                                        speedLean;
        }
#pragma warning disable 0649
        [SerializeField] private Transform                      leanJoint;
        [SerializeField] private LeanSettings                   leanSettings;
#pragma warning restore 0649
        /**************/ private GWCharacterController          controller;
        /**************/ private PrioritizedControllerModule    leanModule;
        /**************/ private IEnumerator                    unsetInstance;

        /// <summary>
        /// Sets up the lean module on Unity Awake
        /// </summary>
        private void Awake()
        {
            controller = GetComponent<GWCharacterController>();
            leanModule = new PrioritizedControllerModule(0, Update_Lean);

            controller.Lean += (val) =>
            {
                if (unsetInstance != null)
                    StopCoroutine(unsetInstance);

                if (val)
                {
                    controller.AddModuleToUpdate(leanModule);
                    if (leanSettings.preventMovement)
                        controller.IsMovementBlocked = true;
                }
                else
                {
                    unsetInstance = UnsetLean();
                    StartCoroutine(unsetInstance);
                    controller.IsMovementBlocked = false;
                    controller.RemoveModuleFromUpdate(leanModule);
                }
            };
        }

        /// <summary>
        /// The Update loop for the Lean module
        /// </summary>
        private void Update_Lean()
        {
            leanJoint.localRotation = Quaternion.Slerp(
                leanJoint.localRotation,
                Quaternion.Euler(new Vector3(0, 0, leanSettings.angleMaxLean * -controller.InputValueLean)),
                Time.deltaTime * leanSettings.speedLean);
        }

        /// <summary>
        /// Coroutine for unsetting lean; runs when the module's update loop is deactivated
        /// to reset the controller's angle outside the loop
        /// </summary>
        private IEnumerator UnsetLean()
        {
            float currAngle;

            do
            {
                leanJoint.localRotation = Quaternion.Slerp(
                    leanJoint.localRotation,
                    Quaternion.identity,
                    Time.deltaTime * leanSettings.speedLean);
                currAngle = leanJoint.localRotation.eulerAngles.z;
                yield return null;
            }
            while (currAngle > 0.01f || currAngle < -0.01f);
        }
    }
}