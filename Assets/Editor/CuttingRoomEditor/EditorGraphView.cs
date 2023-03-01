using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using System;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using Newtonsoft.Json;
using UnityEngine.UI;

namespace CuttingRoom.Editor
{
    public class EditorGraphView : GraphView
    {
        /// <summary>
        /// The nodes currently in the graph.
        /// </summary>
        public Dictionary<string, NarrativeObjectNode> NarrativeObjectNodes { get; private set; } = new();

        public Dictionary<string, NarrativeObjectNode> CopiedNodes { get; private set; } = new();
        public Dictionary<string, NarrativeObjectNode> CutNodes { get; private set; } = new();
        public Dictionary<string, NarrativeObjectNode> DeleteNodes { get; private set; } = new();

        /// <summary>
        /// Edges currently in the graph and their info.
        /// </summary>
        private List<EdgeState> EdgeStates { get; set; } = new List<EdgeState>();

        /// <summary>
        /// Invoked whenever the OnGraphViewChangeEvent method is invoked.
        /// </summary>
        public event Action<GraphViewChange> OnGraphViewChanged;

        /// <summary>
        /// Invoked when paste operation is intiated.
        /// </summary>
        public event Action<string> OnPaste;

        /// <summary>
        /// Invoked when paste operation is completed.
        /// </summary>
        public event Action OnPasteComplete;

        /// <summary>
        /// Invoked whenever the view container changes.
        /// </summary>
        public event Action OnViewContainerPushed;

        /// <summary>
        /// Invoked whenever a view containers root narrative object changes.
        /// </summary>
        public event Action OnRootNarrativeObjectChanged;

        /// <summary>
        /// Invoked whenever the candidates of the visible view container change.
        /// </summary>
        public event Action OnNarrativeObjectCandidatesChanged;

        /// <summary>
        /// Invoked whenever a narrative object node is selected on the graph view.
        /// </summary>
        public event Action OnNarrativeObjectNodeSelected;

        /// <summary>
        /// Invoked whenever a narrative object node is deselected on the graph view.
        /// </summary>
        public event Action OnNarrativeObjectNodeDeselected;

        /// <summary>
        /// Event invoked whenever an edge is selected on the graph view.
        /// </summary>
        public event Action OnEdgeSelected;

        /// <summary>
        /// Event invoked whenever an edge is selected on the graph view.
        /// </summary>
        public event Action OnEdgeDeselected;

        /// <summary>
        /// Invoked whenever the selection on the narrative graph is cleared completely.
        /// </summary>
        public event Action OnClearSelection;

        /// <summary>
        /// List of selected graph view elements.
        /// </summary>
        public List<ISelectable> selected = new List<ISelectable>();

        /// <summary>
        /// The supported node types in this graph view.
        /// </summary>
        private enum NodeType
        {
            NarrativeObject,
        }

        /// <summary>
        /// The supported port types in this graph view.
        /// </summary>
        private enum PortType
        {
            Input,
            Output,
        }

        private Scene? ActiveScene = null;

        /// <summary>
        /// The guid representing the container which is the narrative space.
        /// This container has no associated narrative object.
        /// </summary>
        public const string rootViewContainerGuid = "0";

        /// <summary>
        /// View stack for rendering the recursive layers of the narrative space.
        /// </summary>
        private Stack<ViewContainer> viewContainerStack = new Stack<ViewContainer>();

        /// <summary>
        /// The view container stack.
        /// </summary>
        public Stack<ViewContainer> ViewContainerStack { get { return viewContainerStack; } }

        /// <summary>
        /// Get the narrative object which is the currently visible view container.
        /// </summary>
        public NarrativeObject VisibleViewContainerNarrativeObject 
        { 
            get 
            {
                if (viewContainerStack == null || viewContainerStack.Count == 0)
                {
                    return null;
                }

                ViewContainer viewContainer = viewContainerStack.Peek();

                // If the root view container, return null.
                if (viewContainer.narrativeObjectGuid == rootViewContainerGuid)
                {
                    return null;
                }

                NarrativeObject narrativeObject = GetNarrativeObject(viewContainer.narrativeObjectGuid);

                return narrativeObject;
            } 
        }

        /// <summary>
        /// The root view container of the graph view.
        /// </summary>
        private ViewContainer RootViewContainer { get; set; } = null;

        /// <summary>
        /// The collection of containers which currently exists.
        /// These may or may not currently be on the view stack.
        /// </summary>
        public List<ViewContainer> viewContainers = new List<ViewContainer>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="window"></param>
        public EditorGraphView(CuttingRoomEditorWindow window)
        {
            window.OnWindowCleared += OnWindowCleared;

            window.OnNarrativeObjectCreated += OnNarrativeObjectCreated;

            graphViewChanged += OnGraphViewChangedEvent;
            deleteSelection += OnDeleteSelection;
            serializeGraphElements += CutCopyOperation;
            unserializeAndPaste += PasteOperation;

            // Load the style sheet defining the style of the graph view.
            styleSheets.Add(Resources.Load<StyleSheet>("CuttingRoomEditorGraphView"));

            // Set the min and max zoom scales allowed.
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            // Add manipulators to handle interaction with the editor.
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new FreehandSelector());

            // Create a grid background instance.
            GridBackground gridBackground = new GridBackground();

            // Add the grid to this visual element.
            Insert(0, gridBackground);

            // Fit the grid background to the size of this visual element.
            gridBackground.StretchToParentSize();

            // Store the root view container.
            RootViewContainer = new ViewContainer(rootViewContainerGuid);

            // Root view container always exists.
            viewContainers.Add(RootViewContainer);

            // Push the "root" container of the graph view.
            // This element should never be removed and represents a container
            // for objects which are not part of another narrative object.
            viewContainerStack.Push(viewContainers[0]);
        }

        private string CutCopyOperation(IEnumerable<GraphElement> elements)
        {
            CopiedNodes.Clear();
            // Delete Cut nodes if a new copy is made
            foreach (var narrativeObject in CutNodes.Values)
            {
                UnityEngine.Object.DestroyImmediate(narrativeObject.NarrativeObject.gameObject);
            }
            CutNodes.Clear();
            List<NarrativeObjectNode> narrativeObjectNodes = elements.Where(e => e is NarrativeObjectNode).Select(e => e as NarrativeObjectNode).ToList();

            List<string> narrativeObjectGuids = new();

            foreach (var narrativeObjectNode in narrativeObjectNodes)
            {
                if (narrativeObjectNode != null && narrativeObjectNode.NarrativeObject != null)
                {
                    narrativeObjectGuids.Add(narrativeObjectNode.NarrativeObject.guid);

                    CopiedNodes.Add(narrativeObjectNode.NarrativeObject.guid, narrativeObjectNode);
                }
            }

            return JsonConvert.SerializeObject(narrativeObjectGuids);
        }

        public virtual void PasteOperation(string operationName, string data)
        {
            OnPaste?.Invoke(data);
            // Delete Cut nodes once paste is complete
            foreach (var narrativeObject in CutNodes.Values)
            {
                UnityEngine.Object.DestroyImmediate(narrativeObject.NarrativeObject.gameObject);
            }
            CopiedNodes.Clear();
            CutNodes.Clear();
            OnPasteComplete?.Invoke();
        }

