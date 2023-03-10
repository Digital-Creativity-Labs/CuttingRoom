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
            if (args.candidates != null && args.candidates.Count > 0)
            {
                yield return StartCoroutine(args.onSelection(args.candidates.First()));
            }
        }

        /// <summary>
        /// Select a random candidate of the group.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private IEnumerator Random(MethodContainer.Args args)
        {
            if (args.candidates != null && args.candidates.Count > 0)
            {
                yield return StartCoroutine(args.onSelection(args.candidates[UnityEngine.Random.Range(0, args.candidates.Count)]));
            }
        }

        /// <summary>
        /// Select a random candidate of the group.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private IEnumerator AllRandom(MethodContainer.Args args)
        {
            if (args.candidates != null && args.candidates.Count > 0)
            {
                // Copy and shuffle List
                List<NarrativeObject> selections = args.candidates.ToList();
                for (int i = 0; i < args.candidates.Count; ++i)
                {
                    var temp = selections[i];
                    int randomIndex = UnityEngine.Random.Range(i, selections.Count);
                    selections[i] = selections[randomIndex];
                    selections[randomIndex] = temp;
                }
                yield return StartCoroutine(args.onMultiSelection(selections));
            }
        }
    }
}
