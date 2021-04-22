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
            };
        }

        /// <summary>
        /// Handles checking groundstate and playing audio at specific intervals on Monobehaviour.Update()
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
            if (controller.InputValueCrouch || controller.InputValueCrawl)
            {
                source.pitch = settings.pitchWhileCrouched;
                source.volume = settings.volumeWhileCrouched;
                workingCurrentStepTime = settings.timeBetweenStepsCrouched;
            }

            else
            {
                source.pitch = settings.pitchWhileNotCrouched;
                source.volume = settings.volumeWhileNotCrouched;

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
            isPlaying = controller.Grounded && controller.InputActiveMovement;
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
        /// Determines which audio to play based on what is underneath the controller
        /// </summary>
        private void PlayAudio()
        {
            if (controller.GroundCollider is TerrainCollider)
            {
                Terrain t = controller.GroundCollider.gameObject.GetComponent<Terrain>();
                float[] currentLayerValues = ConvertPositionToTerrain(transform.position, t);
                for (int i = 0; i < currentLayerValues.Length; i++)
                {
                    if (currentLayerValues[i] > 0 && i < clipsTerrain.Length)
                    {
                        float textureVol = source.volume * currentLayerValues[i];
                        source.PlayOneShot(clipsTerrain[i], textureVol);
                    }
                }
            }

            else
            {
                MeshRenderer mr = controller.GroundCollider.gameObject.GetComponent<MeshRenderer>();
                if (mr != null)
                {
                    Material mat = mr.material;
                    if (mat != null)
                    {
                        string sanitizedName = mat.name.Replace(" (Instance)", "");
                        if (workingMaterials.ContainsKey(sanitizedName))
                            source.PlayOneShot(workingMaterials[sanitizedName]);

                        else
                            source.PlayOneShot(clipDefaultMovement);
                    }
                }

                else
                    source.PlayOneShot(clipDefaultMovement);
            }
        }
        #endregion
    }


}