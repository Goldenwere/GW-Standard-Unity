using UnityEngine;

namespace Goldenwere.Unity.PhysicsUtil
{
    /// <summary>
    /// A BodyOfWater is a GameObject ideally with a box collider trigger which notifies ISwimmables that they've entered water
    /// </summary>
    public class BodyOfWater : MonoBehaviour
    {
#pragma warning disable 0649
        [Tooltip                            ("The magnitude in which this BodyOfWater dampens an ISwimmable's velocity")]
        [Range                              (0f, 1f)]
        [SerializeField] private float      velocityDampening;
#pragma warning restore 0649

        /// <summary>
        /// The magnitude (0-1) in which this BodyOfWater dampens an ISwimmable's velocity
        /// </summary>
        public float                        VelocityDampening => velocityDampening;

        /// <summary>
        /// On TriggerEnter, if collider `other` is ISwimmable, send this BodyOfWater
        /// </summary>
        /// <param name="other">The other collider that has interacted with this BodyOfWater</param>
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out ISwimmable swimmable))
                swimmable.OnWaterEnter(this);
        }

        /// <summary>
        /// On TriggerExit, if collider `other` is ISwimmable, send this BodyOfWater
        /// </summary>
        /// <param name="other">The other collider that has interacted with this BodyOfWater</param>
        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out ISwimmable swimmable))
                swimmable.OnWaterExit(this);
        }
    }
}