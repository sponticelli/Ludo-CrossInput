using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Ludo.CrossInput
{
    /// <summary>
    /// Individual player input controller that implements IInputManager for a specific player.
    /// Each player has their own input state, mobile controls, and configuration.
    /// </summary>
    public class PlayerInputController : IInputManager, IDisposable
    {
        // Player identification
        public int PlayerId { get; private set; }
        public PlayerInput PlayerInput { get; private set; }
        
        // Core components
        private readonly MultiPlayerInputManager parentManager;
        private NewInput newInput;
        private InputBuffer inputBuffer;
        private Camera playerCamera;
        private GraphicRaycaster graphicRaycaster;
        private Joystick mobileJoystick;
        
        // Configuration
        private InputActionMapping actionMapping;
        private bool enableInputBuffering = true;
        private float inputBufferTime = 0.1f;
        private float swipeThreshold = 50f;
        
        // Mobile controls
        private readonly Dictionary<string, Button> mobileButtons = new Dictionary<string, Button>();
        private readonly Dictionary<string, UnityEngine.Events.UnityAction> buttonHandlers = new Dictionary<string, UnityAction>();
        
        // State
        private Vector2 swipeStartPos;
        private bool disposed = false;

        public PlayerInputController(int playerId, PlayerInput playerInput, MultiPlayerInputManager parent)
        {
            PlayerId = playerId;
            PlayerInput = playerInput ?? throw new ArgumentNullException(nameof(playerInput));
            parentManager = parent ?? throw new ArgumentNullException(nameof(parent));
            
            Initialize();
        }

        private void Initialize()
        {
            try
            {
                // Initialize input system
                newInput = new NewInput(PlayerInput, actionMapping);
                
                if (enableInputBuffering)
                {
                    inputBuffer = new InputBuffer(inputBufferTime);
                }
                
                // Setup mobile button handlers
                InitializeMobileButtonHandlers();
                
                Debug.Log($"Player {PlayerId} input controller initialized");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error initializing Player {PlayerId} input controller: {ex.Message}");
            }
        }

        private void InitializeMobileButtonHandlers()
        {
            buttonHandlers[InputActionNames.FIRE] = () => OnMobileButtonPressed(InputActionNames.FIRE);
            buttonHandlers[InputActionNames.JUMP] = () => OnMobileButtonPressed(InputActionNames.JUMP);
            buttonHandlers[InputActionNames.CROUCH] = () => OnMobileButtonPressed(InputActionNames.CROUCH);
            buttonHandlers[InputActionNames.SPRINT] = () => OnMobileButtonPressed(InputActionNames.SPRINT);
            buttonHandlers[InputActionNames.RELOAD] = () => OnMobileButtonPressed(InputActionNames.RELOAD);
            buttonHandlers[InputActionNames.INVENTORY] = () => OnMobileButtonPressed(InputActionNames.INVENTORY);
            buttonHandlers[InputActionNames.INTERACT] = () => OnMobileButtonPressed(InputActionNames.INTERACT);
            buttonHandlers[InputActionNames.MAP] = () => OnMobileButtonPressed(InputActionNames.MAP);
            buttonHandlers[InputActionNames.PREVIOUS] = () => OnMobileButtonPressed(InputActionNames.PREVIOUS);
            buttonHandlers[InputActionNames.NEXT] = () => OnMobileButtonPressed(InputActionNames.NEXT);
            buttonHandlers[InputActionNames.BACK] = () => OnMobileButtonPressed(InputActionNames.BACK);
            buttonHandlers[InputActionNames.PAUSE] = () => OnMobileButtonPressed(InputActionNames.PAUSE);
        }

        #region IInputManager Implementation

        public Vector2 GetMove()
        {
            try
            {
                if (mobileJoystick != null && mobileJoystick.Direction != Vector2.zero)
                {
                    return mobileJoystick.Direction;
                }
                
                return newInput?.GetMove(PlayerInput) ?? Vector2.zero;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Player {PlayerId} - Error getting move input: {ex.Message}");
                return Vector2.zero;
            }
        }

        public bool GetLeft() => GetButton("Left");
        public bool GetRight() => GetButton("Right");

        public Vector2 GetScreenPosition()
        {
            try
            {
                return newInput?.GetPosition(PlayerInput) ?? Vector2.zero;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Player {PlayerId} - Error getting screen position: {ex.Message}");
                return Vector2.zero;
            }
        }

        public Vector2 GetWorldPosition() => (Vector2)GetWorldPoint();

        public SwipeDirection GetSwipe()
        {
            try
            {
                if (newInput == null) return SwipeDirection.None;

                var swipeCurrentPos = newInput.GetPosition(PlayerInput);
                var isPressed = newInput.GetPress(PlayerInput);
                var isReleased = newInput.GetRelease(PlayerInput);

                if (isPressed) swipeStartPos = swipeCurrentPos;

                if (!isReleased) return SwipeDirection.None;

                Vector2 swipeDelta = swipeCurrentPos - swipeStartPos;
                if (swipeDelta.magnitude > swipeThreshold)
                {
                    return Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y)
                        ? (swipeDelta.x > 0 ? SwipeDirection.Right : SwipeDirection.Left)
                        : (swipeDelta.y > 0 ? SwipeDirection.Up : SwipeDirection.Down);
                }

                return SwipeDirection.None;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Player {PlayerId} - Error detecting swipe: {ex.Message}");
                return SwipeDirection.None;
            }
        }

        public Collider2D GetCast2D()
        {
            try
            {
                EnsureCamera();
                if (!playerCamera) return null;

                if (IsInputDown())
                {
                    var worldPoint = GetWorldPoint();
                    return Physics2D.Raycast(worldPoint, Vector2.zero).collider;
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Player {PlayerId} - Error performing 2D raycast: {ex.Message}");
                return null;
            }
        }

        public Collider GetCast3D()
        {
            try
            {
                EnsureCamera();
                if (!playerCamera) return null;

                if (IsInputDown())
                {
                    var worldPoint = GetWorldPoint();
                    return Physics.Raycast(worldPoint, Vector3.forward, out RaycastHit hit) ? hit.collider : null;
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Player {PlayerId} - Error performing 3D raycast: {ex.Message}");
                return null;
            }
        }

        public RectTransform GetCastRect()
        {
            try
            {
                EnsureCamera();
                if (!playerCamera) return null;

                if (!IsInputDown()) return null;
                var eventSystem = EventSystem.current;
                if (eventSystem == null) return null;

                PointerEventData pointerData = new PointerEventData(eventSystem)
                {
                    position = GetScreenPosition()
                };

                if (graphicRaycaster == null) return null;

                List<RaycastResult> results = new List<RaycastResult>();
                graphicRaycaster.Raycast(pointerData, results);

                return results.Count > 0 ? results[0].gameObject.GetComponent<RectTransform>() : null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Player {PlayerId} - Error performing UI raycast: {ex.Message}");
                return null;
            }
        }

        // Action inputs
        public bool GetFire() => GetInputActionHeld(InputActionNames.FIRE);
        public bool GetJump() => GetInputAction(InputActionNames.JUMP);
        public bool GetCrouch() => GetInputAction(InputActionNames.CROUCH);
        public bool GetSprint() => GetInputAction(InputActionNames.SPRINT);
        public bool GetReload() => GetInputAction(InputActionNames.RELOAD);
        public bool GetInventory() => GetInputAction(InputActionNames.INVENTORY);
        public bool GetInteract() => GetInputAction(InputActionNames.INTERACT);
        public bool GetMap() => GetInputAction(InputActionNames.MAP);
        public bool GetPrevious() => GetInputAction(InputActionNames.PREVIOUS);
        public bool GetNext() => GetInputAction(InputActionNames.NEXT);
        public bool GetBack() => GetInputAction(InputActionNames.BACK);
        public bool GetPause() => GetInputAction(InputActionNames.PAUSE);

        // Trigger-once input methods (consume input flag, return true only once per press)
        public bool GetFirePressed() => GetInputActionPressed(InputActionNames.FIRE);
        public bool GetJumpPressed() => GetInputActionPressed(InputActionNames.JUMP);
        public bool GetCrouchPressed() => GetInputActionPressed(InputActionNames.CROUCH);
        public bool GetSprintPressed() => GetInputActionPressed(InputActionNames.SPRINT);
        public bool GetReloadPressed() => GetInputActionPressed(InputActionNames.RELOAD);
        public bool GetInventoryPressed() => GetInputActionPressed(InputActionNames.INVENTORY);
        public bool GetInteractPressed() => GetInputActionPressed(InputActionNames.INTERACT);
        public bool GetMapPressed() => GetInputActionPressed(InputActionNames.MAP);
        public bool GetPreviousPressed() => GetInputActionPressed(InputActionNames.PREVIOUS);
        public bool GetNextPressed() => GetInputActionPressed(InputActionNames.NEXT);
        public bool GetBackPressed() => GetInputActionPressed(InputActionNames.BACK);
        public bool GetPausePressed() => GetInputActionPressed(InputActionNames.PAUSE);

        // Held input methods (return true while input is held down)
        public bool GetFireHeld() => GetInputActionHeld(InputActionNames.FIRE);
        public bool GetJumpHeld() => GetInputActionHeld(InputActionNames.JUMP);
        public bool GetCrouchHeld() => GetInputActionHeld(InputActionNames.CROUCH);
        public bool GetSprintHeld() => GetInputActionHeld(InputActionNames.SPRINT);
        public bool GetReloadHeld() => GetInputActionHeld(InputActionNames.RELOAD);
        public bool GetInventoryHeld() => GetInputActionHeld(InputActionNames.INVENTORY);
        public bool GetInteractHeld() => GetInputActionHeld(InputActionNames.INTERACT);
        public bool GetMapHeld() => GetInputActionHeld(InputActionNames.MAP);
        public bool GetPreviousHeld() => GetInputActionHeld(InputActionNames.PREVIOUS);
        public bool GetNextHeld() => GetInputActionHeld(InputActionNames.NEXT);
        public bool GetBackHeld() => GetInputActionHeld(InputActionNames.BACK);
        public bool GetPauseHeld() => GetInputActionHeld(InputActionNames.PAUSE);

        public T GetKey<T>(string actionName) where T : struct
        {
            try
            {
                if (string.IsNullOrEmpty(actionName))
                {
                    Debug.LogWarning($"Player {PlayerId} - Action name is null or empty");
                    return default;
                }

                return PlayerInput?.actions?.FindAction(actionName) != null ? 
                    newInput.GetActionValue<T>(PlayerInput, actionName) : default;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Player {PlayerId} - Error getting key '{actionName}': {ex.Message}");
                return default;
            }
        }

        // Configuration methods (return InputManager for compatibility)
        InputManager IInputManager.SetMainCamera(Camera newMainCamera)
        {
            SetCamera(newMainCamera);
            return null; // Not applicable for individual players
        }

        InputManager IInputManager.SetGraphicRaycaster(GraphicRaycaster newGraphicRaycaster)
        {
            SetGraphicRaycaster(newGraphicRaycaster);
            return null; // Not applicable for individual players
        }

        InputManager IInputManager.SetJoystick(Joystick newJoystick)
        {
            SetJoystick(newJoystick);
            return null; // Not applicable for individual players
        }

        InputManager IInputManager.SetActionMapping(InputActionMapping mapping)
        {
            SetActionMapping(mapping);
            return null; // Not applicable for individual players
        }

        InputManager IInputManager.SetMobileButton(Button fireBtn, Button jumpBtn, Button crouchBtn, 
            Button sprintBtn, Button reloadBtn, Button inventoryBtn, 
            Button interactBtn, Button mapBtn, Button previousBtn, 
            Button nextBtn, Button backBtn, Button pauseBtn)
        {
            SetMobileButtons(fireBtn, jumpBtn, crouchBtn, sprintBtn, reloadBtn, 
                inventoryBtn, interactBtn, mapBtn, previousBtn, nextBtn, backBtn, pauseBtn);
            return null; // Not applicable for individual players
        }

        #endregion

        #region Player-Specific Configuration

        public void SetCamera(Camera camera)
        {
            playerCamera = camera;
        }

        public void SetGraphicRaycaster(GraphicRaycaster raycaster)
        {
            graphicRaycaster = raycaster;
        }

        public void SetJoystick(Joystick joystick)
        {
            mobileJoystick = joystick;
        }

        public void SetActionMapping(InputActionMapping mapping)
        {
            actionMapping = mapping;
            
            // Re-initialize input system if it's already been initialized
            if (newInput != null)
            {
                newInput?.Dispose();
                newInput = new NewInput(PlayerInput, actionMapping);
            }
        }

        public bool SetupMobileControls(MultiPlayerInputManager.MobileControlsConfig config)
        {
            try
            {
                SetJoystick(config.joystick);
                SetGraphicRaycaster(config.uiContainer?.GetComponentInParent<GraphicRaycaster>());
                
                SetMobileButtons(
                    config.fireButton, config.jumpButton, config.crouchButton,
                    config.sprintButton, config.reloadButton, config.inventoryButton,
                    config.interactButton, config.mapButton, config.previousButton,
                    config.nextButton, config.backButton, config.pauseButton
                );
                
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Player {PlayerId} - Error setting up mobile controls: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Private Helper Methods

        private bool GetInputAction(string actionName)
        {
            try
            {
                // Check buffered input first
                if (enableInputBuffering && inputBuffer != null && inputBuffer.ConsumeBufferedInput(actionName))
                {
                    return true;
                }

                // Check input flags from NewInput
                return newInput?.GetInputFlag(actionName) ?? false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Player {PlayerId} - Error getting input action '{actionName}': {ex.Message}");
                return false;
            }
        }

        private bool GetInputActionPressed(string actionName)
        {
            try
            {
                // Check buffered input first
                if (enableInputBuffering && inputBuffer != null && inputBuffer.ConsumeBufferedInput(actionName))
                {
                    return true;
                }

                // Check input flags from NewInput (consumes the flag)
                return newInput?.GetInputFlag(actionName) ?? false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Player {PlayerId} - Error getting input action pressed '{actionName}': {ex.Message}");
                return false;
            }
        }

        private bool GetInputActionHeld(string actionName)
        {
            try
            {
                // Check if input is currently being held down
                return newInput?.GetInputHeld(PlayerInput, actionName) ?? false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Player {PlayerId} - Error getting input action held '{actionName}': {ex.Message}");
                return false;
            }
        }

        private bool GetButton(string buttonType)
        {
            try
            {
                return newInput?.GetButton(PlayerInput, buttonType) ?? false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Player {PlayerId} - Error getting button '{buttonType}': {ex.Message}");
                return false;
            }
        }

        private bool IsInputDown()
        {
            try
            {
                return newInput?.GetPress(PlayerInput) ?? false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Player {PlayerId} - Error checking input down: {ex.Message}");
                return false;
            }
        }

        private Vector3 GetWorldPoint()
        {
            try
            {
                EnsureCamera();
                if (!playerCamera || newInput == null) return Vector3.zero;

                var screenPos = newInput.GetPosition(PlayerInput);
                return playerCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, playerCamera.nearClipPlane));
            }
            catch (Exception ex)
            {
                Debug.LogError($"Player {PlayerId} - Error getting world point: {ex.Message}");
                return Vector3.zero;
            }
        }

        private void EnsureCamera()
        {
            if (playerCamera == null)
            {
                playerCamera = Camera.main;
            }
        }

        private void OnMobileButtonPressed(string actionName)
        {
            try
            {
                newInput?.SetInputFlag(actionName, true);
                
                if (enableInputBuffering && inputBuffer != null)
                {
                    inputBuffer.BufferInput(actionName);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Player {PlayerId} - Error handling mobile button press for '{actionName}': {ex.Message}");
            }
        }

        private void SetMobileButtons(Button fireBtn, Button jumpBtn, Button crouchBtn, 
            Button sprintBtn, Button reloadBtn, Button inventoryBtn, 
            Button interactBtn, Button mapBtn, Button previousBtn, 
            Button nextBtn, Button backBtn, Button pauseBtn)
        {
            try
            {
                // Unregister old buttons
                UnregisterMobileButtons();
                
                // Register new buttons
                RegisterMobileButton(InputActionNames.FIRE, fireBtn);
                RegisterMobileButton(InputActionNames.JUMP, jumpBtn);
                RegisterMobileButton(InputActionNames.CROUCH, crouchBtn);
                RegisterMobileButton(InputActionNames.SPRINT, sprintBtn);
                RegisterMobileButton(InputActionNames.RELOAD, reloadBtn);
                RegisterMobileButton(InputActionNames.INVENTORY, inventoryBtn);
                RegisterMobileButton(InputActionNames.INTERACT, interactBtn);
                RegisterMobileButton(InputActionNames.MAP, mapBtn);
                RegisterMobileButton(InputActionNames.PREVIOUS, previousBtn);
                RegisterMobileButton(InputActionNames.NEXT, nextBtn);
                RegisterMobileButton(InputActionNames.BACK, backBtn);
                RegisterMobileButton(InputActionNames.PAUSE, pauseBtn);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Player {PlayerId} - Error setting mobile buttons: {ex.Message}");
            }
        }

        private void RegisterMobileButton(string actionName, Button button)
        {
            if (button != null && buttonHandlers.TryGetValue(actionName, out var handler))
            {
                button.onClick.AddListener(handler);
                mobileButtons[actionName] = button;
            }
        }

        private void UnregisterMobileButtons()
        {
            foreach (var kvp in mobileButtons)
            {
                if (kvp.Value != null && buttonHandlers.TryGetValue(kvp.Key, out var handler))
                {
                    kvp.Value.onClick.RemoveListener(handler);
                }
            }
            mobileButtons.Clear();
        }

        #endregion

        public void Dispose()
        {
            if (disposed) return;

            try
            {
                UnregisterMobileButtons();
                newInput?.Dispose();
                inputBuffer?.ClearAllBuffers();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Player {PlayerId} - Error during disposal: {ex.Message}");
            }
            finally
            {
                disposed = true;
            }
        }
    }
}