        /// <summary>
        /// Add a node which inherits from NarrativeObjectNode to the graph view.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="narrativeObject"></param>
        /// <returns></returns>
        public NarrativeObjectNode AddNode<T>(NarrativeObject narrativeObject, NarrativeObject parentNarrativeObject) where T : NarrativeObjectNode
        {
            // Careful here as the constructor is being reflected based on the parameters.
            // If parameters change, this method call will still be valid but fail to find ctor.
            NarrativeObjectNode narrativeObjectNode = Activator.CreateInstance(typeof(T), new object[] { narrativeObject, parentNarrativeObject }) as NarrativeObjectNode;

            NarrativeObjectNodes.Add(narrativeObjectNode.NarrativeObject.guid, narrativeObjectNode);

            return narrativeObjectNode;
        }

        /// <summary>
        /// Invoked whenever a selection is made within the graph view.
        /// </summary>
        /// <param name="selectable"></param>
        public override void AddToSelection(ISelectable selectable)
        {
            if (selectable is NarrativeObjectNode)
            {
                selected.Add(selectable);

                NarrativeObjectNode narrativeObjectNode = selectable as NarrativeObjectNode;

                GameObject narrativeObjectGameObject = narrativeObjectNode.NarrativeObject.gameObject;

                if (!Selection.objects.Contains(narrativeObjectGameObject))
                {
                    List<UnityEngine.Object> selectedObjects = new List<UnityEngine.Object>(Selection.objects);

                    selectedObjects.Add(narrativeObjectGameObject);

                    Selection.objects = selectedObjects.ToArray();

                    // Invoke event for selection.
                    OnNarrativeObjectNodeSelected?.Invoke();
                }
            }
            else if (selectable is Edge)
            {
                selected.Add(selectable);

                Edge edge = selectable as Edge;

                // Get the nodes which the edge is attached between.
                NarrativeObjectNode outputNarrativeObjectNode = GetNarrativeObjectNodeWithPort(edge.output);
                NarrativeObjectNode inputNarrativeObjectNode = GetNarrativeObjectNodeWithPort(edge.input);

                // Invoke event for selection.
                OnEdgeSelected?.Invoke();
            }

            base.AddToSelection(selectable);
        }

        /// <summary>
        /// Invoked whenever a deselection is made within the graph view.
        /// </summary>
        /// <param name="selectable"></param>
        public override void RemoveFromSelection(ISelectable selectable)
        {
            if (selectable is NarrativeObjectNode)
            {
                selected.Remove(selectable);

                GameObject narrativeObjectGameObject = (selectable as NarrativeObjectNode).NarrativeObject.gameObject;

                if (Selection.objects.Contains(narrativeObjectGameObject))
                {
                    List<UnityEngine.Object> selectedObjects = new List<UnityEngine.Object>(Selection.objects);

                    selectedObjects.Remove(narrativeObjectGameObject);

                    Selection.objects = selectedObjects.ToArray();

                    // Invoke event for deselection.
                    OnNarrativeObjectNodeDeselected?.Invoke();
                }
            }
            else if (selectable is Edge)
            {
                selected.Remove(selectable);

                OnEdgeDeselected?.Invoke();
            }

            base.RemoveFromSelection(selectable);
        }

        /// <summary>
        /// Invoked whenever the selection is cleared within the graph view.
        /// </summary>
        public override void ClearSelection()
        {
            Selection.objects = new UnityEngine.Object[0];

            selected.Clear();

            // Invoke event for deselection.
            OnClearSelection?.Invoke();

            base.ClearSelection();
        }

        /// <summary>
        /// Invoked whenever the selection is cleared within the graph view.
        /// </summary>
        public void OnDeleteSelection(string operationName, AskUser askUser)
        {
            if (operationName == "Cut")
            {
                foreach (var selection in selected)
                {
                    if (selection is NarrativeObjectNode)
                    {
                        NarrativeObjectNode narrativeObjectNode = selection as NarrativeObjectNode;
                        // A copied object that is to be removed  is a cut object object. Do not delete until they are pasted
                        if (CopiedNodes.ContainsKey(narrativeObjectNode.NarrativeObject.guid))
                        {
                            CutNodes.Add(narrativeObjectNode.NarrativeObject.guid, narrativeObjectNode);
                            CopiedNodes.Remove(narrativeObjectNode.NarrativeObject.guid);
                        }
                    }
                }
            }
            else if (operationName == "Delete")
            {
                foreach (var selection in selected)
                {
                    if (selection is NarrativeObjectNode)
                    {
                        NarrativeObjectNode narrativeObjectNode = selection as NarrativeObjectNode;
                        DeleteNodes[narrativeObjectNode.NarrativeObject.guid] = narrativeObjectNode;
                    }
                }
            }
            DeleteSelection();
        }

        /// <summary>
        /// Returns a list of compatible ports for a specified start port to connect to.
        /// </summary>
        /// <param name="startPort"></param>
        /// <param name="nodeAdapter"></param>
        /// <returns></returns>
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();

            // Work out the type of the start node.
            if (startPort.node is NarrativeObjectNode)
            {
                NarrativeObjectNode startNarrativeObjectNode = startPort.node as NarrativeObjectNode;

                // Whether the compatible ports are input ports or output ports.
                PortType compatiblePortType = PortType.Output;

                // If the start port is an output port, then compatible ports are input ports.
                if (startPort == startNarrativeObjectNode.OutputPort)
                {
                    compatiblePortType = PortType.Input;
                }

                ports.ForEach((port) =>
                {
                    // If the port isnt the start port (cant link to itself) and
                    // the node isn't the same node (can't link own input to own output).
                    if (startPort != port && startPort.node != port.node)
                    {
                        if (port.node is NarrativeObjectNode)
                        {
                            NarrativeObjectNode narrativeObjectNode = port.node as NarrativeObjectNode;

                            // If searching for an input port and the port being examined is
                            // the input port of the node or vice versa for output nodes.
                            if (compatiblePortType == PortType.Input && port == narrativeObjectNode.InputPort ||
                                compatiblePortType == PortType.Output && port == narrativeObjectNode.OutputPort)
                            {
                                // TODO: Dont add port as compatible if a link already exists between the start port and the port being evaluated.
                                EdgeState existingEdgeState = EdgeStates.Where((edgeState) =>
                                {
                                    if (compatiblePortType == PortType.Input)
                                    {
                                        return edgeState.OutputNarrativeObjectNode.OutputPort == startPort && edgeState.InputNarrativeObjectNode.InputPort == port;
                                    }
                                    else
                                    {
                                        return edgeState.InputNarrativeObjectNode.InputPort == startPort && edgeState.OutputNarrativeObjectNode.OutputPort == port;
                                    }
                                }).FirstOrDefault();

                                // If no edge state exists already, then it is a valid port.
                                if (existingEdgeState == null)
                                {
                                    compatiblePorts.Add(port);
                                }
                            }
                        }
                    }
                });
            }

