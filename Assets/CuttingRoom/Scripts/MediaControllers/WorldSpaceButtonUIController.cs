using CuttingRoom.Editor;
using CuttingRoom.VariableSystem.Variables;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CuttingRoom
{
    [RequireComponent(typeof(StringVariableSetter))]
    public class WorldSpaceButtonUIController : MediaController
    {
        public class UIButtonDefinition
        {
            public string text;
            public string value;

            public UIButtonDefinition() { }

            public UIButtonDefinition(string text, string value)
            {
                this.text = text;
                this.value = value;
            }
        }
        public override ContentTypeEnum ContentType => ContentTypeEnum.ButtonUI_VR;

        private readonly Dictionary<int, List<Vector3>> buttonTransforms = new Dictionary<int, List<Vector3>>()
        {
            {1, new List<Vector3>() { new Vector3(0, 0, 0) } },
            {2, new List<Vector3>() { new Vector3(-100, 0, 0), new Vector3(100, 0, 0) } },
            {3, new List<Vector3>() { new Vector3(-200, 0, 0), new Vector3(0, 0, 0), new Vector3(200, 0, 0) } },
            {4, new List<Vector3>() { new Vector3(-300, 0, 0), new Vector3(-100, 0, 0), new Vector3(100, 0, 0), new Vector3(200, 0, 0) } }
        };

        private GameObject uiObject = null;

        public UnityEngine.Object uiPrefab = null;
        public UnityEngine.Object buttonPrefab = null;

        private string defaultUICanvasPrefabPath = "CuttingRoom/UI/WorldSpaceCanvasPrefab";
        private string defaultbuttonPrefabPath = "CuttingRoom/UI/WorldSpaceButtonPrefab";

        private Canvas parentCanvas = null;

        public int numberOfButtons = 0;

        public List<string> buttonTexts = new List<string>();
        public List<string> buttonValues = new List<string>();

        private List<GameObject> buttonsGameObjects = new();
        public List<UIButtonDefinition> Buttons
        {
            get
            {
                var buttons = new List<UIButtonDefinition>(numberOfButtons);
                for (int i = 0; i < numberOfButtons; ++i)
                {
                    UIButtonDefinition button;

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
                    button = new UIButtonDefinition(buttonText, buttonValue);

                    buttons.Add(button);
                }
                return buttons;
            }
            set
            {
                numberOfButtons = value.Count;
                buttonTexts.Clear();
                buttonValues.Clear();
                foreach (UIButtonDefinition button in value)
                {
                    if (button != null)
                    {
                        buttonTexts.Add(button.text);
                        buttonValues.Add(button.value);
                    }
                }
            }
        }

        public override bool HasMedia { get => numberOfButtons > 0; }

        public VariableSetter variableSetter;

        private bool contentEnded = false;

        public override void Init()
        {
            if (variableSetter == null)
            {
                variableSetter = gameObject.GetComponent<StringVariableSetter>();
            }

            if (uiPrefab == null)
            {
                uiPrefab = Resources.Load<UnityEngine.Object>(defaultUICanvasPrefabPath);
            }
            if (buttonPrefab == null)
            {
                buttonPrefab = Resources.Load<UnityEngine.Object>(defaultbuttonPrefabPath);
            }
            Initialised = uiPrefab != null;
        }

        public void Reset()
        {
            variableSetter = gameObject.GetComponent<StringVariableSetter>();
        }

        public void Awake()
        {
            variableSetter = gameObject.GetComponent<StringVariableSetter>();
        }

        /// <summary>
        /// Load the game objects represented by this controller.
        /// </summary>
        /// <param name="atomicNarrativeObject"></param>
        public override void Load(AtomicNarrativeObject atomicNarrativeObject)
        {
            uiObject = Instantiate(uiPrefab as GameObject, atomicNarrativeObject.MediaParent);

            parentCanvas = uiObject.GetComponent<Canvas>();

            if (parentCanvas != null && buttonPrefab != null && buttonTransforms.ContainsKey(numberOfButtons))
            {
                List<Vector3> transforms = buttonTransforms[numberOfButtons];
                for (int i = 0; i < numberOfButtons; ++i)
                {
                    Vector3 buttonTransform = transforms[i];
                    //buttonTransform += parentCanvas.transform.localPosition;
                    GameObject newButton = Instantiate(buttonPrefab as GameObject, parentCanvas.transform);
                    newButton.transform.SetLocalPositionAndRotation(buttonTransform, Quaternion.identity);

                    var newButtonComponent = newButton.GetComponentInChildren<Button>();

                    if (Buttons.Count > i)
                    {
                        var tmpTextComponent = newButton.GetComponentInChildren<TMP_Text>();
                        if (tmpTextComponent != null)
                        {
                            tmpTextComponent.text = Buttons[i].text;
                        }
                        else
                        {
                            var textComponent = newButton.GetComponentInChildren<Text>();
                            if (textComponent != null)
                            {
                                textComponent.text = Buttons[i].text;
                            }
                        }
                        var setValue = Buttons[i].value;
                        newButtonComponent.onClick.AddListener(() =>
                        {
                            if (variableSetter != null)
                            {
                                variableSetter.Set(setValue);
                            }
                        });
                    }

                    buttonsGameObjects.Add(newButton);
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
    }
}
