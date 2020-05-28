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

        private void Start()
        {
            textLoadDemoSlider.UpdateText(3f);
            textLoadDemoSlider.AssociatedSlider.SetValueWithoutNotify(3f);
            transitionExtensions = new Dictionary<Slider, SliderTransitionExtension>()
            {
                { transitionDemoSlider, new SliderTransitionExtension(this, transitionDemoSlider, 0.5f, 0.75f) }
            };
        }

        private void Update()
        {
            if (!transitionCoroutineIsRunning)
                StartCoroutine(PeriodicallyUpdateSlider());

            foreach (SliderTransitionExtension s in transitionExtensions.Values)
                s.Update();
        }

        private IEnumerator PeriodicallyUpdateSlider()
        {
            transitionCoroutineIsRunning = true;
            transitionExtensions[transitionDemoSlider].UpdateValue(Random.Range(0, 500));
            yield return new WaitForSeconds(Random.Range(0.25f, 3f));
            transitionCoroutineIsRunning = false;
        }
    }
}