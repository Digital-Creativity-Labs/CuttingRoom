using System;
using UnityEngine;

namespace CuttingRoom.VariableSystem.Variables
{
	public class IntVariable : Variable
	{
		/// <summary>
		/// Value of this variable.
		/// </summary>
		public int Value { get => value; }
        [SerializeField]
        private int value = 0;

		public void Increment()
		{
			value++;
		}

		public void Decrement()
		{
			value--;
		}

		public void Set(int newValue)
		{
			value = newValue;

			RegisterVariableSet();
		}

		public void Set(string newValue)
		{
			if (int.TryParse(newValue, out int parsedValue))
			{
				Set(parsedValue);
			}
			else
			{
				Debug.LogError($"Int parsing failed. Value: {newValue}");
			}
        }
        public override void SetValue(object newValue)
        {
            if (Value.GetType() == newValue.GetType())
            {
                Set((int)newValue);
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
                int typedVal = (int)val;
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