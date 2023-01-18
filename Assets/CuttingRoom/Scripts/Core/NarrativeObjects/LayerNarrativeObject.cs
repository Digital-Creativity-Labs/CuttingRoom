using System.Collections.Generic;
using UnityEngine;

namespace CuttingRoom
{
    [RequireComponent(typeof(LayerSelectionDecisionPoint))]
    public class LayerNarrativeObject : NarrativeObject
    {
        /// <summary>
        /// The layer selection decision point for this group.
        /// </summary>
        [SerializeField]
        private DecisionPoint layerSelectionDecisionPoint = null;

        /// <summary>
        /// The layer selection decision point for this group.
        /// </summary>
        public DecisionPoint LayerSelectionDecisionPoint { get => layerSelectionDecisionPoint; set => layerSelectionDecisionPoint = value; }

        public NarrativeObject primaryLayerRootNarrativeObject = null;
    }
}
