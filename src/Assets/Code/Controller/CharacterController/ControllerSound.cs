/**
*** Copyright (C) 2020 Goldenwere
*** Part of the Goldenwere Standard Unity repository
*** The Goldenwere Standard Unity Repository is licensed under the MIT license
***
*** File Info:
***     Description - Contains the optional sound module of the CharacterController
***     Pkg Name    - CharacterController
***     Pkg Ver     - 2.0.0
***     Pkg Req     - CoreAPI
**/

using System.Collections.Generic;
using UnityEngine;

namespace Goldenwere.Unity.Controller
{
    /// <summary>
    /// Optional module to handle sounds for the controller
    /// which is separated from the controller.
    /// </summary>
    /// <remarks>
    /// This is separate because there may be other ways one may want to handle audio.
    /// For example, one may want to get velocity to handle various audio settings.
    /// </remarks>
    public class ControllerSound : MonoBehaviour
    {
        /// <summary>
        /// A structure for associating audio clips with materials
        /// </summary>
        [System.Serializable]
        protected struct MaterialCollection
        {
            public AudioClip    associatedClip;
            public Material[]   associatedMaterials;
        }

        /// <summary>
        /// A structure for storing audio source settings related to controller movement
        /// </summary>
        [System.Serializable]
        protected struct AudioSourceSettings
        {
            public float        pitchWhileCrouched;
            public float        pitchWhileNotCrouched;

            public float        volumeWhileCrouched;
            public float        volumeWhileNotCrouched;

            public float        timeBetweenStepsCrouched;
            public float        timeBetweenStepsRun;
            public float        timeBetweenStepsJog;
            public float        timeBetweenStepsWalk;
        }

        #region Fields
#pragma warning disable 0649
        [SerializeField] private CharacterController            controller;
        [SerializeField] private AudioSource                    source;
        [SerializeField] private AudioSourceSettings            settings;

        [Tooltip                                                ("The default clip to play when moving and no other material/texture was found. " +
                                                                "At minimum, if using this system, this should be defined.")]
        [SerializeField] private AudioClip                      clipDefaultMovement;
        [Tooltip                                                ("Associate audio clips with any materials you want to have specific sounds for while stepping on them.")]
        [SerializeField] private MaterialCollection[]           clipsMaterials;
        [Tooltip                                                ("Associate audio clips with terrain textures here. These must match the textures applied to the terrain.")]
        [SerializeField] private AudioClip[]                    clipsTerrain;

        /**************/ private bool                           isPlaying;                      // controller must be Grounded and moving

        /**************/ private float                          workingCurrentStepTime;         // from settings.timeBetweenXYZ
        /**************/ private Dictionary<string, AudioClip>  workingMaterials;               // collection of audio clips to play based on MaterialCollection[] in inspector
        /**************/ private float                          workingTimeSinceLastPlayed;
#pragma warning restore 0649
        #endregion

        #region Methods
        /// <summary>
        /// Associates attached components and sets up a working dictionary of materials to check against for objects
        /// </summary>
        private void Awake()
        {
            workingCurrentStepTime = settings.timeBetweenStepsJog;
            workingMaterials = new Dictionary<string, AudioClip>();

            // convert the MC array into a more useful collection for the module, as the material objects are not relevant but the names of instanced materials are
            foreach(MaterialCollection mc in clipsMaterials)
                foreach (Material m in mc.associatedMaterials)
                    if (!workingMaterials.ContainsKey(m.name))
                        workingMaterials.Add(m.name, mc.associatedClip);

            // handle ControllerLoaded event to listen to input events to set source state
            controller.ControllerLoaded += (c) =>
            {
                controller.Crawl += (_) => SetSettings();
                controller.Crouch += (_) => SetSettings();
                controller.Run += (_) => SetSettings();
                controller.Walk += (_) => SetSettings();
                controller.Movement += (_) => SetState();
                controller.Lean += (val) => SetState();
            };
        }

