using UnityEngine;

namespace Ludo.CrossInput.Examples
{
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