using System;
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

        private CancellationTokenSource rootCancellationTokenSource = null;

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
        public Sequencer(NarrativeObject rootNarrativeObject, NarrativeSpace narrativeSpace = null, CancellationTokenSource cancellationTokenSource = null, int sequenceDepth = 0)
        {
            this.rootNarrativeObject = rootNarrativeObject;
            this.rootCancellationTokenSource = cancellationTokenSource;
            if (rootCancellationTokenSource == null)
            {
                rootCancellationTokenSource = new();
            }
            if (narrativeSpace != null)
            {
                NarrativeSpace = narrativeSpace;
            }
            else
            {
                NarrativeSpace = UnityEngine.Object.FindObjectOfType<NarrativeSpace>();
            }

            this.sequenceDepth = sequenceDepth;
        }

        /// <summary>
        /// Start Function.
        /// </summary>
        /// <param name="cancellationToken"></param>
        public void Start()
        {
            if (NarrativeSpace != null && rootNarrativeObject != null)
            {
                sequenceCoroutine = NarrativeSpace.StartCoroutine(ProcessingCoroutine(rootCancellationTokenSource.Token));
            }
        }

        public void Stop()
        {
            rootCancellationTokenSource?.Cancel();  
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

            SequencedNarrativeObject previousNarrativeObject = null;
            SequencedNarrativeObject currentNarrativeObject = null;
            while (narrativeObjectSequenceQueue.Count > 0 && !rootCancellationTokenSource.IsCancellationRequested)
            {
                currentNarrativeObject = narrativeObjectSequenceQueue.Dequeue();

                if (currentNarrativeObject != null)
                {
                    // Record new item on sequence
                    RecordToHistory(currentNarrativeObject);
                    CurrentNarrativeObjectForSequence = currentNarrativeObject.narrativeObject;

                    currentNarrativeObject.narrativeObject.PreProcess();

                    bool processComplete = false;
                    Coroutine processingCoroutine = NarrativeSpace.StartCoroutine(ProcessNarrativeObject(currentNarrativeObject.narrativeObject, () =>
                    {
                        processComplete = true;
                    }, currentNarrativeObject.cancellationToken));

                    if (currentNarrativeObject.narrativeObject.OutputSelectionDecisionPoint != null
                        && currentNarrativeObject.narrativeObject.OutputSelectionDecisionPoint.Candidates.Count > 0)
                    {
                        foreach (var candidate in currentNarrativeObject.narrativeObject.OutputSelectionDecisionPoint.Candidates)
                        {
                            candidate.PreProcess();
                        }
                    }

                    yield return new WaitUntil(() => processComplete || rootCancellationTokenSource.IsCancellationRequested );

                    currentNarrativeObject.narrativeObject.PostProcess();
                    NarrativeSpace.StopCoroutine(processingCoroutine);

                    // Once complete clear the sub sequences
                    TerminateSubSequences();
                }
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
        /// Add a sub sequence to be processed in parrallel to current sequence.
        /// </summary>
        /// <param name="rootNarrativeObject"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="autoStartSequence"></param>
        /// <returns></returns>
        public Sequencer AddSubSequence(NarrativeObject rootNarrativeObject, bool autoStartSequence = false, CancellationTokenSource cancellationTokenSource = null)
        {
            Sequencer subSequence = new(rootNarrativeObject, NarrativeSpace, cancellationTokenSource, sequenceDepth + 1);
            subSequences.Add(subSequence);
            if (autoStartSequence)
            {
                subSequence.Start();
            }

            return subSequence;
        }

        public void TerminateSubSequences()
        {
            foreach (var subSequence in subSequences)
            {
                subSequence.Stop();
            }
            subSequences.Clear();
        }

        /// <summary>
        /// Start coroutine to process narrative object.
        /// </summary>
        /// <param name="narrativeObject"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public IEnumerator ProcessNarrativeObject(NarrativeObject narrativeObject, Action onProcessingComplete, CancellationToken? cancellationToken = null)
        {
            if (NarrativeSpace == null)
            {
                yield return null;
                onProcessingComplete?.Invoke();
            }
            if (narrativeObject is AtomicNarrativeObject)
            {
                AtomicNarrativeObject atomicNarrativeObject = narrativeObject as AtomicNarrativeObject;

                yield return NarrativeSpace.StartCoroutine(SequenceAtomicNarrativeObject(atomicNarrativeObject, cancellationToken));
            }
            else if (narrativeObject is GroupNarrativeObject)
            {
                GroupNarrativeObject groupNarrativeObject = narrativeObject as GroupNarrativeObject;

                yield return NarrativeSpace.StartCoroutine(SequencerGroupNarrativeObject(groupNarrativeObject, cancellationToken));
            }
            else if (narrativeObject is GraphNarrativeObject)
            {
                GraphNarrativeObject graphNarrativeObject = narrativeObject as GraphNarrativeObject;

                yield return NarrativeSpace.StartCoroutine(SequencerGraphNarrativeObject(graphNarrativeObject, cancellationToken));
            }
            else if (narrativeObject is LayerNarrativeObject)
            {
                LayerNarrativeObject layerNarrativeObject = narrativeObject as LayerNarrativeObject;

                yield return NarrativeSpace.StartCoroutine(SequencerLayerNarrativeObject(layerNarrativeObject, cancellationToken));
            }
            else
            {
                yield return null;
            }

            onProcessingComplete?.Invoke();
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

            yield return atomicNarrativeObjectProcessing.Process(this, cancellationToken);
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

            yield return groupNarrativeObjectProcessing.Process(this, cancellationToken);
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

            yield return graphNarrativeObjectProcessing.Process(this, cancellationToken);
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

            yield return layerNarrativeObjectProcessing.Process(this, cancellationToken);
        }
    }
}
