using Codice.Client.BaseCommands;
using CuttingRoom.VariableSystem;
using CuttingRoom.VariableSystem.Constraints;
using CuttingRoom.VariableSystem.Variables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CuttingRoom.Editor
{
    public class EditorInspector : VisualElement
    {
        /// <summary>
        /// Invoked whenever a constraint is added to a narrative object.
        /// </summary>
        public event Action OnNarrativeObjectAddedConstraint;

        /// <summary>
        /// Invoked whenever a constraint is removed from a narrative object.
        /// </summary>
        public event Action OnNarrativeObjectRemovedConstraint;

        /// <summary>
        /// Invoked whenever a processing trigger is added to a narrative object.
        /// </summary>
        public event Action OnNarrativeObjectAddedEndTrigger;

        /// <summary>
        /// Invoked whenever a processing trigger is removed from a narrative object.
        /// </summary>
        public event Action OnNarrativeObjectRemovedEndTrigger;

        /// <summary>
        /// Invoked whenever a variable is added to a narrative object.
        /// </summary>
        public event Action OnNarrativeObjectAddedVariable;

        /// <summary>
        /// Invoked whenever a variable is removed from a narrative object.
        /// </summary>
        public event Action OnNarrativeObjectRemovedVariable;

        /// <summary>
        /// Invoked whenever a variable is edited on a narrative object.
        /// </summary>
        public event Action OnNarrativeObjectEditVariable;

        /// <summary>
        /// The style sheet for this visual element.
        /// </summary>
        public StyleSheet StyleSheet { get; set; } = null;

        /// <summary>
        /// Scroll view for the whole window to ensure content isn't squashed to fit.
        /// </summary>
        public ScrollView scrollView = new ScrollView(ScrollViewMode.Vertical);

        /// <summary>
        /// Constructor.
        /// </summary>
        public EditorInspector()
        {
            StyleSheet = Resources.Load<StyleSheet>("EditorInspector");

            styleSheets.Add(StyleSheet);

            name = "inspector";

            Add(scrollView);
        }

        /// <summary>
        /// Clear all settings.
        /// </summary>
        public void ClearContent()
        {
            scrollView.Clear();
            VisualElement container = new VisualElement();
            scrollView.Add(container);
        }

        /// <summary>
        /// Show global settings.
        /// </summary>
        public void UpdateContentForGlobal(NarrativeSpace narrativeSpace)
        {
            scrollView.Clear();

            VisualElement container = new VisualElement();

            Variable.VariableCategory variableCategory = Variable.VariableCategory.UserDefined;

            if (narrativeSpace != null && narrativeSpace.UnlockAdvancedFeatures)
            {
                variableCategory = Variable.VariableCategory.Any;
            }

            VisualElement variablesSectionContainer = VariableStoreComponent.Render("Global Variables", narrativeSpace.GlobalVariableStore, variableCategory,
                (variableType) =>
                {
                    Undo.RecordObject(narrativeSpace.GlobalVariableStore, $"Add Variable {variableType}");
                    AddVariable(narrativeSpace.GlobalVariableStore, variableType, Variable.VariableCategory.UserDefined);
                },
                (variable) =>
                {
                    Undo.RecordObject(narrativeSpace.GlobalVariableStore, $"Remove Variable {variable.Name}");
                    RemoveVariable(narrativeSpace.GlobalVariableStore, variable);
                },
                (editedVariable) =>
                {
                    Undo.RecordObject(narrativeSpace.GlobalVariableStore, $"Edit Variable {editedVariable.Name}");
                    narrativeSpace.GlobalVariableStore.RefreshDictionary();
                    OnNarrativeObjectEditVariable?.Invoke();
                });

            variablesSectionContainer.styleSheets.Add(StyleSheet);
            variablesSectionContainer.AddToClassList("inspector-section-container");

            container.Add(variablesSectionContainer);

            scrollView.Add(container);
        }

        /// <summary>
        /// Render the settings for a narrative object node on the sidebar.
        /// </summary>
        /// <param name="narrativeObjectNode"></param>
        public void UpdateContentForNarrativeObjectNode(NarrativeObjectNode narrativeObjectNode)
        {
            scrollView.Clear();

            VisualElement container = new VisualElement();

            TextElement title = new TextElement();
            title.name = "title";
            title.text = $"{UIElementsUtils.AddSpacesToCamelCaseString(narrativeObjectNode.NarrativeObject.GetType().Name)} - {narrativeObjectNode.NarrativeObject.gameObject.name}";

            container.Add(title);

            VisualElement settingsSectionContainer = UIElementsUtils.GetContainerWithLabel("Settings");

            List<VisualElement> settingsRows = narrativeObjectNode.GetEditableFieldRows();

            foreach (VisualElement visualElementRow in settingsRows)
            {
                settingsSectionContainer.Add(visualElementRow);
            }

            settingsSectionContainer.styleSheets.Add(StyleSheet);
            settingsSectionContainer.AddToClassList("inspector-section-container");

            // End Triggers
            Dictionary<ProcessingEndTrigger, VisualElement> processingTriggerRows = narrativeObjectNode.GetProcessingTriggerRows();
            VisualElement processingTriggerSectionContainer = GetProcessingTriggersSection(processingTriggerRows, "End Triggers",
                (triggerType) =>
                {
                    Undo.RecordObject(narrativeObjectNode.NarrativeObject, $"Add End Trigger {triggerType}");
                    AddProcessingTrigger(narrativeObjectNode.NarrativeObject, triggerType);
                },
                (trigger) =>
                {
                    Undo.RecordObject(narrativeObjectNode.NarrativeObject, $"Remove End Trigger {trigger.GetType().Name}");
                    RemoveProcessingTrigger(narrativeObjectNode.NarrativeObject, trigger);
                });

            processingTriggerSectionContainer.styleSheets.Add(StyleSheet);
            processingTriggerSectionContainer.AddToClassList("inspector-section-container");

            VisualElement systemTagsSectionContainer = VariableStoreComponent.Render("System Tags", narrativeObjectNode.NarrativeObject.VariableStore, Variable.VariableCategory.SystemDefined,
                (variableType) =>
                {
                    Undo.RecordObject(narrativeObjectNode.NarrativeObject.VariableStore, $"Add Variable {variableType}");
                    AddVariable(narrativeObjectNode.NarrativeObject.VariableStore, variableType, Variable.VariableCategory.SystemDefined);
                },
                (variable) =>
                {
                    Undo.RecordObject(narrativeObjectNode.NarrativeObject.VariableStore, $"Remove Variable {variable.Name}");
                    RemoveVariable(narrativeObjectNode.NarrativeObject.VariableStore, variable);
                },
                (editedVariable) =>
                {
                    Undo.RecordObject(narrativeObjectNode.NarrativeObject.VariableStore, $"Edit Variable {editedVariable.Name}");
                    narrativeObjectNode.NarrativeObject.VariableStore.RefreshDictionary();
                    OnNarrativeObjectEditVariable?.Invoke();
                });

            systemTagsSectionContainer.styleSheets.Add(StyleSheet);
            systemTagsSectionContainer.AddToClassList("inspector-section-container");

            VisualElement tagsSectionContainer = VariableStoreComponent.Render("Tags", narrativeObjectNode.NarrativeObject.VariableStore, Variable.VariableCategory.UserDefined,
                (variableType) =>
                {
                    Undo.RecordObject(narrativeObjectNode.NarrativeObject.VariableStore, $"Add Variable {variableType}");
                    AddVariable(narrativeObjectNode.NarrativeObject.VariableStore, variableType, Variable.VariableCategory.UserDefined);
                },
                (variable) =>
                {
                    Undo.RecordObject(narrativeObjectNode.NarrativeObject.VariableStore, $"Remove Variable {variable.Name}");
                    RemoveVariable(narrativeObjectNode.NarrativeObject.VariableStore, variable);
                },
                (editedVariable) =>
                {
                    Undo.RecordObject(narrativeObjectNode.NarrativeObject.VariableStore, $"Edit Variable {editedVariable.Name}");
                    narrativeObjectNode.NarrativeObject.VariableStore.RefreshDictionary();
                    OnNarrativeObjectEditVariable?.Invoke();
                });

            tagsSectionContainer.styleSheets.Add(StyleSheet);
            tagsSectionContainer.AddToClassList("inspector-section-container");

            // Self Constraints
            VisualElement candidateConstraintsSection = ConstraintsComponent.Render("Self Constraints", narrativeObjectNode.NarrativeObject, narrativeObjectNode.NarrativeObject.constraints, narrativeObjectNode.NarrativeObject.constraintMode,
                (newConstraintMode) =>
                {
                    narrativeObjectNode.NarrativeObject.constraintMode = newConstraintMode;
                },
                (constraintType) =>
                {
                    AddConstraint(narrativeObjectNode.NarrativeObject, constraintType);
                },
                (removedConstraint) =>
                {
                    RemoveConstraint(narrativeObjectNode.NarrativeObject, removedConstraint);
                }, StyleSheet);
            candidateConstraintsSection.styleSheets.Add(StyleSheet);
            candidateConstraintsSection.AddToClassList("inspector-section-container");

            // Output constraints
            VisualElement outputConstraintsSection = ConstraintsComponent.Render("Output Constraints", narrativeObjectNode.NarrativeObject, narrativeObjectNode.NarrativeObject.OutputSelectionDecisionPoint.Constraints, narrativeObjectNode.NarrativeObject.OutputSelectionDecisionPoint.constraintMode,
                (newConstraintMode) =>
                {
                    narrativeObjectNode.NarrativeObject.OutputSelectionDecisionPoint.constraintMode = newConstraintMode;
                },
                (constraintType) =>
                {
                    AddConstraint(narrativeObjectNode.NarrativeObject.OutputSelectionDecisionPoint, constraintType);
                },
                (removedConstraint) =>
                {
                    RemoveConstraint(narrativeObjectNode.NarrativeObject.OutputSelectionDecisionPoint, removedConstraint);
                }, StyleSheet);

            outputConstraintsSection.styleSheets.Add(StyleSheet);
            outputConstraintsSection.AddToClassList("inspector-section-container");

            container.Add(settingsSectionContainer);
            container.Add(UIElementsUtils.GetHorizontalDivider());
            container.Add(processingTriggerSectionContainer);
            container.Add(UIElementsUtils.GetHorizontalDivider());
            container.Add(systemTagsSectionContainer);
            container.Add(UIElementsUtils.GetHorizontalDivider());
            container.Add(tagsSectionContainer);
            container.Add(UIElementsUtils.GetHorizontalDivider());
            container.Add(candidateConstraintsSection);
            container.Add(UIElementsUtils.GetHorizontalDivider());
            container.Add(outputConstraintsSection);

            scrollView.Add(container);
        }

        /// <summary>
        /// Render the settings for an edge on the sidebar.
        /// </summary>
        /// <param name="originNarrativeObjectNode"></param>
        /// <param name="candidateNarrativeObjectNode"></param>
        public void UpdateContentForEdge(NarrativeObjectNode originNarrativeObjectNode, NarrativeObjectNode candidateNarrativeObjectNode)
        {
            scrollView.Clear();

            //VisualElement container = new VisualElement();

            //// Candidate constraints.
            //Dictionary<Constraint, VisualElement> candidateContraintRows = candidateNarrativeObjectNode.GetCandidateConstraintRows();

            //VisualElement candidateConstraintsSection = candidateNarrativeObjectNode.GetConstraintSection(candidateContraintRows, "Candidate Constraints",
            //    (constraintType) =>
            //    {
            //        AddConstraint(candidateNarrativeObjectNode.NarrativeObject, constraintType);
            //    },
            //    (removedConstraint) =>
            //    {
            //        RemoveConstraint(candidateNarrativeObjectNode.NarrativeObject, removedConstraint);
            //    });

            //candidateConstraintsSection.styleSheets.Add(StyleSheet);
            //candidateConstraintsSection.AddToClassList("inspector-section-container");

            //container.Add(candidateConstraintsSection);

            //scrollView.Add(container);
        }

        /// <summary>
        /// Add a variable with the specified type to the specified narrative object.
        /// </summary>
        /// <param name="variableStore"></param>
        /// <param name="variableType"></param>
        private void AddVariable(VariableStore variableStore, VariableFactory.VariableType variableType, Variable.VariableCategory variableCategory)
        {
            if (variableStore != null)
            {
                Variable variable = VariableFactory.AddVariableToVariableStore(variableStore, variableType, variableCategory);
                if (variable != null)
                {
                    OnNarrativeObjectAddedVariable?.Invoke();
                }
            }
        }

        /// <summary>
        /// Remove a variable from a narrative object.
        /// </summary>
        /// <param name="variableStore"></param>
        /// <param name="variable"></param>
        private void RemoveVariable(VariableStore variableStore, Variable variable)
        {
            if (variableStore != null && variableStore.Variables.ContainsKey(variable.Name))
            {
                Variable variableToRemove = variableStore.Variables[variable.Name];
                Undo.RecordObject(variableStore, "Remove variable");
                variableStore.RemoveVariable(variableToRemove);

                UnityEngine.Object.DestroyImmediate(variableToRemove);

                OnNarrativeObjectRemovedVariable?.Invoke();
            }
        }

        /// <summary>
        /// Remove a processing trigger from a narrative object
        /// </summary>
        /// <param name="narrativeObject"></param>
        /// <param name="processingTrigger"></param>
        private void RemoveProcessingTrigger(NarrativeObject narrativeObject, ProcessingEndTrigger processingTrigger)
        {
            Undo.RecordObject(narrativeObject, "Remove End Trigger");

            if (processingTrigger is VariableEndTrigger)
            {
                var variableTrigger = processingTrigger as VariableEndTrigger;
                if (variableTrigger.ValueMatch != null)
                {
                    narrativeObject.VariableStore.RemoveVariable(variableTrigger.ValueMatch);
                }
            }

            narrativeObject.RemoveProcessingTrigger(processingTrigger);

            UnityEngine.Object.DestroyImmediate(processingTrigger);

            OnNarrativeObjectRemovedEndTrigger?.Invoke();
        }

        /// <summary>
        /// Add a processing trigger to a narrative object
        /// </summary>
        /// <param name="narrativeObject"></param>
        /// <param name="triggerType"></param>
        private void AddProcessingTrigger(NarrativeObject narrativeObject, ProcessingEndTriggerFactory.TriggerType triggerType)
        {
            if (narrativeObject != null)
            {
                ProcessingEndTrigger processingTrigger = ProcessingEndTriggerFactory.AddProcessingTriggerToNarrativeObject(narrativeObject, triggerType);
                if (processingTrigger != null)
                {
                    OnNarrativeObjectAddedEndTrigger?.Invoke();
                }
            }
        }

        /// <summary>
        /// Remove a constraint from a narrative object.
        /// </summary>
        /// <param name="narrativeObject"></param>
        /// <param name="constraint"></param>
        private void RemoveConstraint(NarrativeObject narrativeObject, Constraint constraint)
        {
            Undo.RecordObject(narrativeObject, "Remove Constraint");
            narrativeObject.RemoveConstraint(constraint);

            UnityEngine.Object.DestroyImmediate(constraint);

            OnNarrativeObjectRemovedConstraint?.Invoke();
        }

        /// <summary>
        /// Remove a constraint from a decision point.
        /// </summary>
        /// <param name="narrativeObject"></param>
        /// <param name="constraint"></param>
        private void RemoveConstraint(DecisionPoint decisionPoint, Constraint constraint)
        {
            Undo.RecordObject(decisionPoint, "Remove Constraint");
            decisionPoint.RemoveConstraint(constraint);

            UnityEngine.Object.DestroyImmediate(constraint);

            OnNarrativeObjectRemovedConstraint?.Invoke();
        }

        /// <summary>
        /// Add a constraint to a narrative object.
        /// </summary>
        /// <param name="narrativeObject"></param>
        /// <param name="constraintType"></param>
        private void AddConstraint(NarrativeObject narrativeObject, ConstraintFactory.ConstraintType constraintType)
        {
            if (narrativeObject != null)
            {
                Constraint constraint = ConstraintFactory.AddConstraintToNarrativeObject(narrativeObject, constraintType);
                if (constraint != null)
                {
                    OnNarrativeObjectAddedConstraint?.Invoke();
                }
            }
        }

        /// <summary>
        /// Add a constraint to a decision point.
        /// </summary>
        /// <param name="decisionPoint"></param>
        /// <param name="constraintType"></param>
        private void AddConstraint(DecisionPoint decisionPoint, ConstraintFactory.ConstraintType constraintType)
        {
            if (decisionPoint != null)
            {
                Constraint constraint = ConstraintFactory.AddConstraintToDecisionPoint(decisionPoint, constraintType);
                if (constraint != null)
                {
                    OnNarrativeObjectAddedConstraint?.Invoke();
                }
            }
        }

        /// <summary>
        /// Generate a sidebar section for the constraints specified in the passed rows.
        /// </summary>
        /// <param name="constraintVisualElements"></param>
        /// <param name="labelText"></param>
        /// <param name="onClickAddConstraint"></param>
        /// <param name="onClickRemoveConstraint"></param>
        /// <returns></returns>
        private VisualElement GetProcessingTriggersSection(Dictionary<ProcessingEndTrigger, VisualElement> processingTriggerVisualElements, string labelText, Action<ProcessingEndTriggerFactory.TriggerType> onClickAddProcessingTrigger, Action<ProcessingEndTrigger> onClickRemoveProcessingTrigger)
        {
            VisualElement endTriggersSection = UIElementsUtils.GetContainerWithLabel(labelText);

            EnumField triggerTypeEnumField = new EnumField(ProcessingEndTriggerFactory.TriggerType.None);

            VisualElement addTriggerButton = UIElementsUtils.GetSquareButton("+", () =>
            {
                ProcessingEndTriggerFactory.TriggerType triggerType = (ProcessingEndTriggerFactory.TriggerType)triggerTypeEnumField.value;
                if (triggerType != ProcessingEndTriggerFactory.TriggerType.None)
                {
                    onClickAddProcessingTrigger?.Invoke(triggerType);
                }
            });

            VisualElement addEndTriggerControlsContainer = new VisualElement();
            addEndTriggerControlsContainer.AddToClassList("add-trigger-controls-container");
            addEndTriggerControlsContainer.styleSheets.Add(StyleSheet);

            addEndTriggerControlsContainer.Insert(0, addTriggerButton);
            addEndTriggerControlsContainer.Insert(1, triggerTypeEnumField);

            endTriggersSection.Add(addEndTriggerControlsContainer);

            if (processingTriggerVisualElements.Count > 0)
            {
                foreach (KeyValuePair<ProcessingEndTrigger, VisualElement> kvp in processingTriggerVisualElements)
                {
                    VisualElement rowVisualElement = new VisualElement();

                    rowVisualElement.styleSheets.Add(StyleSheet);

                    rowVisualElement.AddToClassList("trigger-row");

                    VisualElement removeTriggerButton = UIElementsUtils.GetSquareButton("-", () =>
                    {
                        onClickRemoveProcessingTrigger?.Invoke(kvp.Key);
                    });

                    // Apply the constraints field container class to the returned visual element.
                    kvp.Value.styleSheets.Add(StyleSheet);
                    kvp.Value.AddToClassList("trigger-fields-container");

                    rowVisualElement.Add(removeTriggerButton);
                    rowVisualElement.Add(kvp.Value);

                    endTriggersSection.Add(rowVisualElement);
                }
            }

            return endTriggersSection;
        }
    }
}