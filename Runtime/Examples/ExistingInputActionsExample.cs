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
}
