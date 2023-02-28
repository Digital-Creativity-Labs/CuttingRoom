using UnityEditor;
using UnityEngine;

namespace CuttingRoom.VariableSystem.Constraints
{
    public class ConstraintFactory
    {
        public enum ConstraintType
        {
            String,
            Bool,
            Int,
            Float,
            Tag
        }

#if UNITY_EDITOR
        /// <summary>
        /// Add a constraint to a narrative object.
        /// </summary>
        /// <param name="narrativeObject"></param>
        /// <param name="constraintType"></param>
        public static Constraint AddConstraintToNarrativeObject(NarrativeObject narrativeObject, ConstraintType constraintType)
        {
            if (narrativeObject != null)
            {
                Undo.RecordObject(narrativeObject, "Add Constraint");
                Constraint constraint = AddCostraintToGameObject(narrativeObject.gameObject, constraintType);
                // Add the constraint.
                narrativeObject.AddConstraint(constraint);
                return constraint;
            }
            return null;
        }

        /// <summary>
        /// Add a constraint to a descision point.
        /// </summary>
        /// <param name="decisionPoint"></param>
        /// <param name="constraintType"></param>
        public static Constraint AddConstraintToDecisionPoint(DecisionPoint decisionPoint, ConstraintType constraintType)
        {
            if (decisionPoint != null)
            {
                Undo.RecordObject(decisionPoint, "Add Constraint");
                Constraint constraint = AddCostraintToGameObject(decisionPoint.gameObject, constraintType);
                // Add the constraint.
                decisionPoint.AddConstraint(constraint);
                return constraint;
            }
            return null;
        }

        private static Constraint AddCostraintToGameObject(GameObject gameObject, ConstraintType constraintType)
        {
            Constraint constraint = null;

            if (gameObject != null)
            {
                switch (constraintType)
                {
                    case ConstraintType.String:

                        constraint = gameObject.AddComponent<StringVariableConstraint>();

                        break;

                    case ConstraintType.Bool:

                        constraint = gameObject.AddComponent<BoolVariableConstraint>();

                        break;

                    case ConstraintType.Int:

                        constraint = gameObject.AddComponent<IntVariableConstraint>();

                        break;

                    case ConstraintType.Float:

                        constraint = gameObject.AddComponent<FloatVariableConstraint>();

                        break;

                    case ConstraintType.Tag:

                        constraint = gameObject.AddComponent<TagVariableConstraint>();

                        break;
                }
            }

            return constraint;
        }

        public static ConstraintType? ConstraintToTypeEnum(Constraint constraint)
        {
            if (constraint != null)
            {
                if (constraint is StringVariableConstraint)
                {
                    return ConstraintType.String;
                }
                else if (constraint is BoolVariableConstraint)
                {
                    return ConstraintType.Bool;
                }
                else if (constraint is IntVariableConstraint)
                {
                    return ConstraintType.Int;
                }
                else if (constraint is FloatVariableConstraint)
                {
                    return ConstraintType.Float;
                }
                else if (constraint is TagVariableConstraint)
                {
                    return ConstraintType.Tag;
                }
            }
            return null;
        }
#endif
    }
}
