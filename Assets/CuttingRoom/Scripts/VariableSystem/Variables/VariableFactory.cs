using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace CuttingRoom.VariableSystem.Variables
{
    public class VariableFactory
    {
        /// <summary>
        /// Types of variables available within the editor.
        /// </summary>
        public enum VariableType
        {
            String,
            Bool,
            Int,
            Float
        }

#if UNITY_EDITOR
        /// <summary>
        /// Add a variable to a variable store.
        /// </summary>
        /// <param name="variableStore"></param>
        /// <param name="variableType"></param>
        public static Variable AddVariableToVariableStore(VariableStore variableStore, VariableType variableType, Variable.VariableCategory variableCategory = Variable.VariableCategory.UserDefined, object value = null)
        {
            if (variableStore != null)
            {
                Undo.RecordObject(variableStore, "Add variable");
                // Forced add the variable to the variable store with no name. One will be generated.
                Variable variable = null;
                if (variableStore != null)
                {
                    switch (variableType)
                    {
                        case VariableType.String:
                            variable = variableStore.GetOrAddVariable<StringVariable>(string.Empty, variableCategory, value);
                            break;

                        case VariableType.Bool:
                            variable = variableStore.GetOrAddVariable<BoolVariable>(string.Empty, variableCategory, value);
                            break;

                        case VariableType.Float:
                            variable = variableStore.GetOrAddVariable<FloatVariable>(string.Empty, variableCategory, value);
                            break;

                        case VariableType.Int:
                            variable = variableStore.GetOrAddVariable<IntVariable>(string.Empty, variableCategory, value);
                            break;
                    }

                }
                return variable;
            }

            return null;
        }
#endif
    }
}
