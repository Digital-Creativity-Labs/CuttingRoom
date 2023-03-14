using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Video;
using UnityEngine.UIElements;

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
            Url,
            //Resources,
            //StreamingAssets
        }

        /// <summary>
        /// Where the video clip source is loaded from for this controller.
        /// </summary>
        public SourceLocation sourceLocation = SourceLocation.VideoClip;

        public VideoClip Video = null;

        public bool fullscreen = true;

        public string url = string.Empty;

        // Options for non-fullscreen video
        public int width = 1920;
        public int height = 1080;
        public int marginTop = 0;
        public int marginLeft = 0;

        private UIDocument uiDocument;
        private VisualElement rootVisualElement = null;

        private VideoPlayer videoPlayer = null;

        private Camera videoPlayerCamera = null;

        private bool contentEnded = false;

        public override void Init()
        {
            contentEnded = false;

            if (fullscreen)
            {

                if (Camera.main != null)
                {
                    videoPlayerCamera = Camera.main;

                    // By default try to use shared video player
                    // Get or Add video player to main camera
                    if (!videoPlayerCamera.gameObject.TryGetComponent(out videoPlayer))
                    {
                        videoPlayer = videoPlayerCamera.gameObject.AddComponent<VideoPlayer>();
                    }
                }
                // Get or Add video player
                if (videoPlayer == null && !gameObject.TryGetComponent(out videoPlayer))
                {
                    videoPlayer = gameObject.AddComponent<VideoPlayer>();

                    if (videoPlayer != null)
                    {
                        videoPlayer.playOnAwake = false;
                        videoPlayer.aspectRatio = VideoAspectRatio.FitHorizontally;
                    }
                }

                if (videoPlayer != null)
                {
                    videoPlayer.playOnAwake = false;
                    videoPlayer.aspectRatio = VideoAspectRatio.FitHorizontally;

                    // We are rendering to the near plane (for now...)
                    videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
                    // Get or Add a camera for rendering if not already found from main camera.
                    if (videoPlayerCamera == null && !gameObject.TryGetComponent(out videoPlayerCamera))
                    {
                        videoPlayerCamera = gameObject.AddComponent<Camera>();
                    }

                    if (videoPlayerCamera != null)
                    {
                        videoPlayerCamera.clearFlags = CameraClearFlags.SolidColor;
                        videoPlayerCamera.backgroundColor = Color.black;

                        // Set the render depth of the camera. This comes from the layer that this video controller is sequenced onto.
                        //videoPlayer.targetCamera.depth = renderDepth - 1;
                        videoPlayer.targetCamera = videoPlayerCamera;
                    }
                }
            }
            else
            {

                // Get or Add video player. Use local video player for sub fullscreen video.
                if (!gameObject.TryGetComponent(out videoPlayer))
                {
                    videoPlayer = gameObject.AddComponent<VideoPlayer>();

                    if (videoPlayer != null)
                    {
                        videoPlayer.playOnAwake = false;
                        videoPlayer.aspectRatio = VideoAspectRatio.FitHorizontally;
                    }
                }
                videoPlayer.renderMode = VideoRenderMode.RenderTexture;

                RenderTexture videoRenderTex = new RenderTexture(width, height, 0);
                videoPlayer.targetTexture = videoRenderTex;

                if (!gameObject.TryGetComponent(out uiDocument))
                {
                    uiDocument = gameObject.AddComponent<UIDocument>();
                }

                if (uiDocument != null)
                {
                    if (uiDocument.panelSettings == null)
                    {
                        uiDocument.panelSettings = Resources.Load<PanelSettings>("CuttingRoom/UI/OverlayPanelSettings");
                        uiDocument.sortingOrder = 0;
                    }
                    rootVisualElement = uiDocument.rootVisualElement;
                    rootVisualElement.pickingMode = PickingMode.Ignore;

                    rootVisualElement.style.width = Screen.width;
                    rootVisualElement.style.height = Screen.height;
                    rootVisualElement.style.flexDirection = FlexDirection.Row;

                    VisualElement videoImage = new VisualElement();
                    videoImage.style.backgroundImage = new Background() { renderTexture = videoRenderTex } ;
                    videoImage.style.width = width;
                    videoImage.style.height = height;
                    videoImage.style.position = Position.Relative;
                    videoImage.style.marginTop = marginTop;
                    videoImage.style.marginLeft = marginLeft;

                    rootVisualElement.Add(videoImage);
                }
            }

            if (videoPlayer != null)
            {
                if (sourceLocation == SourceLocation.VideoClip && Video != null)
                {
                    videoPlayer.clip = Video;
                }
                else if (sourceLocation == SourceLocation.Url && !string.IsNullOrEmpty(url))
                {
                    videoPlayer.url = url;
                }
                else
                {
                    // Not supported or not configured correctly
                }

                videoPlayer.Pause();

                // Preload the video player content.
                videoPlayer.Prepare();


                Initialised = true;
            }
        }

        /// <summary>
        /// Load the game objects represented by this controller.
        /// </summary>
        /// <param name="atomicNarrativeObject"></param>
        public override void Load(AtomicNarrativeObject atomicNarrativeObject)
        {
            if (videoPlayer != null)
            {
                if (videoPlayerCamera != null)
                {
                    videoPlayerCamera.enabled = true;
                }

                videoPlayer.loopPointReached += (player) => { contentEnded = true; };

                videoPlayer.Play();
            }
        }

        /// <summary>
        /// Unload the game objects represented by this controller.
        /// </summary>
        public override void Unload()
        {
            if (videoPlayer != null)
            {
                videoPlayer.Stop();
            }
            contentEnded = true;
            StartCoroutine(ShutdownDelay());
        }

        private IEnumerator ShutdownDelay()
        {
            // Delay for 3 frames as this seems to prevent empty frames appearing.
            // Possibly triple buffered so three frames before clearing video buffer means no empty frames?
            // Triple buffered on capable platforms, but some are double!
            //yield return new WaitForEndOfFrame();
            //yield return new WaitForEndOfFrame();
            //yield return new WaitForEndOfFrame();

            // Destroy video player if owned by this game object, not if shared
            if (videoPlayer != null && videoPlayer.gameObject == this.gameObject)
            {
                Debug.Log("Destroying video player: " + gameObject.name);
                Destroy(videoPlayer);
            }
            if (uiDocument != null)
            {
                Destroy(uiDocument);
            }
            yield return null;
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
