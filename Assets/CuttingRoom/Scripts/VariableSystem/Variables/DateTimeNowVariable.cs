using System;
using UnityEngine;

namespace CuttingRoom.VariableSystem.Variables
{
	public class DateTimeNowVariable : Variable
	{
		public DateTime Value { get { return DateTime.Now; } }

        public override bool ValueEqual(object val)
        {
            if (Value.GetType() == val.GetType())
            {
                DateTime typedVal = (DateTime)val;
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
