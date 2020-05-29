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

        #region Methods
        /// <summary>
        /// Associates attached components and sets up a working dictionary of materials to check against for objects
        /// </summary>
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

        /// <summary>
        /// Subscribe to the UpdateMovementState event of the controller on Monobehaviour.OnEnable()
        /// </summary>
        private void OnEnable()
        {
            attachedController.UpdateMovementState += OnUpdateMovementState;
        }

        /// <summary>
        /// Unsubscribe from the UpdateMovementState event of the controller on Monobehaviour.OnDisable()
        /// </summary>
        private void OnDisable()
        {
            attachedController.UpdateMovementState -= OnUpdateMovementState;
        }

        /// <summary>
        /// Handles checking groundstate and playing audio at specific intervals on Monobehaviour.Update()
        /// </summary>
        private void Update()
        {
            if (workingCurrentMovementState != MovementState.idle && workingCurrentMovementState != MovementState.idle_crouched &&
                workingCurrentMovementState != MovementState.jumping && workingCurrentMovementState != MovementState.falling)
            {
                workingTimeSinceLastPlayed += Time.deltaTime;
                if (workingTimeSinceLastPlayed >= workingCurrentStepTime)
                {
                    PlayAudio();
                    workingTimeSinceLastPlayed = 0;
                }
            }
        }

        /// <summary>
        /// Converts a world-space position (the controller's) to a position on the alphamap of a designated terrain
        /// </summary>
        /// <param name="worldPos">The position to check in worldspace</param>
        /// <param name="t">The terrain being checked</param>
        /// <returns>The strength values of each texture at the world position provided</returns>
        private float[] ConvertPositionToTerrain(Vector3 worldPos, Terrain t)
        {
            Vector3 terrainPos = worldPos - t.transform.position;
            Vector3 mapPos = new Vector3(terrainPos.x / t.terrainData.size.x, 0, terrainPos.z / t.terrainData.size.z);
            Vector3 scaledPos = new Vector3(mapPos.x * t.terrainData.alphamapWidth, 0, mapPos.z * t.terrainData.alphamapHeight);
            float[] layers = new float[t.terrainData.alphamapLayers];
            float[,,] aMap = t.terrainData.GetAlphamaps((int)scaledPos.x, (int)scaledPos.z, 1, 1);
            for (int i = 0; i < layers.Length; i++)
                layers[i] = aMap[0, 0, i];
            return layers;
        }

        /// <summary>
        /// Handler for the UpdateMovementState event which determines which speed to play audio and at what pitch/volume
        /// </summary>
        /// <param name="state">The current MovementState</param>
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

        /// <summary>
        /// Determines which audio to play based on what is underneath the controller
        /// </summary>
        private void PlayAudio()
        {
            if (Physics.Raycast(new Ray(transform.position, Vector3.down), out RaycastHit hit, attachedController.SettingsMovement.SettingNormalHeight + 0.1f, Physics.AllLayers))
            {
                if (hit.collider is TerrainCollider)
                {
                    Terrain t = hit.collider.gameObject.GetComponent<Terrain>();
                    float[] currentLayerValues = ConvertPositionToTerrain(transform.position, t);
                    for (int i = 0; i < currentLayerValues.Length; i++)
                    {
                        if (currentLayerValues[i] > 0 && i < clipsTerrain.Length)
                        {
                            float textureVol = workingSource.volume * currentLayerValues[i];
                            workingSource.PlayOneShot(clipsTerrain[i], textureVol);
                        }
                    }
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
                                workingSource.PlayOneShot(workingMaterials[sanitizedName]);

                            else
                                workingSource.PlayOneShot(clipDefaultMovement);
                        }
                    }

                    else
                        workingSource.PlayOneShot(clipDefaultMovement);
                }
            }
        }
        #endregion
    }

    /// <summary>
    /// A structure for associating audio clips with materials
    /// </summary>
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