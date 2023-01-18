using System.Collections;
using System.Linq;

namespace CuttingRoom
{
    public partial class OutputSelectionDecisionPoint : DecisionPoint
    {
        public enum SelectionMethod
        {
            None = 0,
            First,
            Random
        }

        /// <summary>
        /// Select the first output.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private IEnumerator First(MethodContainer.Args args)
        {
            yield return StartCoroutine(args.onSelection(args.candidates.First()));
        }

        /// <summary>
        /// Select a random output.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private IEnumerator Random(MethodContainer.Args args)
        {
            yield return StartCoroutine(args.onSelection(args.candidates[UnityEngine.Random.Range(0, args.candidates.Count)]));
        }
    }
}
