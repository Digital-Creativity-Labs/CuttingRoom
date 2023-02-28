using CuttingRoom.VariableSystem.Constraints;
using CuttingRoom.VariableSystem.Variables;
using CuttingRoom.VariableSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.UIElements;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine;

namespace CuttingRoom.Editor
{
    public static class ConstraintsComponent
    {
        public static VisualElement Render(string labelText, NarrativeObject narrativeObject, List<Constraint> constraints, ConstraintMode constraintMode, Action<ConstraintMode> onConstraintModeChanged, Action<ConstraintFactory.ConstraintType> onClickAddConstraint, Action<Constraint> onClickRemoveConstraint, HashSet<ConstraintFactory.ConstraintType> supportedTypes = null, StyleSheet styleSheet = null)
        {
            if (styleSheet != null)
            {
                styleSheet = Resources.Load<StyleSheet>("NarrativeObjectNode");
            }
            VisualElement constraintsComponent;

            var constraintRows = GetConstraintRows(narrativeObject, ref constraints);
            constraintsComponent = CreateConstraintSection(labelText, constraintRows, constraintMode, onConstraintModeChanged, onClickAddConstraint, onClickRemoveConstraint, supportedTypes);

            constraintsComponent.styleSheets.Add(styleSheet);
            constraintsComponent.AddToClassList("inspector-section-container");
            return constraintsComponent;
        }

        /// <summary>
        /// Get the constraints to be displayed on the blackboard.
        /// </summary>
        /// <returns></returns>
        private static Dictionary<Constraint, VisualElement> GetConstraintRows(NarrativeObject narrativeObject, ref List<Constraint> constraints)
        {
            Dictionary<Constraint, VisualElement> constraintRows = new Dictionary<Constraint, VisualElement>();

            foreach (Constraint constraint in constraints)
            {
                Label constraintRowLabel = new Label();
                constraintRowLabel.AddToClassList("constraint-title");
                EnumField comparisonTypeEnumField = null;

                void SetConstraintRowLabelText()
                {
#if UNITY_2021_1_OR_NEWER
                    string constraintName = $"<{ConstraintFactory.ConstraintToTypeEnum(constraint)}> {constraint.Value} {comparisonTypeEnumField.value} {(constraint.variableName != null ? constraint.variableName : "<color=red>Undefined</color>")}";
#else
                    // No rich text before 2021.1
                    string constraintName = $"<{ConstraintFactory.ConstraintToTypeEnum(constraint)}> {constraint.Value} {comparisonTypeEnumField.value} {(constraint.variableName != null ? constraint.variableName : "Undefined")}";
#endif
                    constraintRowLabel.text = constraintName;
                }

                VisualElement constraintContainer = new VisualElement();

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
                            targetVariableStore = narrativeObject.VariableStore; ;
                            break;

                        default:
                            break;
                    }
                }

