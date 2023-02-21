using UnityEngine;
using CuttingRoom.VariableSystem.Variables;
using System.Collections.Generic;
using System;

namespace CuttingRoom.VariableSystem.Constraints
{
	public class IntVariableConstraint : Constraint
	{
		// TODO: This should ideally be a bitmask with equals, less, greater.
		//		 Pain in the butt to implement and not worth it unless it becomes easier!
		public enum ComparisonType
		{
			Undefined,
			EqualTo,
			NotEqualTo,
			LessThan,
			GreaterThan,
			LessThanOrEqualTo,
			GreaterThanOrEqualTo,
		}

		[SerializeField]
		private ComparisonType comparisonType = ComparisonType.Undefined;

		public ComparisonType Comparison { get { return comparisonType; } set { comparisonType = value; } }

		/// <summary>
		/// Value of this constraint.
		/// </summary>
		public int value = 0;
        public override Type ValueType
        {
            get => value.GetType();
        }

        public override string Value => value.ToString();

		public override bool Evaluate(NarrativeSpace narrativeSpace, NarrativeObject narrativeObject)
		{
			return Evaluate<IntVariableConstraint, IntVariable>(narrativeSpace, narrativeObject, comparisonType.ToString());
		}

		public bool EqualTo(IntVariable intVariable)
		{
			return intVariable.Value == value;
		}

		public bool NotEqualTo(IntVariable intVariable)
		{
			return !EqualTo(intVariable);
		}

		public bool LessThan(IntVariable intVariable)
		{
			return value < intVariable.Value;
		}

		public bool GreaterThan(IntVariable intVariable)
        {
            return value > intVariable.Value;
        }

		public bool LessThanOrEqualTo(IntVariable intVariable)
		{
			return LessThan(intVariable) || EqualTo(intVariable);
		}

		public bool GreaterThanOrEqualTo(IntVariable intVariable)
		{
			return GreaterThan(intVariable) || EqualTo(intVariable);
		}
	}
}