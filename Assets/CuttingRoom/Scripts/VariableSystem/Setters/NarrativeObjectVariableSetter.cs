using UnityEngine;

namespace CuttingRoom.VariableSystem.Variables
{
    public class NarrativeObjectVariableSetter : VariableSetter
    {
        [SerializeField]
        private NarrativeObject value = null;

        public void Set()
        {
            Set<NarrativeObjectVariable>(value.ToString());
        }
        public void Set(NarrativeObject value)
        {
            Set<NarrativeObjectVariable>(value.ToString());
        }
    }
}
