using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Goldenwere.Unity.UI
{
    /// <summary>
    /// Defines methods of fading UI elements
    /// </summary>
    public enum FadeMethod
    {
        /// <summary>
        /// From 0 to 1 opacity
        /// </summary>
        FadeIn,

        /// <summary>
        /// From 1 to 0 opacity
        /// </summary>
        FadeOut
    }

    /// <summary>
    /// UI extension to fade in/out CanvasGroups on demand
    /// </summary>
    public class FadeInOutElements
    {
        private Coroutine       runningCoroutine;

        public  MonoBehaviour   AssociatedController    { get; private set; }
        public  CanvasGroup     AssociatedGroup         { get; private set; }
        public  AnimationCurve  FadeCurve               { get; private set; }
        public  float           FadeInLength            { get; private set; }
        public  float           FadeOutLength           { get; private set; }

        /// <summary>
        /// Creates an instance of the fade in/out extension with a default animation curve
        /// </summary>
        /// <param name="controller">A parent MonoBehaviour that can be used to start/end Coroutines on
        /// <para>This MonoBehaviour must never be disabled, or else exceptions will be thrown and values won't transition properly</para></param>
        /// <param name="group">The CanvasGroup being animated</param>
        /// <param name="fadeInLength">How long fade-in transitions should last</param>
        /// <param name="fadeOutLength">How long fade-out transitions should last</param>
        public FadeInOutElements(MonoBehaviour controller, CanvasGroup group, float fadeInLength, float fadeOutLength)
        {
            AssociatedController = controller;
            AssociatedGroup = group;
            FadeCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
            FadeInLength = fadeInLength;
            FadeOutLength = fadeOutLength;
        }

        /// <summary>
        /// Creates an instance of the fade in/out extension with a default animation curve
        /// </summary>
        /// <param name="controller">A parent MonoBehaviour that can be used to start/end Coroutines on
        /// <para>This MonoBehaviour must never be disabled, or else exceptions will be thrown and values won't transition properly</para></param>
        /// <param name="group">The CanvasGroup being animated</param>
        /// <param name="fadeInLength">How long fade-in transitions should last</param>
        /// <param name="fadeOutLength">How long fade-out transitions should last</param>
        /// <param name="curve">The animation curve to apply to the transition</param>
        public FadeInOutElements(MonoBehaviour controller, CanvasGroup group, float fadeInLength, float fadeOutLength, AnimationCurve curve)
        {
            AssociatedController = controller;
            AssociatedGroup = group;
            FadeCurve = curve;
            FadeInLength = fadeInLength;
            FadeOutLength = fadeOutLength;
        }

        /// <summary>
        /// Fades the associated CanvasGroup with the defined method
        /// </summary>
        /// <param name="method">The FadeMethod to commence</param>
        public IEnumerator Fade(FadeMethod method)
        {
            float t = 0;
            float oldVal = AssociatedGroup.alpha;

            if (method == FadeMethod.FadeIn)
            {
                while (t <= FadeInLength)
                {
                    AssociatedGroup.alpha = Mathf.Lerp(oldVal, 1, FadeCurve.Evaluate(t / FadeInLength));
                    t += Time.deltaTime;
                    yield return null;
                }
            }

            else
            {
                while (t <= FadeOutLength)
                {
                    AssociatedGroup.alpha = Mathf.Lerp(oldVal, 0, FadeCurve.Evaluate(t / FadeOutLength));
                    t += Time.deltaTime;
                    yield return null;
                }
            }
        }

        /// <summary>
        /// Call this to fade in the CanvasGroup reliably
        /// </summary>
        public void FadeIn()
        {
            if (runningCoroutine != null)
                AssociatedController.StopCoroutine(runningCoroutine);
            runningCoroutine = AssociatedController.StartCoroutine(Fade(FadeMethod.FadeIn));
        }

        /// <summary>
        /// Call this to fade out the CanvasGroup reliably
        /// </summary>
        public void FadeOut()
        {
            if (runningCoroutine != null)
                AssociatedController.StopCoroutine(runningCoroutine);
            runningCoroutine = AssociatedController.StartCoroutine(Fade(FadeMethod.FadeOut));
        }
    }
}