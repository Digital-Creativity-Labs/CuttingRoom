using CuttingRoom.UI;
using CuttingRoom.VariableSystem.Variables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace CuttingRoom
{
    [RequireComponent(typeof(StringVariableSetter))]
    public class ButtonUIController : MediaController
    {
        public class UIButton : Button
        {
            private string defaultStyleSheetPath = "CuttingRoom/UI/Style/ButtonUITemplate";
            public string value;

            public UIButton()
            {
                styleSheets.Add(Resources.Load<StyleSheet>(defaultStyleSheetPath));
                AddToClassList("button");
            }


            public UIButton(string text, string value, int number, StyleSheet styleSheet)
            {
                styleSheet ??= Resources.Load<StyleSheet>(defaultStyleSheetPath);
                styleSheets.Add(styleSheet);
                AddToClassList("button");
                this.text = text;
                this.value = value;
            }

            public void SetStyle(StyleSheet styleSheet)
            {
                if (styleSheet != null)
                {
                    styleSheets.Clear();
                    styleSheets.Add(styleSheet);
                }
            }
        }    

        public override ContentTypeEnum ContentType => ContentTypeEnum.ButtonUI;

        public int numberOfButtons = 3;

        public StyleSheet styleSheetOverride = null;

        /// <summary>
        /// Sort order defines what UI elements will appear on top. The highest number will be on top.
        /// </summary>
        public int sortOrder = 0;

        private UnityEngine.Object uiPrefab;

        private GameObject uiObject;

        private UIDocument uiDocument;

        private ButtonUITemplateHandler buttonUIHandler;

        public VariableSetter variableSetter;

        public override bool HasMedia { get => numberOfButtons > 0; }

        private bool contentEnded = false;

        public List<string> buttonTexts = new List<string>();
        public List<string> buttonValues = new List<string>();

        public List<UIButton> Buttons
        {
            get
            {
                var buttons = new List<UIButton>(numberOfButtons);
                for (int i = 0; i < numberOfButtons; ++i)
                {
                    UIButton button;
                    StyleSheet styleSheet;
                    if (styleSheetOverride != null)
                    {
                        styleSheet = styleSheetOverride;
                    }
                    else
                    {
                        styleSheet = Resources.Load<StyleSheet>("CuttingRoom/UI/Style/ButtonUITemplate");
                    }

                    string buttonText = string.Empty;
                    if (buttonTexts.Count > i)
                    {
                        buttonText = buttonTexts[i];
                    }
                    string buttonValue = string.Empty;
                    if (buttonValues.Count > i)
                    {
                        buttonValue = buttonValues[i];
                    }
                    button = new UIButton(buttonText, buttonValue, i, styleSheet);

                    buttons.Add(button);
                }
                return buttons;
            }
            set
            {
                numberOfButtons = value.Count;
                buttonTexts.Clear();
                buttonValues.Clear();
                foreach (UIButton button in value)
                {
                    if (button != null)
                    {
                        buttonTexts.Add(button.text);
                        buttonValues.Add(button.value);
                    }
                }
            }
        }

        public void Reset()
        {
            variableSetter = gameObject.GetComponent<StringVariableSetter>();
        }

        public void Awake()
        {
            variableSetter = gameObject.GetComponent<StringVariableSetter>();
        }

        public override void Init()
        {
            if (variableSetter == null)
            {
                variableSetter = gameObject.GetComponent<StringVariableSetter>();
            }

            uiPrefab = Resources.Load<UnityEngine.Object>("CuttingRoom/UI/ButtonUIPrefab");
            Initialised = uiPrefab != null;
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
                    VisualElement buttonContainer = uiDocument.rootVisualElement.Query("ButtonContainer");
                    if (buttonContainer != null)
                    {
                        var buttonsCache = Buttons;
                        for (int i = 0; i < numberOfButtons; ++i)
                        {
                            UIButton button;
                            StyleSheet styleSheet;
                            if (styleSheetOverride != null)
                            {
                                styleSheet = styleSheetOverride;
                            }
                            else
                            {
                                styleSheet = Resources.Load<StyleSheet>("CuttingRoom/UI/Style/ButtonUITemplate");
                            }

                            if (buttonsCache.Count > i)
                            {
                                button = buttonsCache[i];
                                button.SetStyle(styleSheet);
                            }
                            else
                            {
                                string buttonText = string.Empty;
                                if (buttonTexts.Count > i)
                                {
                                    buttonText = buttonTexts[i];
                                }
                                string buttonValue = string.Empty;
                                if (buttonValues.Count > i)
                                {
                                    buttonValue = buttonValues[i];
                                }
                                button = new UIButton(buttonText, buttonValue, i, styleSheet);
                            }

                            buttonContainer.Add(button);
                        }
                        buttonUIHandler.SetupButtonHandlers(variableSetter);
                    }
                }
            }
        }

        /// <summary>
        /// Unload the game objects represented by this controller.
        /// </summary>
        public override void Unload()
        {
            Destroy(uiObject);
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
            uiObject = Instantiate(uiPrefab as GameObject, atomicNarrativeObject.MediaParent);
            uiDocument = uiObject.GetComponentInChildren<UIDocument>();
            if (styleSheetOverride != null)
            {
                uiDocument.rootVisualElement.styleSheets.Clear();
                uiDocument.rootVisualElement.styleSheets.Add(styleSheetOverride);
            }
            buttonUIHandler = uiObject.GetComponentInChildren<ButtonUITemplateHandler>();
        }
    }
}
