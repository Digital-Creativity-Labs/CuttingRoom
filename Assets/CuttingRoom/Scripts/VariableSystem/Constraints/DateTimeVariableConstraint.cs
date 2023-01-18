using System;
using System.Collections.Generic;
using UnityEngine;
using CuttingRoom.VariableSystem.Variables;

namespace CuttingRoom.VariableSystem.Constraints
{
	public class DateTimeConstraint : Constraint
	{
		public int hours = 0;
		public int minutes = 0;
		public int seconds = 0;
		public int milliseconds = 0;
		public int day = 1;
		public int month = 1;
		public int year = 1;

		private DateTime value;
        public override Type ValueType
        {
            get => value.GetType();
        }

        public override string Value => new DateTimeOffset(value).ToUnixTimeSeconds().ToString();

		public enum ComparisonType
		{
			Undefined,
			EqualTo,
			NotEqualTo,
			LessThan,
			GreaterThan,
			LessThanOrEqualTo,
			GreaterThanOrEqualTo,
		}

		private void OnValidate()
		{
			hours = Mathf.Clamp(hours, 0, 23);
			minutes = Mathf.Clamp(minutes, 0, 59);
			seconds = Mathf.Clamp(seconds, 0, 59);
			milliseconds = Mathf.Clamp(milliseconds, 0, 999);
			// To use DateTime.DaysInMonth, year must be clamped to 9999.
			// Don't worry future developer, existence doesn't end in the year 10,000.
			year = Mathf.Clamp(year, 1, 9999);
			month = Mathf.Clamp(month, 1, 12);
			day = Mathf.Clamp(day, 1, DateTime.DaysInMonth(year, month));
		}

		private void Awake()
		{
			value = new DateTime(year, month, day, hours, minutes, seconds, milliseconds);
		}

		public bool EqualTo(DateTimeVariable dateTimeVariable)
		{
			return dateTimeVariable.Value == value;
		}

		public bool NotEqualTo(DateTimeVariable dateTimeVariable)
		{
			return !EqualTo(dateTimeVariable);
		}

		public bool LessThan(DateTimeVariable dateTimeVariable)
		{
			return dateTimeVariable.Value > value;
		}

		public bool GreaterThan(DateTimeVariable dateTimeVariable)
        {
            return dateTimeVariable.Value < value;
        }

		public bool LessThanOrEqualTo(DateTimeVariable dateTimeVariable)
		{
			return LessThan(dateTimeVariable) || EqualTo(dateTimeVariable);
		}

		public bool GreaterThanOrEqualTo(DateTimeVariable dateTimeVariable)
		{
			return GreaterThan(dateTimeVariable) || EqualTo(dateTimeVariable);
		}
	}
}
