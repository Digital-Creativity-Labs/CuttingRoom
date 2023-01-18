using System.Collections;
using System.Collections.Generic;
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

            GroupNarrativeObject.PreProcess();

			yield return GroupNarrativeObject.GroupSelectionDecisionPoint.Process(sequencer, OnSelection);

			yield return base.Process(sequencer, cancellationToken);

			GroupNarrativeObject.PostProcess();
		}

		/// <summary>
		/// Invoked when this group selects a narrative object.
		/// </summary>
		/// <param name="selection"></param>
		/// <returns></returns>
		public IEnumerator OnSelection(NarrativeObject selection)
		{
			if (selection != null)
			{
				contentCoroutine = GroupNarrativeObject.StartCoroutine(sequencer.SequenceNarrativeObject(selection, groupCancellationToken.Token));
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
