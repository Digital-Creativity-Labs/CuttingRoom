using CuttingRoom.VariableSystem;
using CuttingRoom.VariableSystem.Constraints;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using CuttingRoom.VariableSystem.Variables;
using System.Linq;
using CuttingRoom.Editor;

namespace CuttingRoom
{
    [RequireComponent(typeof(OutputSelectionDecisionPoint), typeof(VariableStore))]
    [ExecuteInEditMode]
    public class NarrativeObject : MonoBehaviour
    {
        public static string hasPlayedTagName = "hasPlayed";

        /// <summary>
        /// The guid for this narrative object.
        /// </summary>
        [InspectorVisible]
        public string guid = Guid.NewGuid().ToString();

        /// <summary>
        /// The output selection decision point for this narrative object.
        /// </summary>
        [SerializeField]
        private OutputSelectionDecisionPoint outputSelectionDecisionPoint = null;

        /// <summary>
        /// The output selection decision point for this narrative object.
        /// </summary>
        public OutputSelectionDecisionPoint OutputSelectionDecisionPoint { get { return outputSelectionDecisionPoint; } set { outputSelectionDecisionPoint = value; } }

        /// <summary>
        /// The variable store for this narrative object.
        /// </summary>
        [SerializeField]
        private VariableStore variableStore = null;



        /// <summary>
        /// Get accessor for the variable store on this narrative object.
        /// </summary>
        public VariableStore VariableStore { get { return variableStore; } set { variableStore = value; } }

        /// <summary>
        /// The triggers for when this object should process its output.
        /// </summary>
        [SerializeField]
        protected List<ProcessingEndTrigger> endTriggers = new List<ProcessingEndTrigger>(1);

        /// <summary>
        /// The processing triggers for this narrative object.
        /// </summary>
        public List<ProcessingEndTrigger> EndTriggers { get { return endTriggers; } }

        /// <summary>
        /// The constraints applied to this narrative object.
        /// </summary>
        public List<Constraint> constraints = new List<Constraint>();

        /// <summary>
        /// The mode with which to process the constraints applied to this node.
        /// </summary>
        public ConstraintMode constraintMode = ConstraintMode.ValidIfAll;

        public bool inProgress = false;

#if UNITY_EDITOR
        public void Awake()
        {
            InitialiseVariableStore(forceRefresh: true);
        }

        public void Reset()
        {
            ProcessingEndTrigger defaultEndOfContentTrigger = ProcessingEndTriggerFactory.AddProcessingTriggerToNarrativeObject(this, ProcessingEndTriggerFactory.TriggerType.EndOfContent);
            InitialiseVariableStore(forceRefresh: true);
        }

        /// <summary>
        /// Ensure variable store is correctly initialised and contains required variables.
        /// </summary>
        /// <param name="forceRefresh"></param>
        public void InitialiseVariableStore(bool forceRefresh = false)
        {
            if (VariableStore == null)
            {
                VariableStore = GetComponent<VariableStore>();
                VariableStore.RefreshDictionary();
            }
            else if (forceRefresh)
            {
                VariableStore.RefreshDictionary();
            }
            if (!VariableStore.Variables.ContainsKey(hasPlayedTagName))
            {
                BoolVariable hasPlayedVariable = VariableStore.GetOrAddVariable<BoolVariable>(hasPlayedTagName, Variable.VariableCategory.SystemDefined, false) as BoolVariable;
                VariableStore.RefreshDictionary();
            }
        }

        /// <summary>
        /// Updates event handlers for all triggers and variables
        /// </summary>
        public virtual void OnValidate()
        {
            if (EndTriggers != null)
            {
                foreach (var endTrigger in EndTriggers)
                {
                    if (endTrigger != null)
                    {
                        endTrigger.OnChanged -= OnChanged;
                        endTrigger.OnChanged += OnChanged;
                    }
                }
            }
            if (VariableStore != null)
            {
                foreach (var variable in VariableStore.variableList)
                {
                    if (variable != null)
                    {
                        variable.OnVariableSet -= OnVariableChange;
                        variable.OnVariableSet += OnVariableChange;
                    }
                }
            }
            OnChanged?.Invoke();
        }

#endif

        /// <summary>
        /// Invoked immediately before process method starts execution.
        /// </summary>
        public virtual void PreProcess()
        {
            if (!inProgress)
            {
                inProgress = true;
                foreach (var trigger in endTriggers)
                {
                    if (trigger != null)
                    {
                        trigger.StartMonitoring();
                    }
                }
            }
        }

        /// <summary>
        /// Invoked immediately after process method completes execution.
        /// </summary>
        public virtual void PostProcess()
        {
            foreach (var trigger in endTriggers)
            {
                if (trigger != null)
                {
                    trigger.StopMonitoring();
                }
            }
            inProgress = false;
        }

#if UNITY_EDITOR
        public event Action OnChanged;

        protected Action OnChangedInternal { get { return OnChanged; } }

        private void OnVariableChange(Variable variable)
        {
            OnChanged?.Invoke();
        }

        public void AddConstraint(Constraint constraint)
        {
            if (!constraints.Contains(constraint))
            {
                constraints.Add(constraint);
            }
        }

        public void RemoveConstraint(Constraint constraint)
        {
            if (constraints.Contains(constraint))
            {
                constraints.Remove(constraint);
            }
        }

        public void AddProcessingTrigger(ProcessingEndTrigger trigger)
        {
            if (!endTriggers.Contains(trigger))
            {
                endTriggers.Add(trigger);
            }
        }

        public void RemoveProcessingTrigger(ProcessingEndTrigger trigger)
        {
            if (endTriggers.Contains(trigger))
            {
                endTriggers.Remove(trigger);
            }
        }
#endif
    }
}
