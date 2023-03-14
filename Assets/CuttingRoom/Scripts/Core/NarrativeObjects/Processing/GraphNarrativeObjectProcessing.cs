using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

namespace CuttingRoom
{
    public class GraphNarrativeObjectProcessing : NarrativeObjectProcessing
    {
        /// <summary>
        /// The graph narrative object being processed by this object.
        /// </summary>
        public GraphNarrativeObject GraphNarrativeObject { get { return narrativeObject as GraphNarrativeObject; } }


        private CancellationTokenSource graphCancellationToken = new CancellationTokenSource();

        /// <summary>
        /// The sequencer processing this object.
        /// Cached to pass to selections made by the group.
        /// </summary>
        private Sequencer sequencer = null;

        private Sequencer subSequencer = null;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="graphNarrativeObject"></param>
        public GraphNarrativeObjectProcessing(GraphNarrativeObject graphNarrativeObject)
        {
            narrativeObject = graphNarrativeObject;
            OnCancellation += () => { graphCancellationToken.Cancel(); };
        }

        /// <summary>
        /// Processing method for a graph narrative object.
        /// </summary>
        /// <param name="sequencer"></param>
        /// <returns></returns>
        public override IEnumerator Process(Sequencer sequencer, CancellationToken? cancellationToken = null)
        {
            this.sequencer = sequencer;

            OnProcessingTriggerComplete += GraphEndTriggered;

            // Process from the defined root.
            subSequencer = sequencer.AddSubSequence(GraphNarrativeObject.rootNarrativeObject, autoStartSequence: true, graphCancellationToken);
            contentCoroutine = GraphNarrativeObject.StartCoroutine(subSequencer.WaitForSequenceComplete());

            // Process the base functionality, output selection.
            yield return base.Process(sequencer, cancellationToken);
        }

        /// <summary>
        /// End graph according to graph node processing trigger.
        /// </summary>
        private void GraphEndTriggered()
        {
            graphCancellationToken.Cancel();
        }
    }
}
