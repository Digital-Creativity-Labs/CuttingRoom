using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using static CuttingRoom.Sequencer;

namespace CuttingRoom
{
    public class Sequencer
    {
        /// <summary>
        /// The narrative space being processed by this sequencer.
        /// </summary>
        private NarrativeSpace narrativeSpace = null;

        /// <summary>
        /// Get accessor for the narrative space being processed by this sequencer.
        /// </summary>
        public NarrativeSpace NarrativeSpace { get { return narrativeSpace; } private set { narrativeSpace = value; } }

        private NarrativeObject rootNarrativeObject = null;

        /// <summary>
        /// Whether processing should start automatically when object is created.
        /// </summary>
        private bool autoStartProcessing = true;

        private Coroutine sequenceCoroutine = null;

        /// <summary>
        /// Class to pair Narrative Object with cancellation token.
        /// </summary>
        public class SequencedNarrativeObject
        {
            public NarrativeObject narrativeObject = null;
            public CancellationToken? cancellationToken = null;

            public SequencedNarrativeObject(NarrativeObject narrativeObject, CancellationToken? cancellationToken)
            {
                this.narrativeObject = narrativeObject;
                this.cancellationToken = cancellationToken;
            }
        }

        /// <summary>
        /// Boolean flag indicating if the sequence is complete.
        /// </summary>
        public bool SequenceComplete { get; private set; } = false;

        /// <summary>
        /// Narrative object queue for processing.
        /// </summary>
        private Queue<SequencedNarrativeObject> narrativeObjectSequenceQueue = new Queue<SequencedNarrativeObject>();

        /// <summary>
        /// Reference to latest processing Narrative Object for this sequence.
        /// </summary>
        public NarrativeObject CurrentNarrativeObjectForSequence { get; private set; } = null;

        /// <summary>
        /// Static reference to latest processing Narrative Object.
        /// </summary>
        static public NarrativeObject CurrentNarrativeObject { get; private set; } = null;

        /// <summary>
        /// Static sequence history. Updated by all sequences/ sub sequences.
        /// </summary>
        static public List<NarrativeObject> SequenceHistory { get; private set; } = new();

        /// <summary>
        /// Safely record Narrative Object to history.
        /// </summary>
        /// <param name="narrativeObject"></param>
        static public void RecordToHistory(NarrativeObject narrativeObject)
        {
            if (narrativeObject != null)
            {
                lock (SequenceHistory)
                {
                    CurrentNarrativeObject = narrativeObject;
                    SequenceHistory.Add(narrativeObject);
                }
            }
        }

        /// <summary>
        /// Constructor for sequencer.
        /// </summary>
        /// <param name="rootNarrativeObject"></param>
        /// <param name="narrativeSpace"></param>
        /// <param name="autoStartProcessing"></param>
        public Sequencer(NarrativeObject rootNarrativeObject, NarrativeSpace narrativeSpace = null, bool autoStartProcessing = true)
        {
            this.rootNarrativeObject = rootNarrativeObject;
            if (narrativeSpace != null)
            {
                NarrativeSpace = narrativeSpace;
            }
            else
            {
                NarrativeSpace = Object.FindObjectOfType<NarrativeSpace>();
            }

            this.autoStartProcessing = autoStartProcessing;
        }

        /// <summary>
        /// Start Function.
        /// </summary>
        /// <param name="cancellationToken"></param>
        public void Start(CancellationToken? cancellationToken = null)
        {
            if (autoStartProcessing)
            {
                if (NarrativeSpace != null && rootNarrativeObject != null)
                {
                    sequenceCoroutine = NarrativeSpace.StartCoroutine(ProcessingCoroutine(cancellationToken));
                }
            }
        }

        /// <summary>
        /// Coroutine for waiting for sequence to complete.
        /// </summary>
        /// <returns></returns>
        public IEnumerator WaitForSequenceComplete()
        {
            if (sequenceCoroutine != null)
            {
                yield return new WaitUntil(() => { return SequenceComplete; });
            }
        }

        /// <summary>
        /// Coroutine for processing sequence.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private IEnumerator ProcessingCoroutine(CancellationToken? cancellationToken = null)
        {
            SequenceNarrativeObject(rootNarrativeObject, cancellationToken);

            while (narrativeObjectSequenceQueue.Count > 0)
            {
#if UNITY_EDITOR
                // Log sequence
                string history = "";
                foreach (var narrativeObject in SequenceHistory)
                {
                    history = $"{history}{narrativeObject.name} -> ";
                }
                Debug.Log(history);
#endif
                SequencedNarrativeObject sequencedNarrativeObject = narrativeObjectSequenceQueue.Dequeue();
                yield return ProcessNarrativeObject(sequencedNarrativeObject.narrativeObject, sequencedNarrativeObject.cancellationToken);
            }

            SequenceComplete = true;
        }

