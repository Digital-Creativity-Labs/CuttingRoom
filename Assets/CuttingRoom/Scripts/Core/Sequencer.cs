using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace CuttingRoom
{
    public class Sequencer : MonoBehaviour
    {
        /// <summary>
        /// The narrative space being processed by this sequencer.
        /// </summary>
        [SerializeField]
        private NarrativeSpace narrativeSpace = null;

        /// <summary>
        /// Get accessor for the narrative space being processed by this sequencer.
        /// </summary>
        public NarrativeSpace NarrativeSpace { get { return narrativeSpace; } set { narrativeSpace = value; } }

        /// <summary>
        /// Whether processing should start automatically when object is created.
        /// </summary>
        [SerializeField]
        private bool autoStartProcessing = false;

        public bool SequenceComplete { get; private set; }

        public class SequencerLayer
        {

        }

        /// <summary>
        /// Unity event.
        /// </summary>
        private void Start()
        {
            if (autoStartProcessing)
            {
                StartProcessing();
            }
        }

        public IEnumerator WaitForSequenceComplete()
        {
            yield return new WaitUntil(() => { return SequenceComplete; });
        }

        /// <summary>
        /// Start processing the specified narrative space.
        /// </summary>
        public void StartProcessing()
        {
            StartCoroutine(ProcessingCoroutine());
        }

        /// <summary>
        /// Coroutine for processing narrative objects.
        /// </summary>
        /// <returns></returns>
        private IEnumerator ProcessingCoroutine()
        {
            List<Coroutine> narrativeObjectProcessCoroutines = new List<Coroutine>();

            narrativeObjectProcessCoroutines.Add(StartCoroutine(SequenceNarrativeObject(narrativeSpace.RootNarrativeObject)));

            foreach (Coroutine narrativeObjectProcessCoroutine in narrativeObjectProcessCoroutines)
            {
                yield return narrativeObjectProcessCoroutine;
            }
        }

        /// <summary>
        /// Sequence a narrative object to be processed.
        /// </summary>
        /// <param name="narrativeObject"></param>
        /// <returns></returns>
        public IEnumerator SequenceNarrativeObject(NarrativeObject narrativeObject, CancellationToken? cancellationToken = null)
        {
            if (narrativeObject is AtomicNarrativeObject)
            {
                AtomicNarrativeObject atomicNarrativeObject = narrativeObject as AtomicNarrativeObject;

                yield return StartCoroutine(SequenceAtomicNarrativeObject(atomicNarrativeObject, cancellationToken));
            }
            else if (narrativeObject is GroupNarrativeObject)
            {
                GroupNarrativeObject groupNarrativeObject = narrativeObject as GroupNarrativeObject;

                yield return StartCoroutine(SequencerGroupNarrativeObject(groupNarrativeObject, cancellationToken));
            }
            else if (narrativeObject is GraphNarrativeObject)
            {
                GraphNarrativeObject graphNarrativeObject = narrativeObject as GraphNarrativeObject;

                yield return StartCoroutine(SequencerGraphNarrativeObject(graphNarrativeObject, cancellationToken));
            }
            else if (narrativeObject is LayerNarrativeObject)
            {
                LayerNarrativeObject layerNarrativeObject = narrativeObject as LayerNarrativeObject;

                yield return StartCoroutine(SequencerLayerNarrativeObject(layerNarrativeObject, cancellationToken));
            }
        }

        /// <summary>
        /// Sequence an atomic for processing.
        /// </summary>
        /// <param name="atomicNarrativeObject"></param>
        /// <returns></returns>
        private IEnumerator SequenceAtomicNarrativeObject(AtomicNarrativeObject atomicNarrativeObject, CancellationToken? cancellationToken = null)
        {
            AtomicNarrativeObjectProcessing atomicNarrativeObjectProcessing = new AtomicNarrativeObjectProcessing(atomicNarrativeObject);

            yield return StartCoroutine(atomicNarrativeObjectProcessing.Process(this, cancellationToken));
        }

        /// <summary>
        /// Sequence a group for processing.
        /// </summary>
        /// <param name="groupNarrativeObject"></param>
        /// <returns></returns>
        private IEnumerator SequencerGroupNarrativeObject(GroupNarrativeObject groupNarrativeObject, CancellationToken? cancellationToken = null)
        {
            GroupNarrativeObjectProcessing groupNarrativeObjectProcessing = new GroupNarrativeObjectProcessing(groupNarrativeObject);

            yield return StartCoroutine(groupNarrativeObjectProcessing.Process(this, cancellationToken));
        }

        /// <summary>
        /// Sequence a graph for processing.
        /// </summary>
        /// <param name="graphNarrativeObject"></param>
        /// <returns></returns>
        private IEnumerator SequencerGraphNarrativeObject(GraphNarrativeObject graphNarrativeObject, CancellationToken? cancellationToken = null)
        {
            GraphNarrativeObjectProcessing graphNarrativeObjectProcessing = new GraphNarrativeObjectProcessing(graphNarrativeObject);

            yield return StartCoroutine(graphNarrativeObjectProcessing.Process(this, cancellationToken));
        }

        /// <summary>
        /// Sequence a layer for processing.
        /// </summary>
        /// <param name="layerNarrativeObject"></param>
        /// <returns></returns>
        private IEnumerator SequencerLayerNarrativeObject(LayerNarrativeObject layerNarrativeObject, CancellationToken? cancellationToken = null)
        {
            LayerNarrativeObjectProcessing layerNarrativeObjectProcessing = new LayerNarrativeObjectProcessing(layerNarrativeObject);

            yield return StartCoroutine(layerNarrativeObjectProcessing.Process(this, cancellationToken));
        }
    }
}
