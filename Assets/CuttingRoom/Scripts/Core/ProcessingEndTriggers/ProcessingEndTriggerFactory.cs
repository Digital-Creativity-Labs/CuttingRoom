using System;
using Unity.Collections;
using UnityEditor;
using UnityEngine;

namespace CuttingRoom
{
    public static class ProcessingEndTriggerFactory
    {
        private static readonly ILogger logger = UnityEngine.Debug.unityLogger;

        public enum TriggerType
        {
            None,
            EndOfContent,
            Timed,
            Variable
        }

#if UNITY_EDITOR
        /// <summary>
        /// Add a processing trigger to a narrative object
        /// </summary>
        /// <param name="narrativeObject"></param>
        /// <param name="triggerType"></param>
        public static ProcessingEndTrigger AddProcessingTriggerToNarrativeObject(NarrativeObject narrativeObject, TriggerType triggerType)
        {
            if (narrativeObject != null)
            {
                ProcessingEndTrigger processingTrigger = AddProcessingTriggerToGameObject(narrativeObject.gameObject, triggerType);

                if (processingTrigger != null)
                {
                    Undo.RecordObject(narrativeObject, "Add End Trigger");
                    narrativeObject.AddProcessingTrigger(processingTrigger);
                }

                return processingTrigger;
            }
            return null;
        }

        private static ProcessingEndTrigger AddProcessingTriggerToGameObject(GameObject obj, TriggerType triggerType)
        {
            if (obj == null)
            {
                return null;
            }

            if (triggerType == TriggerType.EndOfContent)
            {
                // Only makes sense to have one end of content trigger so get existing or create new.
                if (obj.TryGetComponent(out EndOfContentTrigger trigger))
                {
                    return trigger;
                }
                else
                {
                    return obj.AddComponent<EndOfContentTrigger>();
                }
            }
            else if (triggerType == TriggerType.Timed)
            {
                // Only makes sense to have one timed trigger so get existing or create new.
                if (obj.TryGetComponent(out TimeEndTrigger trigger))
                {
                    return trigger;
                }
                else
                {
                    return obj.AddComponent<TimeEndTrigger>();
                }
            }
            else if (triggerType == TriggerType.Variable)
            {
                // Can support multiple variable end triggers to just add.
                return obj.AddComponent<VariableEndTrigger>();
            }
            else
            {
                // Invalid Trigger
                return null;
            }
        }
#endif

        public static TriggerType GetTriggerType(ProcessingEndTrigger processingTrigger)
        {
            if (processingTrigger != null)
            {
                Type type = processingTrigger.GetType();

                if (type == typeof(ProcessingEndTrigger))
                {
                    // Cannot create instance of base type.
                    logger.LogError("CuttingRoom", "Cannot create processing trigger of base type.");
                }
                else if (type == typeof(EndOfContentTrigger))
                {
                    return TriggerType.EndOfContent;
                }
                else if (type == typeof(TimeEndTrigger))
                {
                    return TriggerType.Timed;
                }
                else if (type == typeof(VariableEndTrigger))
                {
                    return TriggerType.Variable;
                }
            }
            return TriggerType.None;
        }
    }
}
