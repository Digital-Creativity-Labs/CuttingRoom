using System;
using System.Collections.Generic;
using CuttingRoom;
using CuttingRoom.VariableSystem.Variables;
using UnityEngine;

namespace CuttingRoom.VariableSystem.Constraints
{
    public class BoolVariableConstraint : Constraint
    {
        public enum ComparisonType
        {
            Undefined,
            EqualTo,
            NotEqualTo,
        }

        [SerializeField]
        private ComparisonType comparisonType = ComparisonType.Undefined;

        public ComparisonType Comparison { get { return comparisonType; } set { comparisonType = value; } }

        public bool value = false;
        public override Type ValueType
        {
            get => value.GetType();
        }

        public override string Value => value.ToString();

        public override bool Evaluate(NarrativeSpace narrativeSpace, NarrativeObject narrativeObject)
        {
            return Evaluate<BoolVariableConstraint, BoolVariable>(narrativeSpace, narrativeObject, comparisonType.ToString());
        }

        public bool EqualTo(BoolVariable boolVariable)
        {
			if (boolVariable.Value == value)
			{
				return true;
			}

			return false;
        }

        public bool NotEqualTo(BoolVariable boolVariable)
        {
            return !EqualTo(boolVariable);
        }
    }
}