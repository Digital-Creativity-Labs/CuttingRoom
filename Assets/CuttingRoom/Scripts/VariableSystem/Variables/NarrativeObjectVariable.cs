
using CuttingRoom.Editor;
using System;
using UnityEngine;

namespace CuttingRoom.VariableSystem.Variables
{
    public class NarrativeObjectVariable : Variable
    {
        public NarrativeObject Value { get => value; }
        public NarrativeObject defaultValue = null;

        [InspectorVisible]
        [SerializeField]
        private NarrativeObject value = null;

        public void Start()
        {
            value = defaultValue;
        }

        public void Set(NarrativeObject newValue)
        {
            value = newValue;

#if UNITY_EDITOR
            defaultValue = value;
#endif

            RegisterVariableSet();
        }
        public override void SetValue(object newValue)
        {
            if (Value.GetType() == newValue.GetType())
            {
                Set((NarrativeObject)newValue);
            }
        }

        public override bool ValueEqual(object val)
        {
            if (Value.GetType() == val.GetType())
            {
                NarrativeObject typedVal = (NarrativeObject)val;
                return Value.Equals(typedVal);
            }
            else if (typeof(Variable).IsAssignableFrom(val.GetType()))
            {
                Variable var = val as Variable;
                return var.ValueEqual(Value);
            }
            return false;
        }
    }
}