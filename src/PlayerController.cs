#pragma warning disable 0649

using UnityEngine;

namespace Goldenwere.Unity.Controller
{
    /// <summary>
    /// Used for player's physical and camera motion
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        #region Fields & Properties
        [SerializeField]    private Camera          charCamera;
        [SerializeField]    private new Collider    collider;
        [SerializeField]    private new Rigidbody   rigidbody;
        [SerializeField]    private bool            smoothLook;
        [SerializeField]    private float           smoothTime;
        [SerializeField]    private float           speedWalk;
        [SerializeField]    private float           speedRun;
        [SerializeField]    private float           speedJump;
        [SerializeField]    private float           speedTurn;
        [SerializeField]    private float           stickToGround;
        /**************/    public  bool            canMove;
        /**************/    public  bool            canRun;
        /**************/    private Quaternion      cameraDesiredRotation;
        /**************/    private const float     cameraMinVertRot = -85f;
        /**************/    private const float     cameraMaxVertRot = 80f;
        /**************/    private Quaternion      controllerDesiredRotation;
        /**************/    private float           desiredSpeed;
        /**************/    private Vector3         dir = Vector3.zero;

        private bool IsGrounded
        {
            get { return Physics.Raycast(transform.position, -Vector3.up, collider.bounds.extents.y + 0.1f); }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Handles controller variables before first frame update
        /// </summary>
        private void Start()
        {
            controllerDesiredRotation = transform.localRotation;
            cameraDesiredRotation = charCamera.transform.localRotation;
            canRun = true;
            canMove = true;
        }

        /// <summary>
        /// Updates the character once per frame
        /// </summary>
        private void Update()
        {
            if (canMove)
            {
                RotateCharacter();
                MoveCharacter();
            }
        }

        /// <summary>
        /// Updates the character once per physics frame
        /// </summary>
        private void FixedUpdate()
        {
            RaycastHit hit;
            Vector3 horizontal = new Vector3(dir.x, 0, dir.z);
            // second check is for doors, whose colliders are on layer 9 - "Unbaked"; 
            // player will get stuck in door otherwise until they back up and then go forward even after door opens
            if (!rigidbody.SweepTest(horizontal, out hit, horizontal.magnitude * Time.fixedDeltaTime) || hit.collider.gameObject.layer == 9)
                rigidbody.AddForce(dir, ForceMode.VelocityChange);
            Vector3 clamped = Vector3.ClampMagnitude(rigidbody.velocity, desiredSpeed);
            clamped.y = rigidbody.velocity.y;
            rigidbody.velocity = clamped;
        }

        /// <summary>
        /// Determines desired direction of movement based on player input
        /// </summary>
        /// <returns>The desired direction vector</returns>
        private Vector3 GetInput()
        {
            float moveX;
            float moveZ;

            if (Input.GetKey(KeyCode.W))
                moveZ = 1;
            else if (Input.GetKey(KeyCode.S))
                moveZ = -1;
            else
                moveZ = 0;

            if (Input.GetKey(KeyCode.D))
                moveX = 1;
            else if (Input.GetKey(KeyCode.A))
                moveX = -1;
            else
                moveX = 0;

            return transform.forward * moveZ + transform.right * moveX;
        }

        /// <summary>
        /// Determines player input for character movement
        /// </summary>
        private void MoveCharacter()
        {
            // Translate input into directional movement in x/z
            Vector3 input = GetInput();
            if (Input.GetKey(KeyCode.LeftShift) && canRun)
                desiredSpeed = speedRun;
            else
                desiredSpeed = speedWalk;
            dir.x = input.x * desiredSpeed;
            dir.z = input.z * desiredSpeed;

            // Handle vertical input
            if (IsGrounded)
                if (Input.GetKeyDown(KeyCode.Space))
                    rigidbody.AddForce(Vector3.up * speedJump * Physics.gravity.y, ForceMode.VelocityChange);
                else
                    dir.y = -stickToGround;
            else
                rigidbody.AddForce(Physics.gravity * (rigidbody.mass * 0.1f), ForceMode.Acceleration);
        }

        /// <summary>
        /// Rotates the controller and camera
        /// </summary>
        private void RotateCharacter()
        {
            float yRot = Input.GetAxis("Mouse X") * speedTurn;
            float xRot = Input.GetAxis("Mouse Y") * speedTurn;

            controllerDesiredRotation *= Quaternion.Euler(0f, yRot, 0f);
            cameraDesiredRotation *= Quaternion.Euler(-xRot, 0f, 0f);

            cameraDesiredRotation = cameraDesiredRotation.VerticalClampEuler(cameraMinVertRot, cameraMaxVertRot);

            if (smoothLook)
            {
                transform.localRotation = Quaternion.Slerp(transform.localRotation, controllerDesiredRotation, smoothTime * Time.fixedDeltaTime);
                charCamera.transform.localRotation = Quaternion.Slerp(charCamera.transform.localRotation, cameraDesiredRotation, smoothTime * Time.fixedDeltaTime);
            }

            else
            {
                transform.localRotation = controllerDesiredRotation;
                charCamera.transform.localRotation = cameraDesiredRotation;
            }
        }
        #endregion
    }
}