using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CuttingRoom
{
    public class EndOfContentTrigger : ProcessingEndTrigger
    {
        /// <summary>
        /// Handle to the timed coroutine once started.
        /// </summary>
        public Coroutine ContentCoroutine {
            get => contentCoroutine;
            set
            {
                contentCoroutine = value;
                contentCoroutineStale = false;
            }
        }
        private Coroutine contentCoroutine = null;

        private bool contentCoroutineStale = false;

        /// <summary>
        /// Handle to the timed coroutine once started.
        /// </summary>
        private Coroutine waitForContentCoroutine = null;

        /// <summary>
        /// Invoked to start the timer.
        /// </summary>
        public override void StartMonitoring()
        {
            base.StartMonitoring();

            waitForContentCoroutine = StartCoroutine(WaitForContentCoroutine());
        }

        /// <summary>
        /// Invoked when this trigger is stopped.
        /// </summary>
        public override void StopMonitoring()
        {
            if (waitForContentCoroutine != null)
            {
                StopCoroutine(waitForContentCoroutine);

                contentCoroutine = null;
                waitForContentCoroutine = null;
            }

            base.StopMonitoring();
        }

        /// <summary>
        /// Coroutine for waiting before proceeding with processing.
        /// </summary>
        /// <returns></returns>
        public IEnumerator WaitForContentCoroutine()
        {
            // Wait for content coroutine
            while (contentCoroutine == null || contentCoroutineStale)
            {
                yield return new WaitForEndOfFrame();
            }

            if (contentCoroutine != null)
            {
                yield return contentCoroutine;
            }

            triggered = true;
            contentCoroutineStale = true;
        }
    }
}
