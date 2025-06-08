using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ludo.CrossInput
{
    /// <summary>
    /// Manages input rebinding operations for runtime key/button remapping.
    /// Provides a simple interface for rebinding input actions and saving/loading bindings.
    /// </summary>
    public class InputRebindingManager : MonoBehaviour
    {
        [Header("Rebinding Settings")]
        [Tooltip("Timeout for rebinding operations in seconds")]
        [SerializeField] private float rebindTimeout = 5f;
        
        [Tooltip("Controls to exclude from rebinding (e.g., Mouse for gamepad-only rebinding)")]
        [SerializeField] private string[] excludedControls = { "Mouse" };
        
        [Tooltip("Enable automatic saving of rebindings")]
        [SerializeField] private bool autoSaveBindings = true;

        // Events
        public event Action<string, int> OnRebindStarted;
        public event Action<string, int> OnRebindCompleted;
        public event Action<string, int> OnRebindCanceled;
        public event Action<string, string> OnRebindFailed;

        // Properties
        public bool IsRebinding { get; private set; }
        public string CurrentRebindingAction { get; private set; }
        public int CurrentRebindingBindingIndex { get; private set; }

        private PlayerInput playerInput;
        private InputActionRebindingExtensions.RebindingOperation currentRebindOperation;

        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            if (playerInput == null)
            {
                Debug.LogError("InputRebindingManager requires a PlayerInput component");
            }
        }

        private void Start()
        {
            if (autoSaveBindings)
            {
                LoadBindings();
            }
        }

        private void OnDestroy()
        {
            CancelCurrentRebind();
        }

        /// <summary>
        /// Starts an interactive rebinding operation for the specified action and binding index.
        /// </summary>
        /// <param name="actionName">Name of the action to rebind</param>
        /// <param name="bindingIndex">Index of the binding to rebind</param>
        /// <param name="excludeControls">Additional controls to exclude from rebinding</param>
        public void StartRebinding(string actionName, int bindingIndex = 0, string[] excludeControls = null)
        {
            if (IsRebinding)
            {
                Debug.LogWarning("Already rebinding an action. Cancel current rebind first.");
                return;
            }

            if (playerInput?.actions == null)
            {
                OnRebindFailed?.Invoke(actionName, "PlayerInput or actions not available");
                return;
            }

            var action = playerInput.actions.FindAction(actionName);
            if (action == null)
            {
                OnRebindFailed?.Invoke(actionName, $"Action '{actionName}' not found");
                return;
            }

            if (bindingIndex >= action.bindings.Count)
            {
                OnRebindFailed?.Invoke(actionName, $"Binding index {bindingIndex} out of range");
                return;
            }

            try
            {
                IsRebinding = true;
                CurrentRebindingAction = actionName;
                CurrentRebindingBindingIndex = bindingIndex;

                // Disable the action during rebinding
                action.Disable();

                // Start the rebinding operation
                currentRebindOperation = action.PerformInteractiveRebinding(bindingIndex)
                    .WithTimeout(rebindTimeout)
                    .OnComplete(OnRebindComplete)
                    .OnCancel(OnRebindCancel);

                // Add excluded controls
                var controlsToExclude = excludeControls ?? excludedControls;
                foreach (var control in controlsToExclude)
                {
                    currentRebindOperation.WithControlsExcluding(control);
                }

                currentRebindOperation.Start();

                OnRebindStarted?.Invoke(actionName, bindingIndex);
                Debug.Log($"Started rebinding for action '{actionName}', binding {bindingIndex}");
            }
            catch (Exception ex)
            {
                IsRebinding = false;
                OnRebindFailed?.Invoke(actionName, ex.Message);
                Debug.LogError($"Error starting rebind: {ex.Message}");
            }
        }

        /// <summary>
        /// Cancels the current rebinding operation.
        /// </summary>
        public void CancelCurrentRebind()
        {
            if (!IsRebinding) return;

            try
            {
                currentRebindOperation?.Cancel();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error canceling rebind: {ex.Message}");
            }
        }

        /// <summary>
        /// Resets a specific binding to its default value.
        /// </summary>
        /// <param name="actionName">Name of the action</param>
        /// <param name="bindingIndex">Index of the binding to reset</param>
        public void ResetBinding(string actionName, int bindingIndex = 0)
        {
            if (playerInput?.actions == null) return;

            var action = playerInput.actions.FindAction(actionName);
            if (action == null)
            {
                Debug.LogWarning($"Action '{actionName}' not found");
                return;
            }

            try
            {
                action.RemoveBindingOverride(bindingIndex);
                
                if (autoSaveBindings)
                {
                    SaveBindings();
                }

                Debug.Log($"Reset binding for action '{actionName}', binding {bindingIndex}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error resetting binding: {ex.Message}");
            }
        }

        /// <summary>
        /// Resets all bindings to their default values.
        /// </summary>
        public void ResetAllBindings()
        {
            if (playerInput?.actions == null) return;

            try
            {
                playerInput.actions.RemoveAllBindingOverrides();
                
                if (autoSaveBindings)
                {
                    SaveBindings();
                }

                Debug.Log("Reset all bindings to defaults");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error resetting all bindings: {ex.Message}");
            }
        }

        /// <summary>
        /// Saves current binding overrides to PlayerPrefs.
        /// </summary>
        public void SaveBindings()
        {
            if (playerInput?.actions == null) return;

            try
            {
                var bindingOverrides = playerInput.actions.SaveBindingOverridesAsJson();
                PlayerPrefs.SetString("InputBindings", bindingOverrides);
                PlayerPrefs.Save();
                Debug.Log("Saved input bindings");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error saving bindings: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads binding overrides from PlayerPrefs.
        /// </summary>
        public void LoadBindings()
        {
            if (playerInput?.actions == null) return;

            try
            {
                var bindingOverrides = PlayerPrefs.GetString("InputBindings", string.Empty);
                if (!string.IsNullOrEmpty(bindingOverrides))
                {
                    playerInput.actions.LoadBindingOverridesFromJson(bindingOverrides);
                    Debug.Log("Loaded input bindings");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading bindings: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the display string for a specific binding.
        /// </summary>
        /// <param name="actionName">Name of the action</param>
        /// <param name="bindingIndex">Index of the binding</param>
        /// <returns>Display string for the binding</returns>
        public string GetBindingDisplayString(string actionName, int bindingIndex = 0)
        {
            if (playerInput?.actions == null) return "N/A";

            var action = playerInput.actions.FindAction(actionName);
            if (action == null || bindingIndex >= action.bindings.Count)
                return "N/A";

            try
            {
                return action.GetBindingDisplayString(bindingIndex);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error getting binding display string: {ex.Message}");
                return "Error";
            }
        }

        private void OnRebindComplete(InputActionRebindingExtensions.RebindingOperation operation)
        {
            try
            {
                var actionName = CurrentRebindingAction;
                var bindingIndex = CurrentRebindingBindingIndex;

                // Re-enable the action
                var action = playerInput.actions.FindAction(actionName);
                action?.Enable();

                // Save bindings if auto-save is enabled
                if (autoSaveBindings)
                {
                    SaveBindings();
                }

                OnRebindCompleted?.Invoke(actionName, bindingIndex);
                Debug.Log($"Completed rebinding for action '{actionName}', binding {bindingIndex}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error completing rebind: {ex.Message}");
            }
            finally
            {
                CleanupRebindOperation();
            }
        }

        private void OnRebindCancel(InputActionRebindingExtensions.RebindingOperation operation)
        {
            try
            {
                var actionName = CurrentRebindingAction;
                var bindingIndex = CurrentRebindingBindingIndex;

                // Re-enable the action
                var action = playerInput.actions.FindAction(actionName);
                action?.Enable();

                OnRebindCanceled?.Invoke(actionName, bindingIndex);
                Debug.Log($"Canceled rebinding for action '{actionName}', binding {bindingIndex}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error canceling rebind: {ex.Message}");
            }
            finally
            {
                CleanupRebindOperation();
            }
        }

        private void CleanupRebindOperation()
        {
            currentRebindOperation?.Dispose();
            currentRebindOperation = null;
            IsRebinding = false;
            CurrentRebindingAction = null;
            CurrentRebindingBindingIndex = -1;
        }
    }
}
