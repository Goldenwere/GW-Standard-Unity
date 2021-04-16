using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Goldenwere.Unity.Controller
{ 
    public partial class CharacterController : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private InputSettings              settingsForInput;
#pragma warning restore
        /**************/ private ControllerInputs           inputs;

        private void Start()
        {
            StartCoroutine(LateStart());
        }

        private void Update()
        {
        
        }

        private void FixedUpdate()
        {
        
        }

        private IEnumerator LateStart()
        {
            yield return new WaitForEndOfFrame();

            if (settingsForInput.playerInput == null)
                throw new System.NullReferenceException("[gw-std-unity] Controller's PlayerInput is null; " +
                    "this must be assigned in order for the controller to work properly. Either assign one in inspector or do so in code before the first frame.");
            else
                InitializeInput();
        }
    }
}