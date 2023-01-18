using UnityEngine;
using CuttingRoom;
using CuttingRoom.VariableSystem.Variables;
using System.Collections.Generic;
using System;

namespace CuttingRoom.VariableSystem.Constraints
{
	public class FloatVariableConstraint : Constraint
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
		public ComparisonType comparisonType = ComparisonType.Undefined;

		public ComparisonType Comparison { get { return comparisonType; } set { comparisonType = value; } }

		/// <summary>
		/// Value of this constraint.
		/// </summary>
		[SerializeField]
		public float value = 0;
        public override Type ValueType
        {
            get => value.GetType();
        }

        public override string Value => value.ToString();

		public override bool Evaluate(Sequencer sequencer, NarrativeSpace narrativeSpace, NarrativeObject narrativeObject)
		{
			return Evaluate<FloatVariableConstraint, FloatVariable>(sequencer, narrativeSpace, narrativeObject, comparisonType.ToString());
		}

		public bool EqualTo(FloatVariable floatVariable)
		{
			return floatVariable.Value == value;
		}

		public bool NotEqualTo(FloatVariable floatVariable)
        {
			return !EqualTo(floatVariable);
		}

		public bool LessThan(FloatVariable floatVariable)
        {
			return value < floatVariable.Value;
		}

		public bool GreaterThan(FloatVariable floatVariable)
        {
            return value > floatVariable.Value;
        }

		public bool LessThanOrEqualTo(FloatVariable floatVariable)
		{
			return LessThan(floatVariable) || EqualTo(floatVariable);
		}

		public bool GreaterThanOrEqualTo(FloatVariable floatVariable)
		{
			return GreaterThan(floatVariable) || EqualTo(floatVariable);
		}
	}
}