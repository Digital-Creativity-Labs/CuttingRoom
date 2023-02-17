using CuttingRoom.VariableSystem.Variables;
using CuttingRoom.VariableSystem;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace CuttingRoom
{
    public class NarrativeObjectProcessing
    {
        /// <summary>
        /// The narrative object being processed.
        /// </summary>
        protected NarrativeObject narrativeObject = null;

        protected Coroutine contentCoroutine = null;

        protected bool processingEnded = false;

        /// <summary>
        /// The selected output for this narrative object.
        /// </summary>
        private NarrativeObject selectedOutputNarrativeObject = null;

        protected delegate void OnProcessingTriggerCompleteCallback();
        protected event OnProcessingTriggerCompleteCallback OnProcessingTriggerComplete;

        protected delegate void OnProcessingCompleteCallback();
        protected event OnProcessingCompleteCallback OnProcessingComplete;

        protected delegate void OnCancellationCallback();
        protected event OnCancellationCallback OnCancellation;

        /// <summary>
        /// Process this narrative object.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerator Process(Sequencer sequencer, CancellationToken? cancellationToken = null)
        {
            if (narrativeObject.VariableStore != null)
            {
                BoolVariable hasPlayed = narrativeObject.VariableStore.GetVariable(NarrativeObject.hasPlayedTagName) as BoolVariable;
                hasPlayed.Set(true);
            }

            List<Coroutine> endTriggers = new List<Coroutine>();
            if (narrativeObject.EndTriggers != null && narrativeObject.EndTriggers.Count > 0)
            {
                Coroutine cancellationMonitor = null;
                foreach (var trigger in narrativeObject.EndTriggers)
                {
                    if (trigger is EndOfContentTrigger endOfContentTrigger)
                    {
                        endOfContentTrigger.ContentCoroutine = contentCoroutine;
                    }
                    if (trigger != null)
                    {
                        endTriggers.Add(narrativeObject.StartCoroutine(trigger.WaitForProcessingTrigger()));
                    }
                }

                if (cancellationToken.HasValue)
                {
                    cancellationMonitor = narrativeObject.StartCoroutine(MonitorForCancellation(cancellationToken.Value));
                }

                yield return MonitorForProcessComplete();

                foreach (var endTrig in endTriggers)
                {
                    narrativeObject.StopCoroutine(endTrig);
                }
                yield return cancellationMonitor;
            }


            OnProcessingTriggerComplete?.Invoke();

            // Do not queue next node if cancelled
            if (cancellationToken.HasValue && cancellationToken.Value.IsCancellationRequested)
            {
                OnCancellation?.Invoke();
            }
            else
            {
                yield return narrativeObject.OutputSelectionDecisionPoint.Process(sequencer, OnOutputSelection);

                if (selectedOutputNarrativeObject != null)
                {
                    yield return narrativeObject.StartCoroutine(sequencer.SequenceNarrativeObject(selectedOutputNarrativeObject, cancellationToken));
                }
            }

            OnProcessingComplete?.Invoke();
        }

        private IEnumerator MonitorForProcessComplete()
        {
            while (!processingEnded)
            {
                foreach (var trigger in narrativeObject.EndTriggers)
                {
                    if (trigger != null && trigger.triggered)
                    {
                        processingEnded = true;
                        break;
                    }
                }
                yield return new WaitForEndOfFrame();
            }
        }

        private IEnumerator MonitorForCancellation(CancellationToken cancellationToken)
        {
            if (narrativeObject.EndTriggers != null)
            {
                while (!processingEnded)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        processingEnded = true;
                    }
                    yield return new WaitForEndOfFrame();
                }
            }
        }

        /// <summary>
        /// Invoked when the output selection decision point associated with this narrative object returns a selection.
        /// </summary>
        /// <param name="selection">The selected object. This can be null if nothing is selected.</param>
        /// <returns></returns>
        private IEnumerator OnOutputSelection(NarrativeObject selection)
        {
            if (selection != null)
            {
                selectedOutputNarrativeObject = selection;
            }

            yield return null;
        }
    }
}
