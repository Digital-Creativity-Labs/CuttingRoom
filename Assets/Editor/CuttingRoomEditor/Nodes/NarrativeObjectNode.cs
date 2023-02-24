using Codice.CM.SEIDInfo;
using CuttingRoom.VariableSystem;
using CuttingRoom.VariableSystem.Constraints;
using CuttingRoom.VariableSystem.Variables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace CuttingRoom.Editor
{
    public abstract class NarrativeObjectNode : Node
    {
        /// <summary>
        /// The narrative object represented by this node.
        /// </summary>
        public NarrativeObject NarrativeObject { get; set; } = null;

        /// <summary>
        /// The narrative object which is represented by the view which this node exists within.
        /// </summary>
        public NarrativeObject ParentNarrativeObject { get; private set; } = null;

        /// <summary>
        /// The input port for this node.
        /// </summary>
        public Port InputPort { get; private set; } = null;

        /// <summary>
        /// The output port for this node.
        /// </summary>
        public Port OutputPort { get; private set; } = null;

        /// <summary>
        /// The style sheet for this node.
        /// </summary>
        protected StyleSheet StyleSheet = null;

        /// <summary>
        /// Event invoked when set as root is clicked.
        /// </summary>
        public event Action<NarrativeObjectNode> OnSetAsNarrativeSpaceRoot;

        /// <summary>
        /// Event invoked when set as root is clicked.
        /// </summary>
        public event Action<NarrativeObjectNode> OnSetAsParentNarrativeObjectRoot;

        /// <summary>
        /// Event invoked when set as candidate is clicked in context menu.
        /// </summary>
        public event Action OnSetAsCandidate;

        /// <summary>
        /// Event invoked when remove as candidate is clicked in context menu.
        /// </summary>
        public event Action OnRemoveAsCandidate;

        /// <summary>
        /// Abstract method which must be implememented to update nodes when the
        /// narrative object they are representing has its values changed in the inspector.
        /// </summary>
        protected abstract void OnNarrativeObjectChanged();

        /// <summary>
        /// The root image visual element.
        /// </summary>
        private VisualElement rootImage = null;

        /// <summary>
        /// The root image visual element.
        /// </summary>
        private VisualElement candidateImage = null;

        /// <summary>
        /// The root image visual element.
        /// </summary>
        private VisualElement layerImage = null;

        /// <summary>
        /// The title element.
        /// </summary>
        private VisualElement titleElement = null;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="narrativeObject"></param>
        public NarrativeObjectNode(NarrativeObject narrativeObject, NarrativeObject parentNarrativeObject)
        {
            // Store reference to the narrative object being represented.
            NarrativeObject = narrativeObject;

            // Store reference to narrative object whose view container contains this node.
            ParentNarrativeObject = parentNarrativeObject;

            // Set the title of the node to the name of the game object it represents.
            title = narrativeObject.gameObject.name;

            InputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
            InputPort.portName = "Input";

            OutputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float));
            OutputPort.portName = "Output";

            inputContainer.Add(InputPort);
            outputContainer.Add(OutputPort);

            StyleSheet = Resources.Load<StyleSheet>("NarrativeObjectNode");

            titleElement = this.Q<VisualElement>("title");
            titleElement?.styleSheets.Add(StyleSheet);

            // Get the contents container and add the stylesheet.
            VisualElement contents = this.Q<VisualElement>("contents");
            contents.styleSheets.Add(StyleSheet);
        }

        /// <summary>
        /// Enable the visual element for this being a root.
        /// </summary>
        public void EnableRootVisuals(bool isNarrativeSpaceRoot = false)
        {
            if (titleElement != null && !titleElement.Contains(rootImage))
            {
                rootImage = new VisualElement();
                rootImage.name = "root-icon";
                if (isNarrativeSpaceRoot)
                {
                    rootImage.AddToClassList("title-icon-primary");
                }
                else
                {
                    rootImage.AddToClassList("title-icon-secondary");
                }

                titleElement.Insert(0, rootImage);
            }
        }

        /// <summary>
        /// Enable the visual element for this being a candidate of a decision point.
        /// </summary>
        public void EnableCandidateVisuals()
        {
            if (titleElement != null && !titleElement.Contains(candidateImage))
            {
                candidateImage = new VisualElement();
                candidateImage.name = "candidate-icon";
                candidateImage.AddToClassList("title-icon-secondary");

                titleElement.Insert(0, candidateImage);
            }
        }

        /// <summary>
        /// Enable the visual element for this being a candidate of a decision point.
        /// </summary>
        public void EnableLayerVisuals(bool isPrimaryLayer = false)
        {
            if (titleElement != null && !titleElement.Contains(layerImage))
            {
                layerImage = new VisualElement();
                layerImage.name = "layer-icon";

                if (isPrimaryLayer)
                {
                    layerImage.AddToClassList("title-icon-primary");
                }
                else
                {
                    layerImage.AddToClassList("title-icon-secondary");
                }

                titleElement.Insert(0, layerImage);
            }
        }

        /// <summary>
        /// Called when this node has to construct its contextual menu.
        /// </summary>
        /// <param name="evt"></param>
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Set as Narrative Space Root", OnSetAsNarrativeSpaceRootFromContextualMenu, DropdownMenuAction.Status.Normal);

            // If null, then the object is on the root view level so can't be a candidate.
            if (ParentNarrativeObject != null)
            {
                if (ParentNarrativeObject is GraphNarrativeObject)
                {
                    evt.menu.AppendAction("Set as Narrative Object Root", OnSetAsParentNarrativeObjectRootFromContextualMenu, DropdownMenuAction.Status.Normal);
                }
                // If this node exists inside a view which can have candidates.
                else if (ParentNarrativeObject is GroupNarrativeObject)
                {
                    // If not currently candidate, add add candidate option, else remove candidate option.
                    GroupNarrativeObject groupNarrativeObject = ParentNarrativeObject.GetComponent<GroupNarrativeObject>();

                    if (groupNarrativeObject.GroupSelectionDecisionPoint.Candidates.Contains(NarrativeObject))
                    {
                        evt.menu.AppendAction("Remove as Candidate", OnRemoveAsCandidateFromContextualMenu, DropdownMenuAction.Status.Normal);
                    }
                    else
                    {
                        evt.menu.AppendAction("Set as Candidate", OnSetAsCandidateFromContextualMenu, DropdownMenuAction.Status.Normal);
                    }
                }
                else if (ParentNarrativeObject is LayerNarrativeObject)
                {
                    evt.menu.AppendAction("Set as Primary Layer", OnSetAsParentNarrativeObjectRootFromContextualMenu, DropdownMenuAction.Status.Normal);
                    // If not currently candidate, add add candidate option, else remove candidate option.
                    LayerNarrativeObject layerNarrativeObject = ParentNarrativeObject.GetComponent<LayerNarrativeObject>();

                    if (layerNarrativeObject.LayerSelectionDecisionPoint.Candidates.Contains(NarrativeObject) &&
                        NarrativeObject != layerNarrativeObject.primaryLayerRootNarrativeObject)
                    {
                        evt.menu.AppendAction("Remove as Secondary Layer", OnRemoveAsCandidateFromContextualMenu, DropdownMenuAction.Status.Normal);
                    }
                    else
                    {
                        evt.menu.AppendAction("Set as Secondary Layer", OnSetAsCandidateFromContextualMenu, DropdownMenuAction.Status.Normal);
                    }
                }
            }

            evt.menu.AppendSeparator();
        }

        /// <summary>
        /// Callback for Set as Root option in contextual menu.
        /// </summary>
        /// <param name="action"></param>
        private void OnSetAsNarrativeSpaceRootFromContextualMenu(DropdownMenuAction action)
        {
            OnSetAsNarrativeSpaceRoot?.Invoke(this);
        }

        /// <summary>
        /// Callback for Set as Root option in contextual menu.
        /// </summary>
        /// <param name="action"></param>
        private void OnSetAsParentNarrativeObjectRootFromContextualMenu(DropdownMenuAction action)
        {
            OnSetAsParentNarrativeObjectRoot?.Invoke(this);
        }

        /// <summary>
        /// Callback for Set as Candidate option in contextual menu.
        /// </summary>
        /// <param name="action"></param>
        private void OnSetAsCandidateFromContextualMenu(DropdownMenuAction action)
        {
            OnSetAsCandidate?.Invoke();
        }

        /// <summary>
        /// Callback for Remove as Candidate option in contextual menu.
        /// </summary>
        /// <param name="action"></param>
        private void OnRemoveAsCandidateFromContextualMenu(DropdownMenuAction action)
        {
            OnRemoveAsCandidate?.Invoke();
        }

        /// <summary>
        /// Get the fields to be displayed on the blackboard for this narrative object node.
        /// </summary>
        public virtual List<VisualElement> GetEditableFieldRows()
        {
            List<VisualElement> editorRows = new List<VisualElement>();

            VisualElement nameTextFieldRow = UIElementsUtils.CreateTextFieldRow("Name", NarrativeObject.gameObject.name, multiline: false, (newValue) =>
            {
                Undo.RecordObject(NarrativeObject.gameObject, $"Set Narrative Object Name {(newValue)}");
                NarrativeObject.gameObject.name = newValue;
                NarrativeObject.OnValidate();
            });

            editorRows.Add(nameTextFieldRow);

            OutputSelectionDecisionPoint.SelectionMethod outputSelectionMethod = OutputSelectionDecisionPoint.SelectionMethod.None;
            if (NarrativeObject.OutputSelectionDecisionPoint.methodContainer != null && Enum.TryParse(NarrativeObject.OutputSelectionDecisionPoint.methodContainer.methodName, ignoreCase: true, out OutputSelectionDecisionPoint.SelectionMethod selectionMethod))
            {
                outputSelectionMethod = selectionMethod;
            }

            VisualElement outputDecisionMethodNameFieldRow = UIElementsUtils.CreateEnumFieldRow("Output Decision Method", outputSelectionMethod, (newValue) =>
            {
                if (Enum.TryParse(newValue.ToString(), ignoreCase: true, out OutputSelectionDecisionPoint.SelectionMethod outputSelectionMethod))
                {
                    Undo.RecordObject(NarrativeObject.OutputSelectionDecisionPoint, $"Set Output Selection Method {(outputSelectionMethod)}");
                    if (outputSelectionMethod != OutputSelectionDecisionPoint.SelectionMethod.None)
                    {
                        NarrativeObject.OutputSelectionDecisionPoint.methodContainer.methodName = outputSelectionMethod.ToString();
                    }
                    else
                    {
                        NarrativeObject.OutputSelectionDecisionPoint.methodContainer.methodName = string.Empty;
                    }
                }
            });

            editorRows.Add(outputDecisionMethodNameFieldRow);

            return editorRows;
        }

        public Dictionary<ProcessingEndTrigger, VisualElement> GetProcessingTriggerRows()
        {
            return GetProcessingTriggerRows(NarrativeObject.EndTriggers);
        }

        public Dictionary<Constraint, VisualElement> GetOutputDecisionPointConstraintRows()
        {
            return GetConstraintRows(NarrativeObject.OutputSelectionDecisionPoint.Constraints);
        }

        public Dictionary<Constraint, VisualElement> GetCandidateConstraintRows()
        {
            return GetConstraintRows(NarrativeObject.constraints);
        }

        public Dictionary<ProcessingEndTrigger, VisualElement> GetProcessingTriggerRows(List<ProcessingEndTrigger> processingTriggers)
        {
            Dictionary<ProcessingEndTrigger, VisualElement> processingTriggerRows = new Dictionary<ProcessingEndTrigger, VisualElement>();

            if (processingTriggers != null)
            {
                foreach (var trigger in processingTriggers)
                {
                    if (trigger != null)
                    {
                        //Capture trigger
                        ProcessingEndTrigger processingTrigger = trigger;
                        ProcessingEndTriggerFactory.TriggerType processingTriggerType = ProcessingEndTriggerFactory.GetTriggerType(processingTrigger);
                        // Processing Trigger.
                        VisualElement processingTriggerRow = UIElementsUtils.GetContainer();
                        processingTriggerRow.styleSheets.Add(StyleSheet);

                        //Type based label
                        Label triggerRowLabel = new Label();
                        triggerRowLabel.AddToClassList("trigger-title");
                        triggerRowLabel.text = $"{processingTriggerType} Trigger";

                        processingTriggerRow.Add(triggerRowLabel);

                        if (processingTriggerType == ProcessingEndTriggerFactory.TriggerType.Timed && processingTrigger.GetType() == typeof(TimeEndTrigger))
                        {
                            TimeEndTrigger timedProcessingTrigger = processingTrigger as TimeEndTrigger;

                            // Duration
                            VisualElement durationRow = UIElementsUtils.GetRowContainer();
                            Label durationLabel = new Label("Duration (s): ");
                            durationLabel.AddToClassList("trigger-field-label");
                            durationRow.Add(durationLabel);
                            FloatField durationField = new FloatField();
                            durationField.value = timedProcessingTrigger.duration;
                            durationField.RegisterValueChangedCallback(evt =>
                            {
                                Undo.RecordObject(timedProcessingTrigger, $"Set Timed Trigger Duration");
                                timedProcessingTrigger.duration = evt.newValue;
                            });
                            durationField.AddToClassList("trigger-field-value");
                            durationRow.Add(durationField);

                            processingTriggerRow.Add(durationRow);
                        }
                        else if (processingTriggerType == ProcessingEndTriggerFactory.TriggerType.Variable)
                        {
                            VariableEndTrigger variableProcessingTrigger = processingTrigger as VariableEndTrigger;

                            VariableStore targetVariableStore = null;
                            Variable targetVariable = null;

                            // Find the sequencer.
                            NarrativeSpace narrativeSpace = UnityEngine.Object.FindObjectOfType<NarrativeSpace>();

                            if (narrativeSpace != null && !narrativeSpace.UnlockAdvancedFeatures)
                            {
                                // Force only global variable without advance feature unlock
                                variableProcessingTrigger.VariableLocation = VariableStoreLocation.Global;
                            }

                            // Find Variable Store
                            switch (variableProcessingTrigger.VariableLocation)
                            {
                                case VariableStoreLocation.Global:
                                    if (narrativeSpace != null)
                                    {
                                        targetVariableStore = narrativeSpace.GlobalVariableStore;
                                    }
                                    break;

                                case VariableStoreLocation.Local:
                                    targetVariableStore = NarrativeObject.GetComponent<NarrativeObject>().VariableStore; ;
                                    break;

                                default:
                                    break;
                            }

                            // Variable Trigger Event
                            VisualElement variableTriggerRow = UIElementsUtils.GetRowContainer();
                            variableTriggerRow.styleSheets.Add(StyleSheet);
                            Label varTriggerLabel = new Label("Trigger Event: ");
                            varTriggerLabel.AddToClassList("trigger-field-label");
                            variableTriggerRow.Add(varTriggerLabel);
                            EnumField variableTriggerEnumField = new EnumField(variableProcessingTrigger.VariableTriggerEvent);
                            variableTriggerEnumField.RegisterValueChangedCallback(evt =>
                            {
                                Undo.RecordObject(processingTrigger, $"Set End Processing Trigger");
                                variableProcessingTrigger.VariableTriggerEvent = (VariableEndTrigger.TriggeringEvent)evt.newValue;
                                processingTrigger = variableProcessingTrigger;
                                // Flag that the object has changed.
                                NarrativeObject.OnValidate();
                            });
                            variableTriggerEnumField.AddToClassList("trigger-field-value");
                            variableTriggerRow.Add(variableTriggerEnumField);
                            // Add to parent container
                            processingTriggerRow.Add(variableTriggerRow);

                            // Variable Location
                            if (narrativeSpace != null && narrativeSpace.UnlockAdvancedFeatures)
                            {
                                VisualElement variableLocationRow = UIElementsUtils.GetRowContainer();
                                variableLocationRow.styleSheets.Add(StyleSheet);
                                Label varLocationLabel = new Label("Variable Location: ");
                                varLocationLabel.AddToClassList("trigger-field-label");
                                variableLocationRow.Add(varLocationLabel);
                                EnumField variableStoreLocationEnumField = new EnumField(variableProcessingTrigger.VariableLocation);
                                variableStoreLocationEnumField.RegisterValueChangedCallback(evt =>
                                {
                                    Undo.RecordObject(processingTrigger, $"Set Trigger Variable Location");
                                    variableProcessingTrigger.VariableLocation = (VariableStoreLocation)evt.newValue;
                                    variableProcessingTrigger.VariableName = string.Empty;
                                    processingTrigger = variableProcessingTrigger;
                                    // Flag that the object has changed.
                                    NarrativeObject.OnValidate();
                                });
                                variableStoreLocationEnumField.AddToClassList("trigger-field-value");
                                variableLocationRow.Add(variableStoreLocationEnumField);
                                // Add to parent container
                                processingTriggerRow.Add(variableLocationRow);
                            }

                            if (targetVariableStore != null)
                            {
                                List<string> variableNames = new List<string>();
                                variableNames.Add("Undefined");
                                foreach (var v in targetVariableStore.Variables)
                                {
                                    if (!string.IsNullOrEmpty(v.Key))
                                    {
                                        variableNames.Add(v.Key);
                                    }
                                }

                                if (string.IsNullOrEmpty(variableProcessingTrigger.VariableName)
                                    || !targetVariableStore.Variables.ContainsKey(variableProcessingTrigger.VariableName))
                                {
                                    variableProcessingTrigger.VariableName = "Undefined";
                                }

                                // Variable Name
                                VisualElement variableNameRow = UIElementsUtils.GetRowContainer();
                                variableNameRow.styleSheets.Add(StyleSheet);
                                Label varNameLabel = new Label("Variable Name: ");
                                varNameLabel.AddToClassList("trigger-field-label");
                                variableNameRow.Add(varNameLabel);
                                PopupField<string> variableNamePopUpField = new PopupField<string>(variableNames, variableProcessingTrigger.VariableName);
                                variableNamePopUpField.RegisterValueChangedCallback(evt =>
                                {
                                    Undo.RecordObject(processingTrigger, $"Set Trigger Variable Name");
                                    if (string.IsNullOrEmpty(evt.newValue))
                                    {
                                        variableProcessingTrigger.VariableName = "Undefined";
                                    }
                                    else
                                    {
                                        variableProcessingTrigger.VariableName = evt.newValue;
                                    }
                                    processingTrigger = variableProcessingTrigger;

                                    // Flag that the object has changed.
                                    NarrativeObject.OnValidate();
                                });
                                variableNamePopUpField.AddToClassList("trigger-field-value");
                                variableNameRow.Add(variableNamePopUpField);
                                // Add to parent container
                                processingTriggerRow.Add(variableNameRow);

                                // Target Value
                                if (variableProcessingTrigger.VariableTriggerEvent == VariableEndTrigger.TriggeringEvent.OnValueMatch &&
                                    !string.IsNullOrEmpty(variableProcessingTrigger.VariableName))
                                {
                                    targetVariable = targetVariableStore.GetVariable<Variable>(variableProcessingTrigger.VariableName);
                                    VariableStore localVariableStore = NarrativeObject.GetComponent<NarrativeObject>().VariableStore;

                                    if (targetVariable != null)
                                    {
                                        VisualElement valueMatchRow = UIElementsUtils.GetRowContainer();
                                        valueMatchRow.styleSheets.Add(StyleSheet);
                                        Label valueMatchLabel = new Label("Value To Match: ");
                                        valueMatchLabel.AddToClassList("trigger-field-label");
                                        valueMatchRow.Add(valueMatchLabel);

                                        VisualElement valueMatchField = null;

                                        Type variableType = targetVariable.GetType();
                                        string variableName = $"valueMatch_{targetVariable.Name}";
                                        Undo.RecordObjects(new UnityEngine.Object[] { localVariableStore, processingTrigger }, $"Set Value Match Variable Trigger");
                                        if (variableType == typeof(BoolVariable))
                                        {
                                            variableProcessingTrigger.ValueMatch = localVariableStore.GetOrAddVariable<BoolVariable>(variableName, Variable.VariableCategory.SystemDefined);
                                            bool value = default;
                                            if (variableProcessingTrigger.ValueMatch != null)
                                            {
                                                value = ((BoolVariable)variableProcessingTrigger.ValueMatch).Value;
                                            }

                                            // Value Match
                                            var valueMatchPopUpField = new PopupField<bool>(new List<bool>() { true, false }, value);
                                            valueMatchPopUpField.RegisterValueChangedCallback(evt =>
                                            {
                                                Undo.RecordObject(variableProcessingTrigger.ValueMatch, "Set Value To Match");
                                                ((BoolVariable)variableProcessingTrigger.ValueMatch).SetValue(evt.newValue);
                                                processingTrigger = variableProcessingTrigger;
                                                // Flag that the object has changed.
                                                NarrativeObject.OnValidate();
                                            });
                                            valueMatchPopUpField.AddToClassList("trigger-field-value");
                                            valueMatchField = valueMatchPopUpField;
                                        }
                                        else if (variableType == typeof(IntVariable))
                                        {
                                            variableProcessingTrigger.ValueMatch = localVariableStore.GetOrAddVariable<IntVariable>(variableName, Variable.VariableCategory.SystemDefined);
                                            int value = default;
                                            if (variableProcessingTrigger.ValueMatch != null)
                                            {
                                                value = ((IntVariable)variableProcessingTrigger.ValueMatch).Value;
                                            }

                                            // Value Match
                                            var valueMatchIntField = new IntegerField();
                                            valueMatchIntField.value = value;
                                            valueMatchIntField.RegisterValueChangedCallback(evt =>
                                            {
                                                Undo.RecordObject(variableProcessingTrigger.ValueMatch, "Set Value To Match");
                                                ((IntVariable)variableProcessingTrigger.ValueMatch).SetValue(evt.newValue);
                                                processingTrigger = variableProcessingTrigger;
                                            });
                                            valueMatchIntField.AddToClassList("trigger-field-value");
                                            valueMatchField = valueMatchIntField;
                                        }
                                        else if (variableType == typeof(FloatVariable))
                                        {
                                            variableProcessingTrigger.ValueMatch = localVariableStore.GetOrAddVariable<FloatVariable>(variableName, Variable.VariableCategory.SystemDefined);
                                            float value = default;
                                            if (variableProcessingTrigger.ValueMatch != null)
                                            {
                                                value = ((FloatVariable)variableProcessingTrigger.ValueMatch).Value;
                                            }

                                            // Value Match
                                            var valueMatchFloatField = new FloatField();
                                            valueMatchFloatField.value = value;
                                            valueMatchFloatField.RegisterValueChangedCallback(evt =>
                                            {
                                                Undo.RecordObject(variableProcessingTrigger.ValueMatch, "Set Value To Match");
                                                ((FloatVariable)variableProcessingTrigger.ValueMatch).SetValue(evt.newValue);
                                                processingTrigger = variableProcessingTrigger;
                                            });
                                            valueMatchFloatField.AddToClassList("trigger-field-value");
                                            valueMatchField = valueMatchFloatField;
                                        }
                                        else if (variableType == typeof(StringVariable))
                                        {
                                            variableProcessingTrigger.ValueMatch = localVariableStore.GetOrAddVariable<StringVariable>(variableName, Variable.VariableCategory.SystemDefined);
                                            string value = default;
                                            if (variableProcessingTrigger.ValueMatch != null)
                                            {
                                                value = ((StringVariable)variableProcessingTrigger.ValueMatch).Value;
                                            }

                                            // Value Match
                                            var valueMatchStringField = new TextField();
                                            valueMatchStringField.value = value;
                                            valueMatchStringField.RegisterValueChangedCallback(evt =>
                                            {
                                                Undo.RecordObject(variableProcessingTrigger.ValueMatch, "Set Value To Match");
                                                ((StringVariable)variableProcessingTrigger.ValueMatch).SetValue(evt.newValue);
                                                processingTrigger = variableProcessingTrigger;
                                            });
                                            valueMatchStringField.AddToClassList("trigger-field-value");
                                            valueMatchField = valueMatchStringField;
                                        }
                                        localVariableStore.RefreshDictionary();

                                        if (valueMatchField != null)
                                        {
                                            valueMatchRow.Add(valueMatchField);
                                        }
                                        else
                                        {
                                            valueMatchLabel.text = "Cannot find variable";
                                        }

                                        processingTriggerRow.Add(valueMatchRow);
                                    }
                                }
                            }
                        }
                        processingTriggerRows.Add(processingTrigger, processingTriggerRow);
                    }
                }
            }

            return processingTriggerRows;
        }

        /// <summary>
        /// Get the constraints to be displayed on the blackboard.
        /// </summary>
        /// <returns></returns>
        public Dictionary<Constraint, VisualElement> GetConstraintRows(List<Constraint> constraints)
        {
            Dictionary<Constraint, VisualElement> constraintRows = new Dictionary<Constraint, VisualElement>();

            foreach (Constraint constraint in constraints)
            {
                Label constraintRowLabel = new Label();
                constraintRowLabel.styleSheets.Add(StyleSheet);
                constraintRowLabel.AddToClassList("constraint-title");
                EnumField comparisonTypeEnumField = null;

                void SetConstraintRowLabelText()
                {
#if UNITY_2021_1_OR_NEWER
                    string constraintName = $"{constraint.variableStoreLocation} {(constraint.variableName != null ? constraint.variableName : "<color=red>Undefined</color>")} {comparisonTypeEnumField.value} {constraint.Value}";
#else
                    // No rich text before 2021.1
                    string constraintName = $"{constraint.GetType().Name.Replace("Variable", " ")}: {(constraint.variableStoreLocation != VariableStoreLocation.Undefined ? constraint.variableStoreLocation.ToString() : "<variable location>")} {(constraint.variableName != null ? constraint.variableName : "<name>")} {comparisonTypeEnumField.value} {constraint.Value}";
#endif
                    constraintRowLabel.text = constraintName;
                }

                VisualElement constraintContainer = new VisualElement();

                constraintContainer.styleSheets.Add(StyleSheet);

                constraintContainer.AddToClassList("constraint-container");


                // Find the sequencer.
                NarrativeSpace narrativeSpace = UnityEngine.Object.FindObjectOfType<NarrativeSpace>();

                if (narrativeSpace != null && !narrativeSpace.UnlockAdvancedFeatures)
                {
                    // Force only global variable without advance feature unlock
                    constraint.variableStoreLocation = VariableStoreLocation.Global;
                }

                //Find Target Variable Store
                VariableStore targetVariableStore = null;
                if (constraint.variableStoreLocation != VariableStoreLocation.Undefined)
                {
                    // Find Variable Store
                    switch (constraint.variableStoreLocation)
                    {
                        case VariableStoreLocation.Global:
                            if (narrativeSpace != null)
                            {
                                targetVariableStore = narrativeSpace.GlobalVariableStore;
                            }
                            break;

                        case VariableStoreLocation.Local:
                            targetVariableStore = NarrativeObject.GetComponent<NarrativeObject>().VariableStore; ;
                            break;

                        default:
                            break;
                    }
                }

                if (narrativeSpace != null && narrativeSpace.UnlockAdvancedFeatures)
                {
                    // Variable Location
                    VisualElement varLocationRow = UIElementsUtils.GetRowContainer();
                    varLocationRow.styleSheets.Add(StyleSheet);
                    Label varLocationLabel = new Label("Variable Location: ");
                    varLocationLabel.AddToClassList("constraint-field-label");
                    varLocationRow.Add(varLocationLabel);
                    EnumField variableStoreLocationEnumField = new EnumField(constraint.variableStoreLocation);
                    variableStoreLocationEnumField.RegisterValueChangedCallback(evt =>
                    {
                        Undo.RecordObject(constraint, "Set Constraint Variable Location");
                        constraint.variableStoreLocation = (VariableStoreLocation)evt.newValue;

                        SetConstraintRowLabelText();
                        NarrativeObject.OnValidate();
                    });
                    variableStoreLocationEnumField.AddToClassList("constraint-field-value");
                    varLocationRow.Add(variableStoreLocationEnumField);
                    constraintContainer.Add(varLocationRow);
                }

                List<string> variableNames = new List<string>();
                variableNames.Add("Undefined");

                if (targetVariableStore != null)
                {
                    foreach (Variable v in targetVariableStore.variableList)
                    {
                        if (v == null)
                        {
                            continue;
                        }
                        variableNames.Add(v.Name);
                    }
                }

                if (string.IsNullOrEmpty(constraint.variableName))
                {
                    constraint.variableName = "Undefined";
                }

                // Variable Name
                VisualElement varNameRow = UIElementsUtils.GetRowContainer();
                Label varNameLabel = new Label("Variable Name: ");
                varNameLabel.AddToClassList("constraint-field-label");
                varNameRow.Add(varNameLabel);
                PopupField<string> variableNameField = new PopupField<string>(variableNames, 0);
                variableNameField.value = constraint.variableName;
                variableNameField.RegisterValueChangedCallback(evt =>
                {
                    Undo.RecordObject(constraint, "Set Constraint Variable Name");
                    if (string.IsNullOrEmpty(evt.newValue))
                    {
                        constraint.variableName = "Undefined";
                    }
                    else
                    {
                        constraint.variableName = evt.newValue;
                    }

                    SetConstraintRowLabelText();
                    NarrativeObject.OnValidate();
                });
                variableNameField.AddToClassList("constraint-field-value");
                varNameRow.Add(variableNameField);
                constraintContainer.Add(varNameRow);

                VisualElement comparisonRow = UIElementsUtils.GetRowContainer();
                Label comparisonLabel = new Label("Comparison: ");
                comparisonLabel.AddToClassList("constraint-field-label");
                comparisonRow.Add(comparisonLabel);
                comparisonTypeEnumField = GetConstraintComparisonTypeEnumField(constraint, () =>
                {
                    SetConstraintRowLabelText();
                });
                comparisonTypeEnumField.AddToClassList("constraint-field-value");
                comparisonRow.Add(comparisonTypeEnumField);

                // Add the comparison type enum field based on the type of constraint being visualised.
                constraintContainer.Add(comparisonRow);

                // Add a field for the value of the constraint specified.
                AddConstraintValueField(constraintContainer, constraint, () =>
                {
                    SetConstraintRowLabelText();
                });

                SetConstraintRowLabelText();

                VisualElement constraintRow = new VisualElement();
                constraintRow.Add(constraintRowLabel);
                constraintRow.Add(constraintContainer);

                constraintRows.Add(constraint, constraintRow);
            }

            return constraintRows;
        }

        /// <summary>
        /// Generate a sidebar section for the constraints specified in the passed rows.
        /// </summary>
        /// <param name="constraintVisualElements"></param>
        /// <param name="labelText"></param>
        /// <param name="onClickAddConstraint"></param>
        /// <param name="onClickRemoveConstraint"></param>
        /// <returns></returns>
        public VisualElement GetConstraintSection(Dictionary<Constraint, VisualElement> constraintVisualElements, string labelText, Action<ConstraintFactory.ConstraintType> onClickAddConstraint, Action<Constraint> onClickRemoveConstraint)
        {
            VisualElement constraintsSection = UIElementsUtils.GetContainerWithLabel(labelText);

            EnumField constraintTypeEnumField = new EnumField(ConstraintFactory.ConstraintType.String);

            VisualElement addConstraintButton = UIElementsUtils.GetSquareButton("+", () =>
            {
                onClickAddConstraint?.Invoke((ConstraintFactory.ConstraintType)constraintTypeEnumField.value);
            });

            VisualElement topRow = new();
            topRow.style.width = constraintsSection.style.width;
            topRow.style.flexDirection = FlexDirection.Row;
            topRow.style.justifyContent = Justify.SpaceBetween;

            VisualElement addConstraintControlsContainer = new VisualElement();
            addConstraintControlsContainer.AddToClassList("add-constraint-controls-container");
            addConstraintControlsContainer.styleSheets.Add(StyleSheet);

            addConstraintControlsContainer.Add(addConstraintButton);
            addConstraintControlsContainer.Add(constraintTypeEnumField);

            topRow.Add(addConstraintControlsContainer);

            VisualElement anyOrAllToggle = new();
            var buttonGroup = new RadioButtonGroup("Constraint Mode:");//, new List<string>() { "Valid if ANY", "Valid if ALL" });
            buttonGroup.style.flexDirection = FlexDirection.Row;
            buttonGroup.AddToClassList("constraint-option-pill");
            var anyButton = new RadioButton("Valid if ANY");
            var allButton = new RadioButton("Valid if ALL");
            buttonGroup.Add(anyButton);
            buttonGroup.Add(allButton);
            anyOrAllToggle.Add(buttonGroup);
            topRow.Add(anyOrAllToggle);

            constraintsSection.Add(topRow);

            if (constraintVisualElements.Count > 0)
            {
                foreach (KeyValuePair<Constraint, VisualElement> kvp in constraintVisualElements)
                {
                    VisualElement rowVisualElement = new VisualElement();

                    rowVisualElement.styleSheets.Add(StyleSheet);

                    rowVisualElement.AddToClassList("constraint-row");

                    VisualElement removeConstraintButton = UIElementsUtils.GetSquareButton("-", () =>
                    {
                        onClickRemoveConstraint?.Invoke(kvp.Key);
                    });

                    // Apply the constraints field container class to the returned visual element.
                    kvp.Value.styleSheets.Add(StyleSheet);
                    kvp.Value.AddToClassList("constraint-fields-container");

                    rowVisualElement.Add(removeConstraintButton);
                    rowVisualElement.Add(kvp.Value);

                    constraintsSection.Add(rowVisualElement);
                }
            }

            return constraintsSection;
        }

        /// <summary>
        /// Find the correct constraint object on a game object with a reference to the base instance of constraint.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="constraint"></param>
        /// <returns></returns>
        private T GetConstraint<T>(Constraint constraint) where T : Constraint
        {
            T[] typedVariableConstraints = constraint.GetComponents<T>();

            T typedVariableConstraint = typedVariableConstraints.Where(typedVariableConstraint => typedVariableConstraint == constraint).FirstOrDefault();

            return typedVariableConstraint;
        }

        /// <summary>
        /// Gets the enum field which represents the comparison type of the specified constraint.
        /// </summary>
        /// <param name="constraint"></param>
        /// <returns></returns>
        private EnumField GetConstraintComparisonTypeEnumField(Constraint constraint, Action onValueChanged)
        {
            EnumField enumField = null;

            if (constraint is StringVariableConstraint)
            {
                StringVariableConstraint stringVariableConstraint = GetConstraint<StringVariableConstraint>(constraint);

                enumField = new EnumField(stringVariableConstraint.comparisonType);
                enumField.RegisterValueChangedCallback(evt =>
                {
                    stringVariableConstraint.comparisonType = (StringVariableConstraint.ComparisonType)evt.newValue;

                    onValueChanged?.Invoke();
                });
            }
            else if (constraint is BoolVariableConstraint)
            {
                BoolVariableConstraint boolVariableConstraint = GetConstraint<BoolVariableConstraint>(constraint);

                enumField = new EnumField(boolVariableConstraint.Comparison);
                enumField.RegisterValueChangedCallback(evt =>
                {
                    boolVariableConstraint.Comparison = (BoolVariableConstraint.ComparisonType)evt.newValue;

                    onValueChanged?.Invoke();
                });
            }
            else if (constraint is FloatVariableConstraint)
            {
                FloatVariableConstraint floatVariableConstraint = GetConstraint<FloatVariableConstraint>(constraint);

                enumField = new EnumField(floatVariableConstraint.comparisonType);
                enumField.RegisterValueChangedCallback(evt =>
                {
                    floatVariableConstraint.comparisonType = (FloatVariableConstraint.ComparisonType)evt.newValue;

                    onValueChanged?.Invoke();
                });
            }
            else if (constraint is IntVariableConstraint)
            {
                IntVariableConstraint intVariableConstraint = GetConstraint<IntVariableConstraint>(constraint);

                enumField = new EnumField(intVariableConstraint.Comparison);
                enumField.RegisterValueChangedCallback(evt =>
                {
                    intVariableConstraint.Comparison = (IntVariableConstraint.ComparisonType)evt.newValue;

                    onValueChanged?.Invoke();
                });
            }
            else if (constraint is TagVariableConstraint)
            {
                TagVariableConstraint tagConstraint = GetConstraint<TagVariableConstraint>(constraint);

                enumField = new EnumField(tagConstraint.Comparison);
                enumField.RegisterValueChangedCallback(evt =>
                {
                    tagConstraint.Comparison = (TagVariableConstraint.ComparisonType)evt.newValue;

                    onValueChanged?.Invoke();
                });
            }

            return enumField;
        }

        /// <summary>
        /// Adds the correct type of value field for the specified constraint.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="constraint"></param>
        private void AddConstraintValueField(VisualElement container, Constraint constraint, Action onValueChanged)
        {
            VisualElement row = UIElementsUtils.GetRowContainer();
            Label label = new Label("Value: ");
            label.AddToClassList("constraint-field-label");
            row.Add(label);
            VisualElement valueField = null;
            if (constraint is StringVariableConstraint)
            {
                StringVariableConstraint stringVariableConstraint = GetConstraint<StringVariableConstraint>(constraint);

                TextField textField = new TextField();
                textField.isDelayed = true;
                textField.value = stringVariableConstraint.value;
                textField.RegisterValueChangedCallback(evt =>
                {
                    Undo.RecordObject(constraint, $"Set Constraint Variable Value {(constraint.GetType().Name)}");
                    stringVariableConstraint.value = evt.newValue;

                    onValueChanged?.Invoke();
                });
                textField.AddToClassList("constraint-field-value");

                valueField = textField;
            }
            else if (constraint is BoolVariableConstraint)
            {
                BoolVariableConstraint boolVariableConstraint = GetConstraint<BoolVariableConstraint>(constraint);

                PopupField<bool> boolSelect = new PopupField<bool>(new List<bool>() { true, false }, 0);
                boolSelect.value = boolVariableConstraint.value;
                boolSelect.RegisterValueChangedCallback(evt =>
                {
                    Undo.RecordObject(constraint, $"Set Constraint Variable Value {(constraint.GetType().Name)}");
                    boolVariableConstraint.value = evt.newValue;

                    onValueChanged?.Invoke();
                });
                boolSelect.AddToClassList("constraint-field-value");

                valueField = boolSelect;
            }
            else if (constraint is FloatVariableConstraint)
            {
                FloatVariableConstraint floatVariableConstraint = GetConstraint<FloatVariableConstraint>(constraint);

                FloatField floatField = new FloatField();
                floatField.value = floatVariableConstraint.value;
                floatField.RegisterValueChangedCallback(evt =>
                {
                    Undo.RecordObject(constraint, $"Set Constraint Variable Value {(constraint.GetType().Name)}");
                    floatVariableConstraint.value = evt.newValue;

                    onValueChanged?.Invoke();
                });
                floatField.AddToClassList("constraint-field-value");

                valueField = floatField;
            }
            else if (constraint is IntVariableConstraint)
            {
                IntVariableConstraint intVariableConstraint = GetConstraint<IntVariableConstraint>(constraint);

                IntegerField intField = new IntegerField();
                intField.value = intVariableConstraint.value;
                intField.RegisterValueChangedCallback(evt =>
                {
                    Undo.RecordObject(constraint, $"Set Constraint Variable Value {(constraint.GetType().Name)}");
                    intVariableConstraint.value = evt.newValue;

                    onValueChanged?.Invoke();
                });
                intField.AddToClassList("constraint-field-value");

                valueField = intField;
            }
            else if (constraint is TagVariableConstraint)
            {
                label.text = "Tag Name: ";
                TagVariableConstraint tagConstraint = GetConstraint<TagVariableConstraint>(constraint);

                TextField tagNameField = new TextField();
                tagNameField.isDelayed = true;
                tagNameField.value = tagConstraint.value;
                tagNameField.RegisterValueChangedCallback(evt =>
                {
                    Undo.RecordObject(constraint, $"Set Constraint Variable Value {(constraint.GetType().Name)}");
                    tagConstraint.value = evt.newValue;

                    onValueChanged?.Invoke();
                });
                tagNameField.AddToClassList("constraint-field-value");

                valueField = tagNameField;
            }

            row.Add(label);
            row.Add(valueField);

            container.Add(row);
        }
    }
}
