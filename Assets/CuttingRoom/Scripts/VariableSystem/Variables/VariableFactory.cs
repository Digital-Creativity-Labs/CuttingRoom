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
        public static Variable AddVariableToVariableStore(VariableStore variableStore, VariableType variableType, Variable.VariableCategory variableCategory = Variable.VariableCategory.UserDefined)
        {
            if (variableStore != null)
            {
                Undo.RecordObject(variableStore, "Add variable");
                // Forced add the variable to the variable store with no name
                Variable variable = null;
                if (variableStore != null)
                {
                    switch (variableType)
                    {
                        case VariableType.String:
                            variable = variableStore.GetOrAddVariable<StringVariable>(string.Empty, variableCategory);
                            break;

                        case VariableType.Bool:
                            variable = variableStore.GetOrAddVariable<BoolVariable>(string.Empty, variableCategory);
                            break;

                        case VariableType.Float:
                            variable = variableStore.GetOrAddVariable<FloatVariable>(string.Empty, variableCategory);
                            break;

                        case VariableType.Int:
                            variable = variableStore.GetOrAddVariable<IntVariable>(string.Empty, variableCategory);
                            break;
                    }

                }
                return variable;
            }

            return null;
        }

        private static Variable AddVariableToGameObject(GameObject gameObject, VariableType variableType)
        {
            Variable variable = null;
            if (gameObject != null)
            {
                switch (variableType)
                {
                    case VariableType.String:
                        variable = gameObject.AddComponent<StringVariable>();
                        break;

                    case VariableType.Bool:
                        variable = gameObject.AddComponent<BoolVariable>();
                        break;

                    case VariableType.Float:
                        variable = gameObject.AddComponent<FloatVariable>();
                        break;

                    case VariableType.Int:
                        variable = gameObject.AddComponent<IntVariable>();
                        break;
                }

            }
            return variable;
        }
#endif
    }
}
