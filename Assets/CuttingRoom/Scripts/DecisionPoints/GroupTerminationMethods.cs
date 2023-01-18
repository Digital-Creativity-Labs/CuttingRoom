using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CuttingRoom
{
	public partial class GroupSelectionDecisionPoint : DecisionPoint
    {
        public enum TerminationMethod
        {
            None = 0,
            HasMadeSelection
        }

        /// <summary>
        /// Whether the group has selected at least one candidate.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private bool HasMadeSelection(MethodContainer.Args args)
		{
			if (selections.Count > 0)
			{
				return true;
			}

			return false;
		}
	}
}
