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

        private Sequencer subSequencer = null;

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
            yield return LayerNarrativeObject.LayerSelectionDecisionPoint.Process(OnSelection);

            // Process the base functionality, output selection.
            yield return base.Process(sequencer, cancellationToken);

            yield return WaitForSecondaryLayersToEnd();

            LayerNarrativeObject.PostProcess();
        }

        /// <summary>
        /// End graph according to graph node processing trigger.
        /// </summary>
        private void LayerEndTriggered()
        {
            layerCancellationToken.Cancel();
        }

        private IEnumerator WaitForSecondaryLayersToEnd()
        {
            foreach (var secondaryLayerCoroutine in secondaryLayerCoroutines)
            {
                yield return secondaryLayerCoroutine;
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
                        subSequencer = new(layerRoot);
                        subSequencer.Start(layerCancellationToken.Token);
                        contentCoroutine = LayerNarrativeObject.StartCoroutine(subSequencer.WaitForSequenceComplete());
                    }
                    else
                    {
                        secondaryLayerCoroutines.Add(sequencer.ProcessNarrativeObject(layerRoot, layerCancellationToken.Token));
                    }
                }    
            }
            // Nothing asynchronous needed here so return null.
            yield return null;
        }
    }
}
