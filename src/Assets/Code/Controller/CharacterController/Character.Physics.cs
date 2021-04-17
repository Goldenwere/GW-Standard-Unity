using UnityEngine;

namespace Goldenwere.Unity.Controller
{
    public partial class CharacterController
    {
        private ICharacterPhysics system;

        public ICharacterPhysics System => system;

        /// <summary>
        /// Initializes the physics module
        /// </summary>
        /// <param name="_system">The type of system to use for the physics module</param>
        public void InitializePhysics(ICharacterPhysics _system)
        {
            system = _system;
            system.Initialize(transform, settingsForPhysics);
        }
    }

    /// <summary>
    /// Container for physics-related settings
    /// </summary>
    [System.Serializable]
    public struct PhysicSettings
    {
        public float mass;
        public float drag;
        public float angularDrag;
    }

    /// <summary>
    /// Interface for the controller to utilize similar physics methods between various systems
    /// </summary>
    public interface ICharacterPhysics
    {
        /// <summary>
        /// Initializes the physics system
        /// </summary>
        /// <param name="t">The transform that physics is applied to, as the system does not inherit from MonoBehaviour</param>
        /// <param name="settings">The settings for the physics system to initialize with</param>
        void Initialize(Transform t, PhysicSettings settings);

        /// <summary>
        /// Adds a force to a physic body
        /// </summary>
        /// <param name="force">The global vector to add</param>
        /// <param name="mode">The type of force to add</param>
        void AddForce(Vector3 force, ForceMode mode);
    }

    /// <summary>
    /// General wrapper for a rigidbody-based controller
    /// </summary>
    public class CharacterPhysicsRigidbodyBased : ICharacterPhysics
    {
        private Transform transform;
        private Rigidbody rigidbody;

        public Rigidbody Rigidbody => rigidbody;

        public void Initialize(Transform t, PhysicSettings settings)
        {
            transform = t;
            if (!t.TryGetComponent(out rigidbody))
                rigidbody = t.gameObject.AddComponent<Rigidbody>();

            rigidbody.mass = settings.mass;
            rigidbody.drag = settings.drag;
            rigidbody.angularDrag = settings.angularDrag;
        }

        public void AddForce(Vector3 force, ForceMode mode) => rigidbody.AddForce(force, mode);
    }

    /// <summary>
    /// Custom physics class which allows the direction of gravity to be manipulated and specific to the body
    /// </summary>
    public class CharacterPhysicsShiftableGravity : ICharacterPhysics
    {
        private Transform transform;

        public void Initialize(Transform t, PhysicSettings settings)
        {
            transform = t;
        }

        public void AddForce(Vector3 force, ForceMode mode) => throw new System.NotImplementedException();
    }
}