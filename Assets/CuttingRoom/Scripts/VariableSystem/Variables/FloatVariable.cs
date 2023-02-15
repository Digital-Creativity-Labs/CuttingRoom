using CuttingRoom.Editor;
using System;
using UnityEngine;

namespace CuttingRoom.VariableSystem.Variables
{
	public class FloatVariable : Variable
	{
		public float Value { get => value; }
        public float defaultValue = 0.0f;

        [InspectorVisible]
        [SerializeField]
        private float value = 0.0f;

        public void Start()
        {
            value = defaultValue;
        }

        public void Increment()
		{
			value++;
		}

		public void Decrement()
		{
			value--;
		}

		public void Set(float newValue)
		{
			value = newValue;

			RegisterVariableSet();
		}

		public void Set(string newValue)
		{
			if (float.TryParse(newValue, out float parsedValue))
			{
				Set(parsedValue);
			}
			else
			{
				Debug.LogError($"Float parsing failed. Value: {newValue}");
			}
        }
        public override void SetValue(object newValue)
        {
            if (Value.GetType() == newValue.GetType())
            {
                Set((float)newValue);
            }
        }

        public override string GetValueAsString()
		{
			return Value.ToString();
		}

		public override void SetValueFromString(string newValue)
		{
			Set(newValue);
        }

        public override bool ValueEqual(object val)
        {
            if (Value.GetType() == val.GetType())
            {
                float typedVal = (float)val;
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