using CuttingRoom.VariableSystem;
using CuttingRoom.VariableSystem.Variables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CuttingRoom.Editor
{
    public static class VariableStoreComponent
    {
        /// <summary>
        /// Render a variable stores editor components.
        /// </summary>
        /// <param name="variableStore"></param>
        /// <param name="onVariableAdded"></param>
        /// <param name="onVariableRemoved"></param>
        /// <returns></returns>
        public static VisualElement Render(string labelText, VariableStore variableStore, Variable.VariableCategory variableCategory, Action<VariableFactory.VariableType> onVariableAdded, Action<Variable> onVariableRemoved, Action<Variable> onVariableEdit)
        {
            VisualElement variablesSectionContainer = UIElementsUtils.GetContainerWithLabel(labelText);
            variablesSectionContainer.styleSheets.Add(Resources.Load<StyleSheet>("Editorinspector"));

            bool canAdd = variableCategory != Variable.VariableCategory.SystemDefined;

            VisualElement addVariableRow = UIElementsUtils.GetRowContainer();
            if (canAdd)
            {
                addVariableRow.AddToClassList("add-tag-controls-container");

                EnumField variableTypeEnumField = new EnumField(VariableFactory.VariableType.String);

                VisualElement addVariableButton = UIElementsUtils.GetSquareButton("+", () =>
                {
                    onVariableAdded?.Invoke((VariableFactory.VariableType)variableTypeEnumField.value);
                });

                addVariableRow.Add(addVariableButton);
                addVariableRow.Add(variableTypeEnumField);
            }
            variablesSectionContainer.Add(addVariableRow);

            List<VisualElement> variableRows = GetVariableRows(variableStore, variableCategory, onVariableRemoved, onVariableEdit);

            foreach (VisualElement variableRow in variableRows)
            {
                variablesSectionContainer.Add(variableRow);
            }

            return variablesSectionContainer;
        }

        private static List<VisualElement> GetVariableRows(VariableStore variableStore, Variable.VariableCategory variableCategory, Action<Variable> onVariableRemoved, Action<Variable> onVariableEdit)
        {
            List<VisualElement> variableRows = new List<VisualElement>();
            if (variableStore.Variables.Count > 0)
            {
                foreach (Variable variable in variableStore.GetVariablesOfCategory(variableCategory).Values.ToList())
                {
                    if (variable == null)
                    {
                        continue;
                    }
                    VisualElement rowContainer = UIElementsUtils.GetRowContainer();
                    rowContainer.AddToClassList("tag-row");

                    bool editable = variable.variableCategory == Variable.VariableCategory.UserDefined;

                    if (!editable)
                    {
                        rowContainer.AddToClassList("no-edit-tag");
                    }

                    if (editable)
                    {
                        VisualElement removeVariableButton = UIElementsUtils.GetSquareButton("-", () =>
                        {
                            onVariableRemoved?.Invoke(variable);
                        });
                        rowContainer.Add(removeVariableButton);
                    }

                    VisualElement variableContainer = UIElementsUtils.GetContainer();

                    VisualElement keyRow = UIElementsUtils.GetRowContainer();
                    VisualElement keyLabel = new Label("Key: ");
                    keyLabel.AddToClassList("tag-field-label");
                    keyRow.Add(keyLabel);
                    VisualElement keyField = UIElementsUtils.GetTextField(variable.Name, (variableName) =>
                    {
                        variable.Name = variableName;
                        onVariableEdit?.Invoke(variable);
                    });
                    keyField.AddToClassList("tag-field-value");
                    keyField.SetEnabled(editable);
                    keyRow.Add(keyField);
                    variableContainer.Add(keyRow);


                    VisualElement valRow = UIElementsUtils.GetRowContainer();
                    VisualElement valLabel = new Label($"{variable.GetType().Name.Replace("Variable", "")} Value: ");
                    valLabel.AddToClassList("tag-field-label");
                    VisualElement valueField = null;
                    if (variable is StringVariable)
                    {
                        StringVariable stringVariable = variable as StringVariable;

                        VisualElement textField = UIElementsUtils.GetTextField(stringVariable.Value, (newValue) =>
                        {
                            stringVariable.defaultValue = newValue;
                            stringVariable.Set(newValue);
                            onVariableEdit?.Invoke(variable);
                        });
                        textField.AddToClassList("tag-field-value");

                        valueField = textField;
                    }
                    else if (variable is BoolVariable)
                    {
                        BoolVariable boolVariable = variable as BoolVariable;

                        List<bool> bools = new List<bool>() { true, false };
                        VisualElement boolField = UIElementsUtils.GetPopUpField(boolVariable.Value, bools, (newValue) =>
                        {
                            boolVariable.defaultValue = newValue;
                            boolVariable.Set(newValue);
                            onVariableEdit?.Invoke(variable);
                        });
                        boolField.AddToClassList("tag-field-value");

                        valueField = boolField;
                    }
                    else if (variable is FloatVariable)
                    {
                        FloatVariable floatVariable = variable as FloatVariable;

                        VisualElement floatField = UIElementsUtils.GetFloatField(floatVariable.Value, (newValue) =>
                        {
                            floatVariable.defaultValue = newValue;
                            floatVariable.Set(newValue);
                            onVariableEdit?.Invoke(variable);
                        });
                        floatField.AddToClassList("tag-field-value");

                        valueField = floatField;
                    }
                    else if (variable is IntVariable)
                    {
                        IntVariable intVariable = variable as IntVariable;

                        VisualElement intField = UIElementsUtils.GetIntField(intVariable.Value, (newValue) =>
                        {
                            intVariable.defaultValue = newValue;
                            intVariable.Set(newValue);
                            onVariableEdit?.Invoke(variable);
                        });
                        intField.AddToClassList("tag-field-value");

                        valueField = intField;
                    }
                    valueField.SetEnabled(editable);
                    valRow.Add(valLabel);
                    valRow.Add(valueField);
                    variableContainer.Add(valRow);
                    rowContainer.Add(variableContainer);

                    variableRows.Add(rowContainer);
                }
            }
            return variableRows;
        }
    }
}
