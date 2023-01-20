using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Video;
using UnityEngine.UIElements;
using UnityEditor.SceneManagement;
using UnityEngine.Assertions;
using UnityEditor.VersionControl;

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

        public VideoClip Video = null;

        [SerializeField]
        private bool fullscreen = true;
        private UIDocument uiDocument;

        [SerializeField]
        private int width = 1920;
        [SerializeField]
        private int height = 1080;
        [SerializeField]
        private int marginTopPercent = 0;
        [SerializeField]
        private int marginLeftPercent = 0;
        [SerializeField]
        private int marginTop = 0;
        [SerializeField]
        private int marginLeft = 0;

        private VisualElement rootVisualElement = null;

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

                if (videoPlayer != null)
                {
                    videoPlayer.playOnAwake = false;
                    videoPlayer.aspectRatio = VideoAspectRatio.FitHorizontally;

                    if (fullscreen)
                    {
                        // We are rendering to the near plane (for now...)
                        videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
                        // Get or Add a camera for rendering.
                        if (!gameObject.TryGetComponent(out videoPlayerCamera))
                        {
                            videoPlayerCamera = gameObject.AddComponent<Camera>();
                        }
                        if (videoPlayerCamera != null)
                        {
                            videoPlayerCamera.clearFlags = CameraClearFlags.SolidColor;
                            videoPlayerCamera.backgroundColor = Color.black;

                            // Hide the camera.
                            videoPlayerCamera.enabled = false;

                            // Set the render depth of the camera. This comes from the layer that this video controller is sequenced onto.
                            //videoPlayer.targetCamera.depth = renderDepth - 1;
                            videoPlayer.targetCamera = videoPlayerCamera;
                        }
                    }
                    else
                    {
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
                                uiDocument.panelSettings = Resources.Load<PanelSettings>("CuttingRoom/UI/UIVideoPanelSettings");
                            }
                            rootVisualElement = uiDocument.rootVisualElement;

                            rootVisualElement.style.width = Screen.width;
                            rootVisualElement.style.height = Screen.height;
                            rootVisualElement.style.flexDirection = FlexDirection.Row;
                            rootVisualElement.style.alignContent = Align.Center;


                            VisualElement videoImage = new VisualElement();

                            videoImage.style.backgroundImage = new Background() { renderTexture = videoRenderTex } ;

                            videoImage.style.width = width;
                            videoImage.style.height = height;
                            videoImage.style.position = Position.Relative;
                            videoImage.style.alignSelf = Align.Center;
                            videoImage.style.marginTop = marginTop; // Screen.height * ((float)marginTopPercent / 100);
                            videoImage.style.marginLeft = marginLeft; // Screen.width * ((float)marginLeftPercent / 100);

                            rootVisualElement.Add(videoImage);
                        }
                    }

                    videoPlayer.Pause();

                    // Preload the video player content.
                    videoPlayer.Prepare();


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
            if (uiDocument != null)
            {
                Destroy(uiDocument);
            }
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