        /// <summary>
        /// Sequence a narrative object to be processed.
        /// </summary>
        /// <param name="narrativeObject"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public void SequenceNarrativeObject(NarrativeObject narrativeObject, CancellationToken? cancellationToken = null)
        {
            if (NarrativeSpace != null)
            {
                narrativeObjectSequenceQueue.Enqueue(new SequencedNarrativeObject(narrativeObject, cancellationToken));
            }
        }

        /// <summary>
        /// Start coroutine to process narrative object.
        /// </summary>
        /// <param name="narrativeObject"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Coroutine ProcessNarrativeObject(NarrativeObject narrativeObject, CancellationToken? cancellationToken = null)
        {
            if (NarrativeSpace == null)
            {
                return null;
            }

            Coroutine coroutine = null;
            if (narrativeObject is AtomicNarrativeObject)
            {
                AtomicNarrativeObject atomicNarrativeObject = narrativeObject as AtomicNarrativeObject;

                coroutine = NarrativeSpace.StartCoroutine(SequenceAtomicNarrativeObject(atomicNarrativeObject, cancellationToken));
            }
            else if (narrativeObject is GroupNarrativeObject)
            {
                GroupNarrativeObject groupNarrativeObject = narrativeObject as GroupNarrativeObject;

                coroutine = NarrativeSpace.StartCoroutine(SequencerGroupNarrativeObject(groupNarrativeObject, cancellationToken));
            }
            else if (narrativeObject is GraphNarrativeObject)
            {
                GraphNarrativeObject graphNarrativeObject = narrativeObject as GraphNarrativeObject;

                coroutine = NarrativeSpace.StartCoroutine(SequencerGraphNarrativeObject(graphNarrativeObject, cancellationToken));
            }
            else if (narrativeObject is LayerNarrativeObject)
            {
                LayerNarrativeObject layerNarrativeObject = narrativeObject as LayerNarrativeObject;

                coroutine = NarrativeSpace.StartCoroutine(SequencerLayerNarrativeObject(layerNarrativeObject, cancellationToken));
            }

            // Record new item on sequence
            CurrentNarrativeObjectForSequence = narrativeObject;
            RecordToHistory(CurrentNarrativeObjectForSequence);

            return coroutine;
        }

        /// <summary>
        /// Sequence an atomic for processing.
        /// </summary>
        /// <param name="atomicNarrativeObject"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private IEnumerator SequenceAtomicNarrativeObject(AtomicNarrativeObject atomicNarrativeObject, CancellationToken? cancellationToken = null)
        {
            AtomicNarrativeObjectProcessing atomicNarrativeObjectProcessing = new AtomicNarrativeObjectProcessing(atomicNarrativeObject);

            yield return NarrativeSpace.StartCoroutine(atomicNarrativeObjectProcessing.Process(this, cancellationToken));
        }

        /// <summary>
        /// Sequence a group for processing.
        /// </summary>
        /// <param name="groupNarrativeObject"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private IEnumerator SequencerGroupNarrativeObject(GroupNarrativeObject groupNarrativeObject, CancellationToken? cancellationToken = null)
        {
            GroupNarrativeObjectProcessing groupNarrativeObjectProcessing = new GroupNarrativeObjectProcessing(groupNarrativeObject);

            yield return NarrativeSpace.StartCoroutine(groupNarrativeObjectProcessing.Process(this, cancellationToken));
        }

        /// <summary>
        /// Sequence a graph for processing.
        /// </summary>
        /// <param name="graphNarrativeObject"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private IEnumerator SequencerGraphNarrativeObject(GraphNarrativeObject graphNarrativeObject, CancellationToken? cancellationToken = null)
        {
            GraphNarrativeObjectProcessing graphNarrativeObjectProcessing = new GraphNarrativeObjectProcessing(graphNarrativeObject);

            yield return NarrativeSpace.StartCoroutine(graphNarrativeObjectProcessing.Process(this, cancellationToken));
        }

        /// <summary>
        /// Sequence a layer for processing.
        /// </summary>
        /// <param name="layerNarrativeObject"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private IEnumerator SequencerLayerNarrativeObject(LayerNarrativeObject layerNarrativeObject, CancellationToken? cancellationToken = null)
        {
            LayerNarrativeObjectProcessing layerNarrativeObjectProcessing = new LayerNarrativeObjectProcessing(layerNarrativeObject);

            yield return NarrativeSpace.StartCoroutine(layerNarrativeObjectProcessing.Process(this, cancellationToken));
        }
    }
}
