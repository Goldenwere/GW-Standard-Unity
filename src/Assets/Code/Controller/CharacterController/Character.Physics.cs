using UnityEngine;

namespace Goldenwere.Unity.Controller
{
    public partial class CharacterController
    {
        private ICharacterPhysics   system;
#pragma warning disable CS0108
        private CapsuleCollider     collider;
#pragma warning restore CS0108

        public ICharacterPhysics    System => system;

        public bool                 Grounded                { get; private set; }
        public Vector3              GroundContactNormal     { get; private set; }

        /// <summary>
        /// Initializes the physics module
        /// </summary>
        /// <param name="_system">The type of system to use for the physics module</param>
        public void InitializePhysics(ICharacterPhysics _system)
        {
            system = _system;
            system.Initialize(transform, settingsForPhysics);

            if (!transform.TryGetComponent(out collider))
                collider = gameObject.AddComponent<CapsuleCollider>();

            // we will set center as height / 2 to make grounding math simpler by making ground = the actual bottom of the controller
            collider.height = settingsForPhysics.heightNormal;
            collider.center = new Vector3(0, collider.height / 2, 0);
        }

        /// <summary>
        /// [Performed Under: FixedUpdate()] Updates the controller's physics
        /// </summary>
        private void FixedUpdate_Physics()
        {
            // TODO: implement way of sleeping to prevent calling this every frame; ideally, if not asleep and there's no forces applied and no inputs/is grounded, sleep
            // AddForce automatically unsleeps (dont check for force while asleep)
            // the check nesting would be if !Sleep -> !Input && Grounded -> if velocity.sqrMag < sqrEpsilon -> sleep()
            Grounded = Physics.SphereCast(transform.position,
                collider.radius * (1.0f - settingsForPhysics.shellOffset),
                Vector3.down,
                out RaycastHit hit,
                settingsForPhysics.groundDistance,
                Physics.AllLayers, QueryTriggerInteraction.Ignore);
            GroundContactNormal = hit.normal;
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

        public float shellOffset;
        public float groundDistance;

        public float heightNormal;
        public float heightCrouch;
        public float heightCrawl;
    }

    /// <summary>
    /// Interface for the controller to utilize similar physics methods between various systems
    /// </summary>
    public interface ICharacterPhysics
    {
        Vector3 Velocity { get; }
        float Mass { get; }

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
        public Vector3 Velocity => rigidbody.velocity;
        public float Mass => rigidbody.mass;

        public void Initialize(Transform t, PhysicSettings settings)
        {
            transform = t;
            if (!t.TryGetComponent(out rigidbody))
                rigidbody = t.gameObject.AddComponent<Rigidbody>();

            rigidbody.mass = settings.mass;
            rigidbody.drag = settings.drag;
            rigidbody.angularDrag = settings.angularDrag;
            rigidbody.freezeRotation = true;
        }

        public void AddForce(Vector3 force, ForceMode mode) => rigidbody.AddForce(force, mode);
    }

    /// <summary>
    /// Custom physics class which allows the direction of gravity to be manipulated and specific to the body
    /// </summary>
    public class CharacterPhysicsShiftableGravity : ICharacterPhysics
    {
        private Transform transform;

        public Vector3 Velocity => throw new System.NotImplementedException();
        public float Mass => throw new System.NotImplementedException();

        public void Initialize(Transform t, PhysicSettings settings)
        {
            transform = t;
        }

        public void AddForce(Vector3 force, ForceMode mode) => throw new System.NotImplementedException();
    }
}