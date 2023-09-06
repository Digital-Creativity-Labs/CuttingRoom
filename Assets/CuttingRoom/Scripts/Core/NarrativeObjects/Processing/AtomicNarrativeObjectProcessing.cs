using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace CuttingRoom
{
    public class AtomicNarrativeObjectProcessing : NarrativeObjectProcessing
    {
        /// <summary>
        /// Media controller instantiated by this object.
        /// </summary>
        private MediaController mediaController = null;

        /// <summary>
        /// The atomic narrative object being processed.
        /// </summary>
        private AtomicNarrativeObject AtomicNarrativeObject { get { return narrativeObject as AtomicNarrativeObject; } }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="atomicNarrativeObject"></param>
        public AtomicNarrativeObjectProcessing(AtomicNarrativeObject atomicNarrativeObject)
        {
            narrativeObject = atomicNarrativeObject;
        }

        public override void PreProcess()
        {
            LoadMediaController();
            base.PreProcess();
        }

        /// <summary>
        /// Processessing method for the Atomic Narrative Object.
        /// </summary>
        /// <returns></returns>
        public override IEnumerator Process(Sequencer sequencer, CancellationToken? cancellationToken = null)
        {
            switch (AtomicNarrativeObject.MediaSourceUnloadEvent)
            {
                case MediaSourceUnloadEvent.OnProcessingTriggerComplete:

                    OnProcessingTriggerComplete += UnloadMediaController;

                    break;

                case MediaSourceUnloadEvent.OnProcessingComplete:

                    OnProcessingComplete += UnloadMediaController;

                    break;
            }

            PlayMediaController();

            if (mediaController != null)
            {
                contentCoroutine = AtomicNarrativeObject.StartCoroutine(mediaController.WaitForEndOfContent());
            }

            yield return base.Process(sequencer, cancellationToken);
        }

        /// <summary>
        /// Load the media controller associated with the narrative object being processed by this object.
        /// </summary>
        private void LoadMediaController()
        {
            if (AtomicNarrativeObject.MediaController != null && AtomicNarrativeObject.MediaController.Initialised)
            {
                mediaController = AtomicNarrativeObject.MediaController;
                mediaController.Init();
                mediaController.Load(narrativeObject as AtomicNarrativeObject);
            }
        }

        /// <summary>
        /// Load the media controller associated with the narrative object being processed by this object.
        /// </summary>
        private void PlayMediaController()
        {
            if (AtomicNarrativeObject.MediaController != null && AtomicNarrativeObject.MediaController.Loaded)
            {
                mediaController = AtomicNarrativeObject.MediaController;
                mediaController.Play(narrativeObject as AtomicNarrativeObject);
            }
        }

        /// <summary>
        /// Unload the media controller created by invoking LoadMediaController on this object.
        /// </summary>
        private void UnloadMediaController()
        {
            if (mediaController != null)
            {
                mediaController.Unload();
            }
        }
    }
}