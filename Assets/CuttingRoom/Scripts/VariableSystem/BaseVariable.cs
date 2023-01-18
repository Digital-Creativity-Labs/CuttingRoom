using UnityEngine;
using System;

namespace CuttingRoom.VariableSystem.Variables
{
	public abstract class BaseVariable
    {
        public string Name = null;

        /// <summary>
        /// Callback which is invoked when variable changes.
        /// </summary>
        public delegate void OnVariableSetCallback(BaseVariable variable);
		public event OnVariableSetCallback OnVariableSet = null;

        public virtual void SetValue(object value)
        {
            throw new NotImplementedException();
        }

        public virtual string GetValueAsString()
		{
			return string.Empty;
		}

		public virtual void SetValueFromString(string value)
		{
			throw new NotImplementedException();
        }

        /// <summary>
        /// Compares a value with this variables own value.
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public virtual bool ValueEqual(object val)
        {
            throw new NotImplementedException();
        }

        protected void RegisterVariableSet()
		{
			OnVariableSet?.Invoke(this);
		}
	}
}