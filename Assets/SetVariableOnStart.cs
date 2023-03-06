using CuttingRoom;
using CuttingRoom.VariableSystem.Variables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetVariableOnStart : MonoBehaviour
{
    [SerializeField]
    private VariableSetter variableSetter = null;

    [SerializeField]
    private NarrativeObject narrativeObject = null;

    bool done = false;

    // Update is called once per frame
    void Update()
    {
        if (!done && narrativeObject != null && narrativeObject.VariableStore != null)
        {
            var hasPlayedVariable = narrativeObject.VariableStore.GetVariable("hasPlayed") as BoolVariable;

            if (hasPlayedVariable.Value && variableSetter != null)
            {
                variableSetter.Set("effect");
                Debug.Log($"Set {variableSetter.variableName} to effect");
                done = true;
            }
        }
    }
}
