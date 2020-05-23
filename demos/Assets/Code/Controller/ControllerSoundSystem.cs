using System;
using System.Collections.Generic;
using UnityEngine;

namespace Goldenwere.Unity.Controller
{
    [RequireComponent(typeof(AudioSource))]
    public class ControllerSoundSystem : MonoBehaviour
    {
        #region Fields
#pragma warning disable 0649
        [Tooltip            ("The default clip to play when moving and no other material/texture was found. At minimum, if using this system, this should be defined.")]
        [SerializeField]    private AudioClip               clipDefaultMovement;
        [Tooltip            ("Associate audio clips with any materials you want to have specific sounds for while stepping on them.")]
        [SerializeField]    private MaterialCollection[]    clipsMaterials;
        [Tooltip            ("Associate audio clips with terrain textures here. These must match the textures applied to the terrain.")]
        [SerializeField]    private AudioClip[]             clipsTerrain;
        [Tooltip            ("Any GameObject with the tags defined here will play audio. " +
                            "Do not tag terrain with a tag that is listed here, or else the default audio will play (due to terrain-checking using a different method)")]
        [SerializeField]    private string[]                tagObjects;

        private Dictionary<Material, AudioClip> workingMaterials;
        private AudioSource workingSource;
#pragma warning restore 0649
        #endregion

        private void Awake()
        {
            foreach(MaterialCollection mc in clipsMaterials)
                foreach (Material m in mc.AssociatedMaterials)
                    if (!workingMaterials.ContainsKey(m))
                        workingMaterials.Add(m, mc.AssociatedClip);

            workingSource = GetComponent<AudioSource>();
        }
    }

    [Serializable]
    public struct MaterialCollection
    {
        [SerializeField]    private AudioClip   associatedClip;
        [SerializeField]    private Material[]  associatedMaterials;

        public Material[]   AssociatedMaterials { get { return associatedMaterials; } }
        public AudioClip    AssociatedClip      { get { return associatedClip; } }
    }
}