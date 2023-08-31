using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Video;
using UnityEngine.UIElements;
using CuttingRoom.Utilities.Immersive;

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

        public enum RenderType
        {
            Fullscreen,
            PIP,
            Immersive360,
            Immersive180
        }

        /// <summary>
        /// Where the video clip source is loaded from for this controller.
        /// </summary>
        public SourceLocation sourceLocation = SourceLocation.VideoClip;

        public VideoClip Video = null;

        public RenderType renderType = RenderType.Fullscreen;

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
            Initialised = false;
            contentEnded = false;

            // Get or Add video player. Use local video player for sub fullscreen video.
            if (videoPlayer ==  null && !gameObject.TryGetComponent(out videoPlayer))
            {
                videoPlayer = gameObject.AddComponent<VideoPlayer>();

                if (videoPlayer != null)
                {
                    videoPlayer.playOnAwake = false;
                    videoPlayer.aspectRatio = VideoAspectRatio.FitHorizontally;
                }
            }
            else if (videoPlayer != null)
            {
                videoPlayer.seekCompleted -= StopVideoPlayerHandler;
                videoPlayer.enabled = true;
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

                // Get or Add a camera for rendering.
                if (videoPlayerCamera == null)
                {
                    if (Camera.main != null)
                    {
                        videoPlayerCamera = Camera.main;
                    }
                }

                if (renderType == RenderType.Fullscreen)
                {
                    // We are rendering to the near plane (for now...)
                    videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;


                    if (Camera.main != null)
                    {
                        videoPlayerCamera = Camera.main;
                    }

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
                        //videoPlayer.targetCamera = videoPlayerCamera;
                    }
                }
                else if (renderType == RenderType.PIP)
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
                else if (renderType == RenderType.Immersive360 || renderType == RenderType.Immersive180)
                {
                    videoPlayer.renderMode = VideoRenderMode.RenderTexture;

                    if (videoPlayerCamera != null)
                    {
                        videoPlayerCamera.gameObject.AddComponent<MouseNavigation360>();
                    }

                    RenderTexture videoRenderTex = new RenderTexture((int)videoPlayer.width, (int)videoPlayer.height, 0);
                    videoPlayer.targetTexture = videoRenderTex;

                    Material skyboxMaterial;
                    if (renderType == RenderType.Immersive360)
                    {
                        skyboxMaterial = Resources.Load<Material>("CuttingRoom/Render/Immersive/Skybox360");
                    }
                    else
                    {
                        skyboxMaterial = Resources.Load<Material>("CuttingRoom/Render/Immersive/Skybox180");
                    }
                    skyboxMaterial.mainTexture = videoRenderTex;

                    RenderSettings.skybox = skyboxMaterial;
                }

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

                videoPlayer.prepareCompleted -= VideoPlayer_prepareCompleted;
                if (!videoPlayer.isPrepared)
                {
                    videoPlayer.prepareCompleted += VideoPlayer_prepareCompleted;

                    // Preload the video player content.
                    videoPlayer.Prepare();
                }
                else
                {
                    Debug.Log($"{Video.name} already prepared");
                }


                base.Init();
            }
        }

        private void VideoPlayer_prepareCompleted(VideoPlayer source)
        {
            Debug.Log($"{Video.name} prepared");
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
                    videoPlayer.targetCamera = videoPlayerCamera;

                    if (renderType == RenderType.Fullscreen)
                    {
                        videoPlayerCamera.clearFlags = CameraClearFlags.SolidColor;
                        videoPlayerCamera.backgroundColor = Color.black;
                    }
                    else if (renderType == RenderType.Immersive360 || renderType == RenderType.Immersive180)
                    {
                        videoPlayerCamera.clearFlags = CameraClearFlags.Skybox;
                        videoPlayerCamera.backgroundColor = Color.clear;
                    }
                }

                videoPlayer.loopPointReached += LoopPointReached;

                if (!videoPlayer.enabled)
                {
                    videoPlayer.enabled = true;
                }
                videoPlayer.Play();
            }
        }

        public void LoopPointReached(VideoPlayer player)
        {
            contentEnded = true;
        }

        /// <summary>
        /// Unload the game objects represented by this controller.
        /// </summary>
        public override void Unload()
        {
            if (videoPlayer != null)
            {
                videoPlayer.loopPointReached -= LoopPointReached;
            }
            contentEnded = true;
            StartCoroutine(ShutdownDelay());
        }

        void StopVideoPlayerHandler(VideoPlayer vp)
        {
            vp.Stop();
        }

        private IEnumerator ShutdownDelay()
        {
            if (uiDocument != null)
            {
                Destroy(uiDocument);
            }

            // Delay for 3 frames as this seems to prevent empty frames appearing.
            // Possibly triple buffered so three frames before clearing video buffer means no empty frames?
            // Triple buffered on capable platforms, but some are double!
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            // Destroy video player if owned by this game object, not if shared
            if (videoPlayer != null)
            {
                Debug.Log("Disabling video player: " + gameObject.name);
                //Destroy(videoPlayer);
                //videoPlayer.targetCamera = null;
                //videoPlayer.enabled = false;
                videoPlayer.seekCompleted += StopVideoPlayerHandler;
                videoPlayer.frame = 0;
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
