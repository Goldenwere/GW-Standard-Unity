﻿using UnityEngine;

namespace Goldenwere.Unity.PhysicsUtil
{
    /// <summary>
    /// A BodyOfWater is a GameObject ideally with a box collider trigger which notifies ISwimmables that they've entered water
    /// </summary>
    public class BodyOfFluid : MonoBehaviour
    {
#pragma warning disable 0649
        [Tooltip                            ("The friction of the fluid")]
        [SerializeField] private float      friction;
        [Tooltip                            ("The y-position at which the fluid's surface is")]
        [SerializeField] private float      surfaceLevel;
#pragma warning restore 0649

        /// <summary>
        /// The friction of the fluid
        /// </summary>
        public float                        Friction        => friction;

        /// <summary>
        /// The surface level of the fluid in global space
        /// </summary>
        public float                        SurfaceLevel    => surfaceLevel;

        /// <summary>
        /// On TriggerEnter, if collider `other` is ISwimmable, send this BodyOfFluid
        /// </summary>
        /// <param name="other">The other collider that has interacted with this BodyOfFluid</param>
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out ISwimmable swimmable))
                swimmable.OnFluidEnter(this);
        }

        /// <summary>
        /// On TriggerExit, if collider `other` is ISwimmable, send this BodyOfFluid
        /// </summary>
        /// <param name="other">The other collider that has interacted with this BodyOfFluid</param>
        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out ISwimmable swimmable))
                swimmable.OnFluidExit(this);
        }
    }
}