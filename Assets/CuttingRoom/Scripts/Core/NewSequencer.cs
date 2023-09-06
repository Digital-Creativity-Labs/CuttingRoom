using CuttingRoom;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.CuttingRoom.Scripts.Core
{
    public class NewSequencer
    {

        /// <summary>
        /// Class to pair Narrative Object with cancellation token.
        /// </summary>
        public class SequencedNarrativeObject
        {
            public NarrativeObject narrativeObject;
            public int sequenceDepth = 0;
            public CancellationToken cancellationToken;
            public NarrativeObjectProcessing processor;

            public SequencedNarrativeObject(NarrativeObject narrativeObject, NarrativeObjectProcessing processor, CancellationToken cancellationToken, int sequenceDepth)
            {
                this.narrativeObject = narrativeObject;
                this.processor = processor;
                this.cancellationToken = cancellationToken;
                this.sequenceDepth = sequenceDepth;
            }
        }

        /// <summary>
        /// The narrative space being processed by this sequencer.
        /// </summary>
        private NarrativeSpace narrativeSpace = null;

        /// <summary>
        /// Get accessor for the narrative space being processed by this sequencer.
        /// </summary>
        public NarrativeSpace NarrativeSpace { get { return narrativeSpace; } private set { narrativeSpace = value; } }

        private NarrativeObject rootNarrativeObject = null;

        private Coroutine sequenceCoroutine = null;

        private CancellationTokenSource sequencerCancellationTokenSource = new();

        public int sequenceDepth = 0;

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
        static public SequencedNarrativeObject CurrentNarrativeObject { get; private set; } = null;

        /// <summary>
        /// Static reference to the next processing Narrative Object.
        /// </summary>
        static public SequencedNarrativeObject NextNarrativeObject { get; private set; } = null;

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
                    CurrentNarrativeObject = sequencedNarrativeObject;
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
        public NewSequencer(NarrativeObject rootNarrativeObject, NarrativeSpace narrativeSpace = null, CancellationTokenSource cancellationTokenSource = null, int sequenceDepth = 0)
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

            if (cancellationTokenSource != null)
            {
                sequencerCancellationTokenSource = cancellationTokenSource;
            }

            this.sequenceDepth = sequenceDepth;
        }

        /// <summary>
        /// Sequence a narrative object to be processed.
        /// </summary>
        /// <param name="narrativeObject"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public void SequenceNarrativeObject(NarrativeObject narrativeObject)
        {
            if (NarrativeSpace != null)
            {
                NarrativeObjectProcessing processor = null;
                if (narrativeObject is AtomicNarrativeObject)
                {
                    AtomicNarrativeObject atomicNarrativeObject = narrativeObject as AtomicNarrativeObject;
                    processor = new AtomicNarrativeObjectProcessing(atomicNarrativeObject);
                }
                else if (narrativeObject is GroupNarrativeObject)
                {
                    GroupNarrativeObject groupNarrativeObject = narrativeObject as GroupNarrativeObject;
                    processor = new GroupNarrativeObjectProcessing(groupNarrativeObject);
                }
                else if (narrativeObject is GraphNarrativeObject)
                {
                    GraphNarrativeObject graphNarrativeObject = narrativeObject as GraphNarrativeObject;
                    processor = new GraphNarrativeObjectProcessing(graphNarrativeObject);
                }
                else if (narrativeObject is LayerNarrativeObject)
                {
                    LayerNarrativeObject layerNarrativeObject = narrativeObject as LayerNarrativeObject;
                    processor = new LayerNarrativeObjectProcessing(layerNarrativeObject);
                }

                if (processor != null)
                {
                    narrativeObjectSequenceQueue.Enqueue(new SequencedNarrativeObject(narrativeObject, processor, sequencerCancellationTokenSource.Token, sequenceDepth));
                }
            }
        }

        public void Start()
        {
            if (narrativeObjectSequenceQueue != null && narrativeObjectSequenceQueue.Count > 0)
            {
                // Pre process first narrative object
                SequencedNarrativeObject firstSequencedNarrativeObject = narrativeObjectSequenceQueue.Peek();
                firstSequencedNarrativeObject.processor.PreProcess(this, firstSequencedNarrativeObject.cancellationToken);
                sequenceCoroutine = NarrativeSpace.StartCoroutine(ProcessingCoroutine());
            }
        }

        public void Stop()
        {
            sequencerCancellationTokenSource?.Cancel();
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

        private IEnumerator ProcessingCoroutine()
        {
            SequenceNarrativeObject(rootNarrativeObject);

            while (narrativeObjectSequenceQueue.Count > 0)
            {
                SequencedNarrativeObject sequencedNarrativeObject = narrativeObjectSequenceQueue.Dequeue();

                // Record new item on sequence
                CurrentNarrativeObjectForSequence = sequencedNarrativeObject.narrativeObject;
                RecordToHistory(sequencedNarrativeObject);

                Coroutine currentProcessing = NarrativeSpace.StartCoroutine(ProcessNarrativeObject(sequencedNarrativeObject, sequencedNarrativeObject.cancellationToken));

                List<NarrativeObjectProcessing> outputProcessors = new();
                // PreProcess possible outputs
                foreach(var nextNarrativeObject in sequencedNarrativeObject.narrativeObject.OutputSelectionDecisionPoint.Candidates)
                {
                    PreProcessNarrativeObject(nextNarrativeObject);
                }

                yield return currentProcessing;

                // Once complete clear the sub sequences
                subSequences.Clear();
            }

            SequenceComplete = true;
        }

        private void PreProcessNarrativeObject(NarrativeObject narrativeObject)
        {
            NarrativeObjectProcessing processor = null;
            if (narrativeObject is AtomicNarrativeObject)
            {
                AtomicNarrativeObject atomicNarrativeObject = narrativeObject as AtomicNarrativeObject;
                processor = new AtomicNarrativeObjectProcessing(atomicNarrativeObject);
            }
            else if (narrativeObject is GroupNarrativeObject)
            {
                GroupNarrativeObject groupNarrativeObject = narrativeObject as GroupNarrativeObject;
                processor = new GroupNarrativeObjectProcessing(groupNarrativeObject);
            }
            else if (narrativeObject is GraphNarrativeObject)
            {
                GraphNarrativeObject graphNarrativeObject = narrativeObject as GraphNarrativeObject;
                processor = new GraphNarrativeObjectProcessing(graphNarrativeObject);
            }
            else if (narrativeObject is LayerNarrativeObject)
            {
                LayerNarrativeObject layerNarrativeObject = narrativeObject as LayerNarrativeObject;
                processor = new LayerNarrativeObjectProcessing(layerNarrativeObject);
            }

            processor?.PreProcess();
        }



        private IEnumerator ProcessNarrativeObject(SequencedNarrativeObject narrativeObject, CancellationToken cancellationToken)
        {
            if (NarrativeSpace == null)
            {
                yield return null;
            }

            yield return narrativeObject.processor.Process(this, cancellationToken);
        }
    }
}
