using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Goldenwere.Unity.Controller
{
    /// <summary>
    /// Optional module to handle the controller's height (stand/crouch/crawl)
    /// which is separated from the controller
    /// </summary>
    /// <remarks>
    /// This is separate because there may be other ways one may want to handle height.
    /// For example, one may want to use an Animator in order to make
    /// the camera move forward a bit before entering crawl
    /// </remarks>
    public class ControllerHeight : MonoBehaviour
    {
        /// <summary>
        /// Define the states the controller can have from inputs and assign numerical values to them for calculations
        /// </summary>
        protected enum HeightState : int
        {
            stand   = 0,
            crouch  = 1,
            crawl   = 2
        }

#pragma warning disable 0649
        [SerializeField] private Transform                      cameraHeightJoint;
        [SerializeField] private float                          heightStand;
        [SerializeField] private float                          heightCrouch;
        [SerializeField] private float                          heightCrawl;
        [SerializeField] private float                          radiusNormal;
#pragma warning restore 0649
        /**************/ private CharacterController            controller;
        /**************/ private Dictionary<HeightState, float> heightStateToValue;
        /**************/ private float                          heightToRadiusFactor;

        /// <summary>
        /// Get controller and listen to its loaded event, initialize height values
        /// </summary>
        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            controller.ControllerLoaded += (c) =>
            {
                // we will set center as height / 2 to make grounding math simpler by making ground = the actual bottom of the controller
                controller.Collider.height = heightStand;
                controller.Collider.center = new Vector3(0, controller.Collider.height / 2, 0);
                controller.Collider.radius = radiusNormal;
            };
            heightStateToValue = new Dictionary<HeightState, float>()
            {
                { HeightState.stand,    heightStand },
                { HeightState.crouch,   heightCrouch },
                { HeightState.crawl,    heightCrawl },
            };
            heightToRadiusFactor = radiusNormal / heightStand;
        }

        /// <summary>
        /// Event subscribe on enable
        /// </summary>
        private void OnEnable()
        {
            controller.Crawl += OnCrawl;
            controller.Crouch += OnCrouch;
        }
        
        /// <summary>
        /// Event unsubscribe on disable
        /// </summary>
        private void OnDisable()
        {
            controller.Crawl -= OnCrawl;
            controller.Crouch -= OnCrouch;
        }
        
        /// <summary>
        /// Handle Crouch active change event
        /// </summary>
        /// <param name="val">The active value of the input</param>
        private void OnCrouch(bool val)
        {
            // prioritize crawl over crouch
            if (!controller.InputValueCrawl)
            {
                // set standing regardless of allowed to crouch
                if (!val)
                    SetHeight(HeightState.stand);
                // crouch if allowed
                else if (controller.AllowCrouch)
                    SetHeight(HeightState.crouch);
            }
        }
        
        /// <summary>
        /// Handle Crawl active change event
        /// </summary>
        /// <param name="val">The active value of the input</param>
        private void OnCrawl(bool val)
        {
            // if disabling crawl...
            if (!val)
            {
                // ... go to crouch if in crouching state and allowed to crouch ...
                if (controller.InputValueCrouch && controller.AllowCrouch)
                    SetHeight(HeightState.crouch);
                // ... otherwise just go back to normal
                else
                    SetHeight(HeightState.stand);
            }
            // crawl if allowed
            else if (controller.AllowCrawl)
                SetHeight(HeightState.crawl);
        }

        /// <summary>
        /// Set height of the controller based on the state of inputs
        /// </summary>
        /// <param name="state">The state to set the controller to</param>
        private void SetHeight(HeightState state)
        {
            float oldHeight = controller.Collider.height;
            float newHeight = heightStateToValue[state];
            Vector3 positionDifference = new Vector3(0, (newHeight - oldHeight), 0);
            controller.Collider.height = newHeight;
            controller.Collider.center += positionDifference / 2;
            cameraHeightJoint.position += positionDifference;
            controller.Collider.radius = radiusNormal - (heightToRadiusFactor * (int)state - 0.1f * (int)state);
        }
    }
}