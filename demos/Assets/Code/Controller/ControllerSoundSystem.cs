using System;
using System.Collections.Generic;
using UnityEngine;

namespace Goldenwere.Unity.Controller
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(FirstPersonController))]
    public class ControllerSoundSystem : MonoBehaviour
    {
        #region Fields
#pragma warning disable 0649
        [Tooltip            ("The default clip to play when moving and no other material/texture was found. At minimum, if using this system, this should be defined.")]
        [SerializeField]    private AudioClip                       clipDefaultMovement;
        [Tooltip            ("Associate audio clips with any materials you want to have specific sounds for while stepping on them.")]
        [SerializeField]    private MaterialCollection[]            clipsMaterials;
        [Tooltip            ("Associate audio clips with terrain textures here. These must match the textures applied to the terrain.")]
        [SerializeField]    private AudioClip[]                     clipsTerrain;
        [Tooltip            ("The pitch to use while crouched (ideally lower than the pitch while not crouched)")]
        [SerializeField]    private float                           settingPitchWhileCrouched = 0.5f;
        [Tooltip            ("The pitch to use while not crouched (ideally 1)")]
        [SerializeField]    private float                           settingPitchWhileNotCrouched = 1f;
        [Tooltip            ("The volume to use while crouched (ideally lower than the pitch while not crouched)")]
        [SerializeField]    private float                           settingVolumeWhileCrouched = 0.5f;
        [Tooltip            ("The volume to use while not crouched (ideally 1)")]
        [SerializeField]    private float                           settingVolumeWhileNotCrouched = 1f;
        [Tooltip            ("Any GameObject with the tags defined here will play audio. " +
                            "Do not tag terrain with a tag that is listed here, or else the default audio will play (due to terrain-checking using a different method)")]
        [SerializeField]    private string[]                        tagObjects;
        [Tooltip            ("The time between playing footsteps while moving crouched")]
        [SerializeField]    private float                           timeBetweenStepsCrouched = 1f;
        [Tooltip            ("The time between playing footsteps while moving fast")]
        [SerializeField]    private float                           timeBetweenStepsFast = 0.3333f;
        [Tooltip            ("The time between playing footsteps while moving normally")]
        [SerializeField]    private float                           timeBetweenStepsNorm = 0.5f;
        [Tooltip            ("The time between playing footsteps while moving slow")]
        [SerializeField]    private float                           timeBetweenStepsSlow = 1f;
        
        /**************/    private FirstPersonController           attachedController;
        /**************/    private MovementState                   workingCurrentMovementState;
        /**************/    private float                           workingCurrentStepTime;
        /**************/    private Dictionary<Material, AudioClip> workingMaterials;
        /**************/    private AudioSource                     workingSource;
        /**************/    private float                           workingTimeSinceLastPlayed;
#pragma warning restore 0649
        #endregion

        private void Awake()
        {
            workingMaterials = new Dictionary<Material, AudioClip>();

            foreach(MaterialCollection mc in clipsMaterials)
                foreach (Material m in mc.AssociatedMaterials)
                    if (!workingMaterials.ContainsKey(m))
                        workingMaterials.Add(m, mc.AssociatedClip);

            workingSource = GetComponent<AudioSource>();
            attachedController = GetComponent<FirstPersonController>();
            workingCurrentStepTime = timeBetweenStepsNorm;
        }

        private void OnEnable()
        {
            attachedController.UpdateMovementState += OnUpdateMovementState;
        }

        private void OnDisable()
        {
            attachedController.UpdateMovementState -= OnUpdateMovementState;
        }

        private void Update()
        {
            if (workingCurrentMovementState != MovementState.idle && workingCurrentMovementState != MovementState.idle_crouched &&
                workingCurrentMovementState != MovementState.jumping && workingCurrentMovementState != MovementState.falling)
            {
                if (workingTimeSinceLastPlayed >= workingCurrentStepTime)
                {

                }
            }
        }

        private void OnUpdateMovementState(MovementState state)
        {
            switch (state)
            {
                case MovementState.fast:
                    workingCurrentStepTime = timeBetweenStepsFast;
                    workingSource.pitch = settingPitchWhileNotCrouched;
                    break;
                case MovementState.slow:
                    workingCurrentStepTime = timeBetweenStepsSlow;
                    workingSource.pitch = settingPitchWhileNotCrouched;
                    break;
                case MovementState.fast_crouched:
                case MovementState.norm_crouched:
                case MovementState.slow_crouched:
                    workingCurrentStepTime = timeBetweenStepsCrouched;
                    workingSource.pitch = settingPitchWhileCrouched;
                    break;
                case MovementState.norm:
                    workingCurrentStepTime = timeBetweenStepsNorm;
                    workingSource.pitch = settingPitchWhileNotCrouched;
                    break;
                default:
                    // Do nothing for the other states - sound doesn't play
                    break;
            }
        }
}

    [Serializable]
    public struct MaterialCollection
    {
#pragma warning disable 0649
        [SerializeField]    private AudioClip   associatedClip;
        [SerializeField]    private Material[]  associatedMaterials;
#pragma warning restore 0649

        public Material[]   AssociatedMaterials { get { return associatedMaterials; } }
        public AudioClip    AssociatedClip      { get { return associatedClip; } }
    }
}