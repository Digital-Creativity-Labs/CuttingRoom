using CuttingRoom.VariableSystem;
using CuttingRoom.VariableSystem.Variables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Experimental.AI;
using UnityEngine.UIElements;
using UnityEngine.Video;

namespace CuttingRoom.Editor
{
    public class AtomicNarrativeObjectNode : NarrativeObjectNode
    {
        /// <summary>
        /// The graph narrative object represented by this node.
        /// </summary>
        private AtomicNarrativeObject AtomicNarrativeObject { get; set; } = null;

        /// <summary>
        /// The toggle used to represent whether the atomic narrative object represented by this node has a media source assigned.
        /// </summary>
        private Toggle hasMediaSourceToggle = null;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="atomicNarrativeObject"></param>
        public AtomicNarrativeObjectNode(AtomicNarrativeObject atomicNarrativeObject, NarrativeObject parentNarrativeObject) : base(atomicNarrativeObject, parentNarrativeObject)
        {
            AtomicNarrativeObject = atomicNarrativeObject;

            atomicNarrativeObject.OnChanged += OnNarrativeObjectChanged;

            StyleSheet = Resources.Load<StyleSheet>("AtomicNarrativeObjectNode");

            VisualElement titleElement = this.Q<VisualElement>("title");
            titleElement?.styleSheets.Add(StyleSheet);

            titleContainer.styleSheets.Add(StyleSheet);

            GenerateContents();

            SetContentsFields();
        }

        /// <summary>
        /// Generate the contents of the content container for this node.
        /// </summary>
        private void GenerateContents()
        {
            // Get the contents container for this node.
            VisualElement contents = this.Q<VisualElement>("contents");

            // Add a divider below the ports.
            contents.Add(UIElementsUtils.GetHorizontalDivider());

            // Add toggle to show whether this atomic has a media source.
            hasMediaSourceToggle = new Toggle("Has Media Source");
            hasMediaSourceToggle.SetEnabled(false);
            hasMediaSourceToggle.name = "media-toggle";
            hasMediaSourceToggle.styleSheets.Add(StyleSheet);
            contents.Add(hasMediaSourceToggle);
        }

        /// <summary>
        /// Set the fields representing the atomic narrative object in the contents container.
        /// </summary>
        private void SetContentsFields()
        {
            hasMediaSourceToggle.SetValueWithoutNotify(AtomicNarrativeObject.MediaController != null && AtomicNarrativeObject.MediaController.HasMedia);
        }

        /// <summary>
        /// Invoked when the atomic narrative object represented by this node is changed in the inspector.
        /// </summary>
        protected override void OnNarrativeObjectChanged()
        {
            SetContentsFields();
        }