            return compatiblePorts;
        }

        /// <summary>
        /// Invoked when the editor window this graph view belongs to is cleared.
        /// </summary>
        private void OnWindowCleared()
        {
            // Remove all existing nodes as they have lost their references and must be regenerated.
            foreach (NarrativeObjectNode node in NarrativeObjectNodes.Values)
            {
                if (Contains(node))
                {
                    RemoveElement(node);
                }
            }

            // Clear out old node references.
            NarrativeObjectNodes.Clear();

            // Remove all edge states as they have lost references and must be regenerated.
            foreach (EdgeState edgeState in EdgeStates)
            {
                if (Contains(edgeState.Edge))
                {
                    RemoveElement(edgeState.Edge);
                }
            }

            // Clear out old edge states.
            EdgeStates.Clear();
        }

        /// <summary>
        /// Invoked whenever a new narrative object is created via the toolbar.
        /// </summary>
        private void OnNarrativeObjectCreated(NarrativeObject narrativeObject)
        {
            ViewContainer visibleViewContainer = viewContainerStack.Peek();

            // If the current view container has no root object, then set it.
            if (!ViewContainerHasRootNarrativeObject(visibleViewContainer))
            {
                SetNarrativeObjectAsRootOfViewContainer(visibleViewContainer, narrativeObject);
            }

            AddNarrativeObjectAsCandidateOfViewContainer(visibleViewContainer, narrativeObject);
        }

        /// <summary>
        /// Invoked when the graph view is changed.
        /// </summary>
        /// <param name="graphViewChange"></param>
        /// <returns></returns>
        private GraphViewChange OnGraphViewChangedEvent(GraphViewChange graphViewChange)
        {
            if (graphViewChange.elementsToRemove != null && graphViewChange.elementsToRemove.Count > 0)
            {
                foreach (GraphElement graphElement in graphViewChange.elementsToRemove)
                {
                    // If an element has been removed.
                    if (graphElement is Edge)
                    {
                        // Find the edge
                        EdgeState deletedEdgeState = EdgeStates.Where(edgeState => edgeState.Edge == graphElement as Edge).FirstOrDefault();

                        if (deletedEdgeState == null)
                        {
                            continue;
                        }

                        // Only delete if not part of active cut
                        if (!CutNodes.ContainsKey(deletedEdgeState.OutputNarrativeObjectNode.NarrativeObject.guid))
                        {
                            // Disconnect the narrative objects in the edge state which has been removed.
                            deletedEdgeState.OutputNarrativeObjectNode.NarrativeObject.OutputSelectionDecisionPoint.RemoveCandidate(deletedEdgeState.InputNarrativeObjectNode.NarrativeObject);

                            // Delete the edge state as it's corresponding edge no longer exists.
                            EdgeStates.Remove(deletedEdgeState);
                        }
                    }
                    else if (graphElement is NarrativeObjectNode)
                    {
                        NarrativeObjectNode narrativeObjectNode = graphElement as NarrativeObjectNode;

                        // The view container being rendered.
                        ViewContainer visibleViewContainer = viewContainerStack.Peek();
                        RemoveNarrativeObjectAsCandidateOfViewContainer(visibleViewContainer, narrativeObjectNode.NarrativeObject);

                        // Remove deleted node from list if deleted.
                        if (DeleteNodes.ContainsKey(narrativeObjectNode.NarrativeObject.guid))
                        {
                            DeleteNodes.Remove(narrativeObjectNode.NarrativeObject.guid);

                            if (CopiedNodes.ContainsKey(narrativeObjectNode.NarrativeObject.guid))
                            {
                                CopiedNodes.Remove(narrativeObjectNode.NarrativeObject.guid);
                            }

                            // Destroy the object in the hierarchy that the node being deleted represents.
                            UnityEngine.Object.DestroyImmediate(narrativeObjectNode.NarrativeObject.gameObject);
                        }
                    }
                }
            }

            if (graphViewChange.edgesToCreate != null && graphViewChange.edgesToCreate.Count > 0)
            {
                foreach (Edge edge in graphViewChange.edgesToCreate)
                {
                    NarrativeObjectNode outputNode = GetNarrativeObjectNodeWithPort(edge.output);

                    NarrativeObjectNode inputNode = GetNarrativeObjectNodeWithPort(edge.input);

                    if (outputNode != null && inputNode != null)
                    {
                        // Add the connection to the narrative object.
                        outputNode.NarrativeObject.OutputSelectionDecisionPoint.AddCandidate(inputNode.NarrativeObject);

                        // Add the edge state as this edge has come into existence without being created during the Populate method.
                        EdgeStates.Add(new EdgeState
                        {
                            Edge = edge,
                            InputNarrativeObjectNode = inputNode,
                            OutputNarrativeObjectNode = outputNode
                        });
                    }
                }
            }

            // Invoke change event.
            OnGraphViewChanged?.Invoke(graphViewChange);

            return graphViewChange;
        }

        /// <summary>
        /// Find the narrative object node with the specified narrative object.
        /// </summary>
        /// <param name="narrativeObject"></param>
        /// <returns></returns>
        public NarrativeObjectNode GetNarrativeObjectNode(NarrativeObject narrativeObject)
        {
            foreach (NarrativeObjectNode node in NarrativeObjectNodes.Values)
            {
                if (node != null && node.NarrativeObject.guid == narrativeObject.guid)
                {
                    return node;
                }
            }

            Debug.LogWarning("Narrative Object Node with specified narrative object not found in current graph.");

            return null;
        }

        /// <summary>
        /// Find the narrative object node with the port specified.
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public NarrativeObjectNode GetNarrativeObjectNodeWithPort(Port port)
        {
            foreach (NarrativeObjectNode node in NarrativeObjectNodes.Values)
            {
                if (PortsAreEqual(node.InputPort, port) || PortsAreEqual(node.OutputPort, port))
                {
                    return node;
                }
            }

            Debug.LogError("Narrative Object Node with specified port not found.");

            return null;
        }

        /// <summary>
        /// Evaluate if ports are equal.
        /// This prevents a port being re-rendered and comparison failing due to internal visual element guids not matching.
        /// </summary>
        /// <returns></returns>
        private bool PortsAreEqual(Port a, Port b)
        {
            return a.worldTransform == b.worldTransform;
        }

        /// <summary>
        /// Invoked to push a new view container onto the view stack.
        /// </summary>
        /// <param name="narrativeObjectNode"></param>
        private void PushViewContainer(NarrativeObjectNode narrativeObjectNode)
        {
            ViewContainer viewContainer = viewContainers.Where(viewContainer => viewContainer.narrativeObjectGuid == narrativeObjectNode.NarrativeObject.guid).FirstOrDefault();

            if (viewContainer == null)
            {
                viewContainer = new ViewContainer(narrativeObjectNode.NarrativeObject.guid);

                viewContainers.Add(viewContainer);
            }

            PushViewContainer(viewContainer);
        }

        /// <summary>
        /// Push view container onto the view stack and invoke callbacks.
        /// </summary>
        /// <param name="viewContainer"></param>
        private void PushViewContainer(ViewContainer viewContainer)
        {
            viewContainerStack.Push(viewContainer);

            OnViewContainerPushed?.Invoke();
        }

        /// <summary>
        /// Invoked to pop a view container off the view stack if possible.
        /// </summary>
        /// <returns>Whether a view container was successfully popped from the view stack.</returns>
        public bool PopViewContainer()
        {
            // Never pop off the final element (which is the root view container).
            if (viewContainerStack.Count > 1)
            {
                viewContainerStack.Pop();

                // No callback here as the window handles this event (as it invokes it).

                return true;
            }

            return false;
        }

        /// <summary>
        /// Pop containers until the view desired is top of the view stack.
        /// </summary>
        /// <param name="viewContainer"></param>
        /// <returns></returns>
        public bool PopViewContainersToViewContainer(ViewContainer viewContainer)
        {
            if (!viewContainerStack.Contains(viewContainer))
            {
                Debug.LogError($"View Container Stack does not contain a View Container with the guid: {viewContainer.narrativeObjectGuid}");

                return false;
            }

            bool popOccurred = false;

            while (viewContainerStack.Peek() != viewContainer && viewContainerStack.Count > 1)
            {
                bool pop = PopViewContainer();

                if (!popOccurred)
                {
                    if (pop)
                    {
                        popOccurred = true;
                    }
                }
            }

            return popOccurred;
        }

        /// <summary>
        /// Whether a narrative object is currently visible on the graph view.
        /// </summary>
        /// <param name="narrativeObject"></param>
        /// <returns></returns>
        private bool IsVisible(NarrativeObject narrativeObject)
        {
            // If the narrative objects node is in the currently visible view container.
            if (viewContainerStack.Peek().ContainsNode(narrativeObject.guid))
            {
                return true;
            }

            // Find the view containers which contain the narrative object.
            IEnumerable<ViewContainer> viewContainersWithNarrativeObject = viewContainers.ToList().Where(viewContainer => viewContainer.ContainsNode(narrativeObject.guid));

            // If no view containers contain the narrative object, it has just been created and therefore should be visible.
            if (viewContainersWithNarrativeObject.Count() == 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get the list of deleted view containers and narrative object guids when deleting a view container.
        /// This is recursive so returns the contents of any child view containers and their contained narrative object guids.
        /// </summary>
        /// <param name="viewContainer"></param>
        /// <param name="deletedViewContainers"></param>
        /// <param name="deletedNarrativeObjectGuids"></param>
        private void GetDeletedViewContainersAndNarrativeObjectGuids(ViewContainer viewContainer, ref List<ViewContainer> deletedViewContainers, ref List<string> deletedNarrativeObjectGuids)
        {
            deletedViewContainers.Add(viewContainer);

            deletedNarrativeObjectGuids.Add(viewContainer.narrativeObjectGuid);

            // Check for nested view containers for the contents of the view container passed as parameter.
            foreach (string guid in viewContainer.narrativeObjectNodeGuids)
            {
                // Find the view containers of any children of this view container.
                ViewContainer childViewContainer = viewContainers.Where(viewContainer => viewContainer.narrativeObjectGuid == guid).FirstOrDefault();

                // If the child node has a view container, this and its contents must go too.
                if (childViewContainer != null)
                {
                    // Recursively call this method to get rid of the contents.
                    GetDeletedViewContainersAndNarrativeObjectGuids(childViewContainer, ref deletedViewContainers, ref deletedNarrativeObjectGuids);
                }
                else
                {
                    // Doesn't have a view container so just mark the object itself for deletion (no recursion required).
                    deletedNarrativeObjectGuids.Add(guid);
                }
            }
        }

        /// <summary>
        /// Get the guid for the root narrative object of the specified view container.
        /// </summary>
        /// <param name="viewContainer"></param>
        /// <returns></returns>
        private string GetViewContainerRootNarrativeObjectGuid(ViewContainer viewContainer)
        {
            if (viewContainer.narrativeObjectGuid == rootViewContainerGuid)
            {
                NarrativeSpace narrativeSpace = CuttingRoomEditorUtils.GetOrCreateNarrativeSpace();

                // If a root is set on the narrative space, return its guid.
                if (narrativeSpace != null && narrativeSpace.RootNarrativeObject != null)
                {
                    return narrativeSpace.RootNarrativeObject.guid;
                }
            }
            else
            {
                NarrativeObject narrativeObject = GetNarrativeObject(viewContainer.narrativeObjectGuid);

                if (narrativeObject != null)
                {
                    if (narrativeObject is GraphNarrativeObject)
                    {
                        GraphNarrativeObject graphNarrativeObject = narrativeObject.GetComponent<GraphNarrativeObject>();

                        // If a root is set on the graph object, return its guid.
                        if (graphNarrativeObject.rootNarrativeObject != null)
                        {
                            return graphNarrativeObject.rootNarrativeObject.guid;
                        }
                    }
                }
            }

            return string.Empty;
        }

        public class PopulateResult
        {
            /// <summary>
            /// Whether the call to Populate changed the graph view and should be saved.
            /// </summary>
            public bool GraphViewChanged { get; set; } = false;

            /// <summary>
            /// The guid and position of any newly created nodes.
            /// </summary>
            public List<Tuple<string, Vector2>> CreatedNodes { get; set; } = new List<Tuple<string, Vector2>>();
        }

        private void UpdateViewContainer(ref CuttingRoomEditorGraphViewState graphViewState, string viewContainerNarrativeObjectGuid,
            HashSet<NarrativeObject> viewContainerNarrativeObjects)
        {
            ViewContainer viewContainer = viewContainers.Where(viewContainer => viewContainer.narrativeObjectGuid == viewContainerNarrativeObjectGuid).FirstOrDefault();

            if (viewContainer == null)
            {
                viewContainer = new ViewContainer(viewContainerNarrativeObjectGuid);

                viewContainers.Add(viewContainer);
            }

            if (graphViewState == null)
            {
                return;
            }

            //var viewContainerState = graphViewState.viewContainerStates.Where(state => state.narrativeObjectGuid == viewContainerNarrativeObjectGuid).FirstOrDefault();
            var viewContainerState = graphViewState.ViewContainerStateLookup.ContainsKey(viewContainerNarrativeObjectGuid) ?
                graphViewState.ViewContainerStateLookup[viewContainerNarrativeObjectGuid] : null;

            // If View Container has no recorded state
            if (viewContainerState == null)
            {
                viewContainerState = new();
                viewContainerState.narrativeObjectGuid = viewContainerNarrativeObjectGuid;
            }

            HashSet<string> removedNarrativeObjectGuids = viewContainerState.narrativeObjectNodeGuids.ToHashSet();

            // Have starting position for new nodes
            Vector2 startingNodePosition = contentViewContainer.WorldToLocal(layout.center);
            foreach (var narrativeObject in viewContainerNarrativeObjects)
            {
                if (!viewContainer.narrativeObjectNodeGuids.Contains(narrativeObject.guid))
                {
                    viewContainer.narrativeObjectNodeGuids.Add(narrativeObject.guid);
                }
                // Add narrative object guid to view container state if not present
                if (!viewContainerState.narrativeObjectNodeGuids.Contains(narrativeObject.guid))
                {
                    viewContainerState.narrativeObjectNodeGuids.Add(narrativeObject.guid);
                }
                else
                {
                    // Trim existing nodes from removed list
                    removedNarrativeObjectGuids.Remove(narrativeObject.guid);
                }

                var narrativeObjectNodeState = graphViewState.NarrativeObjectNodeStateLookup.ContainsKey(narrativeObject.guid) ?
                    graphViewState.NarrativeObjectNodeStateLookup[narrativeObject.guid] : null;

                if (narrativeObjectNodeState == null)
                {
                    narrativeObjectNodeState = new();
                    narrativeObjectNodeState.narrativeObjectGuid = narrativeObject.guid;
                    narrativeObjectNodeState.position = startingNodePosition;
                    // Shift starting position to avoid overlapping.
                    startingNodePosition.x += 10;
                    startingNodePosition.y += 10;
                }
                graphViewState.UpdateState(narrativeObject.guid, narrativeObjectNodeState);

                // Check if narrative object may container child nodes
                if (narrativeObject is GraphNarrativeObject ||
                    narrativeObject is LayerNarrativeObject ||
                    narrativeObject is GraphNarrativeObject)
                {
                    // Check for child narrative objects
                    HashSet<NarrativeObject> childNarrativeObjects = new();
                    
                    for (int i = 0; i < narrativeObject.gameObject.transform.childCount; ++i)
                    {
                        GameObject child = narrativeObject.gameObject.transform.GetChild(i).gameObject;
                        if (child.TryGetComponent(out NarrativeObject childNarrativeObject))
                        {
                            childNarrativeObjects.Add(childNarrativeObject);
                        }
                    }

                    if (childNarrativeObjects != null && childNarrativeObjects.Count > 0)
                    {
                        // Update view container for container node
                        UpdateViewContainer(ref graphViewState, narrativeObject.guid, childNarrativeObjects);
                    }
                }
            }
            graphViewState.UpdateState(viewContainerNarrativeObjectGuid, viewContainerState);

            foreach (var removedNodeGuid in removedNarrativeObjectGuids)
            {
                if (removedNodeGuid != rootViewContainerGuid)
                {
                    viewContainerState.narrativeObjectNodeGuids.Remove(removedNodeGuid);

                    ViewContainer deletedViewContainer = viewContainers.Where(vc => vc.narrativeObjectGuid == removedNodeGuid).FirstOrDefault();
                    // The container should not exist. Remove it if it exists.
                    if (deletedViewContainer != null)
                    {
                        if (viewContainerStack.Contains(deletedViewContainer))
                        {
                            PopViewContainersToViewContainer(deletedViewContainer);
                            PopViewContainer();
                        }
                        viewContainers.Remove(deletedViewContainer);
                    }
                }
            }
        }

        /// <summary>
        /// Populate the graph view based on the contents of the scene currently open.
        /// </summary>
        /// <returns>Whether the graph view contents have been changed.</returns>
        public PopulateResult Populate(CuttingRoomEditorGraphViewState graphViewState, HashSet<NarrativeObject> narrativeObjects)
        {
            // Returned populate result.
            PopulateResult populateResult = new PopulateResult();

            if (graphViewState == null)
            {
                return populateResult;
            }

            // Ensure root breadcrumb is present
            if (graphViewState.viewContainerStackGuids.Count == 0 || graphViewState.viewContainerStackGuids[0] != rootViewContainerGuid)
            {
                List<string> newViewStack = new() { rootViewContainerGuid };
                foreach(var guid in graphViewState.viewContainerStackGuids)
                {
                    newViewStack.Add(guid);
                }
                graphViewState.UpdateViewStackGuids(newViewStack);
            }

            Scene scene = SceneManager.GetActiveScene();
            if (scene != ActiveScene)
            {
                OnWindowCleared();
                // Refresh view stack if scene has changed
                viewContainerStack.Clear();
                viewContainerStack.Push(viewContainers[0]);
                ActiveScene = scene;
                populateResult.GraphViewChanged = true;
            }

            if (graphViewState.viewContainerStackGuids.Count != viewContainerStack.Count
                || graphViewState.ViewContainerStateLookup.Count != viewContainers.Count
                || graphViewState.NarrativeObjectNodeStateLookup.Count != narrativeObjects.Count)
            {
                populateResult.GraphViewChanged = true;
            }

            // Get List of root narrative objects
            var rootGameObjects = scene.GetRootGameObjects();
            HashSet<NarrativeObject> rootNarrativeObjects = rootGameObjects.Where(go => go.TryGetComponent<NarrativeObject>(out _))
                .Select(ngo => ngo.GetComponent<NarrativeObject>())
                .ToHashSet();

            if (rootNarrativeObjects == null || rootNarrativeObjects.Count == 0)
            {
                NarrativeSpace narrativeSpace = UnityEngine.Object.FindObjectOfType<NarrativeSpace>();
                if (narrativeSpace != null)
                {
                    // Check for root narrative objects under the narrative space
                    rootNarrativeObjects = new();

                    for (int i = 0; i < narrativeSpace.gameObject.transform.childCount; ++i)
                    {
                        GameObject child = narrativeSpace.gameObject.transform.GetChild(i).gameObject;
                        if (child.TryGetComponent(out NarrativeObject childNarrativeObject))
                        {
                            rootNarrativeObjects.Add(childNarrativeObject);
                        }
                    }
                }
            }

            UpdateViewContainer(ref graphViewState, rootViewContainerGuid, rootNarrativeObjects);

            populateResult.GraphViewChanged = true;

            // Update ViewStack
            foreach (string guid in graphViewState.viewContainerStackGuids)
            {
                ViewContainer viewContainer = viewContainers.Where(viewContainer => viewContainer.narrativeObjectGuid == guid).FirstOrDefault();

                if (viewContainer != null)
                {
                    if (!viewContainerStack.Contains(viewContainer))
                    {
                        PushViewContainer(viewContainer);
                    }
                }
            }

            // The view container being rendered.
            ViewContainer visibleViewContainer = viewContainerStack.Peek();
            // Get the narrative object represented by the visible view container.
            NarrativeObject visibleViewContainerNarrativeObject = GetNarrativeObject(visibleViewContainer.narrativeObjectGuid);

            if (visibleViewContainer.narrativeObjectGuid == rootViewContainerGuid)
            {
                narrativeObjects = rootNarrativeObjects;
            }
            else
            {
                var viewContainerNarrativeObject = narrativeObjects.Where(narrativeObject => narrativeObject.guid == visibleViewContainer.narrativeObjectGuid).First();

                // Check for child narrative objects
                narrativeObjects = new();

                for (int i = 0; i < viewContainerNarrativeObject.gameObject.transform.childCount; ++i)
                {
                    GameObject child = viewContainerNarrativeObject.gameObject.transform.GetChild(i).gameObject;
                    if (child.TryGetComponent(out NarrativeObject childNarrativeObject))
                    {
                        narrativeObjects.Add(childNarrativeObject);
                    }
                }
            }

            // Check if window has been fully cleared
            bool windowCleared = (NarrativeObjectNodes != null && NarrativeObjectNodes.Count == 0);

            foreach (NarrativeObject narrativeObject in narrativeObjects)
            {
                // Find any nodes which already exist in the graph view for the specified narrative object.
                NarrativeObjectNode narrativeObjectNode = NarrativeObjectNodes.ContainsKey(narrativeObject.guid) ? NarrativeObjectNodes[narrativeObject.guid] : null;

                // If a node doesn't exist, create one.
                if (narrativeObjectNode == null)
                {
                    if (narrativeObject is AtomicNarrativeObject)
                    {
                        narrativeObjectNode = AddNode<AtomicNarrativeObjectNode>(narrativeObject, visibleViewContainerNarrativeObject);
                    }
                    else if (narrativeObject is GraphNarrativeObject)
                    {
                        narrativeObjectNode = AddNode<GraphNarrativeObjectNode>(narrativeObject, visibleViewContainerNarrativeObject);

                        GraphNarrativeObjectNode graphNarrativeObjectNode = narrativeObjectNode as GraphNarrativeObjectNode;

                        graphNarrativeObjectNode.OnClickViewContents -= PushViewContainer;
                        graphNarrativeObjectNode.OnClickViewContents += PushViewContainer;
                    }
                    else if (narrativeObject is GroupNarrativeObject)
                    {
                        narrativeObjectNode = AddNode<GroupNarrativeObjectNode>(narrativeObject, visibleViewContainerNarrativeObject);

                        GroupNarrativeObjectNode groupNarrativeObjectNode = narrativeObjectNode as GroupNarrativeObjectNode;

                        groupNarrativeObjectNode.OnClickViewContents -= PushViewContainer;
                        groupNarrativeObjectNode.OnClickViewContents += PushViewContainer;
                    }
                    else if (narrativeObject is LayerNarrativeObject)
                    {
                        narrativeObjectNode = AddNode<LayerNarrativeObjectNode>(narrativeObject, visibleViewContainerNarrativeObject);

                        LayerNarrativeObjectNode layerNarrativeObjectNode = narrativeObjectNode as LayerNarrativeObjectNode;

                        layerNarrativeObjectNode.OnClickViewContents -= PushViewContainer;
                        layerNarrativeObjectNode.OnClickViewContents += PushViewContainer;
                    }
                    else
                    {
                        Debug.LogError($"Node cannot be created as type of associated narrative object is not known.\nName: {narrativeObject.name}.");

                        continue;
                    }

                    if (!viewContainerStack.Peek().ContainsNode(narrativeObjectNode.NarrativeObject.guid))
                    {
                        // Add the node to the visible container on the view stack.
                        viewContainerStack.Peek().AddNode(narrativeObjectNode.NarrativeObject.guid);
                    }

                    narrativeObjectNode.OnSetAsNarrativeSpaceRoot += OnNarrativeObjectNodeSetAsNarrativeSpaceRoot;
                    narrativeObjectNode.OnSetAsParentNarrativeObjectRoot += OnNarrativeObjectNodeSetAsParentNarrativeObjectRoot;

                    narrativeObjectNode.OnSetAsCandidate += OnNarrativeObjectNodeSetAsCandidate;
                    narrativeObjectNode.OnRemoveAsCandidate += OnNarrativeObjectNodeRemoveAsCandidate;

                    AddElement(narrativeObjectNode);

                    // If window hasn't been cleared, add new nodes to selection. This allows pasted nodes to become selected upon paste.
                    if (!windowCleared)
                    {
                        AddToSelection(narrativeObjectNode);
                    }
                }
                else
                {
                    // Ensure this node is visible as it will definitely be in the correct location now.
                    narrativeObjectNode.visible = true;
                }

                // If a save graph view exists, try to find the properties for this node.
                if (graphViewState != null)
                {
                    NarrativeObjectNodeState nodeState = graphViewState.NarrativeObjectNodeStateLookup.GetValueOrDefault(narrativeObject.guid);

                    // If a node state exists then restore the node with the correct values.
                    if (nodeState != null)
                    {
                        narrativeObjectNode.SetPosition(new Rect(nodeState.position, narrativeObjectNode.GetPosition().size));
                    }
                    else
                    {
                        // The current centre of the graph view window.
                        Vector2 graphViewCenter = contentViewContainer.WorldToLocal(layout.center);

                        // Store node as created.
                        populateResult.CreatedNodes.Add(new Tuple<string, Vector2>(narrativeObjectNode.NarrativeObject.guid, graphViewCenter));

                        // Make invisible to avoid popping onto screen at 0,0 before appearing at the centre of the graph view.
                        narrativeObjectNode.visible = false;

                        // Node state for this node doesn't exist. Graph view is different from it's save state.
                        populateResult.GraphViewChanged = true;
                    }
                }
                else
                {
                    // At least one node has been added and there is no save state so create one.
                    populateResult.GraphViewChanged = true;
                }
            }

            // Get the guid of the root narrative object of the view.
            string viewContainerRootNarrativeObjectGuid = GetViewContainerRootNarrativeObjectGuid(visibleViewContainer);

            // Find the nodes which are in the current view container. These will be rendered.
            IEnumerable<NarrativeObjectNode> visibleNarrativeObjectNodes = NarrativeObjectNodes.Values.Where(narrativeObjectNode => visibleViewContainer.ContainsNode(narrativeObjectNode.NarrativeObject.guid));

            // For each node, make sure all edges exist.
            foreach (NarrativeObjectNode narrativeObjectNode in visibleNarrativeObjectNodes)
            {
                NarrativeSpace narrativeSpace = UnityEngine.Object.FindObjectOfType<NarrativeSpace>();
                // If the node is the root of the current view.
                if (narrativeObjectNode.NarrativeObject.guid == viewContainerRootNarrativeObjectGuid || narrativeObjectNode.NarrativeObject.guid == narrativeSpace.RootNarrativeObject.guid)
                {
                    bool mainRoot = false;
                    if (visibleViewContainer.narrativeObjectGuid == rootViewContainerGuid || narrativeObjectNode.NarrativeObject.guid == narrativeSpace.RootNarrativeObject.guid)
                    {
                        mainRoot = true;
                    }
                    narrativeObjectNode.EnableRootVisuals(mainRoot);
                }

                // If the visible view is a representing a group, show candidate graphics on candidate nodes.
                if (visibleViewContainerNarrativeObject is GroupNarrativeObject)
                {
                    GroupNarrativeObject groupNarrativeObject = visibleViewContainerNarrativeObject as GroupNarrativeObject;

                    if (groupNarrativeObject.GroupSelectionDecisionPoint.Candidates.Contains(narrativeObjectNode.NarrativeObject))
                    {
                        narrativeObjectNode.EnableCandidateVisuals();
                    }
                }

                // If the visible view is a representing a layer, show candidate graphics on layer root nodes.
                if (visibleViewContainerNarrativeObject is LayerNarrativeObject)
                {
                    LayerNarrativeObject layerNarrativeObject = visibleViewContainerNarrativeObject as LayerNarrativeObject;

                    if (layerNarrativeObject.LayerSelectionDecisionPoint.Candidates.Contains(narrativeObjectNode.NarrativeObject))
                    {
                        if (layerNarrativeObject.primaryLayerRootNarrativeObject == narrativeObjectNode.NarrativeObject)
                        {
                            narrativeObjectNode.EnableLayerVisuals(true);
                        }
                        else
                        {
                            narrativeObjectNode.EnableLayerVisuals();
                        }
                    }
                }

                // If output connections are populated
                if (narrativeObjectNode.NarrativeObject.OutputSelectionDecisionPoint != null)
                {
                    // For each connection from the nodes outputs.
                    foreach (NarrativeObject candidateNarrativeObject in narrativeObjectNode.NarrativeObject.OutputSelectionDecisionPoint.Candidates)
                    {
                        // Find an edge state for the connection between the two nodes.
                        EdgeState edgeState = EdgeStates.Where(edgeState => edgeState.OutputNarrativeObjectNode.NarrativeObject.guid == narrativeObjectNode.NarrativeObject.guid && edgeState.InputNarrativeObjectNode.NarrativeObject.guid == candidateNarrativeObject.guid).FirstOrDefault();

                        // If no connection already exists.
                        if (edgeState == null)
                        {
                            // Get the node representing the input narrative object.
                            NarrativeObjectNode inputNarrativeObjectNode = null;

                            // If the input node doesn't exist, the narrative object has moved so delete edge state
                            if (!NarrativeObjectNodes.TryGetValue(candidateNarrativeObject.guid, out inputNarrativeObjectNode))
                            {
                                EdgeStates.Remove(edgeState);
                                continue;
                            }

                            // Create a new edge between the output port of the first narrative object node and the input port of the connected narrative object node.
                            Edge edge = new Edge()
                            {
                                output = narrativeObjectNode.OutputPort,
                                input = inputNarrativeObjectNode.InputPort,
                            };

                            edge.input.Connect(edge);
                            edge.output.Connect(edge);

                            // Store the edge state for this new edge.
                            EdgeStates.Add(new EdgeState { Edge = edge, InputNarrativeObjectNode = inputNarrativeObjectNode, OutputNarrativeObjectNode = narrativeObjectNode });

                            // Add edge to the graph view.
                            AddElement(edge);
                        }
                        else
                        {
                            if (!Contains(edgeState.Edge))
                            {
                                AddElement(edgeState.Edge);
                            }
                        }
                    }
                }
            }

            return populateResult;
        }

        /// <summary>
        /// Get the narrative object with the specified guid.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        private NarrativeObject GetNarrativeObject(string guid)
        {
            NarrativeObject[] narrativeObjects = UnityEngine.Object.FindObjectsOfType<NarrativeObject>();

            return narrativeObjects.Where(narrativeObject => narrativeObject.guid == guid).FirstOrDefault();
        }

        /// <summary>
        /// Invoked whenever a narrative object is set as root on the graph view.
        /// </summary>
        /// <param name="narrativeObjectNode"></param>
        private void OnNarrativeObjectNodeSetAsNarrativeSpaceRoot(NarrativeObjectNode narrativeObjectNode)
        {
            SetNarrativeObjectAsRootOfViewContainer(RootViewContainer, narrativeObjectNode.NarrativeObject);

            // Invoke the callback as a root has changed somewhere on the graph.
            OnRootNarrativeObjectChanged?.Invoke();
        }

        /// <summary>
        /// Invoked whenever a narrative object is set as the root of a parent narrative object.
        /// </summary>
        /// <param name="narrativeObjectNode"></param>
        private void OnNarrativeObjectNodeSetAsParentNarrativeObjectRoot(NarrativeObjectNode narrativeObjectNode)
        {
            SetNarrativeObjectAsRootOfViewContainer(ViewContainerStack.Peek(), narrativeObjectNode.NarrativeObject);

            // Invoke the callback as a root has changed somewhere on the graph.
            OnRootNarrativeObjectChanged?.Invoke();
        }

        /// <summary>
        /// Invoked whenever a narrative object is Set as a candidate on the graph view.
        /// </summary>
        /// <param name="narrativeObjectNode"></param>
        private void OnNarrativeObjectNodeSetAsCandidate()
        {
            ViewContainer visibleViewContainer = viewContainerStack.Peek();

            // If the container is the root view, no candidates are allowed so ignore.
            if (visibleViewContainer.narrativeObjectGuid != rootViewContainerGuid)
            {
                NarrativeObject visibleViewContainerNarrativeObject = GetNarrativeObject(visibleViewContainer.narrativeObjectGuid);

                if (visibleViewContainerNarrativeObject is GroupNarrativeObject)
                {
                    GroupNarrativeObject groupNarrativeObject = visibleViewContainerNarrativeObject.GetComponent<GroupNarrativeObject>();

                    foreach (ISelectable selectable in selection)
                    {
                        if (selectable is NarrativeObjectNode)
                        {
                            NarrativeObjectNode candidate = selectable as NarrativeObjectNode;

                            // Ensure selection is inside the current view (group graph).
                            if (visibleViewContainer.ContainsNode(candidate.NarrativeObject.guid))
                            {
                                groupNarrativeObject.GroupSelectionDecisionPoint.AddCandidate(candidate.NarrativeObject);
                            }
                        }
                    }

                    OnNarrativeObjectCandidatesChanged?.Invoke();
                }
                else if (visibleViewContainerNarrativeObject is LayerNarrativeObject)
                {
                    LayerNarrativeObject layerNarrativeObject = visibleViewContainerNarrativeObject.GetComponent<LayerNarrativeObject>();

                    foreach (ISelectable selectable in selection)
                    {
                        if (selectable is NarrativeObjectNode)
                        {
                            NarrativeObjectNode candidate = selectable as NarrativeObjectNode;

                            // Ensure selection is inside the current view (group graph).
                            if (visibleViewContainer.ContainsNode(candidate.NarrativeObject.guid))
                            {
                                layerNarrativeObject.LayerSelectionDecisionPoint.AddCandidate(candidate.NarrativeObject);
                            }
                        }
                    }

                    OnNarrativeObjectCandidatesChanged?.Invoke();
                }
            }
        }

        /// <summary>
        /// Invoked whenever a narrative object is removed as a candidate on the graph view.
        /// </summary>
        /// <param name="narrativeObjectNode"></param>
        private void OnNarrativeObjectNodeRemoveAsCandidate()
        {
            ViewContainer visibleViewContainer = viewContainerStack.Peek();

            // If the container is the root view, no candidates are allowed so ignore.
            if (visibleViewContainer.narrativeObjectGuid != rootViewContainerGuid)
            {
                NarrativeObject visibleViewContainerNarrativeObject = GetNarrativeObject(visibleViewContainer.narrativeObjectGuid);

                if (visibleViewContainerNarrativeObject is GroupNarrativeObject)
                {
                    GroupNarrativeObject groupNarrativeObject = visibleViewContainerNarrativeObject.GetComponent<GroupNarrativeObject>();
                   
                    foreach (ISelectable selectable in selection)
                    {
                        if (selectable is NarrativeObjectNode)
                        {
                            NarrativeObjectNode candidate = selectable as NarrativeObjectNode;

                            // Ensure selection is inside the current view (group graph).
                            if (visibleViewContainer.ContainsNode(candidate.NarrativeObject.guid))
                            {
                                groupNarrativeObject.GroupSelectionDecisionPoint.RemoveCandidate(candidate.NarrativeObject);
                            }
                        }
                    }

                    OnNarrativeObjectCandidatesChanged?.Invoke();
                }
                else if (visibleViewContainerNarrativeObject is LayerNarrativeObject)
                {
                    LayerNarrativeObject layerNarrativeObject = visibleViewContainerNarrativeObject.GetComponent<LayerNarrativeObject>();

                    foreach (ISelectable selectable in selection)
                    {
                        if (selectable is NarrativeObjectNode)
                        {
                            NarrativeObjectNode candidate = selectable as NarrativeObjectNode;

                            // Ensure selection is inside the current view (group graph).
                            if (visibleViewContainer.ContainsNode(candidate.NarrativeObject.guid))
                            {
                                layerNarrativeObject.LayerSelectionDecisionPoint.RemoveCandidate(candidate.NarrativeObject);
                            }
                        }
                    }

                    OnNarrativeObjectCandidatesChanged?.Invoke();
                }
            }
        }

        /// <summary>
        /// Set the root narrative object of a view container.
        /// </summary>
        /// <param name="viewContainer"></param>
        /// <param name="narrativeObject"></param>
        /// <returns></returns>
        private bool SetNarrativeObjectAsRootOfViewContainer(ViewContainer viewContainer, NarrativeObject narrativeObject)
        {
            // If the current view is the narrative space.
            if (viewContainer.narrativeObjectGuid == rootViewContainerGuid)
            {
                NarrativeSpace narrativeSpace = CuttingRoomEditorUtils.GetOrCreateNarrativeSpace();

                if (narrativeSpace == null)
                {
                    return false;
                }

                narrativeSpace.RootNarrativeObject = narrativeObject;
            }
            else
            {
                // Find the narrative object which has the same guid as the current view container.
                NarrativeObject viewContainerNarrativeObject = GetNarrativeObject(viewContainer.narrativeObjectGuid);

                if (viewContainerNarrativeObject == null)
                {
                    Debug.LogError($"Narrative Object with guid {narrativeObject.guid} does not exist.");

                    return false;
                }

                if (viewContainerNarrativeObject is GraphNarrativeObject)
                {
                    GraphNarrativeObject graphNarrativeObject = viewContainerNarrativeObject.GetComponent<GraphNarrativeObject>();

                    graphNarrativeObject.rootNarrativeObject = narrativeObject;
                }
                else if (viewContainerNarrativeObject is GroupNarrativeObject)
                {
                    // TODO: Do nothing but perhaps add as candidate?
                }
                else if (viewContainerNarrativeObject is LayerNarrativeObject)
                {
                    LayerNarrativeObject layerNarrativeObject = viewContainerNarrativeObject.GetComponent<LayerNarrativeObject>();

                    layerNarrativeObject.primaryLayerRootNarrativeObject = narrativeObject;
                    layerNarrativeObject.LayerSelectionDecisionPoint.AddCandidate(narrativeObject);
                }
                else
                {
                    Debug.LogError("Cannot set root node of narrative object as type is unknown.");

                    return false;
                }
            }

            return true;
        }

        private bool AddNarrativeObjectAsCandidateOfViewContainer(ViewContainer viewContainer, NarrativeObject narrativeObject)
        {
            if (viewContainer == null || viewContainer.narrativeObjectGuid == rootViewContainerGuid)
            {
                // View container is null or is root so just return.
                return false;
            }

            // Find the narrative object which has the same guid as the current view container.
            NarrativeObject viewContainerNarrativeObject = GetNarrativeObject(viewContainer.narrativeObjectGuid);

            if (viewContainerNarrativeObject == null)
            {
                Debug.LogError($"Narrative Object with guid {narrativeObject.guid} does not exist.");

                return false;
            }

            if (viewContainerNarrativeObject is GroupNarrativeObject)
            {
                GroupNarrativeObject groupNarrativeObject = viewContainerNarrativeObject.GetComponent<GroupNarrativeObject>();

                groupNarrativeObject.GroupSelectionDecisionPoint.AddCandidate(narrativeObject);
            }
            else if (viewContainerNarrativeObject is LayerNarrativeObject)
            {
                LayerNarrativeObject layerNarrativeObject = viewContainerNarrativeObject.GetComponent<LayerNarrativeObject>();

                layerNarrativeObject.LayerSelectionDecisionPoint.AddCandidate(narrativeObject);
            }
            else
            {
                // View container type does not have candidates
                return false;
            }

            return true;
        }

        private bool RemoveNarrativeObjectAsCandidateOfViewContainer(ViewContainer viewContainer, NarrativeObject narrativeObject)
        {
            if (viewContainer == null || viewContainer.narrativeObjectGuid == rootViewContainerGuid)
            {
                // View container is null or is root so just return.
                return false;
            }

            // Find the narrative object which has the same guid as the current view container.
            NarrativeObject viewContainerNarrativeObject = GetNarrativeObject(viewContainer.narrativeObjectGuid);

            if (viewContainerNarrativeObject == null)
            {
                Debug.LogError($"Narrative Object with guid {narrativeObject.guid} does not exist.");

                return false;
            }

            if (viewContainerNarrativeObject is GroupNarrativeObject)
            {
                GroupNarrativeObject groupNarrativeObject = viewContainerNarrativeObject.GetComponent<GroupNarrativeObject>();

                groupNarrativeObject.GroupSelectionDecisionPoint.RemoveCandidate(narrativeObject);
            }
            else if (viewContainerNarrativeObject is LayerNarrativeObject)
            {
                LayerNarrativeObject layerNarrativeObject = viewContainerNarrativeObject.GetComponent<LayerNarrativeObject>();

                layerNarrativeObject.LayerSelectionDecisionPoint.RemoveCandidate(narrativeObject);
            }
            else
            {
                // View container type does not have candidates
                return false;
            }

            return true;

        }

        /// <summary>
        /// Query whether a view container has a root narrative object set.
        /// </summary>
        /// <param name="viewContainer"></param>
        /// <returns></returns>
        private bool ViewContainerHasRootNarrativeObject(ViewContainer viewContainer)
        {
            if (viewContainer.narrativeObjectGuid == rootViewContainerGuid)
            {
                NarrativeSpace narrativeSpace = CuttingRoomEditorUtils.GetOrCreateNarrativeSpace();

                if (narrativeSpace == null)
                {
                    return false;
                }

                return narrativeSpace.RootNarrativeObject != null;
            }
            else
            {
                // Find the narrative object which has the same guid as the current view container.
                NarrativeObject viewContainerNarrativeObject = GetNarrativeObject(viewContainer.narrativeObjectGuid);

                if (viewContainerNarrativeObject == null)
                {
                    Debug.LogError($"Narrative Object with guid {viewContainer.narrativeObjectGuid} does not exist.");

                    return false;
                }

                if (viewContainerNarrativeObject is GraphNarrativeObject)
                {
                    GraphNarrativeObject graphNarrativeObject = viewContainerNarrativeObject.GetComponent<GraphNarrativeObject>();

                    return graphNarrativeObject.rootNarrativeObject != null;
                }
                else if (viewContainerNarrativeObject is GroupNarrativeObject)
                {
                    // TODO: No roots inside a group.

                    return false;
                }
                else if (viewContainerNarrativeObject is LayerNarrativeObject)
                {
                    LayerNarrativeObject layerNarrativeObject = viewContainerNarrativeObject.GetComponent<LayerNarrativeObject>();

                    return layerNarrativeObject.primaryLayerRootNarrativeObject != null;
                }
                else
                {
                    Debug.LogError("Cannot determine if root exists as ");
                }
            }

            return false;
        }

    }
}
