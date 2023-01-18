using CuttingRoom.VariableSystem.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using static CuttingRoom.ButtonUIController;

namespace CuttingRoom.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class ButtonUITemplateHandler : MonoBehaviour
    {
        [SerializeField]
        private VariableSetter variableSetter = null;

        VisualElement rootVisualElement = null;

        void OnEnable()
        {
            variableSetter = GetComponent<StringVariableSetter>();
            var uiDocument = GetComponent<UIDocument>();
            rootVisualElement = uiDocument.rootVisualElement;
        }

        //Functions as the event handlers for your button click and number counts 
        public void SetupButtonHandlers(VariableSetter variableSetter = null)
        {
            this.variableSetter = variableSetter ?? this.variableSetter;
            var buttons = rootVisualElement.Query<UIButton>();
            buttons.ForEach(RegisterHandler);
        }

        private void RegisterHandler(Button button)
        {
            button.RegisterCallback<ClickEvent>(SetVariable);
        }

        public void SetVariable(ClickEvent evt)
        {
            UIButton button = evt.currentTarget as UIButton;

            variableSetter.Set(button.value);
        }
    }
}
