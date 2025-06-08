using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ludo.CrossInput
{
    /// <summary>
    /// Maps custom action names to InputManager's expected action names.
    /// This allows using existing Input Actions files without modification.
    /// </summary>
    [CreateAssetMenu(fileName = "InputActionMapping", menuName = "Ludo/CrossInput/Action Mapping")]
    public class InputActionMapping : ScriptableObject
    {
        [Header("Action Name Mappings")]
        [Tooltip("Map your existing action names to InputManager's expected names")]
        
        [SerializeField] private ActionNameMap[] actionMappings = new ActionNameMap[]
        {
            new ActionNameMap(InputActionNames.MOVE, "Move"),
            new ActionNameMap(InputActionNames.FIRE, "Attack"), // Example: Attack -> Fire
            new ActionNameMap(InputActionNames.JUMP, "Jump"),
            new ActionNameMap(InputActionNames.CROUCH, "Crouch"),
            new ActionNameMap(InputActionNames.SPRINT, "Run"), // Example: Run -> Sprint
            new ActionNameMap(InputActionNames.RELOAD, "Reload"),
            new ActionNameMap(InputActionNames.INVENTORY, "Inventory"),
            new ActionNameMap(InputActionNames.INTERACT, "Interact"),
            new ActionNameMap(InputActionNames.MAP, "Map"),
            new ActionNameMap(InputActionNames.PREVIOUS, "Previous"),
            new ActionNameMap(InputActionNames.NEXT, "Next"),
            new ActionNameMap(InputActionNames.PAUSE, "Pause"),
            new ActionNameMap(InputActionNames.BACK, "Back"),
            new ActionNameMap(InputActionNames.LEFT, "Left"),
            new ActionNameMap(InputActionNames.RIGHT, "Right"),
            new ActionNameMap(InputActionNames.PRESS, "Press"),
            new ActionNameMap(InputActionNames.POSITION, "Position")
        };

        private Dictionary<string, string> mappingCache;

        [Serializable]
        public class ActionNameMap
        {
            [Tooltip("The action name that InputManager expects")]
            public string expectedName;
            
            [Tooltip("Your existing action name in the Input Actions file")]
            public string actualName;

            public ActionNameMap(string expected, string actual)
            {
                expectedName = expected;
                actualName = actual;
            }
        }

        /// <summary>
        /// Gets the actual action name for the given expected name.
        /// </summary>
        public string GetActualActionName(string expectedName)
        {
            if (mappingCache == null)
            {
                BuildMappingCache();
            }

            return mappingCache.TryGetValue(expectedName, out string actualName) ? actualName : expectedName;
        }

        /// <summary>
        /// Gets the expected action name for the given actual name.
        /// </summary>
        public string GetExpectedActionName(string actualName)
        {
            if (mappingCache == null)
            {
                BuildMappingCache();
            }

            foreach (var kvp in mappingCache)
            {
                if (kvp.Value == actualName)
                    return kvp.Key;
            }

            return actualName;
        }

        /// <summary>
        /// Checks if an action mapping exists for the given expected name.
        /// </summary>
        public bool HasMapping(string expectedName)
        {
            if (mappingCache == null)
            {
                BuildMappingCache();
            }

            return mappingCache.ContainsKey(expectedName);
        }

        /// <summary>
        /// Validates that all mapped actions exist in the provided Input Actions asset.
        /// </summary>
        public ValidationResult ValidateMapping(UnityEngine.InputSystem.InputActionAsset actionAsset)
        {
            var result = new ValidationResult();
            
            if (actionAsset == null)
            {
                result.AddError("Input Actions asset is null");
                return result;
            }

            foreach (var mapping in actionMappings)
            {
                if (string.IsNullOrEmpty(mapping.actualName))
                {
                    result.AddWarning($"No actual name mapped for expected action '{mapping.expectedName}'");
                    continue;
                }

                var action = actionAsset.FindAction(mapping.actualName);
                if (action == null)
                {
                    result.AddError($"Action '{mapping.actualName}' not found in Input Actions asset (mapped from '{mapping.expectedName}')");
                }
                else
                {
                    result.AddSuccess($"Action '{mapping.actualName}' found for '{mapping.expectedName}'");
                }
            }

            return result;
        }

        private void BuildMappingCache()
        {
            mappingCache = new Dictionary<string, string>();
            
            foreach (var mapping in actionMappings)
            {
                if (!string.IsNullOrEmpty(mapping.expectedName) && !string.IsNullOrEmpty(mapping.actualName))
                {
                    mappingCache[mapping.expectedName] = mapping.actualName;
                }
            }
        }

        /// <summary>
        /// Creates a default mapping that assumes action names match InputActionNames.
        /// </summary>
        [ContextMenu("Create Default Mapping")]
        public void CreateDefaultMapping()
        {
            actionMappings = new ActionNameMap[]
            {
                new ActionNameMap(InputActionNames.MOVE, InputActionNames.MOVE),
                new ActionNameMap(InputActionNames.FIRE, InputActionNames.FIRE),
                new ActionNameMap(InputActionNames.JUMP, InputActionNames.JUMP),
                new ActionNameMap(InputActionNames.CROUCH, InputActionNames.CROUCH),
                new ActionNameMap(InputActionNames.SPRINT, InputActionNames.SPRINT),
                new ActionNameMap(InputActionNames.RELOAD, InputActionNames.RELOAD),
                new ActionNameMap(InputActionNames.INVENTORY, InputActionNames.INVENTORY),
                new ActionNameMap(InputActionNames.INTERACT, InputActionNames.INTERACT),
                new ActionNameMap(InputActionNames.MAP, InputActionNames.MAP),
                new ActionNameMap(InputActionNames.PREVIOUS, InputActionNames.PREVIOUS),
                new ActionNameMap(InputActionNames.NEXT, InputActionNames.NEXT),
                new ActionNameMap(InputActionNames.PAUSE, InputActionNames.PAUSE),
                new ActionNameMap(InputActionNames.BACK, InputActionNames.BACK),
                new ActionNameMap(InputActionNames.LEFT, InputActionNames.LEFT),
                new ActionNameMap(InputActionNames.RIGHT, InputActionNames.RIGHT),
                new ActionNameMap(InputActionNames.PRESS, InputActionNames.PRESS),
                new ActionNameMap(InputActionNames.POSITION, InputActionNames.POSITION)
            };

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        public class ValidationResult
        {
            public List<string> Errors { get; } = new List<string>();
            public List<string> Warnings { get; } = new List<string>();
            public List<string> Successes { get; } = new List<string>();

            public bool IsValid => Errors.Count == 0;

            public void AddError(string message) => Errors.Add(message);
            public void AddWarning(string message) => Warnings.Add(message);
            public void AddSuccess(string message) => Successes.Add(message);

            public string GetReport()
            {
                var report = "Input Action Mapping Validation Report:\n\n";
                
                if (Errors.Count > 0)
                {
                    report += "ERRORS:\n";
                    foreach (var error in Errors)
                        report += $"❌ {error}\n";
                    report += "\n";
                }

                if (Warnings.Count > 0)
                {
                    report += "WARNINGS:\n";
                    foreach (var warning in Warnings)
                        report += $"⚠️ {warning}\n";
                    report += "\n";
                }

                if (Successes.Count > 0)
                {
                    report += "SUCCESS:\n";
                    foreach (var success in Successes)
                        report += $"✅ {success}\n";
                }

                return report;
            }
        }
    }
}
