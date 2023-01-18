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
        /// Add a constraint to a narrative object.
        /// </summary>
        /// <param name="narrativeObject"></param>
        /// <param name="constraintType"></param>
        public static Variable AddVariableToVariableStore(VariableStore variableStore, VariableType variableType)
        {
            if (variableStore != null)
            {
                Undo.RecordObject(variableStore, "Add variable");
                Variable variable = AddVariableToGameObject(variableStore.gameObject, variableType);
                // Add the constraint.
                variableStore.variableList.Add(variable);

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
