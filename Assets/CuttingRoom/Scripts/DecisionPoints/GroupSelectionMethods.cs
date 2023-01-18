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
            Random
        }

        /// <summary>
        /// Select the first candidate.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private IEnumerator First(MethodContainer.Args args)
        {
            yield return StartCoroutine(args.onSelection(args.candidates.First()));
        }

        /// <summary>
        /// Select a random candidate of the group.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private IEnumerator Random(MethodContainer.Args args)
        {
            yield return StartCoroutine(args.onSelection(args.candidates[UnityEngine.Random.Range(0, args.candidates.Count)]));
        }
    }
}
