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

		private static string defaultNewVariableName = "New";

		/// <summary>
		/// Public read only accessor for variables
		/// </summary>
		public IReadOnlyDictionary<string, Variable> Variables => variables;

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

                for (int i = 0; i < variables.Length; ++i)
                {
                    if (!variableList.Contains(variables[i]))
                    {
                        variableList.Add(variables[i]);
                    }
                }
            }

            variables = new Dictionary<string, Variable>();
            // Generate a dictionary for quick look up of variables.
            for (int count = 0; count < variableList.Count; ++count)
            {
				Variable variable = variableList[count];
                // Check that the entry in the variable list exists.
				if (variable == null)
				{
					// There can be null entries due to human error/use. Remove them if they exist.
					variableList.RemoveAt(count);
					--count;
				}
                else if (!string.IsNullOrEmpty(variable.Name))
                {
                    variables[variable.Name] = variable;
                }
            }
        }

		public IReadOnlyDictionary<string, Variable> GetVariablesOfCategory(Variable.VariableCategory variableCategory)
		{
			if (variableList != null && variableList.Count > 0)
			{
				if (variableCategory == Variable.VariableCategory.Any)
                {
                    return variableList.ToDictionary(variable => variable.Name);
                }
				else
				{
					return variableList.Where(variable => variable.variableCategory == variableCategory).ToDictionary(variable => variable.Name);
				}
			}

			return new Dictionary<string, Variable>();
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

        public Variable GetOrAddVariable<T>(string variableName, Variable.VariableCategory variableCategory, object value = null) where T : Variable
        {
			if (variables.ContainsKey(variableName))
			{
				return variables[variableName];
			}
			else
			{
				Variable variable = gameObject.AddComponent<T>();
				variable.variableCategory = variableCategory;
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

		private string GenerateVariableName()
		{
			int i;
			for (i = 0; Variables.ContainsKey($"{defaultNewVariableName}-{i}"); ++i) ;

			return $"{defaultNewVariableName}-{i}";
        }

        public void AddVariable(Variable variable)
		{
			if (string.IsNullOrEmpty(variable.Name))
			{
				variable.Name = GenerateVariableName();
            }

			if (!variables.ContainsKey(variable.Name))
			{
                variableList.Add(variable);
				variables.Add(variable.Name, variable);
			}
        }

        public void RemoveVariable(Variable variable)
        {
			if (string.IsNullOrEmpty(variable.Name))
			{
				throw new InvalidVariableException("Variables must have a VariableName assigned to them.");
			}

            if (variables.ContainsKey(variable.Name))
            {
				Variable variableToRemove = variables[variable.Name];
                variables.Remove(variable.Name);
				variableList.Remove(variableToRemove);
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