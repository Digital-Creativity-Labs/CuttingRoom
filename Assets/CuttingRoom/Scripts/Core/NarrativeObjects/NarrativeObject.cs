using CuttingRoom.VariableSystem;
using CuttingRoom.VariableSystem.Constraints;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using CuttingRoom.VariableSystem.Variables;
using System.Linq;
#if UNITY_EDITOR
using CuttingRoom.Editor;
#endif

namespace CuttingRoom
{
    [RequireComponent(typeof(OutputSelectionDecisionPoint), typeof(VariableStore))]
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

        public void Awake()
        {
            VariableStore = GetComponent<VariableStore>();
#if UNITY_EDITOR
            InitialiseVariableStore();
#endif
        }


#if UNITY_EDITOR
        public void Reset()
        {
            ProcessingEndTrigger defaultEndOfContentTrigger = ProcessingEndTriggerFactory.AddProcessingTriggerToNarrativeObject(this, ProcessingEndTriggerFactory.TriggerType.EndOfContent);
            InitialiseVariableStore();
        }

        public void InitialiseVariableStore()
        {
            if (VariableStore == null)
            {
                VariableStore = GetComponent<VariableStore>();
            }
            if (!VariableStore.Variables.ContainsKey(hasPlayedTagName))
            {
                BoolVariable hasPlayedVariable = VariableStore.GetOrAddVariable<BoolVariable>(hasPlayedTagName, Variable.VariableCategory.SystemDefined) as BoolVariable;
                hasPlayedVariable.Name = hasPlayedTagName;
                hasPlayedVariable.SetValue(false);
            }
        }

#endif

        /// <summary>
        /// Invoked immediately before process method starts execution.
        /// </summary>
        public virtual void PreProcess()
        {
            foreach(var trigger in endTriggers)
            {
                if (trigger != null)
                {
                    trigger.StartMonitoring();
                }
            }
        }

        /// <summary>
        /// Invoked immediately after process method completes execution.
        /// </summary>
        public virtual void PostProcess()
        {
            if (VariableStore != null)
            {
                BoolVariable hasPlayed = VariableStore.GetVariable(hasPlayedTagName) as BoolVariable;
                hasPlayed.Set(true);
            }

            foreach (var trigger in endTriggers)
            {
                if (trigger != null)
                {
                    trigger.StopMonitoring();
                }
            }
        }

#if UNITY_EDITOR
        public event Action OnChanged;

        protected Action OnChangedInternal { get { return OnChanged; } }

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
