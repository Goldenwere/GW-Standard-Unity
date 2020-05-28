using UnityEngine;
using UnityEngine.UI;
using Goldenwere.Unity.UI;
using System.Collections.Generic;
using System.Collections;

namespace Goldenwere.Unity.Demos
{
    public class SliderDemo : MonoBehaviour
    {
        [SerializeField]    private SliderTextLoadExtension[]                       textLoadDemoSliders;
        [SerializeField]    private Slider[]                                        transitionDemoSliders;
        [SerializeField]    private AnimationCurve[]                                transitionDemoCurves;
        // this dictionary is only for the demo; 
        // normally one does not need to keep track of any coroutines, one would just set the slider whenever an associated value is updated
        /**************/    private Dictionary<Slider, bool>                        transitionDemoIsRunning;
        /**************/    private Dictionary<Slider, SliderTransitionExtension>   transitionExtensions;

        /// <summary>
        /// Initializes sliders on Monobehaviour.Start()
        /// </summary>
        private void Start()
        {
            textLoadDemoSliders[0].UpdateText(3f);
            textLoadDemoSliders[0].AssociatedSlider.SetValueWithoutNotify(3f);
            textLoadDemoSliders[1].UpdateText("Off");
            textLoadDemoSliders[1].AssociatedSlider.SetValueWithoutNotify(0);
            transitionExtensions = new Dictionary<Slider, SliderTransitionExtension>();
            foreach (Slider s in transitionDemoSliders)
                transitionExtensions.Add(s, new SliderTransitionExtension(this, s, Random.Range(0.1f, 0.5f), 0.75f/*, transitionDemoCurves[Random.Range(0, transitionDemoCurves.Length)]*/));
            transitionDemoIsRunning = new Dictionary<Slider, bool>();
            foreach (Slider s in transitionDemoSliders)
                transitionDemoIsRunning.Add(s, false);
        }

        /// <summary>
        /// Determines whether to update the transitioned slider value and calls Update on transitioned sliders on Monobehaviour.Update()
        /// </summary>
        private void Update()
        {
            foreach (SliderTransitionExtension s in transitionExtensions.Values)
            {
                s.Update();
                if (!transitionDemoIsRunning[s.AssociatedSlider])
                    StartCoroutine(PeriodicallyUpdateSlider(s.AssociatedSlider));
            }
        }

        /// <summary>
        /// Handler for the OnValueChanged event on the second demo slider
        /// </summary>
        /// <param name="val">The new slider value, translated into an example antialiasing setting</param>
        public void OnUpdateDemoSliderTwo(float val)
        {
            switch((int)val)
            {
                case 2:
                    textLoadDemoSliders[1].UpdateText("SMAA");
                    break;
                case 1:
                    textLoadDemoSliders[1].UpdateText("FXAA");
                    break;
                case 0:
                default:
                    textLoadDemoSliders[1].UpdateText("Off");
                    break;
            }
        }

        /// <summary>
        /// This is used to demonstrate the sliders by randomly assigning values to the slider at random times 
        /// <para>(with a range lower than the slider's transition times to show off the stale mechanic that prevents playing transitions too soon)</para>
        /// </summary>
        private IEnumerator PeriodicallyUpdateSlider(Slider s)
        {
            transitionDemoIsRunning[s] = true;
            transitionExtensions[s].UpdateValue(Random.Range(0, 500));
            yield return new WaitForSeconds(Random.Range(0.25f, 3f));
            transitionDemoIsRunning[s] = false;
        }
    }
}