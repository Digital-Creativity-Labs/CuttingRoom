using CuttingRoom.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CuttingRoom.VariableSystem.Variables
{
	public class BoolVariable : Variable
	{
		public bool Value { get => value; }
        public bool defaultValue = false;

		[InspectorVisible]
		public bool value = false;

		public void Start()
		{
			value = defaultValue;
		}

		public void Invert()
		{
			Set(!Value);
		}

		public void Set(bool value)
		{
			this.value = value;

			RegisterVariableSet();
		}

		public void Set(string value)
		{
			bool parsedValue;

			if (bool.TryParse(value, out parsedValue))
			{
				Set(parsedValue);
			}
			else
			{
				Debug.LogError($"Bool parsing failed. Value: {value}");
			}
		}

		public override void SetValue(object newValue)
		{
			if (Value.GetType() == newValue.GetType())
			{
				Set((bool)newValue);
			}
		}

		public override string GetValueAsString()
		{
			return Value.ToString();
		}

		public override void SetValueFromString(string value)
		{
			Set(value);
        }

        public override bool ValueEqual(object val)
        {
            if (Value.GetType() == val.GetType())
			{
				bool typedVal = (bool)val;
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
