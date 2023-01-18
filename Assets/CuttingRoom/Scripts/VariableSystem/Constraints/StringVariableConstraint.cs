using CuttingRoom.VariableSystem.Variables;
using System;
using System.Collections.Generic;
using Unity.XR.Oculus.Input;

namespace CuttingRoom.VariableSystem.Constraints
{
	public class StringVariableConstraint : Constraint
	{
		public enum ComparisonType
		{
			Undefined,
			EqualTo,
			NotEqualTo,
			Contains,
			DoesNotContain,
		}

		public ComparisonType comparisonType = ComparisonType.Undefined;

		public ComparisonType Comparison { get { return comparisonType; } set { comparisonType = value; } }

		public string value = string.Empty;
        public override Type ValueType
        {
            get => value.GetType();
        }

        public override string Value => value.ToString();

		public override bool Evaluate(Sequencer sequencer, NarrativeSpace narrativeSpace, NarrativeObject narrativeObject)
		{
			return Evaluate<StringVariableConstraint, StringVariable>(sequencer, narrativeSpace, narrativeObject, comparisonType.ToString());
		}

		public bool EqualTo(StringVariable stringVariable)
		{
			return value.Equals(stringVariable.Value);
		}

		public bool NotEqualTo(StringVariable stringVariable)
		{
			return !EqualTo(stringVariable);
		}

		public bool Contains(StringVariable stringVariable)
		{
			return value.Contains(stringVariable.Value);
		}

		public bool DoesNotContain(StringVariable stringVariable)
		{
			return !Contains(stringVariable);
		}
	}
}