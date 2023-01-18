using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CuttingRoom.Editor
{
    public class EditorToolbar : EditorToobarBase
    {
        /// <summary>
        /// Invoked when the dev toolbar button is clicked.
        /// </summary>
        public event Action OnClickToggleDevToolbar;

        /// <summary>
        /// Invoked when the Add Atomic Narrative Object Node button is clicked.
        /// </summary>
        public event Action OnClickAddAtomicNarrativeObjectNode;

        /// <summary>
        /// Invoked when the Add Graph Narrative Object Node button is clicked.
        /// </summary>
        public event Action OnClickAddGraphNarrativeObjectNode;

        /// <summary>
        /// Invoked when the Add Group Narrative Object Node button is clicked.
        /// </summary>
        public event Action OnClickAddGroupNarrativeObjectNode;

        /// <summary>
        /// Invoked when the Add Layer Narrative Object Node button is clicked.
        /// </summary>
        public event Action OnClickAddLayerNarrativeObjectNode;

        /// <summary>
        /// Invoked when the Global Variables button is clicked.
        /// </summary>
        public event Action OnClickOpenGlobalVariables;

        /// <summary>
        /// The button which creates atomic objects when clicked.
        /// </summary>
        public Button CreateAtomicNarrativeObjectButton { get; private set; } = null;

        /// <summary>
        /// The button which creates graph objects when clicked.
        /// </summary>
        public Button CreateGraphNarrativeObjectButton { get; private set; } = null;

        /// <summary>
        /// The button which creates group objects when clicked.
        /// </summary>
        public Button CreateGroupNarrativeObjectButton { get; private set; } = null;

        /// <summary>
        /// The button which creates layer objects when clicked.
        /// </summary>
        public Button CreateLayerNarrativeObjectButton { get; private set; } = null;

        public EditorToolbar()
        {
            StyleSheet = Resources.Load<StyleSheet>("Toolbars/EditorToolbar");
            styleSheets.Add(StyleSheet);
            name = "editor-toolbar";
            //AddButton(InvokeOnClickToggleDevToolbar, "Show Dev Toolbar");
            Add(UIElementsUtils.GetVerticalDivider());
            Label textElement = new Label();
            textElement.text = "Add Node: ";
            textElement.name = "toolbar-text";
            Add(textElement);
            CreateAtomicNarrativeObjectButton = AddButton(InvokeOnClickAddAtomicNarrativeObjectNode, "Atomic");
            CreateAtomicNarrativeObjectButton.AddToClassList("atomic-node-button");
            CreateGraphNarrativeObjectButton = AddButton(InvokeOnClickAddGraphNarrativeObjectNode, "Graph");
            CreateGraphNarrativeObjectButton.AddToClassList("graph-node-button");
            CreateGroupNarrativeObjectButton = AddButton(InvokeOnClickAddGroupNarrativeObjectNode, "Group");
            CreateGroupNarrativeObjectButton.AddToClassList("group-node-button");
            CreateLayerNarrativeObjectButton = AddButton(InvokeOnClickAddLayerNarrativeObjectNode, "Layer");
            CreateLayerNarrativeObjectButton.AddToClassList("layer-node-button");
            Button globalVariableButton = new Button(() =>
            {
                OnClickOpenGlobalVariables?.Invoke();
            });
            globalVariableButton.text = "Global Variables";
            globalVariableButton.name = "global-variable-button";

            Add(globalVariableButton);
        }

        private void InvokeOnClickToggleDevToolbar()
        {
            OnClickToggleDevToolbar?.Invoke();
        }

        private void InvokeOnClickAddAtomicNarrativeObjectNode()
        {
            OnClickAddAtomicNarrativeObjectNode?.Invoke();
        }

        private void InvokeOnClickAddGraphNarrativeObjectNode()
        {
            OnClickAddGraphNarrativeObjectNode?.Invoke();
        }

        private void InvokeOnClickAddGroupNarrativeObjectNode()
        {
            OnClickAddGroupNarrativeObjectNode?.Invoke();
        }

        private void InvokeOnClickAddLayerNarrativeObjectNode()
        {
            OnClickAddLayerNarrativeObjectNode?.Invoke();
        }


        /// <summary>
        /// Add a button to the toolbar.
        /// </summary>
        /// <param name="onClick"></param>
        /// <param name="text"></param>
        protected Button AddButton(Action onClick, string text)
        {
            Button button = new Button(() =>
            {
                onClick?.Invoke();
            });

            button.text = text;
            button.name = "toolbar-button";

            Add(button);

            return button;
        }
    }
}