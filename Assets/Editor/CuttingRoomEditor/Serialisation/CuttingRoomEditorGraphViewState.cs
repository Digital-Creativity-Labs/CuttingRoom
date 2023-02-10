using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using UnityEngine;

namespace CuttingRoom.Editor
{
    [Serializable]
    public class CuttingRoomEditorGraphViewState : ScriptableObject
    {
        /// <summary>
        /// The states of the narrative object nodes making up the graph.
        /// </summary>
        public List<NarrativeObjectNodeState> narrativeObjectNodeStates = new();

        /// <summary>
        /// The states of the narrative object nodes making up the graph.
        /// </summary>
        public Dictionary<string, NarrativeObjectNodeState> NarrativeObjectNodeStateLookup { get; private set; } = new();

        /// <summary>
        /// The states of the view containers in the graph.
        /// </summary>
        public List<ViewContainerState> viewContainerStates = new();

        /// <summary>
        /// The states of the view containers in the graph.
        /// </summary>
        public Dictionary<string, ViewContainerState> ViewContainerStateLookup { get; private set; } = new();

        /// <summary>
        /// The guids of the view containers making up the view stack.
        /// </summary>
        public List<string> viewContainerStackGuids = new();

        public void UpdateState(CuttingRoomEditorGraphViewState graphViewState)
        {
            if (graphViewState != null)
            {
                NarrativeObjectNodeStateLookup = graphViewState.NarrativeObjectNodeStateLookup;
                narrativeObjectNodeStates = NarrativeObjectNodeStateLookup.Values.ToList();
                ViewContainerStateLookup = graphViewState.ViewContainerStateLookup;
                viewContainerStates = ViewContainerStateLookup.Values.ToList();
                viewContainerStackGuids = graphViewState.viewContainerStackGuids;
            }
        }

        public void UpdateState(string viewContainerNarrativeObjectGuid, ViewContainerState viewContainerState)
        {
            if (viewContainerState != null)
            {
                ViewContainerStateLookup[viewContainerNarrativeObjectGuid] = viewContainerState;
            }
            else
            {
                ViewContainerStateLookup.Remove(viewContainerNarrativeObjectGuid);
            }
            viewContainerStates = ViewContainerStateLookup.Values.ToList();
        }

        public void UpdateState(string narrativeObjectGuid, NarrativeObjectNodeState narrativeObjectNodeState)
        {
            if (narrativeObjectNodeState != null)
            {
                NarrativeObjectNodeStateLookup[narrativeObjectGuid] = narrativeObjectNodeState;
            }
            else
            {
                NarrativeObjectNodeStateLookup.Remove(narrativeObjectGuid);
            }
            narrativeObjectNodeStates = NarrativeObjectNodeStateLookup.Values.ToList();
        }

        public void UpdateViewStackGuids(List<string> viewContainerStackGuids)
        {
            if (viewContainerStackGuids != null)
            {
                this.viewContainerStackGuids = viewContainerStackGuids;
            }
        }

        public void OnEnable()
        {
            NarrativeObjectNodeStateLookup = narrativeObjectNodeStates.ToDictionary(val => val.narrativeObjectGuid);
            ViewContainerStateLookup = viewContainerStates.ToDictionary(val => val.narrativeObjectGuid);
        }
    }
}