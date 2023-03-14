using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace CuttingRoom
{
    public class GroupNarrativeObjectProcessing : NarrativeObjectProcessing
    {
		/// <summary>
		/// The group narrative object being processed by this object.
		/// </summary>
		public GroupNarrativeObject GroupNarrativeObject { get { return narrativeObject as GroupNarrativeObject; } }

		public CancellationTokenSource groupCancellationToken = new CancellationTokenSource();

		/// <summary>
		/// The sequencer processing this object.
		/// Cached to pass to selections made by the group.
		/// </summary>
		private Sequencer sequencer = null;

		private Sequencer subSequencer = null;

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="groupNarrativeObject"></param>
		public GroupNarrativeObjectProcessing(GroupNarrativeObject groupNarrativeObject)
		{
			narrativeObject = groupNarrativeObject;
			OnCancellation += () => { groupCancellationToken.Cancel(); };
		}

		/// <summary>
		/// Processing method for a group narrative object.
		/// </summary>
		/// <param name="sequencer"></param>
		/// <returns></returns>
		public override IEnumerator Process(Sequencer sequencer, CancellationToken? cancellationToken = null)
		{
			this.sequencer = sequencer;

            OnProcessingTriggerComplete += GroupEndTriggered;

			yield return GroupNarrativeObject.GroupSelectionDecisionPoint.Process(OnSelection);

			yield return base.Process(sequencer, cancellationToken);
		}

        /// <summary>
        /// Invoked when this group selects multiple narrative objects.
        /// </summary>
        /// <param name="selections"></param>
        /// <returns></returns>
        public IEnumerator OnSelection(List<NarrativeObject> selections)
        {
			if (selections != null && selections.Count > 0)
			{
				subSequencer = sequencer.AddSubSequence(selections.First(), autoStartSequence: false, groupCancellationToken);

				// Queue all selections before starting to avoid race condition of an empty first selection
				NarrativeObject selection;
				for (int i = 1; i < selections.Count; ++i)
				{
					selection = selections[i];
					if (selection != null)
					{
						subSequencer.SequenceNarrativeObject(selection);
					}
				}

				subSequencer.Start();

				contentCoroutine = GroupNarrativeObject.StartCoroutine(subSequencer.WaitForSequenceComplete());
			}
			else
			{
				// If no selections made then terminate the group
				processingEnded = true;
			}

            // Nothing asynchronous needed here so return null.
            yield return null;
        }

        /// <summary>
        /// End group according to graph node processing trigger.
        /// </summary>
        private void GroupEndTriggered()
        {
            groupCancellationToken.Cancel();
        }
    }
}
