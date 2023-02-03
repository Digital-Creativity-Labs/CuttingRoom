using CuttingRoom.VariableSystem.Variables;
using System;
using System.Collections.Generic;
using Unity.XR.Oculus.Input;

namespace CuttingRoom.VariableSystem.Constraints
{
	public class TagVariableConstraint : Constraint
	{
		public enum ComparisonType
		{
			Undefined,
			EqualTo,
			NotEqualTo
		}

		public ComparisonType comparisonType = ComparisonType.Undefined;

		public ComparisonType Comparison { get { return comparisonType; } set { comparisonType = value; } }

        public string value = null;
        public Variable tagVariable = null;
        public override Type ValueType
        {
			get => typeof(Variable);
        }

        public override string Value => value;

		public override bool Evaluate(Sequencer sequencer, NarrativeSpace narrativeSpace, NarrativeObject narrativeObject)
		{
			if (narrativeObject != null && narrativeObject.VariableStore != null)
			{
				tagVariable = narrativeObject.VariableStore.GetVariable<Variable>(value);
			}

			if (tagVariable != null)
			{
				return Evaluate<TagVariableConstraint, Variable>(sequencer, narrativeSpace, narrativeObject, comparisonType.ToString());
			}
			else
			{
				return false;
			}
		}

		public bool EqualTo(Variable variable)
		{
			return tagVariable != null ? tagVariable.ValueEqual(variable) : false;
		}

		public bool NotEqualTo(Variable variable)
		{
			return !EqualTo(variable);
		}
	}
}