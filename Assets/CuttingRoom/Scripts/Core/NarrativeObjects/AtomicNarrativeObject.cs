using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CuttingRoom
{
    public class AtomicNarrativeObject : NarrativeObject
    {
        /// <summary>
        /// The media source related to this object.
        /// </summary>
        [SerializeField]
        private MediaController mediaController = null;

        /// <summary>
        /// The media source related to this object.
        /// </summary>
        public MediaController MediaController { get { return mediaController; } set { mediaController = value; } }

        /// <summary>
        /// The parent transform of any media instantiated by this object.
        /// </summary>
        [SerializeField]
        private Transform mediaParent = null;

        /// <summary>
        /// The parent transform of any media instantiated by this object.
        /// </summary>
        public Transform MediaParent { get { return mediaParent; } }

        /// <summary>
        /// When the media source associated with this game object should be unloaded, if at all.
        /// </summary>
        [SerializeField]
        private MediaSourceUnloadEvent mediaSourceUnloadEvent = MediaSourceUnloadEvent.OnProcessingTriggerComplete;

        /// <summary>
        /// Media source unload event for this object.
        /// </summary>
        public MediaSourceUnloadEvent MediaSourceUnloadEvent { get { return mediaSourceUnloadEvent; } }

        public override void PreProcess()
        {
            if (!inProgress)
            {
                base.PreProcess();

                if (MediaController != null)
                {
                    MediaController.Init();
                }
            }
        }
    }
}
