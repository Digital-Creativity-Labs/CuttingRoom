using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CuttingRoom
{
    public class ProcessingEndTrigger : MonoBehaviour
    {
        /// <summary>
        /// The triggered state of this processing trigger.
        /// </summary>
        [HideInInspector]
        public bool triggered = false;

        /// <summary>
        /// Called to tell this trigger to start monitoring for its condition to be met.
        /// </summary>
        public virtual void StartMonitoring()
        {
            triggered = false;
        }

        /// <summary>
        /// Called to tell this trigger to stop monitoring for being triggered.
        /// </summary>
        public virtual void StopMonitoring()
        {
        }

        /// <summary>
        /// Called to forcibly end and trigger this processing trigger.
        /// </summary>
        public virtual void ForceTrigger()
        {
            triggered = true;
            StopMonitoring();
        }

        /// <summary>
        /// Awaitable coroutine for processing to continue.
        /// </summary>
        /// <returns></returns>
        public IEnumerator WaitForProcessingTrigger()
        {
            while (!triggered)
            {
                yield return new WaitForEndOfFrame();
            }
        }


#if UNITY_EDITOR
        public event Action OnChanged;

        public void OnValidate()
        {
            OnChanged?.Invoke();
        }
#endif
    }
}