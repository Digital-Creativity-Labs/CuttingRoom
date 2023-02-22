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
    public class TextController : MediaController
    {
        public override ContentTypeEnum ContentType => ContentTypeEnum.Text;

        public string text = string.Empty;

        public Color fontColour = Color.white;

        public float fontSize = 32;

        public StyleSheet styleSheetOverride = null;

        /// <summary>
        /// Sort order defines what UI elements will appear on top. The highest number will be on top.
        /// </summary>
        public int sortOrder = 0;

        private UnityEngine.Object textScreenPrefab;

        private GameObject textScreenObject;

        private UIDocument uiDocument;

        public override bool HasMedia { get => true; }

        private bool contentEnded = false;

        public override void Init()
        {
            textScreenPrefab = Resources.Load<UnityEngine.Object>("CuttingRoom/UI/TextScreenPrefab");
            Initialised = textScreenPrefab != null;
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
                    VisualElement textContainer = uiDocument.rootVisualElement.Query("TextContainer");
                    if (textContainer != null && textContainer is TextElement)
                    {
                        TextElement textElement = textContainer as TextElement;
                        textElement.text = text;

                        if (styleSheetOverride == null)
                        {
                            if (fontColour != textElement.style.color)
                            {
                                textElement.style.color = fontColour;
                            }

                            if (fontSize != textElement.style.fontSize)
                            {
                                textElement.style.fontSize = fontSize;
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
            Destroy(textScreenObject);
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
            textScreenObject = Instantiate(textScreenPrefab as GameObject, atomicNarrativeObject.MediaParent);
            uiDocument = textScreenObject.GetComponentInChildren<UIDocument>();
            if (styleSheetOverride != null)
            {
                uiDocument.rootVisualElement.styleSheets.Clear();
                uiDocument.rootVisualElement.styleSheets.Add(styleSheetOverride);
            }
        }
    }
}
