using CuttingRoom.VariableSystem.Variables;
using UnityEngine;

public class StringVariableSetter : VariableSetter
{
    [SerializeField]
    private string value = string.Empty;

    public void Set()
    {
        Set<StringVariable>(value);
    }
    public override void Set(string value)
    {
        Set<StringVariable>(value);
    }
}