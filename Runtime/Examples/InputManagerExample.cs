using UnityEngine;
using UnityEngine.UI;

namespace Ludo.CrossInput.Examples
{
    /// <summary>
    /// Example script demonstrating how to use the improved InputManager system.
    /// This script shows best practices for input handling and error management.
    /// </summary>
    public class InputManagerExample : MonoBehaviour
    {
        [Header("Input Manager")]
        [SerializeField] private InputManager inputManager;
        
        [Header("UI References")]
        [SerializeField] private Text debugText;
        [SerializeField] private Text performanceText;
        
        [Header("Performance Monitoring")]
        [SerializeField] private InputPerformanceMonitor performanceMonitor;
        
        [Header("Device Management")]
        [SerializeField] private InputDeviceManager deviceManager;

        // Player movement
        private Vector2 currentMovement;
        private bool isJumping;
        private bool isFiring;

        private void Start()
        {
            InitializeInputManager();
            SetupPerformanceMonitoring();
            SetupDeviceManagement();
        }

        private void Update()
        {
            HandleInput();
            UpdateDebugDisplay();
        }

        private void InitializeInputManager()
        {
            if (inputManager == null)
            {
                inputManager = FindFirstObjectByType<InputManager>();
                if (inputManager == null)
                {
                    Debug.LogError("InputManager not found in scene!");
                    return;
                }
            }

            // Example of fluent configuration
            inputManager
                .SetMainCamera(Camera.main)
                .SetGraphicRaycaster(FindFirstObjectByType<GraphicRaycaster>());

            Debug.Log("InputManager initialized successfully");
        }

        private void SetupPerformanceMonitoring()
        {
            if (performanceMonitor != null)
            {
                performanceMonitor.OnPerformanceWarning += OnPerformanceWarning;
                performanceMonitor.OnPerformanceSampleRecorded += OnPerformanceSampleRecorded;
            }
        }

        private void SetupDeviceManagement()
        {
            if (deviceManager != null)
            {
                deviceManager.OnDeviceAdded += OnDeviceAdded;
                deviceManager.OnDeviceRemoved += OnDeviceRemoved;
            }
        }

        private void HandleInput()
        {
            if (inputManager == null) return;

            // Start performance timing
            performanceMonitor?.StartInputTiming();

            try
            {
                // Get movement input (works with keyboard, gamepad, or mobile joystick)
                currentMovement = inputManager.GetMove();

                // Handle action inputs with proper error handling
                HandleActionInputs();

                // Handle positional inputs
                HandlePositionalInputs();

                // Apply movement to player (example)
                ApplyMovement();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error handling input: {ex.Message}");
            }
            finally
            {
                // End performance timing
                performanceMonitor?.EndInputTiming(GetInputEventCount());
            }
        }

        private void HandleActionInputs()
        {
            // Jump input with buffering support
            if (inputManager.GetJump())
            {
                isJumping = true;
                Debug.Log("Jump input detected");
            }

            // Fire input
            if (inputManager.GetFire())
            {
                isFiring = true;
                Debug.Log("Fire input detected");
            }

            // Other action inputs
            if (inputManager.GetInteract())
            {
                Debug.Log("Interact input detected");
            }

            if (inputManager.GetInventory())
            {
                Debug.Log("Inventory input detected");
            }

            if (inputManager.GetPause())
            {
                Debug.Log("Pause input detected");
                // Handle pause logic
            }
        }

        private void HandlePositionalInputs()
        {
            // Get screen position for UI interactions
            Vector2 screenPos = inputManager.GetScreenPosition();
            
            // Get world position for world interactions
            Vector2 worldPos = inputManager.GetWorldPosition();

            // Handle swipe gestures
            SwipeDirection swipe = inputManager.GetSwipe();
            if (swipe != SwipeDirection.None)
            {
                Debug.Log($"Swipe detected: {swipe}");
                HandleSwipe(swipe);
            }

            // Handle raycasting
            HandleRaycasting();
        }

