using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CuttingRoom.VariableSystem;
using CuttingRoom.VariableSystem.Variables;
using CuttingRoom.VariableSystem.Constraints;

namespace CuttingRoom
{
    public class VariableEndTrigger : ProcessingEndTrigger
    {
        /// <summary>
        /// Types of variable events which can trigger this event.
        /// </summary>
        public enum TriggeringEvent
        {
            Undefined,
            OnSet,
            OnValueMatch
        }

        /// <summary>
        /// The location of the triggering variable.
        /// </summary>
        [SerializeField]
        private VariableStoreLocation variableLocation = VariableStoreLocation.Undefined;

        public VariableStoreLocation VariableLocation { get => variableLocation; set => variableLocation = value; }

        /// <summary>
        /// The triggering event which triggers this event.
        /// </summary>
        [SerializeField]
        private TriggeringEvent triggeringEvent = TriggeringEvent.Undefined;
        public TriggeringEvent VariableTriggerEvent { get => triggeringEvent; set => triggeringEvent = value; }

        /// <summary>
        /// The name of the variable that triggers this event.
        /// </summary>
        [SerializeField]
        private string variableName = null;

        public string VariableName { get => variableName; set => variableName = value; }

        [SerializeField]
        private Variable valueMatch = null;
        public Variable ValueMatch { get => valueMatch; set => valueMatch = value; }

        private Variable variable = null;
        public Variable Variable { get => variable; }

        /// <summary>
        /// Start monitoring logic.
        /// </summary>
        public override void StartMonitoring()
        {
            base.StartMonitoring();

            // Generate a list of variables which this event is watching.
            switch (variableLocation)
            {
                case VariableStoreLocation.Global:

                    // Find the sequencer.
                    NarrativeSpace narrativeSpace = FindObjectOfType<NarrativeSpace>();

                    if (narrativeSpace != null)
                    {
                        variable = narrativeSpace.GlobalVariableStore.GetVariable<Variable>(variableName);
                    }

                    break;

                case VariableStoreLocation.Local:

                    VariableStore variableStore = GetComponent<NarrativeObject>().VariableStore;

                    if (variableStore != null)
                    {
                        variable = variableStore.GetVariable<Variable>(variableName);
                    }

                    break;
            }

            // Now register callback to the event being watched for on each variable found.
            switch (triggeringEvent)
            {
                case TriggeringEvent.OnSet:
                case TriggeringEvent.OnValueMatch:
                    if (variable != null)
                    {
                        variable.OnVariableSet += OnVariableSet;
                    }

                    break;
            }
        }

        /// <summary>
        /// Stop monitoring logic.
        /// </summary>
        public override void StopMonitoring()
        {
            // Unregister from any callbacks being listened to as monitoring has stopped.
            switch (triggeringEvent)
            {
                case TriggeringEvent.OnSet:
                case TriggeringEvent.OnValueMatch:
                    variable.OnVariableSet -= OnVariableSet;
                    break;
            }

            base.StopMonitoring();
        }

        /// <summary>
        /// Callback for monitoring variable being set.
        /// </summary>
        public void OnVariableSet(Variable variable)
        {
            switch (triggeringEvent)
            {
                case TriggeringEvent.OnSet:
                    triggered = true;
                    break;
                case TriggeringEvent.OnValueMatch:
                    if (valueMatch != null)
                    {
                        triggered = variable.ValueEqual(valueMatch);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
