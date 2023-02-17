using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CuttingRoom.VariableSystem.Constraints;
using System;
using System.Linq;
using UnityEditor;

namespace CuttingRoom
{
    public abstract class DecisionPoint : MonoBehaviour
    {
        /// <summary>
        /// The possible candidates for this decision point.
        /// </summary>
        [SerializeField]
        protected List<NarrativeObject> candidates = new List<NarrativeObject>();

		/// <summary>
		/// The candidates for this decision point.
		/// </summary>
		public List<NarrativeObject> Candidates { get { return candidates; } }

		/// <summary>
		/// Constraints applied to this decision point.
		/// </summary>
		[SerializeField]
		protected List<Constraint> constraints = new List<Constraint>();

		/// <summary>
		/// The contraints for this decision point.
		/// </summary>
		public List<Constraint> Constraints { get { return constraints; } }

		/// <summary>
		/// Invoked when this decision point makes a selection.
		/// </summary>
		protected OnSelectionCallback onSelection = null;

		public abstract IEnumerator Process(OnSelectionCallback onSelection);
        public abstract IEnumerator Process(OnMultiSelectionCallback onSelection);

        protected List<NarrativeObject> ProcessConstraints(List<Constraint> decisionPointConstraints)
        {
			// All candidates are an option to begin with.
			List<NarrativeObject> candidatesMatchingConstraints = new List<NarrativeObject>(candidates);

			NarrativeSpace narrativeSpace = FindObjectOfType<NarrativeSpace>();

			if (narrativeSpace != null)
			{

				// Iterate candidates and check them against decision point constraints.
				for (int candidateCount = candidatesMatchingConstraints.Count - 1; candidateCount >= 0; candidateCount--)
				{
					NarrativeObject candidate = candidatesMatchingConstraints[candidateCount];

					// For each constraint.
					for (int constraintCount = 0; constraintCount < decisionPointConstraints.Count; constraintCount++)
					{
						Constraint constraint = decisionPointConstraints[constraintCount];

						if (!constraint.Evaluate(narrativeSpace, candidate))
						{
							candidatesMatchingConstraints.Remove(candidate);
						}
					}
				}

				for (int candidateCount = candidatesMatchingConstraints.Count - 1; candidateCount >= 0; candidateCount--)
				{
					NarrativeObject candidate = candidatesMatchingConstraints[candidateCount];

					// For each constraint on the candidate.
					for (int candidateConstraintCount = 0; candidateConstraintCount < candidate.constraints.Count; candidateConstraintCount++)
					{
						Constraint candidateConstraint = candidate.constraints[candidateConstraintCount];

						if (!candidateConstraint.Evaluate(narrativeSpace, candidate))
						{
							candidatesMatchingConstraints.Remove(candidate);
						}
					}
				}
			}

			return candidatesMatchingConstraints;
		}

#if UNITY_EDITOR

		public event Action OnCandidatesChanged;
        public event Action OnChanged;

        private List<NarrativeObject> cachedCandidates = new List<NarrativeObject>();

		/// <summary>
		/// Used by the Narrative Space editor to link nodes together.
		/// </summary>
		/// <param name="candidate"></param>
		public void AddCandidate(NarrativeObject candidate)
		{
			if (!candidates.Contains(candidate))
			{
				candidates.Add(candidate);
			}
		}

		public void RemoveCandidate(NarrativeObject candidate)
		{
			if (candidates.Contains(candidate))
			{
				candidates.Remove(candidate);
			}
		}

		public virtual void OnValidate()
		{
			// Find candidates which are in the cached list but not in the actual inspector list (so they have been removed in inspector).
			IEnumerable<NarrativeObject> removedCandidates = cachedCandidates.Except(candidates);

			// Find candidates which are in the actual inspector list but not in the cached list (so they have been added in inspector).
			IEnumerable<NarrativeObject> addedCandidates = candidates.Except(cachedCandidates);

			if (removedCandidates.Count() > 0 || addedCandidates.Count() > 0)
			{
				OnCandidatesChanged?.Invoke();

				CacheCandidates();
            }
            OnChanged?.Invoke();
        }

		private void CacheCandidates()
		{
			// Cache the new connected guids list.
			cachedCandidates.Clear();
			cachedCandidates.AddRange(candidates);
		}

		public void AddConstraint(Constraint constraint)
		{
			if (!constraints.Contains(constraint))
			{
				constraints.Add(constraint);
			}
		}

        public void RemoveConstraint(Constraint constraint)
		{
			if (constraints.Contains(constraint))
			{
				constraints.Remove(constraint);
			}
		}

#endif
	}
}
