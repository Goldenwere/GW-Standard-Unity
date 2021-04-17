using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Goldenwere.Unity.Controller
{ 
    public partial class CharacterController : MonoBehaviour
    {
        [System.Serializable]
        protected struct MovementSettings
        {
            public bool allowWalk;
            public bool allowRun;
            public bool allowCrouch;
            public bool allowCrawl;
        }

        private void Fixed_Update()
        {
            
        }
    }
}