                if (narrativeSpace != null && narrativeSpace.UnlockAdvancedFeatures)
                {
                    // Variable Location
                    VisualElement varLocationRow = UIElementsUtils.GetRowContainer();
                    Label varLocationLabel = new Label("Variable Location: ");
                    varLocationLabel.AddToClassList("constraint-field-label");
                    varLocationRow.Add(varLocationLabel);
                    EnumField variableStoreLocationEnumField = new EnumField(constraint.variableStoreLocation);
                    variableStoreLocationEnumField.RegisterValueChangedCallback(evt =>
                    {
                        Undo.RecordObject(constraint, "Set Constraint Variable Location");
                        constraint.variableStoreLocation = (VariableStoreLocation)evt.newValue;

                        SetConstraintRowLabelText();
                        narrativeObject.OnValidate();
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

                // Add a field for the value of the constraint specified.
                VisualElement valueField = GetConstraintValueField(constraint, () =>
                {
                    SetConstraintRowLabelText();
                });
                constraintContainer.Add(valueField);

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
                    narrativeObject.OnValidate();
                });
                variableNameField.AddToClassList("constraint-field-value");
                varNameRow.Add(variableNameField);
                constraintContainer.Add(varNameRow);

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
        private static VisualElement CreateConstraintSection(string labelText, Dictionary<Constraint, VisualElement> constraintVisualElements,
            ConstraintMode constraintMode, Action<ConstraintMode> onConstraintModeChanged,
            Action<ConstraintFactory.ConstraintType> onClickAddConstraint, Action<Constraint> onClickRemoveConstraint, HashSet<ConstraintFactory.ConstraintType> supportedTypes = null)
        {
            VisualElement constraintsSection = UIElementsUtils.GetContainerWithLabel(labelText);
            var constraintTypes = Enum.GetNames(typeof(ConstraintFactory.ConstraintType)).ToList();

            if (supportedTypes != null)
            {
                if (supportedTypes.Count == 0)
                {
                    return null;
                }
                constraintTypes = constraintTypes.Where((constraintType) => 
                supportedTypes.Contains((ConstraintFactory.ConstraintType)Enum.Parse(typeof(ConstraintFactory.ConstraintType), constraintType))).ToList();
            }


            var constraintTypeField = new PopupField<string>(constraintTypes, 0);

            VisualElement addConstraintButton = UIElementsUtils.GetSquareButton("+", () =>
            {
                onClickAddConstraint?.Invoke((ConstraintFactory.ConstraintType)Enum.Parse(typeof(ConstraintFactory.ConstraintType), constraintTypeField.value));
            });

            VisualElement topRow = new();
            topRow.style.width = constraintsSection.style.width;
            topRow.style.flexDirection = FlexDirection.Row;
            topRow.style.justifyContent = Justify.SpaceBetween;

            VisualElement addConstraintControlsContainer = new VisualElement();
            addConstraintControlsContainer.AddToClassList("add-constraint-controls-container");

            addConstraintControlsContainer.Add(addConstraintButton);
            addConstraintControlsContainer.Add(constraintTypeField);

            topRow.Add(addConstraintControlsContainer);

            VisualElement anyOrAllToggle = new();
            var buttonGroup = new RadioButtonGroup("Constraint Mode:");//, new List<string>() { "Valid if ANY", "Valid if ALL" });
            buttonGroup.style.flexDirection = FlexDirection.Row;
            buttonGroup.AddToClassList("constraint-option-pill");
            var allButton = new RadioButton("Valid if ALL");
            allButton.RegisterValueChangedCallback((selectedEvent) =>
            {
                if (selectedEvent.newValue)
                {
                    onConstraintModeChanged(ConstraintMode.ValidIfAll);
                }
            });
            var anyButton = new RadioButton("Valid if ANY");
            anyButton.RegisterValueChangedCallback((selectedEvent) =>
            {
                if (selectedEvent.newValue)
                {
                    onConstraintModeChanged(ConstraintMode.ValidIfAny);
                }
            });
            allButton.SetSelected(constraintMode == ConstraintMode.ValidIfAll);
            anyButton.SetSelected(constraintMode == ConstraintMode.ValidIfAny);
            buttonGroup.Add(allButton);
            buttonGroup.Add(anyButton);
            anyOrAllToggle.Add(buttonGroup);
            topRow.Add(anyOrAllToggle);

            constraintsSection.Add(topRow);

            if (constraintVisualElements.Count > 0)
            {
                foreach (KeyValuePair<Constraint, VisualElement> kvp in constraintVisualElements)
                {
                    VisualElement rowVisualElement = new VisualElement();

                    rowVisualElement.AddToClassList("constraint-row");

                    VisualElement removeConstraintButton = UIElementsUtils.GetSquareButton("-", () =>
                    {
                        onClickRemoveConstraint?.Invoke(kvp.Key);
                    });

                    // Apply the constraints field container class to the returned visual element.
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
        private static T GetConstraint<T>(Constraint constraint) where T : Constraint
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
        private static EnumField GetConstraintComparisonTypeEnumField(Constraint constraint, Action onValueChanged)
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
        private static VisualElement GetConstraintValueField(Constraint constraint, Action onValueChanged)
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

            return row;
        }
    }
}
