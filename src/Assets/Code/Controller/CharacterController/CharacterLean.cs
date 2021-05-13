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
            [Tooltip                                        ("TODO")]
            public bool                                     preventMovement;
            [Tooltip                                        ("The maximum angle the character can lean")]
            public float                                    angleMaxLean;
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
                Quaternion.Euler(new Vector3(0, 0, leanSettings.angleMaxLean * controller.InputValueLean)),
                Time.deltaTime / leanSettings.speedLean);
        }

        private IEnumerator UnsetLean()
        {
            AnimationCurve curve = new AnimationCurve(new Keyframe[] {
                new Keyframe(0, 1, 0.50f, 0.25f),
                new Keyframe(1, 0, 0.50f, 0.50f)
            });
            float currAngle;
            float startAngle = leanJoint.localRotation.eulerAngles.z;
            Vector3 currEulers;
            float t = 0;
            float endTime = 1.0f / leanSettings.speedLean;

            while (t <= endTime)
            {
                currAngle = Mathf.Lerp(startAngle, 0, curve.Evaluate(t / endTime));
                currEulers = new Vector3(0, 0, currAngle);
                leanJoint.localRotation = Quaternion.Euler(currEulers);
                t += Time.deltaTime;
                yield return null;
            }
        }
    }
}