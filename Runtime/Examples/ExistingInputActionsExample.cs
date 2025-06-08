using UnityEngine;
using Ludo.CrossInput;

namespace Ludo.CrossInput.Examples
{
    /// <summary>
    /// Example showing how to use InputManager with existing Input Actions files
    /// that have different action names (e.g., "Attack" instead of "Fire").
    /// </summary>
    public class ExistingInputActionsExample : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private InputManager inputManager;
        [SerializeField] private InputActionMapping actionMapping;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;

        private void Start()
        {
            SetupInputManagerWithExistingActions();
        }

        private void Update()
        {
            HandleInputWithMapping();
            
            if (showDebugInfo)
            {
                DisplayDebugInfo();
            }
        }

        /// <summary>
        /// Example of setting up InputManager with existing Input Actions
        /// </summary>
        private void SetupInputManagerWithExistingActions()
        {
            if (inputManager == null)
            {
                inputManager = FindFirstObjectByType<InputManager>();
            }

            if (actionMapping == null)
            {
                // Try to find an existing mapping asset
                actionMapping = Resources.Load<InputActionMapping>("DefaultActionMapping");
                
                if (actionMapping == null)
                {
                    Debug.LogWarning("No InputActionMapping found. Create one using the Setup Wizard or manually.");
                    return;
                }
            }

            // Configure InputManager with action mapping
            inputManager.SetActionMapping(actionMapping);
            
            Debug.Log("InputManager configured with existing Input Actions mapping");
        }

        /// <summary>
        /// Example of handling input with action mapping
        /// </summary>
        private void HandleInputWithMapping()
        {
            if (inputManager == null) return;

            // These calls will automatically use the mapped action names
            // For example, if your Input Actions has "Attack" instead of "Fire",
            // the mapping will translate GetFire() to check the "Attack" action
            
            Vector2 movement = inputManager.GetMove(); // Maps to your "Movement" or "WASD" action
            
            if (inputManager.GetFire()) // Maps to your "Attack" or "Shoot" action
            {
                Debug.Log("Fire/Attack action triggered!");
            }
            
            if (inputManager.GetJump()) // Maps to your "Jump" or "Space" action
            {
                Debug.Log("Jump action triggered!");
            }
            
            if (inputManager.GetSprint()) // Maps to your "Run" or "Dash" action
            {
                Debug.Log("Sprint/Run action triggered!");
            }
            
            if (inputManager.GetInteract()) // Maps to your "Use" or "E" action
            {
                Debug.Log("Interact/Use action triggered!");
            }

            // Apply movement
            if (movement != Vector2.zero)
            {
                transform.Translate(movement * Time.deltaTime * 5f);
            }
        }

        /// <summary>
        /// Display debug information about the current mapping
        /// </summary>
        private void DisplayDebugInfo()
        {
            if (actionMapping == null) return;

            // You can check what actual action names are being used
            string fireActionName = actionMapping.GetActualActionName(InputActionNames.FIRE);
            string jumpActionName = actionMapping.GetActualActionName(InputActionNames.JUMP);
            string moveActionName = actionMapping.GetActualActionName(InputActionNames.MOVE);

            Debug.Log($"Using action mappings - Fire: '{fireActionName}', Jump: '{jumpActionName}', Move: '{moveActionName}'");
        }

        /// <summary>
        /// Example of creating a mapping programmatically
        /// </summary>
        [ContextMenu("Create Example Mapping")]
        public void CreateExampleMapping()
        {
            // This shows how you might create a mapping in code
            // (though using the Setup Wizard is recommended)
            
            var mapping = ScriptableObject.CreateInstance<InputActionMapping>();
            
            // Example: Your Input Actions has "Attack" instead of "Fire"
            // The mapping tells InputManager to look for "Attack" when GetFire() is called
            
            Debug.Log("Example mapping created (not saved). Use the Setup Wizard for actual configuration.");
        }
    }

    /// <summary>
    /// Example of a more complex setup with validation
    /// </summary>
    public class AdvancedInputSetupExample : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private InputManager inputManager;
        [SerializeField] private InputActionMapping actionMapping;
        
        [Header("Validation")]
        [SerializeField] private bool validateOnStart = true;
        [SerializeField] private bool showValidationReport = true;

        private void Start()
        {
            if (validateOnStart)
            {
                ValidateInputConfiguration();
            }
            
            SetupInputManager();
        }

        private void ValidateInputConfiguration()
        {
            if (actionMapping == null)
            {
                Debug.LogError("No InputActionMapping assigned!");
                return;
            }

            var playerInput = GetComponent<UnityEngine.InputSystem.PlayerInput>();
            if (playerInput?.actions == null)
            {
                Debug.LogError("No PlayerInput or Input Actions found!");
                return;
            }

            var validationResult = actionMapping.ValidateMapping(playerInput.actions);
            
            if (showValidationReport)
            {
                Debug.Log(validationResult.GetReport());
            }

            if (!validationResult.IsValid)
            {
                Debug.LogWarning("Input configuration has issues. Check the validation report above.");
            }
            else
            {
                Debug.Log("Input configuration validated successfully!");
            }
        }

        private void SetupInputManager()
        {
            if (inputManager == null)
            {
                inputManager = GetComponent<InputManager>();
                if (inputManager == null)
                {
                    Debug.LogError("No InputManager found!");
                    return;
                }
            }

            // Configure with fluent interface
            inputManager
                .SetActionMapping(actionMapping)
                .SetMainCamera(Camera.main);

            Debug.Log("Advanced input setup completed");
        }

        /// <summary>
        /// Example of runtime mapping changes
        /// </summary>
        public void ChangeActionMapping(InputActionMapping newMapping)
        {
            if (inputManager != null && newMapping != null)
            {
                inputManager.SetActionMapping(newMapping);
                Debug.Log("Action mapping changed at runtime");
                
                if (validateOnStart)
                {
                    ValidateInputConfiguration();
                }
            }
        }

        /// <summary>
        /// Example of checking if specific mappings exist
        /// </summary>
        public bool CheckRequiredMappings()
        {
            if (actionMapping == null) return false;

            var requiredActions = new[]
            {
                InputActionNames.MOVE,
                InputActionNames.FIRE,
                InputActionNames.JUMP
            };

            foreach (var action in requiredActions)
            {
                if (!actionMapping.HasMapping(action))
                {
                    Debug.LogWarning($"Missing mapping for required action: {action}");
                    return false;
                }
            }

            return true;
        }
    }
}
