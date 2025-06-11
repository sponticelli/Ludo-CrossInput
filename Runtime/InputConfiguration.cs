using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ludo.CrossInput
{
    /// <summary>
    /// Configuration data for custom input actions that can be added at runtime.
    /// </summary>
    [Serializable]
    public class InputConfiguration
    {
        [Header("Action Settings")]
        public string actionName;
        public InputActionType actionType = InputActionType.Button;
        public string expectedControlType = "";
        
        [Header("Bindings")]
        public List<InputBindingData> bindings = new List<InputBindingData>();
        
        [Header("Processors and Interactions")]
        public string processors = "";
        public string interactions = "";
        
        [Header("Options")]
        public bool initialStateCheck = false;

        public InputConfiguration()
        {
            bindings = new List<InputBindingData>();
        }

        public InputConfiguration(string name, InputActionType type)
        {
            actionName = name;
            actionType = type;
            bindings = new List<InputBindingData>();
        }

        /// <summary>
        /// Validates the configuration for completeness and correctness.
        /// </summary>
        public bool IsValid(out string errorMessage)
        {
            errorMessage = "";

            if (string.IsNullOrEmpty(actionName))
            {
                errorMessage = "Action name cannot be empty";
                return false;
            }

            if (bindings.Count == 0)
            {
                errorMessage = "At least one binding is required";
                return false;
            }

            foreach (var binding in bindings)
            {
                if (!binding.IsValid(out string bindingError))
                {
                    errorMessage = $"Invalid binding: {bindingError}";
                    return false;
                }
            }

            return true;
        }
    }
}