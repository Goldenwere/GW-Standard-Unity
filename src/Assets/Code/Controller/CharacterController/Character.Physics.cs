using System.Collections.Generic;
using UnityEngine;

namespace Goldenwere.Unity.Controller
{
    public partial class CharacterController
    {
#pragma warning disable CS0108
        private CapsuleCollider     collider;
#pragma warning restore CS0108
        private bool                grounded;
        private ICharacterPhysics   system;

        public ICharacterPhysics    System                  => system;
        public CapsuleCollider      Collider                => collider;

        public bool                 Grounded
        {
            get { return grounded; }
            set
            {
                if (value != grounded)
                {
                    grounded = value;
                    GroundStateChanged?.Invoke(value);
                }
            }
        }
        public Vector3              GroundContactNormal     { get; private set; }
        public Collider             GroundCollider          { get; private set; }
        public float                SlopeAngle              { get; private set; }
        public float                Height                  => collider.height;

        public event InputActive    GroundStateChanged;

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
        }

        /// <summary>
        /// [Performed Under: FixedUpdate()] Updates the controller's physics
        /// </summary>
        private void FixedUpdate_Physics()
        {
            // TODO: implement way of sleeping to prevent calling this every frame; ideally, if not asleep and there's no forces applied and no inputs/is grounded, sleep
            // AddForce automatically unsleeps (dont check for force while asleep)
            // the check nesting would be if !Sleep -> !Input && Grounded -> if velocity.sqrMag < sqrEpsilon -> sleep()
            Grounded = Physics.SphereCast(transform.position + transform.up * settingsForPhysics.shellOffset,
                collider.radius * (1.0f - settingsForPhysics.shellOffset),
                Vector3.down,
                out RaycastHit hit,
                settingsForPhysics.groundDistance,
                Physics.AllLayers, QueryTriggerInteraction.Ignore);
            GroundContactNormal = hit.normal;
            GroundCollider = hit.collider;
            if (hit.collider != null)
                SlopeAngle = Vector3.Angle(GroundContactNormal, IntendedDirection) - 90;

            if (!IsPhysicsBlocked)
            {
                if (Grounded)
                {
                    system.AddForce(-transform.up * (settingsForPhysics.forceStickToGround + system.HorizontalVelocity.sqrMagnitude), ForceMode.Impulse);
                    system.AddForce(-system.HorizontalVelocity * settingsForPhysics.frictionGround, ForceMode.Impulse);
                    if (Mathf.Abs(SlopeAngle) < 90f)
                        system.Velocity = Vector3.ProjectOnPlane(system.Velocity, GroundContactNormal);
                }
                else 
                {
                    system.AddForce(Physics.gravity * settingsForPhysics.forceGravity, ForceMode.Acceleration);
                    system.AddForce(-system.HorizontalVelocity * settingsForPhysics.frictionAir, ForceMode.Force);
                }
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (UnityEditor.EditorApplication.isPlaying)
            {
                Gizmos.DrawSphere(transform.position + transform.up * settingsForPhysics.shellOffset, collider.radius * (1.0f - settingsForPhysics.shellOffset));
                Gizmos.DrawSphere(transform.position - transform.up * settingsForPhysics.groundDistance, collider.radius * (1.0f - settingsForPhysics.shellOffset));
                Gizmos.DrawLine(transform.position + transform.up * settingsForPhysics.shellOffset, transform.position + Vector3.down * settingsForPhysics.groundDistance);
            }
        }
#endif
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
        public float frictionGround;
        public float frictionAir;

        public float shellOffset;
        public float groundDistance;

        public float forceStickToGround;
        public float forceGravity;
    }

    /// <summary>
    /// Interface for the controller to utilize similar physics methods between various systems
    /// </summary>
    public interface ICharacterPhysics
    {
        Vector3 Velocity { get; set; }
        Vector3 HorizontalVelocity { get; }
        Vector3 VerticalVelocity { get; }
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
        public Vector3 Velocity
        {
            get => rigidbody.velocity;
            set => rigidbody.velocity = value;
        }
        public float Mass => rigidbody.mass;
        public Vector3 HorizontalVelocity => new Vector3(Velocity.x, 0, Velocity.z);
        public Vector3 VerticalVelocity => new Vector3(0, Velocity.y, 0);

        public void Initialize(Transform t, PhysicSettings settings)
        {
            transform = t;
            if (!t.TryGetComponent(out rigidbody))
                rigidbody = t.gameObject.AddComponent<Rigidbody>();

            rigidbody.mass = settings.mass;
            rigidbody.drag = settings.drag;
            rigidbody.angularDrag = settings.angularDrag;
            rigidbody.freezeRotation = true;
            rigidbody.useGravity = false;
        }

        public void AddForce(Vector3 force, ForceMode mode) => rigidbody.AddForce(force, mode);
    }

    /// <summary>
    /// Custom physics class which allows the direction of gravity to be manipulated and specific to the body
    /// </summary>
    public class CharacterPhysicsShiftableGravity : ICharacterPhysics
    {
        private Transform transform;
        
        public Vector3 Velocity
        {
            get => throw new System.NotImplementedException();
            set => throw new System.NotImplementedException();
        }
        public float Mass => throw new System.NotImplementedException();
        public Vector3 HorizontalVelocity => new Vector3(Velocity.x, 0, Velocity.z);
        public Vector3 VerticalVelocity => new Vector3(0, Velocity.y, 0);

        public void Initialize(Transform t, PhysicSettings settings)
        {
            transform = t;
        }

        public void AddForce(Vector3 force, ForceMode mode) => throw new System.NotImplementedException();
    }
}