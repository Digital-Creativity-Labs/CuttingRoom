using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CuttingRoom
{
	[RequireComponent(typeof(GroupSelectionDecisionPoint))]
	public class GroupNarrativeObject : NarrativeObject
	{
		/// <summary>
		/// The group selection decision point for this group.
		/// </summary>
		[SerializeField]
		private GroupSelectionDecisionPoint groupSelectionDecisionPoint = null;

		/// <summary>
		/// The group selection decision point for this group.
		/// </summary>
		public GroupSelectionDecisionPoint GroupSelectionDecisionPoint { get { return groupSelectionDecisionPoint; } set { groupSelectionDecisionPoint = value; } }


#if UNITY_EDITOR
        public override void OnValidate()
        {
            if (GroupSelectionDecisionPoint != null)
            {
                GroupSelectionDecisionPoint.OnChanged -= OnChangedInternal;
                GroupSelectionDecisionPoint.OnChanged += OnChangedInternal;
            }
			base.OnValidate();
        }
#endif
	}
}
