using System.Collections.Generic;
using UnityEngine;

namespace CuttingRoom.VariableSystem.Variables
{
    public class VariableSetter : MonoBehaviour
    {
        public string variableName = null;
        public VariableStoreLocation variableStoreLocation = VariableStoreLocation.Undefined;

        private NarrativeSpace narrativeSpace = null;

        private void Awake()
        {
            narrativeSpace = FindObjectOfType<NarrativeSpace>();
        }

        protected void Set<T>(string value) where T : Variable
        {
            T variable = default(T);

            switch (variableStoreLocation)
            {
                case VariableStoreLocation.Global:

                    variable = narrativeSpace.GlobalVariableStore.GetVariable<T>(variableName);

                    break;

                case VariableStoreLocation.Local:

                    NarrativeObject narrativeObject = gameObject.GetComponent<NarrativeObject>();
                    variable = narrativeObject.VariableStore.GetVariable<T>(variableName);

                    break;
            }

            if (variable != null)
            {
                variable.SetValueFromString(value);
            }
        }

        public virtual void Set(string value)
        {
            Variable variable = null;

            switch (variableStoreLocation)
            {
                case VariableStoreLocation.Global:

                    variable = narrativeSpace.GlobalVariableStore.GetVariable(variableName);

                    break;

                case VariableStoreLocation.Local:

                    NarrativeObject narrativeObject = gameObject.GetComponent<NarrativeObject>();
                    variable = narrativeObject.VariableStore.GetVariable(variableName);

                    break;
            }

            if (variable != null)
            {
                variable.SetValueFromString(value);
            }
        }
    }
}
