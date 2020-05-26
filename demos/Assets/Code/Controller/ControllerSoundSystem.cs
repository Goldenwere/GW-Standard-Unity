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
        /**************/    private Dictionary<string, AudioClip>   workingMaterials;
        /**************/    private AudioSource                     workingSource;
        /**************/    private float                           workingTimeSinceLastPlayed;
#pragma warning restore 0649
        #endregion

        private void Awake()
        {
            workingMaterials = new Dictionary<string, AudioClip>();

            foreach(MaterialCollection mc in clipsMaterials)
                foreach (Material m in mc.AssociatedMaterials)
                    if (!workingMaterials.ContainsKey(m.name))
                        workingMaterials.Add(m.name, mc.AssociatedClip);

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
            workingTimeSinceLastPlayed += Time.deltaTime;
            if (workingCurrentMovementState != MovementState.idle && workingCurrentMovementState != MovementState.idle_crouched &&
                workingCurrentMovementState != MovementState.jumping && workingCurrentMovementState != MovementState.falling)
            {
                if (workingTimeSinceLastPlayed >= workingCurrentStepTime)
                {
                    workingSource.PlayOneShot(DetermineAudioClip());
                    workingTimeSinceLastPlayed = 0;
                }
            }
        }

        private AudioClip DetermineAudioClip()
        {
            if (Physics.Raycast(new Ray(transform.position, Vector3.down), out RaycastHit hit, attachedController.SettingsMovement.SettingNormalHeight + 0.1f, Physics.AllLayers)) 
            {
                if (hit.collider is TerrainCollider)
                {
                    Terrain t = hit.collider.gameObject.GetComponent<Terrain>();
                }

                else
                {
                    MeshRenderer mr = hit.collider.gameObject.GetComponent<MeshRenderer>();
                    if (mr != null)
                    {
                        Material mat = mr.material;
                        if (mat != null)
                        {
                            string sanitizedName = mat.name.Replace(" (Instance)", "");
                            if (workingMaterials.ContainsKey(sanitizedName))
                                return workingMaterials[sanitizedName];
                        }
                    }
                }
            }

            return clipDefaultMovement;
        }

        private void OnUpdateMovementState(MovementState state)
        {
            workingCurrentMovementState = state;
            switch (state)
            {
                case MovementState.fast:
                    workingCurrentStepTime = timeBetweenStepsFast;
                    workingSource.pitch = settingPitchWhileNotCrouched;
                    workingSource.volume = settingVolumeWhileNotCrouched;
                    break;
                case MovementState.slow:
                    workingCurrentStepTime = timeBetweenStepsSlow;
                    workingSource.pitch = settingPitchWhileNotCrouched;
                    workingSource.volume = settingVolumeWhileNotCrouched;
                    break;
                case MovementState.fast_crouched:
                case MovementState.norm_crouched:
                case MovementState.slow_crouched:
                    workingCurrentStepTime = timeBetweenStepsCrouched;
                    workingSource.pitch = settingPitchWhileCrouched;
                    workingSource.volume = settingVolumeWhileCrouched;
                    break;
                case MovementState.norm:
                    workingCurrentStepTime = timeBetweenStepsNorm;
                    workingSource.pitch = settingPitchWhileNotCrouched;
                    workingSource.volume = settingVolumeWhileNotCrouched;
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