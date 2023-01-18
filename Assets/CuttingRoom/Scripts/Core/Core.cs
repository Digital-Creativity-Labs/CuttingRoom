using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CuttingRoom
{
    /// <summary>
    /// Delegate used by decision points to report a single selection.
    /// </summary>
    /// <param name="selection"></param>
    /// <returns></returns>
    public delegate IEnumerator OnSelectionCallback(NarrativeObject selection);

    /// <summary>
    /// Delegate used by decision points to report multiple selections.
    /// </summary>
    /// <param name="selection"></param>
    /// <returns></returns>
    public delegate IEnumerator OnMultiSelectionCallback(List<NarrativeObject> selection);

    /// <summary>
    /// Events which trigger media sources to unload (or not).
    /// </summary>
    public enum MediaSourceUnloadEvent
    {
        Never,
        OnProcessingTriggerComplete,
        OnProcessingComplete,
    }
}