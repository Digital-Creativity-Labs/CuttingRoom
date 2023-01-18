using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Video;

namespace CuttingRoom
{
    public class VideoController : MediaController
    {
        public override ContentTypeEnum ContentType => ContentTypeEnum.Video;

        /// <summary>
        /// Where this video is loaded from.
        /// </summary>
        public enum SourceLocation
        {
            VideoClip,
            Resources,
            StreamingAssets
        }

        /// <summary>
        /// Where the video clip source is loaded from for this controller.
        /// </summary>
        private SourceLocation sourceLocation = SourceLocation.VideoClip;

        [SerializeField]
        public VideoClip Video = null;

        private VideoPlayer videoPlayer = null;

        private Camera videoPlayerCamera = null;

        private bool contentEnded = false;

        public override void Init()
        {
            contentEnded = false;
            if (sourceLocation == SourceLocation.VideoClip)
            {
                // Get or Add video player
                if (!gameObject.TryGetComponent(out videoPlayer))
                {
                    videoPlayer = gameObject.AddComponent<VideoPlayer>();
                }
                // Get or Add a camera for rendering.
                if (!gameObject.TryGetComponent(out videoPlayerCamera))
                {
                    videoPlayerCamera = gameObject.AddComponent<Camera>();
                }

                if (videoPlayer != null && videoPlayerCamera != null)
                {
                    videoPlayerCamera.clearFlags = CameraClearFlags.SolidColor;
                    videoPlayerCamera.backgroundColor = Color.black;

                    // Hide the camera.
                    videoPlayerCamera.enabled = false;

                    videoPlayer.playOnAwake = false;

                    videoPlayer.aspectRatio = VideoAspectRatio.FitHorizontally;

                    // We are rendering to the near plane (for now...)
                    videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;

                    videoPlayer.Pause();

                    // Preload the video player content.
                    videoPlayer.Prepare();

                    // Set the render depth of the camera. This comes from the layer that this video controller is sequenced onto.
                    //videoPlayer.targetCamera.depth = renderDepth - 1;
                    videoPlayer.targetCamera = videoPlayerCamera;

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
            if (Video != null)
            {
                videoPlayer.clip = Video;

                videoPlayerCamera.enabled = true;

                videoPlayer.loopPointReached += (player) => { contentEnded = true; };

                videoPlayer.Play();
            }
        }

        /// <summary>
        /// Unload the game objects represented by this controller.
        /// </summary>
        public override void Unload()
        {
            videoPlayer.Stop();
            StartCoroutine(ShutdownDelay());
        }

        private IEnumerator ShutdownDelay()
        {
            // Delay for 3 frames as this seems to prevent empty frames appearing.
            // Possibly triple buffered so three frames before clearing video buffer means no empty frames?
            // Triple buffered on capable platforms, but some are double!
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            Debug.Log("Destroying video player: " + gameObject.name);

            Destroy(videoPlayer);
        }

        public override IEnumerator WaitForEndOfContent()
        {
            while (!contentEnded)
            {
                yield return new WaitForEndOfFrame();
            }
        }

        public override bool HasMedia
        {
            get => Video != null;
        }
    }
}
