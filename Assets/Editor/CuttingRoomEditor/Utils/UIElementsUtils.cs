using CuttingRoom.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public static class UIElementsUtils
{
    /// <summary>
    /// Get a visual element for a horizontal divider.
    /// </summary>
    /// <returns></returns>
    public static VisualElement GetHorizontalDivider()
    {
        VisualElement horizontalDivider = GetDivider("horizontal");

        // The .horizontal class isnt built into Unity like .horizontal so get it from stylesheet and apply.
        StyleSheet styleSheet = Resources.Load<StyleSheet>("Divider");
        horizontalDivider.styleSheets.Add(styleSheet);

        return horizontalDivider;
    }

    /// <summary>
    /// Get a visual element for a vertical divider.
    /// </summary>
    /// <returns></returns>
    public static VisualElement GetVerticalDivider()
    {
        VisualElement verticalDivider = GetDivider("vertical");

        // The .vertical class isnt built into Unity like .horizontal so get it from stylesheet and apply.
        StyleSheet styleSheet = Resources.Load<StyleSheet>("Divider");
        verticalDivider.styleSheets.Add(styleSheet);

        return verticalDivider;
    }

    /// <summary>
    /// Get a divider with the specified orientation class added.
    /// </summary>
    /// <param name="orientationClass"></param>
    /// <returns></returns>
    private static VisualElement GetDivider(string orientationClass)
    {
        VisualElement divider = new VisualElement();
        divider.AddToClassList(orientationClass);

        return divider;
    }

    /// <summary>
    /// Get an empty container visual element with a label at the top.
    /// </summary>
    /// <param name="labelText"></param>
    /// <returns></returns>
    public static VisualElement GetContainerWithLabel(string labelText)
    {
        VisualElement container = GetContainer();

        VisualElement labelContainer = GetLabelContainer();

        Label label = new Label(labelText);

        labelContainer.Add(new Label(labelText));

        container.Add(labelContainer);

        return container;
    }

    /// <summary>
    /// Get a container with a row flex direction.
    /// </summary>
    /// <returns></returns>
    public static VisualElement GetRowContainer()
    {
        VisualElement container = new VisualElement();

        StyleSheet styleSheet = Resources.Load<StyleSheet>("Row");
        container.styleSheets.Add(styleSheet);

        container.AddToClassList("row");

        return container;
    }

    /// <summary>
    /// Get a text field.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="onValueChanged"></param>
    /// <returns></returns>
    public static VisualElement GetTextField(string value, Action<string> onValueChanged)
    {
        TextField textField = new TextField();
        textField.isDelayed = true;
        textField.value = value;
        textField.RegisterValueChangedCallback(evt =>
        {
            onValueChanged?.Invoke(evt.newValue);
        });

        StyleSheet styleSheet = Resources.Load<StyleSheet>("TextField");
        textField.styleSheets.Add(styleSheet);
        textField.AddToClassList("text-field");

        return textField;
    }

    public static VisualElement GetBoolField(bool value, Action<bool> onValueChanged)
    {
        Toggle toggle = new Toggle();
        toggle.value = value;
        toggle.RegisterValueChangedCallback(evt =>
        {
            onValueChanged?.Invoke(evt.newValue);
        });

        StyleSheet styleSheet = Resources.Load<StyleSheet>("BoolField");
        toggle.styleSheets.Add(styleSheet);
        toggle.AddToClassList("bool-field");

        return toggle;
    }

    public static VisualElement GetFloatField(float value, Action<float> onValueChanged)
    {
        FloatField floatField = new FloatField();
        floatField.value = value;
        floatField.RegisterValueChangedCallback(evt =>
        {
            onValueChanged?.Invoke(evt.newValue);
        });

        StyleSheet styleSheet = Resources.Load<StyleSheet>("FloatField");
        floatField.styleSheets.Add(styleSheet);
        floatField.AddToClassList("float-field");

        return floatField;
    }

    public static VisualElement GetIntField(int value, Action<int> onValueChanged)
    {
        IntegerField intField = new IntegerField();
        intField.value = value;
        intField.RegisterValueChangedCallback(evt =>
        {
            onValueChanged?.Invoke(evt.newValue);
        });

        StyleSheet styleSheet = Resources.Load<StyleSheet>("IntField");
        intField.styleSheets.Add(styleSheet);
        intField.AddToClassList("int-field");

        return intField;
    }

    public static VisualElement GetEnumField<T>(T value, Action<Enum> onValueChanged)
    {
        if (typeof(T).IsEnum)
        {
            Enum enumValue = value as Enum;
            if (enumValue != null)
            {
                EnumField enumField = new EnumField(enumValue);
                enumField.value = enumValue;
                enumField.RegisterValueChangedCallback(evt =>
                {
                    onValueChanged?.Invoke(evt.newValue);
                });

                StyleSheet styleSheet = Resources.Load<StyleSheet>("EnumField");
                enumField.styleSheets.Add(styleSheet);
                enumField.AddToClassList("enum-field");

                return enumField;
            }
        }

        return null;
    }

    public static VisualElement GetPopUpField<T>(T value, List<T> choices, Action<T> onValueChanged)
    {
        PopupField<T> popUpField = new PopupField<T>(choices, 0);
        popUpField.value = value;
        popUpField.RegisterValueChangedCallback(evt =>
        {
            onValueChanged?.Invoke(evt.newValue);
        });

        StyleSheet styleSheet = Resources.Load<StyleSheet>("PopUpField");
        popUpField.styleSheets.Add(styleSheet);
        popUpField.AddToClassList("pop-up-field");

        return popUpField;
    }

    public static VisualElement GetObjectField<T>(T value, Action<T> onValueChanged) where T : UnityEngine.Object
    {
        ObjectField objectField = new ObjectField();
        objectField.objectType = typeof(T);
        objectField.value = value;
        objectField.RegisterValueChangedCallback(evt =>
        {
            onValueChanged?.Invoke((T)evt.newValue);
        });

        StyleSheet styleSheet = Resources.Load<StyleSheet>("ObjectField");
        objectField.styleSheets.Add(styleSheet);
        objectField.AddToClassList("object-field");

        return objectField;
    }

    /// <summary>
    /// Get a standard container.
    /// </summary>
    /// <returns></returns>
    public static VisualElement GetContainer()
    {
        VisualElement container = new VisualElement();

        StyleSheet styleSheet = Resources.Load<StyleSheet>("Container");

        container.styleSheets.Add(styleSheet);
        container.AddToClassList("container");

        return container;
    }

    /// <summary>
    /// Get a standard container for a label.
    /// </summary>
    /// <returns></returns>
    private static VisualElement GetLabelContainer()
    {
        VisualElement labelContainer = new VisualElement();

        StyleSheet styleSheet = Resources.Load<StyleSheet>("LabelContainer");

        labelContainer.styleSheets.Add(styleSheet);
        labelContainer.AddToClassList("label-container");

        return labelContainer;
    }

    /// <summary>
    /// Get a small square button.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="onClick"></param>
    /// <returns></returns>
    public static VisualElement GetSquareButton(string text, Action onClick)
    {
        Button squareButton = new Button(() =>
        {
            onClick?.Invoke();
        });

        StyleSheet styleSheet = Resources.Load<StyleSheet>("SquareButton");

        squareButton.text = text;

        squareButton.styleSheets.Add(styleSheet);
        squareButton.AddToClassList("square-button");

        return squareButton;
    }

    /// <summary>
    /// Create a blackboard row with a text field.
    /// </summary>
    /// <param name="labelText"></param>
    /// <param name="value"></param>
    /// <param name="OnValueChanged"></param>
    /// <returns></returns>
    public static VisualElement CreateTextFieldRow(string labelText, string value, Action<string> OnValueChanged)
    {
        TextField textField = new TextField();
        textField.isDelayed = true;
        textField.value = value;
        textField.RegisterValueChangedCallback(evt =>
        {
            OnValueChanged?.Invoke(evt.newValue);
        });

        BlackboardRow blackboardRow = new BlackboardRow(new Label(labelText), textField);

        blackboardRow.expanded = true;

        return blackboardRow;
    }

    /// <summary>
    /// Create a blackboard row with an object field.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="labelText"></param>
    /// <param name="value"></param>
    /// <param name="OnValueChanged"></param>
    /// <returns></returns>
    public static VisualElement CreateObjectFieldRow<T>(string labelText, T value, Action<T> OnValueChanged) where T : UnityEngine.Object
    {
        VisualElement objectField = GetObjectField<T>(value, OnValueChanged);

        BlackboardRow blackboardRow = new BlackboardRow(new Label(labelText), objectField);

        blackboardRow.expanded = true;

        return blackboardRow;
    }

    /// <summary>
    /// Create a blackboard row with an int field.
    /// </summary>
    /// <param name="labelText"></param>
    /// <param name="value"></param>
    /// <param name="OnValueChanged"></param>
    /// <returns></returns>
    public static VisualElement CreateIntegerFieldRow(string labelText, int value, Action<int> OnValueChanged)
    {
        IntegerField intField = new IntegerField();
        intField.isDelayed = true;
        intField.value = value;
        intField.RegisterValueChangedCallback(evt =>
        {
            OnValueChanged?.Invoke(evt.newValue);
        });

        BlackboardRow blackboardRow = new BlackboardRow(new Label(labelText), intField);

        blackboardRow.expanded = true;

        return blackboardRow;
    }

    /// <summary>
    /// Create a blackboard row with a float field.
    /// </summary>
    /// <param name="labelText"></param>
    /// <param name="value"></param>
    /// <param name="OnValueChanged"></param>
    /// <returns></returns>
    public static VisualElement CreateFloatFieldRow(string labelText, float value, Action<float> OnValueChanged)
    {
        FloatField floatField = new FloatField();
        floatField.isDelayed = true;
        floatField.value = value;
        floatField.RegisterValueChangedCallback(evt =>
        {
            OnValueChanged?.Invoke(evt.newValue);
        });

        BlackboardRow blackboardRow = new BlackboardRow(new Label(labelText), floatField);

        blackboardRow.expanded = true;

        return blackboardRow;
    }

    /// <summary>
    /// Create a blackboard row with a bool field.
    /// </summary>
    /// <param name="labelText"></param>
    /// <param name="value"></param>
    /// <param name="OnValueChanged"></param>
    /// <returns></returns>
    public static VisualElement CreateBoolFieldRow(string labelText, bool value, Action<bool> OnValueChanged)
    {
        Toggle boolField = new Toggle();
        boolField.value = value;
        boolField.RegisterValueChangedCallback(evt =>
        {
            OnValueChanged?.Invoke(evt.newValue);
        });

        BlackboardRow blackboardRow = new BlackboardRow(new Label(labelText), boolField);

        blackboardRow.expanded = true;

        return blackboardRow;
    }

    /// <summary>
    /// Create a blackboard row with an enum field.
    /// </summary>
    /// <param name="labelText"></param>
    /// <param name="value"></param>
    /// <param name="OnValueChanged"></param>
    /// <returns></returns>
    public static VisualElement CreateEnumFieldRow<T>(string labelText, T value, Action<Enum> OnValueChanged)
    {
        if (typeof(T).IsEnum)
        {
            Enum enumValue = value as Enum;
            if (enumValue != null)
            {
                EnumField enumField = new EnumField(enumValue);
                enumField.value = enumValue;
                enumField.RegisterValueChangedCallback(evt =>
                {
                    OnValueChanged?.Invoke(evt.newValue);
                });

                BlackboardRow blackboardRow = new BlackboardRow(new Label(labelText), enumField);

                blackboardRow.expanded = true;

                return blackboardRow;
            }
        }

        return null;
    }

    /// <summary>
    /// Create a blackboard row with an enum field.
    /// </summary>
    /// <param name="labelText"></param>
    /// <param name="value"></param>
    /// <param name="choices"></param>
    /// <param name="OnValueChanged"></param>
    /// <returns></returns>
    public static VisualElement CreatePopUpFieldRow<T>(string labelText, T value, List<T> choices, Action<T> OnValueChanged)
    {
        PopupField<T> popUpField = new PopupField<T>(choices, 0);
        popUpField.value = value;
        popUpField.RegisterValueChangedCallback(evt =>
        {
            OnValueChanged?.Invoke(evt.newValue);
        });
        BlackboardRow blackboardRow = new BlackboardRow(new Label(labelText), popUpField);
        blackboardRow.expanded = true;

        return blackboardRow;
    }

    /// <summary>
    /// Create a blackboard row with an enum field.
    /// </summary>
    /// <param name="labelText"></param>
    /// <param name="value"></param>
    /// <param name="OnValueChanged"></param>
    /// <returns></returns>
    public static VisualElement CreateColorFieldRow(string labelText, Color value, Action<Color> OnValueChanged)
    {
        ColorField colorField = new ColorField();
        colorField.value = value;
        colorField.RegisterValueChangedCallback(evt =>
        {
            OnValueChanged?.Invoke(evt.newValue);
        });
        BlackboardRow blackboardRow = new BlackboardRow(new Label(labelText), colorField);
        blackboardRow.expanded = true;

        return blackboardRow;
    }

    /// <summary>
    /// Create a blackboard row with a typed list field.
    /// </summary>
    /// <param name="labelText"></param>
    /// <param name="values"></param>
    /// <param name="onListChanged"></param>
    /// <returns></returns>
    public static VisualElement CreateListFieldRow<T>(string labelText, List<T> values, Action<List<T>> onListChanged) where T : new()
    {
        VisualElement labelWithAdd = GetRowContainer();
        labelWithAdd.Add(new Label(labelText));
        labelWithAdd.Add(GetSquareButton("+", () =>
        {
            values.Add(new T());

            onListChanged?.Invoke(values);
        }));

        VisualElement elements = GetContainer();

        Type elementType = typeof(T);
        int index = 0;
        foreach (var val in values)
        {
            int valIndex = index;

            VisualElement elementRow = GetRowContainer();
            VisualElement removeElementButton = GetSquareButton("-", () =>
            {
                values.RemoveAt(valIndex);

                onListChanged?.Invoke(values);
            });

            VisualElement valueField = null;
            if (elementType == typeof(bool))
            {
                bool valCache = Convert.ToBoolean(val);
                valueField = GetPopUpField(valCache, new List<bool> { true, false }, (newValue) =>
                {
                    values[valIndex] = (T)(object)newValue;
                    onListChanged?.Invoke(values);
                });
            }
            else if (elementType == typeof(int))
            {
                int valCache = Convert.ToInt32(val);
                valueField = GetIntField(valCache, (newValue) =>
                {
                    values[valIndex] = (T)(object)newValue;
                    onListChanged?.Invoke(values);
                });
            }
            else if (elementType == typeof(float))
            {
                float valCache = Convert.ToSingle(val);
                valueField = GetFloatField(valCache, (newValue) =>
                {
                    values[valIndex] = (T)(object)newValue;
                    onListChanged?.Invoke(values);
                });
            }
            else if (elementType == typeof(UnityEngine.Object))
            {
                UnityEngine.Object valCache = (UnityEngine.Object)(object)val;
                valueField = GetObjectField(valCache, (newValue) =>
                {
                    values[valIndex] = (T)(object)newValue;
                    onListChanged?.Invoke(values);
                });
            }

            if (valueField == null)
            {
                break;
            }
            else
            {
                elementRow.Add(removeElementButton);
                elementRow.Add(valueField);
                elements.Add(elementRow);
            }
            ++index;
        }

        BlackboardRow blackboardRow = new BlackboardRow(labelWithAdd, elements);
        blackboardRow.expanded = true;

        return blackboardRow;
    }



    /// <summary>
    /// Create a blackboard row with a list field with custom visual elements per element.
    /// </summary>
    /// <param name="labelText"></param>
    /// <param name="values"></param>
    /// <param name="onListChanged"></param>
    /// <returns></returns>
    public static VisualElement CreateCustomListFieldRow<T>(string labelText, List<T> values, Action<List<T>> onListChanged, Func<T, Action<T>, VisualElement> renderElement, StyleSheet elementStyleSheet = null, string elementStyleClass = null) where T : VisualElement, new()
    {
        VisualElement labelWithAdd = GetRowContainer();
        labelWithAdd.Add(new Label(labelText));
        labelWithAdd.Add(GetSquareButton("+", () =>
        {
            values.Add(new T());

            onListChanged?.Invoke(values);
        }));

        VisualElement elements = GetContainer();


        int index = 0;
        foreach (var val in values)
        {
            int valIndex = index;

            VisualElement elementRow = GetRowContainer();

            if (elementStyleSheet != null)
            {
                elementRow.styleSheets.Add(elementStyleSheet);
                elementRow.AddToClassList(elementStyleClass);
            }

            VisualElement removeElementButton = GetSquareButton("-", () =>
            {
                values.RemoveAt(valIndex);

                onListChanged?.Invoke(values);
            });

            VisualElement valueElement = renderElement(val, (editedElement) =>
            {
                values[valIndex] = editedElement;

                onListChanged?.Invoke(values);
            });

            if (valueElement == null)
            {
                break;
            }
            else
            {
                elementRow.Add(removeElementButton);
                elementRow.Add(valueElement);
                elements.Add(elementRow);
            }
            ++index;
        }

        BlackboardRow blackboardRow = new BlackboardRow(labelWithAdd, elements);
        blackboardRow.expanded = true;

        return blackboardRow;
    }

    public static string AddSpacesToCamelCaseString(string camelCaseString)
    {
        return Regex.Replace(camelCaseString, @"(\B[A-Z]+?(?=[A-Z][^A-Z])|\B[A-Z]+?(?=[^A-Z]))", " $1");
    }
}
