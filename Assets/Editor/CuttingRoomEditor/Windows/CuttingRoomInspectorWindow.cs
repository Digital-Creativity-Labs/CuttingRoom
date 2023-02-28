using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace CuttingRoom.Editor
{
    public class CuttingRoomInspectorWindow : EditorWindow
    {
        /// <summary>
        /// Reference to the open cutting room editor window if it exists.
        /// </summary>
        public CuttingRoomEditorWindow CREditorWindow { get; set; } = null;

        /// <summary>
        /// Boolean flag indicating if the CR Editor Window is connected.
        /// </summary>
        public bool CREditorConnected { get; set; } = false;

        /// <summary>
        /// Inspector visual element.
        /// </summary>
        public EditorInspector Inspector = null;

        /// <summary>
        /// Invoked whenever a end trigger is added.
        /// </summary>
        public Action OnAddEndTrigger;

        /// <summary>
        /// Invoked whenever a end trigger is removed.
        /// </summary>
        public Action OnRemoveEndTrigger;

        /// <summary>
        /// Invoked whenever a constraint is added.
        /// </summary>
        public Action OnAddConstraint;

        /// <summary>
        /// Invoked whenever a constraint is removed.
        /// </summary>
        public Action OnRemoveConstraint;

        private enum SelectionSource
        {
            None,
            Editor,
            Hierarchy
        }
        SelectionSource selectionSource = SelectionSource.Editor;

        /// <summary>
        /// Menu option to open editor window.
        /// </summary>
        [MenuItem("Cutting Room/Inspector")]
        public static void OpenInspector()
        {
            CreateCuttingRoomInspectorWindow();
        }

        /// <summary>
        /// Create a new instance of the editor window.
        /// </summary>
        /// <returns></returns>
        public static CuttingRoomInspectorWindow CreateCuttingRoomInspectorWindow()
        {
            CuttingRoomInspectorWindow window = GetWindow<CuttingRoomInspectorWindow>(typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.InspectorWindow"));
            window.titleContent = new GUIContent(text: "Cutting Room Inspector");

            return window;
        }

        /// <summary>
        /// Unity calls the CreateGUI method automatically when the window needs to display
        /// </summary>
        public void OnCreateGUI()
        {
            CheckForEditorWindow();
            RegenerateContents();
        }

        /// <summary>
        /// Ensure this window is connected to necessary windows in Cutting Room ecosystem.
        /// </summary>
        public void ConnectEditorWindow(CuttingRoomEditorWindow editorWindow)
        {
            CREditorWindow = editorWindow;
            CREditorConnected = true;

            CREditorWindow.OnDelete -= RegenerateContents;
            CREditorWindow.OnDelete += RegenerateContents;

            CREditorWindow.OnSelect -= OnEditorSelection;
            CREditorWindow.OnSelect += OnEditorSelection;

            CREditorWindow.OnDeselect -= OnEditorSelection;
            CREditorWindow.OnDeselect += OnEditorSelection;

            CREditorWindow.OnSelectionCleared -= OnEditorSelection;
            CREditorWindow.OnSelectionCleared += OnEditorSelection;

            Selection.selectionChanged -= OnHierarchySelection;
            Selection.selectionChanged += OnHierarchySelection;

            EditorApplication.playModeStateChanged -= HandlePlayModeChange;
            EditorApplication.playModeStateChanged += HandlePlayModeChange;

            Undo.undoRedoPerformed -= RegenerateContents;
            Undo.undoRedoPerformed += RegenerateContents;

            RegenerateContents();
        }

        private void HandlePlayModeChange(PlayModeStateChange playModeStateChange)
        {
            // Refresh connection on play mode change to ensure connection is valid
            Inspector.ClearContent();
            DisconnectEditorWindow();
        }

        /// <summary>
        /// Ensure this window is disconnected from other windows in Cutting Room ecosystem.
        /// </summary>
        public void DisconnectEditorWindow()
        {
            CREditorConnected = false;
            if (CREditorWindow != null)
            {
                CREditorWindow.OnDelete -= RegenerateContents;
                CREditorWindow.OnSelect -= OnEditorSelection;
                CREditorWindow.OnDeselect -= OnEditorSelection;
                CREditorWindow.OnSelectionCleared -= OnEditorSelection;
                Selection.selectionChanged -= OnHierarchySelection;
                EditorApplication.playModeStateChanged -= HandlePlayModeChange;
                Undo.undoRedoPerformed -= RegenerateContents;
                CREditorWindow = null;
            }
            RegenerateContents();
        }

        public void Initialise()
        {
            if (Inspector == null)
            {
                Inspector = new EditorInspector();

                Inspector.OnNarrativeObjectAddedEndTrigger += OnNarrativeObjectAddedEndTrigger;
                Inspector.OnNarrativeObjectRemovedEndTrigger += OnNarrativeObjectRemovedEndTrigger;

                Inspector.OnNarrativeObjectAddedConstraint += OnNarrativeObjectAddedConstraint;
                Inspector.OnNarrativeObjectRemovedConstraint += OnNarrativeObjectRemovedConstraint;

                Inspector.OnNarrativeObjectAddedVariable += OnNarrativeObjectAddedVariable;
                Inspector.OnNarrativeObjectRemovedVariable += OnNarrativeObjectRemovedVariable;
                Inspector.OnNarrativeObjectEditVariable += OnNarrativeObjectEditVariable;
            }
        }

        /// <summary>
        /// Unity event invoked whenever this window is opened.
        /// </summary>
        private void OnInspectorUpdate()
        {
            CheckForEditorWindow();
        }

        /// <summary>
        /// Used to monitor for open Cutting Room Editor Window.
        /// If Open connect to it, if closed disconnect.
        /// </summary>
        private void CheckForEditorWindow()
        {
            bool crEditorOpen = HasOpenInstances<CuttingRoomEditorWindow>();
            if (CREditorConnected && !crEditorOpen)
            {
                DisconnectEditorWindow();
            }
            else if (!CREditorConnected)
            {
                if (crEditorOpen)
                {
                    CuttingRoomEditorWindow cuttingRoomEditorWindow;
                    if (EditorApplication.isPlaying)
                    {
                        cuttingRoomEditorWindow = EditorWindowUtils.GetWindowIfOpen<CuttingRoomEditorWindow>(focus: false);
                    }
                    else
                    {
                        cuttingRoomEditorWindow = EditorWindowUtils.GetWindowIfOpen<CuttingRoomEditorWindow>(focus: true);
                    }

                    if (cuttingRoomEditorWindow != null)
                    {
                        ConnectEditorWindow(cuttingRoomEditorWindow);
                    }
                }
            }
        }

        /// <summary>
        /// Unity event invoked whenever this window is closed.
        /// </summary>
        private void OnDisable()
        {
            DisconnectEditorWindow();
        }

        private void OnEditorSelection()
        {
            selectionSource = SelectionSource.Editor;
            RegenerateContents();
            selectionSource = SelectionSource.None;
        }

        private void OnHierarchySelection()
        {
            selectionSource = SelectionSource.Hierarchy;
            RegenerateContents();
            selectionSource = SelectionSource.None;
        }

        private void RegenerateContents()
        {
            Initialise();

            if (CREditorWindow == null)
            {
                // If the editor window doesnt exist, then show nothing.
                Inspector.ClearContent();
                return;
            }

            AddInspector();

            AddObjectControls();
        }

        private void AddInspector()
        {
            rootVisualElement.Add(Inspector);
        }

        private void AddObjectControls()
        {
            List<ISelectable> selected = new List<ISelectable>();

            // When opening Unity, the window comes into existence without the editor window having initialised.
            // In this case, the "selected" list is going to be empty, rendering global settings (no editor selection).
            if (CREditorWindow != null && CREditorWindow.GraphView != null)
            {
                if (selectionSource == SelectionSource.Hierarchy)
                {
                    GameObject selectedGameObject = Selection.activeGameObject;
                    if (selectedGameObject != null && selectedGameObject.TryGetComponent(out NarrativeObject narrativeObject))
                    {
                        if (narrativeObject != null)
                        {
                            NarrativeObjectNode narrativeObjectNode = CREditorWindow.GraphView.GetNarrativeObjectNode(narrativeObject);
                            if (narrativeObjectNode != null)
                            {
                                selected.Add(narrativeObjectNode);
                            }
                        }
                    }
                }
                else
                {
                    selected = CREditorWindow.GraphView.selected;
                }
            }

            // Gets or creates narrative space.
            NarrativeSpace narrativeSpace = CuttingRoomEditorUtils.GetOrCreateNarrativeSpace();

            if (selected.Count == 1)
            {
                if (selected[0] is NarrativeObjectNode)
                {
                    NarrativeObjectNode narrativeObjectNode = selected[0] as NarrativeObjectNode;

                    // When deleting a node, the narrative object will be null but the selection persists, so check this.
                    if (narrativeObjectNode.NarrativeObject == null)
                    {
                        Action onGlobalChange = () => { Inspector.UpdateContentForGlobal(narrativeSpace); };
                        narrativeSpace.OnChanged -= onGlobalChange;
                        narrativeSpace.OnChanged += onGlobalChange;
                        // If the gameobject doesnt exist, then show global settings.
                        Inspector.UpdateContentForGlobal(narrativeSpace);
                    }
                    else
                    {
                        // Add callback to refresh inspector content on object changes.
                        Action onNarrativeObjectChanged = () => { Inspector.UpdateContentForNarrativeObjectNode(narrativeObjectNode); };
                        narrativeObjectNode.NarrativeObject.OnChanged -= onNarrativeObjectChanged;
                        narrativeObjectNode.NarrativeObject.OnChanged += onNarrativeObjectChanged;
                        Inspector.UpdateContentForNarrativeObjectNode(narrativeObjectNode);
                    }
                }
                else if (selected[0] is Edge)
                {
                    Edge edge = selected[0] as Edge;

                    NarrativeObjectNode outputNarrativeObjectNode = CREditorWindow.GraphView.GetNarrativeObjectNodeWithPort(edge.output);
                    NarrativeObjectNode inputNarrativeObjectNode = CREditorWindow.GraphView.GetNarrativeObjectNodeWithPort(edge.input);

                    Inspector.UpdateContentForEdge(outputNarrativeObjectNode, inputNarrativeObjectNode);
                }
            }
            // Do not update to global content if the selection change is from the Hierarchy. Hierarchy cleared selections happen when edges are selected.
            else if (selectionSource == SelectionSource.Editor)
            {
                // No selection so show variables for global things.
                Inspector.UpdateContentForGlobal(narrativeSpace);
            }
        }

        private void OnNarrativeObjectAddedEndTrigger()
        {
            OnAddEndTrigger?.Invoke();

            RegenerateContents();
        }

        private void OnNarrativeObjectRemovedEndTrigger()
        {
            OnRemoveEndTrigger?.Invoke();

            RegenerateContents();
        }

        private void OnNarrativeObjectAddedConstraint()
        {
            OnAddConstraint?.Invoke();

            RegenerateContents();
        }

        private void OnNarrativeObjectRemovedConstraint()
        {
            OnRemoveConstraint?.Invoke();

            RegenerateContents();
        }

        private void OnNarrativeObjectAddedVariable()
        {
            RegenerateContents();
        }

        private void OnNarrativeObjectRemovedVariable()
        {
            RegenerateContents();
        }

        private void OnNarrativeObjectEditVariable()
        {
            RegenerateContents();
        }
    }
}