        /// <summary>
        /// Handles updating audio and timer if playing
        /// </summary>
        private void Update()
        {
            if (isPlaying)
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
        /// Handles updating audio settings based on controller inputs
        /// </summary>
        private void SetSettings()
        {
            // set pitch/volume and step time if crouched
            if (controller.InputValueCrouch || controller.InputValueCrawl)
            {
                source.pitch = settings.pitchWhileCrouched;
                source.volume = settings.volumeWhileCrouched;
                workingCurrentStepTime = settings.timeBetweenStepsCrouched;
            }

            else
            {
                // reset pitch/volume to not-crouched if not crouched
                source.pitch = settings.pitchWhileNotCrouched;
                source.volume = settings.volumeWhileNotCrouched;

                // set step time based on movement speedstate
                if (controller.InputValueRun)
                    workingCurrentStepTime = settings.timeBetweenStepsRun;

                else if (controller.InputValueWalk)
                    workingCurrentStepTime = settings.timeBetweenStepsWalk;
                
                else
                    workingCurrentStepTime = settings.timeBetweenStepsJog;
            }
        }

        /// <summary>
        /// Handles updating whether this module is active based on controller ground/movement state
        /// </summary>
        private void SetState()
        {
            isPlaying = controller.Grounded && controller.InputActiveMovement && !controller.IsMovementBlocked;
        }

        /// <summary>
        /// Converts a world-space position (the controller's) to a position on the alphamap of a designated terrain
        /// </summary>
        /// <param name="worldPos">The position to check in worldspace</param>
        /// <param name="t">The terrain being checked</param>
        /// <returns>The strength values of each texture at the world position provided</returns>
        private float[] ConvertPositionToTerrain(Vector3 worldPos, Terrain t)
        {
            // get the position on the terrain
            Vector3 terrainPos = worldPos - t.transform.position;

            // convert it to a position useful for the terrain's alphamaps
            Vector3 mapPos = new Vector3(terrainPos.x / t.terrainData.size.x, 0, terrainPos.z / t.terrainData.size.z);
            Vector3 scaledPos = new Vector3(mapPos.x * t.terrainData.alphamapWidth, 0, mapPos.z * t.terrainData.alphamapHeight);

            // create a float array that represents the strength of each texture at a specific coordinate
            float[] layers = new float[t.terrainData.alphamapLayers];

            // get the terrain's alphamaps and use it for retrieving the texture strength of each texture
            float[,,] aMap = t.terrainData.GetAlphamaps((int)scaledPos.x, (int)scaledPos.z, 1, 1);
            for (int i = 0; i < layers.Length; i++)
                layers[i] = aMap[0, 0, i];

            return layers;
        }

        /// <summary>
        /// Determines which audio to play based on what is underneath the controller
        /// </summary>
        private void PlayAudio()
        {
            // only play audio if there is a collider that the controller is aware of for grounding
            if (controller.GroundCollider != null)
            {
                // perform terrain-based audio playing ...
                if (controller.GroundCollider is TerrainCollider)
                {
                    // ... by using the terrain to get texture strength as volumes
                    Terrain t = controller.GroundCollider.gameObject.GetComponent<Terrain>();
                    float[] currentLayerValues = ConvertPositionToTerrain(transform.position, t);

                    // set each texture volume and play
                    for (int i = 0; i < currentLayerValues.Length; i++)
                    {
                        if (currentLayerValues[i] > 0 && i < clipsTerrain.Length)
                        {
                            float textureVol = source.volume * currentLayerValues[i];
                            source.PlayOneShot(clipsTerrain[i], textureVol);
                        }
                    }
                }

                // perform mesh-based audio playing ...
                else
                {
                    // ... by getting the mesh renderer ...
                    MeshRenderer mr = controller.GroundCollider.gameObject.GetComponent<MeshRenderer>();
                    // ... and if it's not null ...
                    if (mr != null)
                    {
                        // ... get its material, and determine whether there is a clip associated with its material assigned to the sound module ...
                        Material mat = mr.material;
                        if (mat != null)
                        {
                            string sanitizedName = mat.name.Replace(" (Instance)", "");
                            if (workingMaterials.ContainsKey(sanitizedName))
                                source.PlayOneShot(workingMaterials[sanitizedName]);

                            // ... or to just play the default sound
                            else
                                source.PlayOneShot(clipDefaultMovement);
                        }
                    }

                    // ... otherwise play the default clip if no valid collider
                    else
                        source.PlayOneShot(clipDefaultMovement);
                }
            }
        }
        #endregion
    }


}