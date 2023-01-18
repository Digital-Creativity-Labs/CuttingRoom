using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Video;

namespace CuttingRoom
{
    public class AudioController : MediaController
    {
        public override ContentTypeEnum ContentType => ContentTypeEnum.Audio;

        /// <summary>
        /// Where this audio is loaded from.
        /// </summary>
        public enum SourceLocation
        {
            AudioClip,
            Resources,
            StreamingAssets
        }

        /// <summary>
        /// Where the audio clip source is loaded from for this controller.
        /// </summary>
        private SourceLocation sourceLocation = SourceLocation.AudioClip;

        [SerializeField]
        public AudioClip Audio = null;

        private AudioSource audioSource = null;

        public override void Init()
        {
            if (sourceLocation == SourceLocation.AudioClip)
            {
                // Get or Add audio source
                if (!gameObject.TryGetComponent(out audioSource))
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }

                if (audioSource != null)
                {
                    audioSource.playOnAwake = false;
                    audioSource.spatialize = false;
                    audioSource.Pause();

                    Initialised = true;
                }
            }
        }

        /// <summary>
        /// Load the game objects represented by this controller.
        /// </summary>
        /// <param name="atomicNarrativeObject"></param>
        public override void Load(AtomicNarrativeObject atomicNarrativeObject)
        {
            if (Audio != null)
            {
                audioSource.clip = Audio;

                audioSource.Play();
            }
        }

        /// <summary>
        /// Unload the game objects represented by this controller.
        /// </summary>
        public override void Unload()
        {
            audioSource.Stop();
            Debug.Log("Destroying audio source: " + gameObject.name);

            Destroy(audioSource);
        }

        public override IEnumerator WaitForEndOfContent()
        {
            while (audioSource != null && audioSource.isPlaying)
            {
                yield return new WaitForEndOfFrame();
            }
        }

        public override bool HasMedia
        {
            get => Audio != null;
        }
    }
}
