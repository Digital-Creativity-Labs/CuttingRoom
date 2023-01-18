using CuttingRoom.VariableSystem.Variables;
using UnityEngine;

public class IntVariableSetter : VariableSetter
{
    [SerializeField]
    private int value = 0;

    public void Set()
    {
        Set<IntVariable>(value.ToString());
    }
    public void Set(int value)
    {
        Set<IntVariable>(value.ToString());
    }
}
