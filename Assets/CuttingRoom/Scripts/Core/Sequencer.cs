using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

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

        public int sequenceDepth = 0;

        /// <summary>
        /// Class to pair Narrative Object with cancellation token.
        /// </summary>
        public class SequencedNarrativeObject
        {
            public NarrativeObject narrativeObject = null;
            public int sequenceDepth = 0;
            public CancellationToken? cancellationToken = null;

            public SequencedNarrativeObject(NarrativeObject narrativeObject, CancellationToken? cancellationToken, int sequenceDepth)
            {
                this.narrativeObject = narrativeObject;
                this.cancellationToken = cancellationToken;
                this.sequenceDepth = sequenceDepth;
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
        /// List of sub sequences. These run in parallel.
        /// </summary>
        private List<Sequencer> subSequences = new List<Sequencer>();

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
        static public List<SequencedNarrativeObject> SequenceHistory { get; private set; } = new();

        /// <summary>
        /// Safely record Narrative Object to history.
        /// </summary>
        /// <param name="narrativeObject"></param>
        static public void RecordToHistory(SequencedNarrativeObject sequencedNarrativeObject)
        {
            if (sequencedNarrativeObject != null)
            {
                lock (SequenceHistory)
                {
                    CurrentNarrativeObject = sequencedNarrativeObject.narrativeObject;
                    SequenceHistory.Add(sequencedNarrativeObject);

#if UNITY_EDITOR
                    LogSequence();
#endif
                }
            }
        }

#if UNITY_EDITOR
        static void LogSequence()
        {
            // Log sequence
            string history = "";
            int currentDepth = 0;

            foreach (var playedNarrativeObject in SequenceHistory)
            {
                int nodeDepth = playedNarrativeObject.sequenceDepth;
                string depthStartMarker = string.Empty;
                string depthEndMarker = string.Empty;
                if (nodeDepth > currentDepth)
                {
                    for (int i = currentDepth; i < nodeDepth; ++i)
                    {
                        depthStartMarker += "[";
                    }
                    currentDepth = nodeDepth;
                }
                else if (nodeDepth < currentDepth)
                {
                    for (int i = currentDepth; i > nodeDepth; --i)
                    {
                        depthEndMarker += "]";
                    }
                    currentDepth = nodeDepth;
                }
                if (!string.IsNullOrEmpty(history))
                {
                    history = $"{history}{depthEndMarker} -> {depthStartMarker}";
                }
                history = $"{history}{playedNarrativeObject.narrativeObject.name}";
            }
            Debug.Log(history);
        }
#endif

        /// <summary>
        /// Constructor for sequencer.
        /// </summary>
        /// <param name="rootNarrativeObject"></param>
        /// <param name="narrativeSpace"></param>
        /// <param name="autoStartProcessing"></param>
        public Sequencer(NarrativeObject rootNarrativeObject, NarrativeSpace narrativeSpace = null, bool autoStartProcessing = true, int sequenceDepth = 0)
        {
            this.rootNarrativeObject = rootNarrativeObject;
            if (narrativeSpace != null)
            {
                NarrativeSpace = narrativeSpace;
            }
            else
            {
                NarrativeSpace = UnityEngine.Object.FindObjectOfType<NarrativeSpace>();
            }

            this.autoStartProcessing = autoStartProcessing;
            this.sequenceDepth = sequenceDepth;
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
                SequencedNarrativeObject sequencedNarrativeObject = narrativeObjectSequenceQueue.Dequeue();

                // Record new item on sequence
                CurrentNarrativeObjectForSequence = sequencedNarrativeObject.narrativeObject;
                RecordToHistory(sequencedNarrativeObject);

                yield return ProcessNarrativeObject(sequencedNarrativeObject.narrativeObject, sequencedNarrativeObject.cancellationToken);

                // Once complete clear the sub sequences
                subSequences.Clear();
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
                narrativeObjectSequenceQueue.Enqueue(new SequencedNarrativeObject(narrativeObject, cancellationToken, sequenceDepth));
            }
        }

        /// <summary>
        /// Sequence a narrative object to be processed in parrallel to current sequence.
        /// </summary>
        /// <param name="rootNarrativeObject"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Sequencer StartSubSequence(NarrativeObject rootNarrativeObject, CancellationToken? cancellationToken = null)
        {
            Sequencer subSequence = new(rootNarrativeObject, NarrativeSpace, autoStartProcessing, sequenceDepth + 1);
            subSequence.Start(cancellationToken);
            subSequences.Add(subSequence);

            return subSequence;
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
