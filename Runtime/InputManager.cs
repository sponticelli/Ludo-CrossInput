using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Ludo.CrossInput
{
    /// <summary>
    /// Main input manager that provides a unified interface for handling input across different platforms and input methods.
    /// Implements proper event handling, memory management, and error handling.
    /// </summary>
    public class InputManager : MonoBehaviour, IInputManager, IDisposable
    {
        [Header("Core Components")]
        [Tooltip("The PlayerInput component for Unity's new Input System")]
        [SerializeField] private PlayerInput playerInput;

        [Tooltip("The main camera used for input-related calculations")]
        [SerializeField] private Camera mainCamera;

        [Tooltip("The GraphicRaycaster used for UI raycasting")]
        [SerializeField] private GraphicRaycaster graphicRaycaster;

        [Tooltip("The mobile joystick used for movement input")]
        [SerializeField] private Joystick mobileJoystick;

        [Header("Input Settings")]
        [Tooltip("Optional mapping to use existing Input Actions with different names")]
        [SerializeField] private InputActionMapping actionMapping;

        [Tooltip("Enable input buffering for more forgiving input timing")]
        [SerializeField] private bool enableInputBuffering = true;

        [Tooltip("Input buffer time in seconds")]
        [SerializeField] private float inputBufferTime = 0.1f;

        [Tooltip("Swipe threshold in pixels")]
        [SerializeField] private float swipeThreshold = 50f;

        [Header("Mobile Buttons")]
        [Tooltip("Button for firing actions")]
        [SerializeField] private Button fireButton;

        [Tooltip("Button for jumping actions")]
        [SerializeField] private Button jumpButton;

        [Tooltip("Button for crouching actions")]
        [SerializeField] private Button crouchButton;

        [Tooltip("Button for sprinting actions")]
        [SerializeField] private Button sprintButton;

        [Tooltip("Button for reloading actions")]
        [SerializeField] private Button reloadButton;

        [Tooltip("Button for opening the inventory")]
        [SerializeField] private Button inventoryButton;

        [Tooltip("Button for opening the map")]
        [SerializeField] private Button mapButton;

        [Tooltip("Button for interacting with objects")]
        [SerializeField] private Button interactButton;

        [Tooltip("Button for selecting the previous item")]
        [SerializeField] private Button previousButton;

        [Tooltip("Button for selecting the next item")]
        [SerializeField] private Button nextButton;

        [Tooltip("Button for going back")]
        [SerializeField] private Button backButton;

        [Tooltip("Button for pausing the game")]
        [SerializeField] private Button pauseButton;

        // Private fields
        private enum ButtonType { Non, Left, Right }

        private Vector2 swipeStartPos;
        private NewInput newInput;
        private InputBuffer inputBuffer;
        private bool disposed = false;

        // Mobile button event handlers - stored as references for proper cleanup
        private UnityEngine.Events.UnityAction fireButtonHandler;
        private UnityEngine.Events.UnityAction jumpButtonHandler;
        private UnityEngine.Events.UnityAction crouchButtonHandler;
        private UnityEngine.Events.UnityAction sprintButtonHandler;
        private UnityEngine.Events.UnityAction reloadButtonHandler;
        private UnityEngine.Events.UnityAction inventoryButtonHandler;
        private UnityEngine.Events.UnityAction interactButtonHandler;
        private UnityEngine.Events.UnityAction mapButtonHandler;
        private UnityEngine.Events.UnityAction previousButtonHandler;
        private UnityEngine.Events.UnityAction nextButtonHandler;
        private UnityEngine.Events.UnityAction backButtonHandler;
        private UnityEngine.Events.UnityAction pauseButtonHandler;
        #region Unity Lifecycle

        private void Awake()
        {
            InitializeComponents();
            InitializeEventHandlers();
        }

        private void Start()
        {
            try
            {
                InitializeInputSystem();
                RegisterMobileButtonActions();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error initializing InputManager: {ex.Message}");
            }
        }

        private void Update()
        {
            if (enableInputBuffering && inputBuffer != null)
            {
                inputBuffer.Update();
            }
        }

        private void OnDestroy()
        {
            Dispose();
        }

        #endregion

        #region Initialization

        private void InitializeComponents()
        {
            // Auto-find components if not assigned
            playerInput ??= GetComponent<PlayerInput>();
            mainCamera ??= Camera.main;
            graphicRaycaster ??= FindFirstObjectByType<GraphicRaycaster>();
            mobileJoystick ??= FindFirstObjectByType<Joystick>();
        }

        private void InitializeEventHandlers()
        {
            // Create event handler references for proper cleanup
            fireButtonHandler = () => OnMobileButtonPressed(InputActionNames.FIRE);
            jumpButtonHandler = () => OnMobileButtonPressed(InputActionNames.JUMP);
            crouchButtonHandler = () => OnMobileButtonPressed(InputActionNames.CROUCH);
            sprintButtonHandler = () => OnMobileButtonPressed(InputActionNames.SPRINT);
            reloadButtonHandler = () => OnMobileButtonPressed(InputActionNames.RELOAD);
            inventoryButtonHandler = () => OnMobileButtonPressed(InputActionNames.INVENTORY);
            interactButtonHandler = () => OnMobileButtonPressed(InputActionNames.INTERACT);
            mapButtonHandler = () => OnMobileButtonPressed(InputActionNames.MAP);
            previousButtonHandler = () => OnMobileButtonPressed(InputActionNames.PREVIOUS);
            nextButtonHandler = () => OnMobileButtonPressed(InputActionNames.NEXT);
            backButtonHandler = () => OnMobileButtonPressed(InputActionNames.BACK);
            pauseButtonHandler = () => OnMobileButtonPressed(InputActionNames.PAUSE);
        }

        private void InitializeInputSystem()
        {
            if (playerInput == null)
            {
                Debug.LogError("PlayerInput component is required for InputManager");
                return;
            }

            // Validate action mapping if provided
            if (actionMapping != null)
            {
                var validationResult = actionMapping.ValidateMapping(playerInput.actions);
                if (!validationResult.IsValid)
                {
                    Debug.LogWarning($"Input Action Mapping validation issues:\n{validationResult.GetReport()}");
                }
                else
                {
                    Debug.Log("Input Action Mapping validated successfully");
                }
            }

            newInput = new NewInput(playerInput, actionMapping);

            if (enableInputBuffering)
            {
                inputBuffer = new InputBuffer(inputBufferTime);
            }
        }

        #endregion

        #region Configuration Methods (Fluent Interface)

        public InputManager SetMainCamera(Camera newMainCamera)
        {
            mainCamera = newMainCamera;
            return this;
        }

        public InputManager SetGraphicRaycaster(GraphicRaycaster newGraphicRaycaster)
        {
            graphicRaycaster = newGraphicRaycaster;
            return this;
        }

        public InputManager SetJoystick(Joystick newJoystick)
        {
            mobileJoystick = newJoystick;
            return this;
        }

        public InputManager SetActionMapping(InputActionMapping mapping)
        {
            actionMapping = mapping;

            // Re-initialize input system if it's already been initialized
            if (newInput != null)
            {
                newInput?.Dispose();
                InitializeInputSystem();
            }

            return this;
        }

        public InputManager SetMobileButton(Button fireBtn, Button jumpBtn, Button crouchBtn,
            Button sprintBtn, Button reloadBtn, Button inventoryBtn,
            Button interactBtn, Button mapBtn, Button previousBtn,
            Button nextBtn, Button backBtn, Button pauseBtn)
        {
            try
            {
                UnregisterMobileButtonActions();

                fireButton = fireBtn;
                jumpButton = jumpBtn;
                crouchButton = crouchBtn;
                sprintButton = sprintBtn;
                reloadButton = reloadBtn;
                inventoryButton = inventoryBtn;
                interactButton = interactBtn;
                mapButton = mapBtn;
                previousButton = previousBtn;
                nextButton = nextBtn;
                backButton = backBtn;
                pauseButton = pauseBtn;

                RegisterMobileButtonActions();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error setting mobile buttons: {ex.Message}");
            }

            return this;
        }

        #endregion

        #region Mobile Button Management

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
                Debug.LogError($"Error handling mobile button press for '{actionName}': {ex.Message}");
            }
        }

        private void RegisterMobileButtonActions()
        {
            try
            {
                fireButton?.onClick.AddListener(fireButtonHandler);
                jumpButton?.onClick.AddListener(jumpButtonHandler);
                crouchButton?.onClick.AddListener(crouchButtonHandler);
                sprintButton?.onClick.AddListener(sprintButtonHandler);
                reloadButton?.onClick.AddListener(reloadButtonHandler);
                inventoryButton?.onClick.AddListener(inventoryButtonHandler);
                interactButton?.onClick.AddListener(interactButtonHandler);
                mapButton?.onClick.AddListener(mapButtonHandler);
                previousButton?.onClick.AddListener(previousButtonHandler);
                nextButton?.onClick.AddListener(nextButtonHandler);
                backButton?.onClick.AddListener(backButtonHandler);
                pauseButton?.onClick.AddListener(pauseButtonHandler);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error registering mobile button actions: {ex.Message}");
            }
        }

        private void UnregisterMobileButtonActions()
        {
            try
            {
                fireButton?.onClick.RemoveListener(fireButtonHandler);
                jumpButton?.onClick.RemoveListener(jumpButtonHandler);
                crouchButton?.onClick.RemoveListener(crouchButtonHandler);
                sprintButton?.onClick.RemoveListener(sprintButtonHandler);
                reloadButton?.onClick.RemoveListener(reloadButtonHandler);
                inventoryButton?.onClick.RemoveListener(inventoryButtonHandler);
                interactButton?.onClick.RemoveListener(interactButtonHandler);
                mapButton?.onClick.RemoveListener(mapButtonHandler);
                previousButton?.onClick.RemoveListener(previousButtonHandler);
                nextButton?.onClick.RemoveListener(nextButtonHandler);
                backButton?.onClick.RemoveListener(backButtonHandler);
                pauseButton?.onClick.RemoveListener(pauseButtonHandler);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error unregistering mobile button actions: {ex.Message}");
            }
        }

        #endregion

        #region IInputManager Implementation

        /// <summary>
        /// Gets movement input from the appropriate method (new input system or mobile joystick).
        /// </summary>
        public Vector2 GetMove()
        {
            try
            {
                if (mobileJoystick != null && mobileJoystick.Direction != Vector2.zero)
                {
                    return mobileJoystick.Direction;
                }

                return newInput?.GetMove(playerInput) ?? Vector2.zero;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error getting move input: {ex.Message}");
                return Vector2.zero;
            }
        }

        /// <summary>Returns true if left button is pressed.</summary>
        public bool GetLeft() => GetButton(ButtonType.Left);

        /// <summary>Returns true if right button is pressed.</summary>
        public bool GetRight() => GetButton(ButtonType.Right);

        /// <summary>Returns the current screen touch or pointer position.</summary>
        public Vector2 GetScreenPosition()
        {
            try
            {
                return newInput?.GetPosition(playerInput) ?? Vector2.zero;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error getting screen position: {ex.Message}");
                return Vector2.zero;
            }
        }

        /// <summary>Converts the screen position to a world position using the main camera.</summary>
        public Vector2 GetWorldPosition() => (Vector2)GetWorldPoint();

        /// <summary>Detects swipe direction based on input movement.</summary>
        public SwipeDirection GetSwipe()
        {
            try
            {
                if (newInput == null) return SwipeDirection.None;

                var swipeCurrentPos = newInput.GetPosition(playerInput);
                var isPressed = newInput.GetPress(playerInput);
                var isReleased = newInput.GetRelease(playerInput);

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
                Debug.LogError($"Error detecting swipe: {ex.Message}");
                return SwipeDirection.None;
            }
        }

        /// <summary>Performs a Physics2D raycast and returns the hit Collider2D, if any.</summary>
        public Collider2D GetCast2D()
        {
            try
            {
                EnsureCamera();
                if (!mainCamera) return null;

                if (IsInputDown())
                {
                    var worldPoint = GetWorldPoint();
                    return Physics2D.Raycast(worldPoint, Vector2.zero).collider;
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error performing 2D raycast: {ex.Message}");
                return null;
            }
        }

        /// <summary>Performs a Physics raycast and returns the hit 3D Collider, if any.</summary>
        public Collider GetCast3D()
        {
            try
            {
                EnsureCamera();
                if (!mainCamera) return null;

                if (IsInputDown())
                {
                    var worldPoint = GetWorldPoint();
                    return Physics.Raycast(worldPoint, Vector3.forward, out RaycastHit hit) ? hit.collider : null;
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error performing 3D raycast: {ex.Message}");
                return null;
            }
        }

        /// <summary>Performs a UI raycast and returns the first hit RectTransform, if any.</summary>
        public RectTransform GetCastRect()
        {
            try
            {
                EnsureCamera();
                if (!mainCamera) return null;

                if (!IsInputDown()) return null;
                var eventSystem = EventSystem.current;
                if (eventSystem == null) return null;

                PointerEventData pointerData = new PointerEventData(eventSystem)
                {
                    position = GetScreenPosition()
                };

                graphicRaycaster ??= FindFirstObjectByType<GraphicRaycaster>();
                if (graphicRaycaster == null) return null;

                List<RaycastResult> results = new List<RaycastResult>();
                graphicRaycaster.Raycast(pointerData, results);

                return results.Count > 0 ? results[0].gameObject.GetComponent<RectTransform>() : null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error performing UI raycast: {ex.Message}");
                return null;
            }
        }

        /// <summary>Returns true if the fire input is pressed or held.</summary>
        public bool GetFire() => GetInputAction(InputActionNames.FIRE);

        /// <summary>Returns true if the jump input is pressed or held.</summary>
        public bool GetJump() => GetInputAction(InputActionNames.JUMP);

        /// <summary>Returns true if the crouch input is pressed or held.</summary>
        public bool GetCrouch() => GetInputAction(InputActionNames.CROUCH);

        /// <summary>Returns true if the sprint input is pressed or held.</summary>
        public bool GetSprint() => GetInputAction(InputActionNames.SPRINT);

        /// <summary>Returns true if the reload input is pressed or held.</summary>
        public bool GetReload() => GetInputAction(InputActionNames.RELOAD);

        /// <summary>Returns true if the inventory input is pressed or held.</summary>
        public bool GetInventory() => GetInputAction(InputActionNames.INVENTORY);

        /// <summary>Returns true if the interact input is pressed or held.</summary>
        public bool GetInteract() => GetInputAction(InputActionNames.INTERACT);

        /// <summary>Returns true if the map input is pressed or held.</summary>
        public bool GetMap() => GetInputAction(InputActionNames.MAP);

        /// <summary>Returns true if the previous input is pressed or held.</summary>
        public bool GetPrevious() => GetInputAction(InputActionNames.PREVIOUS);

        /// <summary>Returns true if the next input is pressed or held.</summary>
        public bool GetNext() => GetInputAction(InputActionNames.NEXT);

        /// <summary>Returns true if the back input is pressed or held.</summary>
        public bool GetBack() => GetInputAction(InputActionNames.BACK);

        /// <summary>Returns true if the pause input is pressed or held.</summary>
        public bool GetPause() => GetInputAction(InputActionNames.PAUSE);
        /// <summary>
        /// Gets the current value of an action (button, float, or vector) using the new Input System.
        /// </summary>
        /// <typeparam name="T">The type of input to return (bool, float, Vector2).</typeparam>
        /// <param name="actionName">The action name defined in Input Actions asset.</param>
        public T GetKey<T>(string actionName) where T : struct
        {
            try
            {
                if (string.IsNullOrEmpty(actionName))
                {
                    Debug.LogWarning("Action name is null or empty");
                    return default;
                }

                return playerInput?.actions?.FindAction(actionName) != null ?
                    newInput.GetActionValue<T>(playerInput, actionName) : default;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error getting key '{actionName}': {ex.Message}");
                return default;
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Gets an input action state, checking both input system and buffered input.
        /// </summary>
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
                Debug.LogError($"Error getting input action '{actionName}': {ex.Message}");
                return false;
            }
        }

        private bool IsInputDown()
        {
            try
            {
                return newInput?.GetPress(playerInput) ?? false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error checking input down: {ex.Message}");
                return false;
            }
        }

        private Vector3 GetWorldPoint()
        {
            try
            {
                EnsureCamera();
                if (!mainCamera || newInput == null) return Vector3.zero;

                var screenPos = newInput.GetPosition(playerInput);
                return mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, mainCamera.nearClipPlane));
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error getting world point: {ex.Message}");
                return Vector3.zero;
            }
        }

        private bool GetButton(ButtonType type)
        {
            try
            {
                return newInput?.GetButton(playerInput, type.ToString()) ?? false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error getting button '{type}': {ex.Message}");
                return false;
            }
        }

        private void EnsureCamera() => mainCamera ??= Camera.main;

        #endregion

        #region IDisposable Implementation

        public void Dispose()
        {
            if (disposed) return;

            try
            {
                UnregisterMobileButtonActions();
                newInput?.Dispose();
                inputBuffer?.ClearAllBuffers();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error during InputManager disposal: {ex.Message}");
            }
            finally
            {
                disposed = true;
            }
        }

        #endregion
    }
}