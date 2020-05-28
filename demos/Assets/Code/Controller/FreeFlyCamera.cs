using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Goldenwere.Unity.Controller
{
    public class FreeFlyCamera : MonoBehaviour
    {
        [SerializeField] private PlayerInput attachedControls;
        private bool workingDoHorizontal;
        private bool workingDoRotation;
        private bool workingDoVertical;

        private void Update()
        {
            if (workingDoHorizontal)
            {
                Vector2 value = attachedControls.actions["Horizontal"].ReadValue<Vector2>().normalized;
                Vector3 dir = transform.forward * value.y + transform.right * value.x;
                transform.Translate(dir * Time.deltaTime);
            }

            if (workingDoRotation)
            {

            }

            if (workingDoVertical)
            {

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