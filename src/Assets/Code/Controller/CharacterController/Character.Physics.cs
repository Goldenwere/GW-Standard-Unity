using UnityEngine;

namespace Goldenwere.Unity.Controller
{
    /// <summary>
    /// Interface for the controller to utilize similar physics methods between various systems
    /// </summary>
    public interface ICharacterPhysics
    {
        void Initialize(Transform t);
    }

    /// <summary>
    /// General wrapper for a rigidbody-based controller
    /// </summary>
    public class CharacterPhysicsRigidbodyBased : ICharacterPhysics
    {
        private Transform transform;
        private Rigidbody rigidbody;

        public void Initialize(Transform t)
        {
            transform = t;
            if (!t.TryGetComponent(out rigidbody))
                rigidbody = t.gameObject.AddComponent<Rigidbody>();
        }
    }

    /// <summary>
    /// Custom physics class which allows the direction of gravity to be manipulated and specific to the body
    /// </summary>
    public class CharacterPhysicsShiftableGravity : ICharacterPhysics
    {
        private Transform transform;

        public void Initialize(Transform t)
        {
            transform = t;
        }
    }
}