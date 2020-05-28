using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Goldenwere.Unity.Controller
{
    public class FreeFlyCamera : MonoBehaviour
    {
        [SerializeField]    private PlayerInput attachedControls;
        [SerializeField]    private GameObject  pointCamera;
        [SerializeField]    private GameObject  pointPivot;
        [SerializeField]    private float       settingMoveSpeed = 2f;
        /**************/    public  float       settingRotationSensitivity = 3f;
        /**************/    private bool        workingDoHorizontal;
        /**************/    private bool        workingDoRotation;
        /**************/    private bool        workingDoVertical;
        /**************/    private bool        workingIsLocked;

        private void Update()
        {
            if (!workingIsLocked)
            {
                if (workingDoHorizontal)
                {
                    Vector2 value = attachedControls.actions["Horizontal"].ReadValue<Vector2>().normalized;
                    Vector3 dir = pointCamera.transform.forward * value.y + pointCamera.transform.right * value.x;
                    transform.Translate(dir * Time.deltaTime * settingMoveSpeed);
                }

                if (workingDoRotation)
                {
                    Vector2 value = attachedControls.actions["Rotation"].ReadValue<Vector2>();
                    pointCamera.transform.localRotation *= Quaternion.Euler(-value.y * Time.deltaTime * settingRotationSensitivity, 0, 0);
                    pointPivot.transform.localRotation *= Quaternion.Euler(0, value.x * Time.deltaTime * settingRotationSensitivity, 0);
                }

                if (workingDoVertical)
                {
                    float value = attachedControls.actions["Vertical"].ReadValue<float>();
                    Vector3 dir = pointCamera.transform.up * value;
                    transform.Translate(dir * Time.deltaTime * settingMoveSpeed);
                }
            }
        }

        public void OnHorizontal(InputAction.CallbackContext context)
        {
            workingDoHorizontal = context.performed;
        }

        public void OnLock(InputAction.CallbackContext context)
        {
            if (context.performed)
                workingIsLocked = !workingIsLocked;
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