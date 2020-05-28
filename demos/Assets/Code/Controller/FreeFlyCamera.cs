using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Goldenwere.Unity.Controller
{
    public class FreeFlyCamera : MonoBehaviour
    {
        [SerializeField]    private PlayerInput attachedControls;
        [SerializeField]    private float       settingMoveSpeed = 2f;
        /**************/    public  float       settingRotationSensitivity = 3f;
        /**************/    private bool        workingDoHorizontal;
        /**************/    private bool        workingDoRotation;
        /**************/    private bool        workingDoVertical;

        private void Start()
        {
            workingDoHorizontal = false;
            workingDoRotation = false;
            workingDoVertical = false;
        }

        private void Update()
        {
            if (workingDoHorizontal)
            {
                Vector2 value = attachedControls.actions["Horizontal"].ReadValue<Vector2>().normalized;
                Vector3 dir = transform.forward * value.y + transform.right * value.x;
                transform.Translate(dir * Time.deltaTime * settingMoveSpeed);
            }

            if (workingDoRotation)
            {

            }

            if (workingDoVertical)
            {
                float value = attachedControls.actions["Vertical"].ReadValue<float>();
                Vector3 dir = transform.up * value;
                transform.Translate(dir * Time.deltaTime * settingMoveSpeed);
            }
        }

        public void OnHorizontal(InputAction.CallbackContext context)
        {
            workingDoHorizontal = context.performed;
        }

        public void OnRotation(InputAction.CallbackContext context)
        {
            workingDoRotation = context.performed;
        }

        public void OnVertical(InputAction.CallbackContext context)
        {
            workingDoVertical = context.performed;
        }
    }
}