using UnityEngine;

namespace Goldenwere.Unity.PhysicsUtil
{
    /// <summary>
    /// A BodyOfWater is a GameObject ideally with a box collider trigger which notifies ISwimmables that they've entered water
    /// </summary>
    public class BodyOfFluid : MonoBehaviour
    {
#pragma warning disable 0649
        [Tooltip                            ("The magnitude in which this BodyOfFluid dampens an ISwimmable's velocity")]
        [SerializeField] private float      velocityDampening;
        [Tooltip                            ("The friction of the fluid")]
        [SerializeField] private float      friction;
#pragma warning restore 0649

        /// <summary>
        /// The magnitude in which this BodyOfFluid dampens an ISwimmable's velocity
        /// </summary>
        public float                        VelocityDampening => velocityDampening;

        /// <summary>
        /// The friction of the fluid
        /// </summary>
        public float                        Friction => friction;

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