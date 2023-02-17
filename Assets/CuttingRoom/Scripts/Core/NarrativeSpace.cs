using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CuttingRoom.VariableSystem;
using CuttingRoom.VariableSystem.Variables;
using System;

namespace CuttingRoom
{
    [RequireComponent(typeof(VariableStore))]
    public class NarrativeSpace : MonoBehaviour
    {
        /// <summary>
        /// The first object processed in this narrative space.
        /// </summary>
        [SerializeField]
        private NarrativeObject rootNarrativeObject = null;

        /// <summary>
        /// Flag to unlock advanced features.
        /// </summary>
        [SerializeField]
        private bool unlockAdvancedFeatures = false;
        public bool UnlockAdvancedFeatures { get => unlockAdvancedFeatures; }

        /// <summary>
        /// Get accessor for the root narrative object.
        /// </summary>
        public NarrativeObject RootNarrativeObject { get { return rootNarrativeObject; } set { rootNarrativeObject = value; } }

        /// <summary>
        /// The global variable store.
        /// </summary>
        [SerializeField]
        private VariableStore globalVariableStore = null;

        /// <summary>
        /// Get accessor for the global variable store.
        /// </summary>
        public VariableStore GlobalVariableStore { get { return globalVariableStore; } set { globalVariableStore = value; } }

        public void Awake()
        {
            globalVariableStore = GetComponent<VariableStore>();
#if UNITY_EDITOR
            InitialiseVariableStore();
#endif
        }

#if UNITY_EDITOR
        public void Reset()
        {
            if (globalVariableStore == null)
            {
                // Set reference to variable store.
                globalVariableStore = GetComponent<VariableStore>();
            }
            InitialiseVariableStore();
        }

        public void InitialiseVariableStore()
        {
            if (globalVariableStore == null)
            {
                globalVariableStore = GetComponent<VariableStore>();
            }
        }

        public event Action OnChanged;

        public virtual void OnValidate()
        {
            if (globalVariableStore != null)
            {
                foreach (var variable in globalVariableStore.variableList)
                {
                    if (variable != null)
                    {
                        variable.OnVariableSet -= OnVariableChange;
                        variable.OnVariableSet += OnVariableChange;
                    }
                }
            }
            OnChanged?.Invoke();
        }

        private void OnVariableChange(Variable variable)
        {
            OnChanged?.Invoke();
        }
#endif
    }
}