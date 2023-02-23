using CuttingRoom.UI;
using CuttingRoom.VariableSystem.Variables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace CuttingRoom
{
    public class ImageController : MediaController
    {
        public override ContentTypeEnum ContentType => ContentTypeEnum.Image;

        public Texture2D image = null;

        public bool fullscreen = true;

        public StyleSheet styleSheetOverride = null;

        // Options for non-fullscreen image
        public int width = 1920;
        public int height = 1080;
        public int marginTop = 0;
        public int marginLeft = 0;

        /// <summary>
        /// Sort order defines what UI elements will appear on top. The highest number will be on top.
        /// </summary>
        public int sortOrder = 0;

        private UnityEngine.Object imageScreenPrefab;

        private GameObject imageScreenObject;

        private UIDocument uiDocument;

        public override bool HasMedia { get => image != null; }

        private bool contentEnded = false;

        public override void Init()
        {
            imageScreenPrefab = Resources.Load<UnityEngine.Object>("CuttingRoom/UI/ImageScreenPrefab");
            Initialised = imageScreenPrefab != null;
        }

        /// <summary>
        /// Load the game objects represented by this controller.
        /// </summary>
        /// <param name="atomicNarrativeObject"></param>
        public override void Load(AtomicNarrativeObject atomicNarrativeObject)
        {
            contentEnded = false;
            LoadUIDocument(atomicNarrativeObject);
            if (uiDocument != null)
            {
                if (uiDocument.visualTreeAsset != null)
                {
                    VisualElement imageContainer = uiDocument.rootVisualElement.Query("ImageContainer");
                    if (imageContainer != null)
                    {
                        imageContainer.style.backgroundImage = new StyleBackground(image);
                        if (styleSheetOverride == null)
                        {
                            if (!fullscreen)
                            {
                                imageContainer.style.width = width;
                                imageContainer.style.height = height;
                                imageContainer.style.position = Position.Relative;
                                imageContainer.style.marginTop = marginTop;
                                imageContainer.style.marginLeft = marginLeft;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Unload the game objects represented by this controller.
        /// </summary>
        public override void Unload()
        {
            Destroy(imageScreenObject);
        }

        public override IEnumerator WaitForEndOfContent()
        {
            while (!contentEnded)
            {
                yield return new WaitForEndOfFrame();
            }
        }

        private void LoadUIDocument(AtomicNarrativeObject atomicNarrativeObject)
        {
            imageScreenObject = Instantiate(imageScreenPrefab as GameObject, atomicNarrativeObject.MediaParent);
            uiDocument = imageScreenObject.GetComponentInChildren<UIDocument>();
            if (uiDocument != null && styleSheetOverride != null)
            {
                uiDocument.rootVisualElement.styleSheets.Clear();
                uiDocument.rootVisualElement.styleSheets.Add(styleSheetOverride);
            }
        }
    }
}
