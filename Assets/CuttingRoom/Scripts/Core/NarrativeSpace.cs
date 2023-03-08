using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CuttingRoom.VariableSystem;
using CuttingRoom.VariableSystem.Variables;
using System;
using UnityEditor;
using System.Threading;

namespace CuttingRoom
{
    [RequireComponent(typeof(VariableStore))]
    [ExecuteInEditMode]
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

        private Sequencer sequencer = null;

        public Sequencer Sequencer { get => sequencer; }

        /// <summary>
        /// Get accessor for the global variable store.
        /// </summary>
        public VariableStore GlobalVariableStore { get { return globalVariableStore; } set { globalVariableStore = value; } }

        /// <summary>
        /// Root cancellation token for sequencer
        /// </summary>
        public readonly CancellationTokenSource rootCancellationToken = new();

        public void Start()
        {
            bool isPlaying = Application.isPlaying;
#if UNITY_EDITOR
            isPlaying = isPlaying || EditorApplication.isPlaying;
#endif
            if (isPlaying)
            {
                sequencer = new(rootNarrativeObject, this, true);
                sequencer.Start(rootCancellationToken.Token);
            }
        }

#if UNITY_EDITOR
        public void Awake()
        {
            InitialiseVariableStore();
        }

        /// <summary>
        /// Ensure variable store is correctly initialised and contains required variables.
        /// </summary>
        public void InitialiseVariableStore()
        {
            if (globalVariableStore == null)
            {
                globalVariableStore = GetComponent<VariableStore>();
            }
            if (!globalVariableStore.Variables.ContainsKey("true"))
            {
                BoolVariable trueVariable = globalVariableStore.GetOrAddVariable<BoolVariable>("TRUE", Variable.VariableCategory.SystemDefined, true) as BoolVariable;
                globalVariableStore.RefreshDictionary();
            }
            if (!globalVariableStore.Variables.ContainsKey("false"))
            {
                BoolVariable falseVariable = globalVariableStore.GetOrAddVariable<BoolVariable>("FALSE", Variable.VariableCategory.SystemDefined, false) as BoolVariable;
                globalVariableStore.RefreshDictionary();
            }
        }

        public event Action OnChanged;

        /// <summary>
        /// Updates event handlers for all variables
        /// </summary>
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