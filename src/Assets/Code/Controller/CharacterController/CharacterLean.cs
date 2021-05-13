using System.Collections;
using UnityEngine;

namespace Goldenwere.Unity.Controller
{
    /// <summary>
    /// Optional module to handle the controller's leaning,
    /// which is separated from the controller
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class CharacterLean : MonoBehaviour
    {
        [System.Serializable]
        protected struct LeanSettings
        {
            [Tooltip                                        ("The maximum angle the character can lean")]
            public float                                    angleMaxLean;
            [Tooltip                                        ("TODO")]
            public bool                                     preventMovement;
            [Tooltip                                        ("The speed at which the character leans/unleans")]
            public float                                    speedLean;
        }
#pragma warning disable 0649
        [SerializeField] private Transform                  leanJoint;
        [SerializeField] private LeanSettings               leanSettings;
#pragma warning restore 0649
        /**************/ private CharacterController        controller;
        /**************/ private PrioritizedOptionalModule  leanModule;
        /**************/ private IEnumerator                unsetInstance;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            leanModule = new PrioritizedOptionalModule(0, Update_Lean);

            controller.Lean += (val) =>
            {
                if (unsetInstance != null)
                    StopCoroutine(unsetInstance);

                if (val)
                    controller.AddModuleToUpdate(leanModule);
                else
                {
                    unsetInstance = UnsetLean();
                    StartCoroutine(unsetInstance);
                    controller.RemoveModuleFromUpdate(leanModule);
                }
            };
        }

        private void Update_Lean()
        {
            leanJoint.localRotation = Quaternion.Slerp(
                leanJoint.localRotation,
                Quaternion.Euler(new Vector3(0, 0, leanSettings.angleMaxLean * -controller.InputValueLean)),
                Time.deltaTime * leanSettings.speedLean);
        }

        private IEnumerator UnsetLean()
        {
            float currAngle;

            do
            {
                leanJoint.localRotation = Quaternion.Slerp(
                    leanJoint.localRotation,
                    Quaternion.identity,
                    Time.deltaTime * leanSettings.speedLean);
                currAngle = leanJoint.localRotation.eulerAngles.z;
                yield return null;
            }
            while (currAngle > 0.01f || currAngle < -0.01f);
        }
    }
}