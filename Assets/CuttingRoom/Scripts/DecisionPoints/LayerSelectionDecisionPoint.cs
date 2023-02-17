using CuttingRoom;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuttingRoom
{
    public class LayerSelectionDecisionPoint : DecisionPoint
    {

        public override IEnumerator Process(OnSelectionCallback onSelection)
        {
            // Not implemented
            throw new NotImplementedException("Process() not implemented for multi selection on groups.");
        }

        public override IEnumerator Process(OnMultiSelectionCallback onSelection)
        {
            var validCandidates = ProcessConstraints(constraints);
            if (validCandidates != null)
            {
                yield return onSelection.Invoke(validCandidates);
            }
        }
    }
}
