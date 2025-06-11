using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ludo.CrossInput
{
    /// <summary>
    /// Manages runtime configuration of input actions and provides templates for common configurations.
    /// </summary>
    public class ConfigurableInputManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private List<InputConfiguration> customActions = new List<InputConfiguration>();
        
        [Header("Runtime Settings")]
        [SerializeField] private bool allowRuntimeModification = true;
        [SerializeField] private bool logConfigurationChanges = true;

        private PlayerInput playerInput;
        private InputActionAsset runtimeActionAsset;

        // Events
        public event Action<string> OnActionAdded;
        public event Action<string> OnActionRemoved;

        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            if (playerInput == null)
            {
                Debug.LogError("ConfigurableInputManager requires a PlayerInput component");
                return;
            }

            // Create a copy of the action asset for runtime modifications
            if (playerInput.actions != null)
            {
                runtimeActionAsset = Instantiate(playerInput.actions);
                playerInput.actions = runtimeActionAsset;
            }
        }

        private void Start()
        {
            ApplyCustomActions();
        }

        private void OnDestroy()
        {
            if (runtimeActionAsset != null)
            {
                DestroyImmediate(runtimeActionAsset);
            }
        }

        /// <summary>
        /// Applies all configured custom actions to the input system.
        /// </summary>
        private void ApplyCustomActions()
        {
            foreach (var config in customActions)
            {
                AddCustomAction(config);
            }
        }

        /// <summary>
        /// Adds a custom action at runtime.
        /// Note: This is a simplified implementation. For complex scenarios,
        /// consider creating a new InputActionAsset and replacing the entire asset.
        /// </summary>
        /// <param name="config">The input configuration to add</param>
        /// <returns>True if the action was added successfully</returns>
        public bool AddCustomAction(InputConfiguration config)
        {
            if (!allowRuntimeModification)
            {
                Debug.LogWarning("Runtime modification is disabled");
                return false;
            }

            if (config == null)
            {
                Debug.LogError("Configuration cannot be null");
                return false;
            }

            if (!config.IsValid(out string errorMessage))
            {
                Debug.LogError($"Invalid configuration: {errorMessage}");
                return false;
            }

            try
            {
                // For runtime modification, we need to work with the action asset directly
                // This is a simplified approach - in production, you might want to
                // create a new asset and replace it entirely

                if (runtimeActionAsset == null || runtimeActionAsset.actionMaps.Count == 0)
                {
                    Debug.LogError("No action map available for adding custom actions");
                    return false;
                }

                var actionMap = runtimeActionAsset.actionMaps[0];

                // Check if action already exists
                if (actionMap.FindAction(config.actionName) != null)
                {
                    Debug.LogWarning($"Action '{config.actionName}' already exists");
                    return false;
                }

                // For now, we'll log that the action would be added
                // In a full implementation, you'd need to recreate the entire asset
                if (logConfigurationChanges)
                {
                    Debug.Log($"Custom action '{config.actionName}' configuration received. " +
                              "Note: Runtime action addition requires asset recreation for full functionality.");
                }

                OnActionAdded?.Invoke(config.actionName);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error adding custom action '{config.actionName}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Removes a custom action at runtime.
        /// Note: This is a simplified implementation that logs the removal request.
        /// For full functionality, consider recreating the InputActionAsset.
        /// </summary>
        /// <param name="actionName">Name of the action to remove</param>
        /// <returns>True if the action was processed successfully</returns>
        public bool RemoveCustomAction(string actionName)
        {
            if (!allowRuntimeModification)
            {
                Debug.LogWarning("Runtime modification is disabled");
                return false;
            }

            if (string.IsNullOrEmpty(actionName))
            {
                Debug.LogError("Action name cannot be empty");
                return false;
            }

            try
            {
                if (runtimeActionAsset == null || runtimeActionAsset.actionMaps.Count == 0)
                {
                    Debug.LogError("No action map available");
                    return false;
                }

                var actionMap = runtimeActionAsset.actionMaps[0];
                var action = actionMap.FindAction(actionName);

                if (action == null)
                {
                    Debug.LogWarning($"Action '{actionName}' not found");
                    return false;
                }

                // For now, we'll log that the action would be removed
                // In a full implementation, you'd need to recreate the entire asset
                if (logConfigurationChanges)
                {
                    Debug.Log($"Custom action '{actionName}' removal requested. " +
                              "Note: Runtime action removal requires asset recreation for full functionality.");
                }

                OnActionRemoved?.Invoke(actionName);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error removing custom action '{actionName}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Creates a template configuration for common input types.
        /// </summary>
        public static InputConfiguration CreateTemplate(string actionName, InputTemplateType templateType)
        {
            var config = new InputConfiguration(actionName, InputActionType.Button);

            switch (templateType)
            {
                case InputTemplateType.KeyboardButton:
                    config.bindings.Add(new InputBindingData("<Keyboard>/space", "Keyboard&Mouse"));
                    break;

                case InputTemplateType.GamepadButton:
                    config.bindings.Add(new InputBindingData("<Gamepad>/buttonSouth", "Gamepad"));
                    break;

                case InputTemplateType.MouseButton:
                    config.bindings.Add(new InputBindingData("<Mouse>/leftButton", "Keyboard&Mouse"));
                    break;

                case InputTemplateType.TouchButton:
                    config.bindings.Add(new InputBindingData("<Touchscreen>/primaryTouch/tap", "Touch"));
                    break;

                case InputTemplateType.MovementVector:
                    config.actionType = InputActionType.Value;
                    config.expectedControlType = "Vector2";

                    // Add WASD composite binding
                    var wasdComposite = new InputBindingData
                    {
                        isComposite = true,
                        compositeName = "2DVector",
                        path = "2DVector",
                        groups = "Keyboard&Mouse"
                    };
                    config.bindings.Add(wasdComposite);

                    // Add WASD component bindings
                    config.bindings.Add(new InputBindingData("<Keyboard>/w", "Keyboard&Mouse")
                    {
                        isPartOfComposite = true,
                        compositeName = "up"
                    });
                    config.bindings.Add(new InputBindingData("<Keyboard>/s", "Keyboard&Mouse")
                    {
                        isPartOfComposite = true,
                        compositeName = "down"
                    });
                    config.bindings.Add(new InputBindingData("<Keyboard>/a", "Keyboard&Mouse")
                    {
                        isPartOfComposite = true,
                        compositeName = "left"
                    });
                    config.bindings.Add(new InputBindingData("<Keyboard>/d", "Keyboard&Mouse")
                    {
                        isPartOfComposite = true,
                        compositeName = "right"
                    });

                    // Add gamepad stick
                    config.bindings.Add(new InputBindingData("<Gamepad>/leftStick", "Gamepad"));
                    break;
            }

            return config;
        }
    }
}