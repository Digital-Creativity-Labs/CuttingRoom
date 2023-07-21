using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace CuttingRoom.Editor
{
    public static class WorldSpaceUIButtonInspectorComponent
    {
        public static VisualElement Render(WorldSpaceButtonUIController.UIButtonDefinition uiButton, Action<WorldSpaceButtonUIController.UIButtonDefinition> onEdit)
        {
            VisualElement buttonElement = UIElementsUtils.GetContainer();
            buttonElement.styleSheets.Add(Resources.Load<StyleSheet>("UIButtonInspector"));

            // Button Text
            VisualElement textRow = UIElementsUtils.GetRowContainer();
            VisualElement textTag = new Label("Button Text");
            textTag.AddToClassList("button-field-label");
            VisualElement textValue = UIElementsUtils.GetTextField(uiButton.text, (newValue) =>
            {
                uiButton.text = newValue;
                onEdit.Invoke(uiButton);
            });
            textValue.AddToClassList("button-field-value");
            textRow.Add(textTag);
            textRow.Add(textValue);
            buttonElement.Add(textRow);

            // Button Value
            VisualElement valueRow = UIElementsUtils.GetRowContainer();
            VisualElement valueTag = new Label("Button Value");
            valueTag.AddToClassList("button-field-label");
            VisualElement valueValue = UIElementsUtils.GetTextField(uiButton.value, (newValue) =>
            {
                uiButton.value = newValue;
                onEdit.Invoke(uiButton);
            });
            valueValue.AddToClassList("button-field-value");
            valueRow.Add(valueTag);
            valueRow.Add(valueValue);
            buttonElement.Add(valueRow);

            return buttonElement;
        }
    }
}
