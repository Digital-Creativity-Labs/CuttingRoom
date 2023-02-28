using CuttingRoom.VariableSystem.Constraints;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace CuttingRoom.Editor
{
    public class GroupNarrativeObjectNode : NarrativeObjectNode
    {
        /// <summary>
        /// The group narrative object represented by this node.
        /// </summary>
        private GroupNarrativeObject GroupNarrativeObject { get; set; } = null;

        /// <summary>
		/// Button allowing contents to be viewed.
		/// </summary>
		private Button viewContentsButton = null;

        /// <summary>
		/// Invoked when the view contents button is clicked.
		/// </summary>
		public event Action<NarrativeObjectNode> OnClickViewContents;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="groupNarrativeObject"></param>
        public GroupNarrativeObjectNode(GroupNarrativeObject groupNarrativeObject, NarrativeObject parentNarrativeObject) : base(groupNarrativeObject, parentNarrativeObject)
        {
            GroupNarrativeObject = groupNarrativeObject;

            groupNarrativeObject.OnChanged += OnNarrativeObjectChanged;

            StyleSheet = Resources.Load<StyleSheet>("GroupNarrativeObjectNode");

            VisualElement titleElement = this.Q<VisualElement>("title");
            titleElement?.styleSheets.Add(StyleSheet);

            GenerateContents();
        }

        /// <summary>
        /// Generate the contents of the content container for this node.
        /// </summary>
        private void GenerateContents()
        {
            // Get the contents container for this node.
            VisualElement contents = this.Q<VisualElement>("contents");

            // Add a divider below the ports.
            contents.Add(UIElementsUtils.GetHorizontalDivider());

            // Add button to push view for this graph node onto the stack.
            viewContentsButton = new Button(() =>
            {
                OnClickViewContents?.Invoke(this);
            });
            viewContentsButton.text = "View Contents";
            viewContentsButton.name = "view-contents-button";
            viewContentsButton.styleSheets.Add(StyleSheet);
            contents?.Add(viewContentsButton);
        }

        /// <summary>
        /// Invoked when the group narrative object represented by this node is changed in the inspector.
        /// </summary>
        protected override void OnNarrativeObjectChanged()
        {
        }

        public override List<VisualElement> GetEditableFieldRows()
        {
            List<VisualElement> rows = new List<VisualElement>(base.GetEditableFieldRows());

            GroupSelectionDecisionPoint.SelectionMethod groupSelectionMethod = GroupSelectionDecisionPoint.SelectionMethod.None;
            if (GroupNarrativeObject.GroupSelectionDecisionPoint.selectionMethodContainer != null && Enum.TryParse(GroupNarrativeObject.GroupSelectionDecisionPoint.selectionMethodContainer.methodName, ignoreCase: true, out GroupSelectionDecisionPoint.SelectionMethod selectionMethod))
            {
                groupSelectionMethod = selectionMethod;
            }

            VisualElement groupSelectionMethodFieldRow = UIElementsUtils.CreateEnumFieldRow("Group Selection Method", groupSelectionMethod, (newValue) =>
            {
                if (Enum.TryParse(newValue.ToString(), ignoreCase: true, out GroupSelectionDecisionPoint.SelectionMethod selectionMethod))
                {
                    Undo.RecordObject(GroupNarrativeObject.GroupSelectionDecisionPoint, $"Set Group Selection Method {(selectionMethod)}");
                    if (selectionMethod != GroupSelectionDecisionPoint.SelectionMethod.None)
                    {
                        GroupNarrativeObject.GroupSelectionDecisionPoint.selectionMethodContainer.methodName = selectionMethod.ToString();
                    }
                    else
                    {
                        GroupNarrativeObject.GroupSelectionDecisionPoint.selectionMethodContainer.methodName = string.Empty;
                    }
                    GroupNarrativeObject.OnValidate();
                }
            });

            rows.Add(groupSelectionMethodFieldRow);

            GroupSelectionDecisionPoint.TerminationMethod groupTerminationMethod = GroupSelectionDecisionPoint.TerminationMethod.None;
            if (GroupNarrativeObject.GroupSelectionDecisionPoint.terminationMethodContainer != null && Enum.TryParse(GroupNarrativeObject.GroupSelectionDecisionPoint.terminationMethodContainer.methodName, ignoreCase: true, out GroupSelectionDecisionPoint.TerminationMethod terminationMethod))
            {
                groupTerminationMethod = terminationMethod;
            }

            VisualElement groupTerminationMethodFieldRow = UIElementsUtils.CreateEnumFieldRow("Group Termination Method", groupTerminationMethod, (newValue) =>
            {
                if (Enum.TryParse(newValue.ToString(), ignoreCase: true, out GroupSelectionDecisionPoint.TerminationMethod terminationMethod))
                {
                    Undo.RecordObject(GroupNarrativeObject.GroupSelectionDecisionPoint, $"Set Group Termination Method {(terminationMethod)}");
                    if (terminationMethod != GroupSelectionDecisionPoint.TerminationMethod.None)
                    {
                        GroupNarrativeObject.GroupSelectionDecisionPoint.terminationMethodContainer.methodName = terminationMethod.ToString();
                    }
                    else
                    {
                        GroupNarrativeObject.GroupSelectionDecisionPoint.terminationMethodContainer.methodName = string.Empty;
                    }
                    GroupNarrativeObject.OnValidate();
                }
            });

            rows.Add(groupTerminationMethodFieldRow);

            rows.Add(UIElementsUtils.GetHorizontalDivider());

            // Group selection constraints.
            VisualElement groupSelectionConstraintsSection = ConstraintsComponent.Render("Group Selection Constraints", GroupNarrativeObject, GroupNarrativeObject.GroupSelectionDecisionPoint.Constraints, GroupNarrativeObject.GroupSelectionDecisionPoint.constraintMode,
                (newConstraintMode) =>
                {
                    GroupNarrativeObject.GroupSelectionDecisionPoint.constraintMode = newConstraintMode;
                },
                (constraintType) =>
                {
                    Undo.RecordObject(GroupNarrativeObject.GroupSelectionDecisionPoint, $"Add Constraint {(constraintType)}");
                    Constraint constraint = ConstraintFactory.AddConstraintToDecisionPoint(GroupNarrativeObject.GroupSelectionDecisionPoint, constraintType);
                    if (constraint != null)
                    {
                        NarrativeObject.OnValidate();
                    }
                },
                (removedConstraint) =>
                {
                    if (NarrativeObject != null && removedConstraint != null)
                    {
                        Undo.RecordObject(GroupNarrativeObject.GroupSelectionDecisionPoint, "Remove Constraint");
                        GroupNarrativeObject.GroupSelectionDecisionPoint.RemoveConstraint(removedConstraint);

                        UnityEngine.Object.DestroyImmediate(removedConstraint);
                        NarrativeObject.OnValidate();
                    }
                }, supportedTypes: new() { ConstraintFactory.ConstraintType.Tag }, StyleSheet);

            rows.Add(groupSelectionConstraintsSection);

            return rows;
        }
    }
}