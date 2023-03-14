using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CuttingRoom
{
    public partial class GroupSelectionDecisionPoint : DecisionPoint
    {
        public enum SelectionMethod
        {
            None = 0,
            First,
            Random,
            AllRandom
        }

        /// <summary>
        /// Select the first candidate.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private IEnumerator First(MethodContainer.Args args)
        {
            NarrativeObject selection = new();
            if (args.candidates != null && args.candidates.Count > 0)
            {
                selection = args.candidates.First();
                yield return StartCoroutine(args.onSelection(args.candidates.First()));
            }
            yield return StartCoroutine(args.onSelection(selection));
        }

        /// <summary>
        /// Select a random candidate of the group.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private IEnumerator Random(MethodContainer.Args args)
        {
            NarrativeObject selection = new();
            if (args.candidates != null && args.candidates.Count > 0)
            {
                selection = args.candidates[UnityEngine.Random.Range(0, args.candidates.Count)];
            }
            yield return StartCoroutine(args.onSelection(selection));
        }

        /// <summary>
        /// Select a random candidate of the group.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private IEnumerator AllRandom(MethodContainer.Args args)
        {
            List<NarrativeObject> selections = new();
            if (args.candidates != null && args.candidates.Count > 0)
            {
                // Copy and shuffle List
                selections = args.candidates.ToList();
                for (int i = 0; i < args.candidates.Count; ++i)
                {
                    var temp = selections[i];
                    int randomIndex = UnityEngine.Random.Range(i, selections.Count);
                    selections[i] = selections[randomIndex];
                    selections[randomIndex] = temp;
                }
            }
            yield return StartCoroutine(args.onMultiSelection(selections));
        }
    }
}
