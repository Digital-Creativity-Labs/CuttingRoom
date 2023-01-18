using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CuttingRoom.VariableSystem.Variables;

namespace CuttingRoom.VariableSystem
{
	public class VariableStore : MonoBehaviour
	{
		[Header("Settings")]
		/// <summary>
		/// If true, the Awake method will add the variables attached to this game object automatically.
		/// </summary>
		[Tooltip("If true, the Awake method will add the variables attached to this game object automatically.")]
		public bool autoAddVariablesOnGameObject = true;

		public List<Variable> variableList = new List<Variable>();

		private Dictionary<string, Variable> variables = new Dictionary<string, Variable>();

		public class InvalidVariableException : Exception { public InvalidVariableException(string message) : base(message) { } }

		public class VariableState
		{
			public string variableName = string.Empty;
			public string guid = string.Empty;
			public string value = string.Empty;
		}

		public void Awake()
		{
			// Generate a dictionary for quick look up of variables.
			RefreshDictionary();
		}

		public void OnValidate()
        {
            // Generate a dictionary for quick look up of variables.
            RefreshDictionary();
        }

        public void RefreshDictionary()
        {
            if (autoAddVariablesOnGameObject)
            {
                Variable[] variables = GetComponents<Variable>();

                for (int i = 0; i < variables.Length; i++)
                {
                    if (!variableList.Contains(variables[i]))
                    {
                        variableList.Add(variables[i]);
                    }
                }
            }

            variables = new Dictionary<string, Variable>();
            // Generate a dictionary for quick look up of variables.
            for (int count = 0; count < variableList.Count; count++)
            {
				Variable variable = variableList[count];
                // Check that the entry in the variable list exists.
                // There can be null entries due to human error/use.
                if (variable != null && !string.IsNullOrEmpty(variable.Name))
                {
                    variables[variable.Name] = variable;
                }
            }
        }

		public void RegisterOnVariableSetCallback(Action<Variable> onVariableSet)
		{
			foreach (KeyValuePair<string, Variable> pair in variables)
			{
				Variable v = pair.Value;
                v.OnVariableSet +=
					(v) =>
					{
						onVariableSet?.Invoke(v);
					};
			}
        }

        public Variable GetOrAddVariableToGameObject<T>(GameObject obj, string variableName, object value = null) where T : Variable
        {
            if (obj == null)
            {
                return null;
            }
            Variable variable = obj.GetComponents<T>().FirstOrDefault((v) =>
			{
				return v.Name.Equals(variableName);
			});

			if (variable != null)
			{
				return variable;
			}
			else
			{
				variable = obj.AddComponent<T>();
				if (variable != null)
				{
					variable.Name = variableName;
					if (value != null)
					{
						variable.SetValue(value);
					}

					AddVariable(variable);
					return variable;
				}
			}

            // Invalid
            return null;
        }

        public void AddVariable(Variable variable)
		{
			if (string.IsNullOrEmpty(variable.Name))
			{
				throw new InvalidVariableException("Variables must have a VariableName assigned to them.");
			}

            if (!variableList.Contains(variable))
            {
                variableList.Add(variable);
            }

			if (!variables.ContainsKey(variable.Name))
			{
				variables.Add(variable.Name, variable);
			}
        }

        public Variable GetVariable(string variableName)
        {
            if (variables.ContainsKey(variableName))
            {
                return variables[variableName];

            }
            return default;
        }

        public T GetVariable<T>(string variableName) where T : Variable
		{
			if (variables.ContainsKey(variableName))
			{
				return variables[variableName] as T;

            }
            return default;
        }

		public List<T> GetAllVariables<T>() where T : Variable
		{
			List<T> values = new List<T>();

			// For each variable key and any associated variables.
			foreach (Variable v in variableList)
			{
				// If its the correct type.
				if (v is T)
				{
					// Add it as a returned variable.
					values.Add(v as T);
				}
			}

			// Return all variables.
			return values;
		}
	}
}