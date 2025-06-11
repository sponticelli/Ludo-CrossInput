using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ludo.CrossInput
{
    /// <summary>
    /// Data structure for input bindings that can be serialized and configured.
    /// </summary>
    [Serializable]
    public class InputBindingData
    {
        [Header("Binding Configuration")]
        public string path = "";
        public string groups = "";
        public string processors = "";
        public string interactions = "";
        
        [Header("Composite Settings")]
        public bool isComposite = false;
        public bool isPartOfComposite = false;
        public string compositeName = "";

        public InputBindingData()
        {
        }

        public InputBindingData(string bindingPath)
        {
            path = bindingPath;
        }

        public InputBindingData(string bindingPath, string bindingGroups)
        {
            path = bindingPath;
            groups = bindingGroups;
        }

        /// <summary>
        /// Converts this data to an InputBinding struct.
        /// </summary>
        public InputBinding ToInputBinding()
        {
            var binding = new InputBinding
            {
                path = path,
                groups = groups,
                processors = processors,
                interactions = interactions,
                isComposite = isComposite,
                isPartOfComposite = isPartOfComposite
            };

            // Set the name field appropriately based on binding type
            if (isComposite && !string.IsNullOrEmpty(compositeName))
            {
                binding.name = compositeName;
            }
            else if (isPartOfComposite && !string.IsNullOrEmpty(compositeName))
            {
                binding.name = compositeName;
            }

            return binding;
        }

        /// <summary>
        /// Validates the binding data.
        /// </summary>
        public bool IsValid(out string errorMessage)
        {
            errorMessage = "";

            if (string.IsNullOrEmpty(path) && !isComposite)
            {
                errorMessage = "Binding path cannot be empty for non-composite bindings";
                return false;
            }

            if (isComposite && string.IsNullOrEmpty(compositeName))
            {
                errorMessage = "Composite name cannot be empty for composite bindings";
                return false;
            }

            return true;
        }
    }
}