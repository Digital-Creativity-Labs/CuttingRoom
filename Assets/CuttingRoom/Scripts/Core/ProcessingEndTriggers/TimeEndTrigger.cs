using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CuttingRoom
{
    public class TimeEndTrigger : ProcessingEndTrigger
    {
        /// <summary>
        /// The duration before the trigger is invoked.
        /// </summary>
        [SerializeField]
        public float duration = 0.0f;

        /// <summary>
        /// Handle to the timed coroutine once started.
        /// </summary>
        private Coroutine timedCoroutine = null;

        /// <summary>
        /// Invoked to start the timer.
        /// </summary>
        public override void StartMonitoring()
        {
            base.StartMonitoring();

            timedCoroutine = StartCoroutine(TimedCoroutine());
        }

        /// <summary>
        /// Invoked when this trigger is stopped.
        /// </summary>
        public override void StopMonitoring()
        {
            if (timedCoroutine != null)
            {
                StopCoroutine(timedCoroutine);

                timedCoroutine = null;
            }

            base.StopMonitoring();
        }

        /// <summary>
        /// Coroutine for waiting before proceeding with processing.
        /// </summary>
        /// <returns></returns>
        public IEnumerator TimedCoroutine()
        {
            yield return new WaitForSeconds(duration);

            triggered = true;
        }
    }
}
