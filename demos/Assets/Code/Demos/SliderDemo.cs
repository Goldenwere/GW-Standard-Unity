using UnityEngine;
using UnityEngine.UI;
using Goldenwere.Unity.UI;
using System.Collections.Generic;
using System.Collections;

namespace Goldenwere.Unity.Demos
{
    public class SliderDemo : MonoBehaviour
    {
        [SerializeField]    private SliderTextLoadExtension                         textLoadDemoSlider;
        [SerializeField]    private Slider                                          transitionDemoSlider;
        /**************/    private bool                                            transitionCoroutineIsRunning;
        /**************/    private Dictionary<Slider, SliderTransitionExtension>   transitionExtensions;

        /// <summary>
        /// Initializes sliders on Monobehaviour.Start()
        /// </summary>
        private void Start()
        {
            textLoadDemoSlider.UpdateText(3f);
            textLoadDemoSlider.AssociatedSlider.SetValueWithoutNotify(3f);
            transitionExtensions = new Dictionary<Slider, SliderTransitionExtension>()
            {
                { transitionDemoSlider, new SliderTransitionExtension(this, transitionDemoSlider, 0.5f, 0.75f) }
            };
        }

        /// <summary>
        /// Determines whether to update the transitioned slider value and calls Update on transitioned sliders on Monobehaviour.Update()
        /// </summary>
        private void Update()
        {
            if (!transitionCoroutineIsRunning)
                StartCoroutine(PeriodicallyUpdateSlider());

            foreach (SliderTransitionExtension s in transitionExtensions.Values)
                s.Update();
        }

        /// <summary>
        /// This is used to demonstrate the sliders by randomly assigning values to the slider at random times 
        /// <para>(with a range lower than the slider's transition times to show off the stale mechanic that prevents playing transitions too soon)</para>
        /// </summary>
        private IEnumerator PeriodicallyUpdateSlider()
        {
            transitionCoroutineIsRunning = true;
            transitionExtensions[transitionDemoSlider].UpdateValue(Random.Range(0, 500));
            yield return new WaitForSeconds(Random.Range(0.25f, 3f));
            transitionCoroutineIsRunning = false;
        }
    }
}