using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

namespace Ludo.CrossInput
{
    /// <summary>
    /// Handles Unity's new Input System integration with proper event management and error handling.
    /// </summary>
    public class NewInput : IDisposable
    {
        private readonly Dictionary<string, bool> _inputFlags = new Dictionary<string, bool>();
        private readonly object _inputLock = new object();
        private PlayerInput _playerInput;
        private InputActionMapping _actionMapping;
        private bool _disposed = false;

        /// <summary>
        /// Event triggered when an input action is performed.
        /// </summary>
        public event Action<string, InputAction.CallbackContext> OnInputPerformed;

        public NewInput(PlayerInput playerInput, InputActionMapping actionMapping = null)
        {
            _playerInput = playerInput ?? throw new ArgumentNullException(nameof(playerInput));
            _actionMapping = actionMapping;
            InitializeInputFlags();
            SubscribeToInputEvents();
        }

        private void InitializeInputFlags()
        {
            lock (_inputLock)
            {
                _inputFlags[InputActionNames.FIRE] = false;
                _inputFlags[InputActionNames.JUMP] = false;
                _inputFlags[InputActionNames.CROUCH] = false;
                _inputFlags[InputActionNames.SPRINT] = false;
                _inputFlags[InputActionNames.RELOAD] = false;
                _inputFlags[InputActionNames.INVENTORY] = false;
                _inputFlags[InputActionNames.INTERACT] = false;
                _inputFlags[InputActionNames.MAP] = false;
                _inputFlags[InputActionNames.PREVIOUS] = false;
                _inputFlags[InputActionNames.NEXT] = false;
                _inputFlags[InputActionNames.BACK] = false;
                _inputFlags[InputActionNames.PAUSE] = false;
            }
        }

        private void SubscribeToInputEvents()
        {
            if (_playerInput?.actions == null)
            {
                Debug.LogError("PlayerInput or actions is null in NewInput constructor");
                return;
            }

            try
            {
                SubscribeToAction(InputActionNames.FIRE);
                SubscribeToAction(InputActionNames.JUMP);
                SubscribeToAction(InputActionNames.CROUCH);
                SubscribeToAction(InputActionNames.SPRINT);
                SubscribeToAction(InputActionNames.RELOAD);
                SubscribeToAction(InputActionNames.INVENTORY);
                SubscribeToAction(InputActionNames.INTERACT);
                SubscribeToAction(InputActionNames.MAP);
                SubscribeToAction(InputActionNames.PREVIOUS);
                SubscribeToAction(InputActionNames.NEXT);
                SubscribeToAction(InputActionNames.BACK);
                SubscribeToAction(InputActionNames.PAUSE);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error subscribing to input events: {ex.Message}");
            }
        }

        private void SubscribeToAction(string expectedActionName)
        {
            // Get the actual action name from mapping (if available)
            string actualActionName = _actionMapping?.GetActualActionName(expectedActionName) ?? expectedActionName;

            var action = _playerInput.actions.FindAction(actualActionName);
            if (action != null)
            {
                action.performed += ctx => OnActionPerformed(expectedActionName, ctx);
            }
            else
            {
                if (_actionMapping != null && actualActionName != expectedActionName)
                {
                    Debug.LogWarning($"Input action '{actualActionName}' (mapped from '{expectedActionName}') not found in PlayerInput");
                }
                else
                {
                    Debug.LogWarning($"Input action '{expectedActionName}' not found in PlayerInput");
                }
            }
        }

        private void OnActionPerformed(string actionName, InputAction.CallbackContext context)
        {
            try
            {
                lock (_inputLock)
                {
                    _inputFlags[actionName] = context.ReadValue<float>() > 0.5f;
                }
                OnInputPerformed?.Invoke(actionName, context);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error handling input action '{actionName}': {ex.Message}");
            }
        }

        /// <summary>
        /// Gets and consumes an input flag. Returns true if the input was triggered since last check.
        /// </summary>
        public bool GetInputFlag(string actionName)
        {
            lock (_inputLock)
            {
                if (_inputFlags.TryGetValue(actionName, out bool value) && value)
                {
                    _inputFlags[actionName] = false;
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Sets an input flag manually (useful for mobile button integration).
        /// </summary>
        public void SetInputFlag(string actionName, bool value)
        {
            lock (_inputLock)
            {
                if (_inputFlags.ContainsKey(actionName))
                {
                    _inputFlags[actionName] = value;
                }
            }
        }

        public T GetActionValue<T>(PlayerInput input, string expectedActionName) where T : struct
        {
            try
            {
                // Get the actual action name from mapping (if available)
                string actualActionName = _actionMapping?.GetActualActionName(expectedActionName) ?? expectedActionName;

                var action = input?.actions?.FindAction(actualActionName);
                if (action != null)
                {
                    return action.ReadValue<T>();
                }

                if (_actionMapping != null && actualActionName != expectedActionName)
                {
                    Debug.LogWarning($"Input action '{actualActionName}' (mapped from '{expectedActionName}') not found");
                }
                else
                {
                    Debug.LogWarning($"Input action '{expectedActionName}' not found");
                }
                return default;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error reading input action '{expectedActionName}': {ex.Message}");
                return default;
            }
        }

        public Vector2 GetMove(PlayerInput input)
        {
            return GetActionValue<Vector2>(input, InputActionNames.MOVE);
        }

        public bool GetButton(PlayerInput input, string expectedActionName)
        {
            try
            {
                // Get the actual action name from mapping (if available)
                string actualActionName = _actionMapping?.GetActualActionName(expectedActionName) ?? expectedActionName;

                var action = input?.actions?.FindAction(actualActionName);
                return action != null && action.WasPressedThisFrame();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error checking button '{expectedActionName}': {ex.Message}");
                return false;
            }
        }

        public Vector2 GetPosition(PlayerInput input)
        {
            return GetActionValue<Vector2>(input, InputActionNames.POSITION);
        }

        public bool GetPress(PlayerInput input)
        {
            return GetButton(input, InputActionNames.PRESS);
        }

        public bool GetRelease(PlayerInput input)
        {
            try
            {
                var action = input?.actions?.FindAction(InputActionNames.PRESS);
                return action != null && action.WasReleasedThisFrame();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error checking press release: {ex.Message}");
                return false;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                UnsubscribeFromInputEvents();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error during NewInput disposal: {ex.Message}");
            }
            finally
            {
                _disposed = true;
            }
        }

        private void UnsubscribeFromInputEvents()
        {
            if (_playerInput?.actions == null) return;

            UnsubscribeFromAction(InputActionNames.FIRE);
            UnsubscribeFromAction(InputActionNames.JUMP);
            UnsubscribeFromAction(InputActionNames.CROUCH);
            UnsubscribeFromAction(InputActionNames.SPRINT);
            UnsubscribeFromAction(InputActionNames.RELOAD);
            UnsubscribeFromAction(InputActionNames.INVENTORY);
            UnsubscribeFromAction(InputActionNames.INTERACT);
            UnsubscribeFromAction(InputActionNames.MAP);
            UnsubscribeFromAction(InputActionNames.PREVIOUS);
            UnsubscribeFromAction(InputActionNames.NEXT);
            UnsubscribeFromAction(InputActionNames.BACK);
            UnsubscribeFromAction(InputActionNames.PAUSE);
        }

        private void UnsubscribeFromAction(string actionName)
        {
            var action = _playerInput.actions.FindAction(actionName);
            if (action != null)
            {
                action.performed -= ctx => OnActionPerformed(actionName, ctx);
            }
        }
    }
}