using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CuttingRoom
{
	public partial class GroupSelectionDecisionPoint : DecisionPoint
	{
		/// <summary>
		/// Method container for the group selection method for this decision point.
		/// </summary>
		[SerializeField]
		public MethodContainer selectionMethodContainer = new();

		/// <summary>
		/// Method container for the group termination method for this decision point.
		/// </summary>
		[SerializeField]
        public MethodContainer terminationMethodContainer = new();

		/// <summary>
		/// List of selections made by this object.
		/// </summary>
		private List<NarrativeObject> selections = new List<NarrativeObject>();

        /// <summary>
        /// The method passed to the process method.
        /// This is cached for use in the local "OnMultiSelection" method.
        /// </summary>
        private new OnMultiSelectionCallback onMultiSelection = null;

#if UNITY_EDITOR
        // Set default group selection and termination methods
        public void Reset()
        {
            if (string.IsNullOrEmpty(selectionMethodContainer.methodName))
            {
                selectionMethodContainer.methodName = nameof(Random);
            }

            if (string.IsNullOrEmpty(terminationMethodContainer.methodName))
			{
				terminationMethodContainer.methodName = nameof(HasMadeSelection);
            }
        }
#endif

        /// <summary>
        /// Unity event invoked by engine.
        /// </summary>
        private void Awake()
		{
			selectionMethodContainer.methodClass = typeof(GroupSelectionDecisionPoint).AssemblyQualifiedName;

			selectionMethodContainer.Init();

			terminationMethodContainer.methodClass = typeof(GroupSelectionDecisionPoint).AssemblyQualifiedName;

			terminationMethodContainer.Init();
		}

		public override IEnumerator Process(OnSelectionCallback onSelection)
        {
            // Not implemented
            throw new NotImplementedException("Process() not implemented for single selection on groups.");
        }

		public override IEnumerator Process(OnMultiSelectionCallback onSelection)
        {
            if (selectionMethodContainer.Initialised && terminationMethodContainer.Initialised)
            {
                // Store out the callback to be invoked by local OnSelection method.
                this.onMultiSelection = onSelection;

                // Clear the selections made (if any from previous process call).
                selections.Clear();

                bool terminate = false;

                while (!terminate)
                {
                    object terminateObj = terminationMethodContainer.methodInfo.Invoke(this, new object[] { new MethodContainer.Args() });

                    if (terminateObj != null && terminateObj is bool)
                    {
                        terminate = (bool)terminateObj;

                        if (!terminate)
                        {
                            // Store the number of selections made before invoking the process method.
                            int preProcessSelectionCount = selections.Count;

                            List<NarrativeObject> validCandidates = ProcessConstraints(constraints);

                            MethodContainer.Args args = new MethodContainer.Args { onSelection = OnSelection, onMultiSelection = OnMultiSelection, candidates = validCandidates };

                            yield return StartCoroutine(selectionMethodContainer.methodInfo.Name, args);

                            // If the group has selected nothing then terminate (to avoid infinite loops of nothing ever being selected but termination never occurring either).
                            // In an ideal world this wouldn't be needed but this helps prevent poor implementations of selection and termination methods.
                            if (preProcessSelectionCount == selections.Count)
                            {
                                terminate = true;
                            }
                        }
                    }
                    else
                    {
                        terminate = true;
                    }
                }
            }
        }

        /// <summary>
        /// On selection method intercepts callbacks with selections before forwarding to group narrative object script.
        /// </summary>
        /// <param name="selection"></param>
        /// <returns></returns>
        private IEnumerator OnSelection(NarrativeObject newSelection)
        {
            if (newSelection != null)
            {
                selections.Add(newSelection);
            }

            yield return StartCoroutine(onMultiSelection(new() { newSelection }));
        }

        /// <summary>
        /// On selection method intercepts callbacks with selections before forwarding to group narrative object script.
        /// </summary>
        /// <param name="selection"></param>
        /// <returns></returns>
        private IEnumerator OnMultiSelection(List<NarrativeObject> newSelections)
        {
            if (newSelections != null)
            {
                foreach (NarrativeObject newSelection in newSelections)
                {
                    selections.Add(newSelection);
                }
            }

            yield return StartCoroutine(onMultiSelection(newSelections));
        }
    }
}