        public override List<VisualElement> GetEditableFieldRows()
        {
            List<VisualElement> rows = new List<VisualElement>(base.GetEditableFieldRows());

            MediaController.ContentTypeEnum contentType = MediaController.ContentTypeEnum.Undefined;

            if (AtomicNarrativeObject.MediaController != null)
            {
                contentType = AtomicNarrativeObject.MediaController.ContentType;
            }

            // Media source.
            VisualElement mediaSourceRow = UIElementsUtils.CreateEnumFieldRow("Content Type", contentType, (newValue) =>
            {
                MediaController.ContentTypeEnum newContentType = (MediaController.ContentTypeEnum)newValue;
                if (AtomicNarrativeObject.MediaController == null)
                {
                    Undo.RecordObject(AtomicNarrativeObject, $"Set Content Type {newContentType}");
                }
                else
                {
                    Undo.RecordObject(AtomicNarrativeObject.MediaController, $"Set Content Type {newContentType}");
                }
                MediaController mediaController = MediaController.GetOrCreateMediaController(newContentType, AtomicNarrativeObject.gameObject);
                AtomicNarrativeObject.MediaController = mediaController;
                if (mediaController != null)
                {
                    contentType = AtomicNarrativeObject.MediaController.ContentType;
                }
                else
                {
                    contentType = MediaController.ContentTypeEnum.Undefined;
                }
                // Flag that the object has changed.
                AtomicNarrativeObject.OnValidate();
            });

            if (contentType == MediaController.ContentTypeEnum.Video)
            {
                VideoController videoController = AtomicNarrativeObject.MediaController as VideoController;
                if (videoController != null)
                {
                    VisualElement videoPicker = UIElementsUtils.CreateObjectFieldRow("Video", videoController.Video, (newValue) =>
                    {
                        Undo.RecordObject(videoController, "Set Video Content");
                        videoController.Video = newValue;
                        AtomicNarrativeObject.MediaController = videoController;
                        // Flag that the object has changed.
                        AtomicNarrativeObject.OnValidate();
                    });
                    mediaSourceRow.Add(videoPicker);
                }
            }
            else if (contentType == MediaController.ContentTypeEnum.Audio)
            {
                AudioController audioController = AtomicNarrativeObject.MediaController as AudioController;
                if (audioController != null)
                {
                    VisualElement audioPicker = UIElementsUtils.CreateObjectFieldRow("Audio", audioController.Audio, (newValue) =>
                    {
                        Undo.RecordObject(audioController, "Set Audio Content");
                        audioController.Audio = newValue;
                        AtomicNarrativeObject.MediaController = audioController;
                        // Flag that the object has changed.
                        AtomicNarrativeObject.OnValidate();
                    });
                    mediaSourceRow.Add(audioPicker);
                }
            }
            else if (contentType == MediaController.ContentTypeEnum.ButtonUI)
            {
                ButtonUIController buttonController = AtomicNarrativeObject.MediaController as ButtonUIController;
                if (buttonController != null)
                {
                    VariableSetter variableSetter = buttonController.variableSetter;
                    VariableStore targetVariableStore = null;

                    // Find the sequencer.
                    Sequencer sequencer = UnityEngine.Object.FindObjectOfType<Sequencer>();

                    if (sequencer != null && sequencer.NarrativeSpace != null && !sequencer.NarrativeSpace.UnlockAdvancedFeatures)
                    {
                        // Force only global variable without advance feature unlock
                        variableSetter.variableStoreLocation = VariableStoreLocation.Global;
                    }

                    // Find Variable Store
                    switch (variableSetter.variableStoreLocation)
                    {
                        case VariableStoreLocation.Global:
                            if (sequencer != null && sequencer.NarrativeSpace != null)
                            {
                                targetVariableStore = sequencer.NarrativeSpace.GlobalVariableStore;
                            }
                            break;

                        case VariableStoreLocation.Local:
                            targetVariableStore = NarrativeObject.GetComponent<NarrativeObject>().VariableStore; ;
                            break;

                        default:
                            break;
                    }

                    if (sequencer != null && sequencer.NarrativeSpace != null && sequencer.NarrativeSpace.UnlockAdvancedFeatures)
                    {
                        // Variable Location
                        VisualElement variableStoreLocationEnumField = UIElementsUtils.CreateEnumFieldRow("Button Variable Location", variableSetter.variableStoreLocation, (newValue) =>
                        {
                            Undo.RecordObject(variableSetter, "Edit Button UI Variable Location");
                            variableSetter.variableStoreLocation = (VariableStoreLocation)newValue;
                            variableSetter.variableName = string.Empty;
                            buttonController.variableSetter = variableSetter;
                            // Flag that the object has changed.
                            NarrativeObject.OnValidate();
                        });
                        // Add to parent container
                        mediaSourceRow.Add(variableStoreLocationEnumField);
                    }

                    if (targetVariableStore != null)
                    {
                        List<string> variableNames = new List<string>();
                        variableNames.Add("Undefined");
                        foreach (Variable v in targetVariableStore.variableList)
                        {
                            variableNames.Add(v.Name);
                        }

                        if (string.IsNullOrEmpty(variableSetter.variableName))
                        {
                            variableSetter.variableName = "Undefined";
                        }

                        // Variable Name
                        VisualElement variableNamePopUpField = UIElementsUtils.CreatePopUpFieldRow("Variable Name", variableSetter.variableName, variableNames, (newValue) =>
                        {
                            Undo.RecordObject(variableSetter, "Edit Button UI Variable Name");
                            if (string.IsNullOrEmpty(newValue))
                            {
                                variableSetter.variableName = "Undefined";
                            }
                            else
                            {
                                variableSetter.variableName = newValue;
                            }
                            buttonController.variableSetter = variableSetter;

                            // Flag that the object has changed.
                            NarrativeObject.OnValidate();
                        });
                        // Add to parent container
                        mediaSourceRow.Add(variableNamePopUpField);
                    }

                    VisualElement buttonStyle = UIElementsUtils.CreateObjectFieldRow("Custom Style Sheet", buttonController.styleSheetOverride, (newValue) =>
                    {
                        Undo.RecordObject(buttonController, "Change Custom Button UI Stylesheet");
                        buttonController.styleSheetOverride = newValue;
                        AtomicNarrativeObject.MediaController = buttonController;
                        // Flag that the object has changed.
                        AtomicNarrativeObject.OnValidate();
                    });
                    mediaSourceRow.Add(buttonStyle);

                    VisualElement numOfButtons = UIElementsUtils.CreateIntegerFieldRow("Number Of Buttons", buttonController.numberOfButtons, (newValue) =>
                    {
                        Undo.RecordObject(buttonController, "Set Number of Buttons");
                        buttonController.numberOfButtons = newValue;
                        AtomicNarrativeObject.MediaController = buttonController;
                        // Flag that the object has changed.
                        AtomicNarrativeObject.OnValidate();
                    });
                    mediaSourceRow.Add(numOfButtons);

                    List<ButtonUIController.UIButton> uiButtons = buttonController.Buttons;
                    VisualElement buttonList = UIElementsUtils.CreateCustomListFieldRow("Buttons", uiButtons, (newValue) =>
                    {
                        bool elementAddedOrDeleted = false;
                        List<ButtonUIController.UIButton> updatedButtons = new List<ButtonUIController.UIButton>();
                        foreach (var button in newValue)
                        {
                            updatedButtons.Add(button);
                        }

                        elementAddedOrDeleted = updatedButtons.Count != buttonController.Buttons.Count;

                        Undo.RecordObject(buttonController, "Add or Edit Button");

                        buttonController.Buttons = updatedButtons;
                        AtomicNarrativeObject.MediaController = buttonController;

                        if (elementAddedOrDeleted)
                        {
                            // Flag that the object has changed.
                            AtomicNarrativeObject.OnValidate();
                        }
                    }, UIButtonInspectorComponent.Render, Resources.Load<StyleSheet>("UIButtonInspector"), "button-list-element");

                    mediaSourceRow.Add(buttonList);
                }
            }
            else if(contentType == MediaController.ContentTypeEnum.GameObject)
            {
                GameObjectController gameObjController = AtomicNarrativeObject.MediaController as GameObjectController;
                if (gameObjController != null)
                {
                    VisualElement gameObjList = UIElementsUtils.CreateListFieldRow("Game Objects", gameObjController.gameObjects, (newValue) =>
                    {
                        Undo.RecordObject(gameObjController, "Add or Edit Game Objects");
                        gameObjController.gameObjects = newValue;
                        AtomicNarrativeObject.MediaController = gameObjController;
                        // Flag that the object has changed.
                        AtomicNarrativeObject.OnValidate();
                    });
                    mediaSourceRow.Add(gameObjList);
                }
            }

            rows.Add(mediaSourceRow);

            return rows;
        }
    }
}