        private void HandleRaycasting()
        {
            // 2D Physics raycast
            var collider2D = inputManager.GetCast2D();
            if (collider2D != null)
            {
                Debug.Log($"2D Raycast hit: {collider2D.name}");
            }

            // 3D Physics raycast
            var collider3D = inputManager.GetCast3D();
            if (collider3D != null)
            {
                Debug.Log($"3D Raycast hit: {collider3D.name}");
            }

            // UI raycast
            var rectTransform = inputManager.GetCastRect();
            if (rectTransform != null)
            {
                Debug.Log($"UI Raycast hit: {rectTransform.name}");
            }
        }

        private void HandleSwipe(SwipeDirection direction)
        {
            switch (direction)
            {
                case SwipeDirection.Up:
                    Debug.Log("Swipe up - could trigger jump or menu navigation");
                    break;
                case SwipeDirection.Down:
                    Debug.Log("Swipe down - could trigger crouch or menu navigation");
                    break;
                case SwipeDirection.Left:
                    Debug.Log("Swipe left - could trigger previous item or menu navigation");
                    break;
                case SwipeDirection.Right:
                    Debug.Log("Swipe right - could trigger next item or menu navigation");
                    break;
            }
        }

        private void ApplyMovement()
        {
            if (currentMovement != Vector2.zero)
            {
                // Apply movement to player transform
                transform.Translate(currentMovement * Time.deltaTime * 5f);
            }

            // Reset one-frame inputs
            isJumping = false;
            isFiring = false;
        }

        private int GetInputEventCount()
        {
            // Count how many input events were processed this frame
            int eventCount = 0;
            
            if (currentMovement != Vector2.zero) eventCount++;
            if (isJumping) eventCount++;
            if (isFiring) eventCount++;
            
            return Mathf.Max(1, eventCount);
        }

        private void UpdateDebugDisplay()
        {
            if (debugText != null)
            {
                debugText.text = $"Movement: {currentMovement}\n" +
                               $"Jumping: {isJumping}\n" +
                               $"Firing: {isFiring}\n" +
                               $"Screen Pos: {inputManager?.GetScreenPosition()}\n" +
                               $"World Pos: {inputManager?.GetWorldPosition()}";
            }

            if (performanceText != null && performanceMonitor != null)
            {
                performanceText.text = performanceMonitor.GetPerformanceReport();
            }
        }

        #region Event Handlers

        private void OnPerformanceWarning(string warning)
        {
            Debug.LogWarning($"Performance Warning: {warning}");
        }

        private void OnPerformanceSampleRecorded(InputPerformanceMonitor.PerformanceSample sample)
        {
            // Could log to analytics or display in UI
            if (sample.inputProcessingTime > 10f) // More than 10ms
            {
                Debug.Log($"High input processing time: {sample.inputProcessingTime:F2}ms");
            }
        }

        private void OnDeviceAdded(UnityEngine.InputSystem.InputDevice device)
        {
            Debug.Log($"Input device connected: {device.displayName}");
            // Could update UI to show new input options
        }

        private void OnDeviceRemoved(UnityEngine.InputSystem.InputDevice device)
        {
            Debug.Log($"Input device disconnected: {device.displayName}");
            // Could update UI to hide unavailable input options
        }

        #endregion

        #region Public Methods for UI

        /// <summary>
        /// Example method that can be called from UI buttons
        /// </summary>
        public void OnUIButtonPressed(string actionName)
        {
            Debug.Log($"UI button pressed: {actionName}");
            
            // You could manually trigger input actions here
            // This is useful for accessibility or alternative input methods
        }

        /// <summary>
        /// Toggle performance monitoring from UI
        /// </summary>
        public void TogglePerformanceMonitoring()
        {
            if (performanceMonitor != null)
            {
                performanceMonitor.SetMonitoringEnabled(!performanceMonitor.IsMonitoring);
            }
        }

        #endregion

        private void OnDestroy()
        {
            // Clean up event subscriptions
            if (performanceMonitor != null)
            {
                performanceMonitor.OnPerformanceWarning -= OnPerformanceWarning;
                performanceMonitor.OnPerformanceSampleRecorded -= OnPerformanceSampleRecorded;
            }

            if (deviceManager != null)
            {
                deviceManager.OnDeviceAdded -= OnDeviceAdded;
                deviceManager.OnDeviceRemoved -= OnDeviceRemoved;
            }
        }
    }
}
