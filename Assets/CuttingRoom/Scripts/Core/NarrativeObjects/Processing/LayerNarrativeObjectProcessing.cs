using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

namespace CuttingRoom
{
    public class LayerNarrativeObjectProcessing : NarrativeObjectProcessing
    {
        /// <summary>
        /// The layer narrative object being processed by this object.
        /// </summary>
        public LayerNarrativeObject LayerNarrativeObject { get { return narrativeObject as LayerNarrativeObject; } }


        private CancellationTokenSource layerCancellationToken = new CancellationTokenSource();

        /// <summary>
        /// The sequencer processing this object.
        /// Cached to pass to selections made by the group.
        /// </summary>
        private Sequencer sequencer = null;

        private List<Coroutine> secondaryLayerCoroutines = new List<Coroutine>();

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="graphNarrativeObject"></param>
        public LayerNarrativeObjectProcessing(LayerNarrativeObject layerNarrativeObject)
        {
            narrativeObject = layerNarrativeObject;
            OnCancellation += () => { layerCancellationToken.Cancel(); };
        }

        /// <summary>
        /// Processing method for a graph narrative object.
        /// </summary>
        /// <param name="sequencer"></param>
        /// <returns></returns>
        public override IEnumerator Process(Sequencer sequencer, CancellationToken? cancellationToken = null)
        {
            this.sequencer = sequencer;

            OnProcessingTriggerComplete += LayerEndTriggered;

            LayerNarrativeObject.PreProcess();
            yield return LayerNarrativeObject.LayerSelectionDecisionPoint.Process(sequencer, OnSelection);

            // Process the base functionality, output selection.
            yield return base.Process(sequencer, cancellationToken);

            LayerNarrativeObject.PostProcess();
        }

        /// <summary>
        /// End graph according to graph node processing trigger.
        /// </summary>
        private void LayerEndTriggered()
        {
            layerCancellationToken.Cancel();
            foreach (var secondaryLayerCoroutine in secondaryLayerCoroutines)
            {
                LayerNarrativeObject.StopCoroutine(secondaryLayerCoroutine);
            }
        }

        /// <summary>
        /// Invoked when this layer selects valid layers' root narrative objects.
        /// </summary>
        /// <param name="selection"></param>
        /// <returns></returns>
        public IEnumerator OnSelection(List<NarrativeObject> selection)
        {
            if (selection != null && LayerNarrativeObject != null)
            {
                foreach (var layerRoot in selection)
                {
                    if (layerRoot == LayerNarrativeObject.primaryLayerRootNarrativeObject)
                    {
                        contentCoroutine = LayerNarrativeObject.StartCoroutine(sequencer.SequenceNarrativeObject(layerRoot, layerCancellationToken.Token));
                    }
                    else
                    {
                        secondaryLayerCoroutines.Add(LayerNarrativeObject.StartCoroutine(sequencer.SequenceNarrativeObject(layerRoot, layerCancellationToken.Token)));
                    }
                }    
            }
            // Nothing asynchronous needed here so return null.
            yield return null;
        }
    }
}
