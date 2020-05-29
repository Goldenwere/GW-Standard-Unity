using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Goldenwere.Unity.UI
{
    /// <summary>
    /// This extends sliders so that they smoothly transition when their values are updated.
    /// <para>Rather than directly modifying a Slider.value, use this class' UpdateValue after associating this class with a slider</para>
    /// </summary>
    public class SliderTransitionExtension
    {
        private Coroutine       runningCoroutine;
        private float           sliderWaitTimer;

        public MonoBehaviour    AssociatedController            { get; private set; }
        public Slider           AssociatedSlider                { get; private set; }
        public bool             SliderIsStale                   { get; private set; }
        public float            SliderLengthBetweenTransitions  { get; private set; }
        public float            SliderStaleValue                { get; private set; }
        public float            SliderTransitionLength          { get; private set; }
        public AnimationCurve   SliderTransitionCurve           { get; private set; }

        /// <summary>
        /// Creates a new slider with defined parameters; these parameters are to be treated as though they are constant
        /// </summary>
        /// <param name="controller">A parent MonoBehaviour that can be used to start/end Coroutines on
        /// <para>This MonoBehaviour must never be disabled, or else exceptions will be thrown and values won't transition properly</para></param>
        /// <param name="slider">The slider that transitions are being added to</param>
        /// <param name="transitionLength">The duration that transitions last</param>
        /// <param name="lengthBetweenTransitions">How often transitions occur</param>
        public SliderTransitionExtension(MonoBehaviour controller, Slider slider, float transitionLength, float lengthBetweenTransitions)
        {
            AssociatedController = controller;
            AssociatedSlider = slider;
            SliderLengthBetweenTransitions = lengthBetweenTransitions;
            SliderTransitionLength = transitionLength;
            SliderTransitionCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

            SliderIsStale = false;
            sliderWaitTimer = 1;
        }

        /// <summary>
        /// Creates a new slider with defined parameters; these parameters are to be treated as though they are constant
        /// </summary>
        /// <param name="controller">A parent MonoBehaviour that can be used to start/end Coroutines on
        /// <para>This MonoBehaviour must never be disabled, or else exceptions will be thrown and values won't transition properly</para></param>
        /// <param name="slider">The slider that transitions are being added to</param>
        /// <param name="transitionLength">The duration that transitions last</param>
        /// <param name="lengthBetweenTransitions">How often transitions occur</param>
        /// <param name="curve">The animation curve to apply to the transition</param>
        public SliderTransitionExtension(MonoBehaviour controller, Slider slider, float transitionLength, float lengthBetweenTransitions, AnimationCurve curve)
        {
            AssociatedController = controller;
            AssociatedSlider = slider;
            SliderLengthBetweenTransitions = lengthBetweenTransitions;
            SliderTransitionLength = transitionLength;
            SliderTransitionCurve = curve;

            SliderIsStale = false;
            sliderWaitTimer = 1;
        }

        /// <summary>
        /// Transitions a slider between an old and new value over time
        /// </summary>
        /// <param name="controlled">The slider that is being manipulated</param>
        /// <param name="oldVal">The old value of the slider that the animation starts from</param>
        /// <param name="newVal">The new value of the slider that the animation ends at</param>
        /// <param name="length">The length of time to transition the slider</param>
        public IEnumerator TransitionSlider(Slider controlled, float oldVal, float newVal, float length)
        {
            float t = 0;
            while (t <= length)
            {
                controlled.value = Mathf.Lerp(oldVal, newVal, SliderTransitionCurve.Evaluate(t / length));
                t += Time.deltaTime;
                yield return null;
            }
        }

        /// <summary>
        /// Call this under MonoBehaviour.Update(); ensures the extension functions properly
        /// </summary>
        public void Update()
        {
            sliderWaitTimer += Time.deltaTime;

            if (SliderIsStale && sliderWaitTimer >= SliderLengthBetweenTransitions)
            {
                if (runningCoroutine != null)
                    AssociatedController.StopCoroutine(runningCoroutine);
                runningCoroutine = AssociatedController.StartCoroutine(TransitionSlider(AssociatedSlider, AssociatedSlider.value, SliderStaleValue, SliderTransitionLength));
                SliderIsStale = false;
                sliderWaitTimer = 0;
            }
        }

        /// <summary>
        /// Reports a new value to the extension and marks the slider as stale so that it updates to this new value when the next transition can occur
        /// </summary>
        /// <param name="newVal">The new value to transition to</param>
        public void UpdateValue(float newVal)
        {
            SliderStaleValue = newVal;
            SliderIsStale = true;
        }
    }
}