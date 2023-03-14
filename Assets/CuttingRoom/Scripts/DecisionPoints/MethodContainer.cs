using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CuttingRoom
{
	/// <summary>
	/// Container for the method name, which allows rendering with a custom property drawer.
	/// </summary>
	[Serializable]
	public class MethodContainer
	{
		/// <summary>
		/// When method reflection fails, this is thrown.
		/// </summary>
		public class InvalidMethodException : Exception { }

		/// <summary>
		/// The class which contains the method being used.
		/// </summary>
		[HideInInspector]
		public string methodClass = string.Empty;

		/// <summary>
		/// The name of the method to be invoked within the defined method class.
		/// </summary>
		public string methodName = string.Empty;

		/// <summary>
		/// MethodInfo for the defined, reflected method.
		/// </summary>
		public MethodInfo methodInfo { get; private set; } = null;

		/// <summary>
		/// Data structure used to pass required data to decision point methods.
		/// </summary>
		public struct Args
		{
			/// <summary>
			/// Method to invoke when a selection is made.
			/// </summary>
			public OnSelectionCallback onSelection;

            /// <summary>
            /// Method to invoke when a multi-selection is made.
            /// </summary>
            public OnMultiSelectionCallback onMultiSelection;

            /// <summary>
            /// Valid candidates to select from.
            /// </summary>
            public List<NarrativeObject> candidates;
		}

		/// <summary>
		/// Whether this component has successfully initialised.
		/// </summary>
		public bool Initialised { get { return methodInfo != null; } }

		/// <summary>
		/// Initialises the class and fetches the method to be invoked.
		/// </summary>
		internal void Init()
		{
			Type methodClassType = Type.GetType(methodClass.ToString());

			if (methodClassType != null)
			{
				if (!string.IsNullOrEmpty(methodName))
				{
					methodInfo = methodClassType.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(Args) }, null);
				}
			}
		}
	}
}
