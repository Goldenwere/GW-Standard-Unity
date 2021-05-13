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
            [Tooltip                                        ("The point above water in which the controller can no longer swim up")]
            public float                                    heightAboveWater;
        }

#pragma warning disable 0649
        [SerializeField] private SwimSettings               swimSettings;
#pragma warning restore 0649
        /**************/ private CharacterController        controller;

        private void Awake()
        {
            
        }

        public void OnWaterEnter(BodyOfWater water)
        {
            throw new System.NotImplementedException();
        }

        public void OnWaterExit(BodyOfWater water)
        {
            throw new System.NotImplementedException();
        }
    }
}