using UnityEngine;
using UnityEngine.UI;
using Ludo.CrossInput;

namespace Ludo.CrossInput.Examples
{
    /// <summary>
    /// Simple example showing basic multi-player setup and usage.
    /// This example demonstrates the corrected API usage.
    /// </summary>
    public class SimpleMultiPlayerExample : MonoBehaviour
    {
        [Header("Multi-Player Manager")]
        [SerializeField] private MultiPlayerInputManager multiPlayerManager;
        
        [Header("UI")]
        [SerializeField] private Text statusText;
        [SerializeField] private Button addPlayerButton;
        [SerializeField] private Button removePlayerButton;
        
        [Header("Player Objects")]
        [SerializeField] private GameObject[] playerCubes; // Visual representation of players
        
        private void Start()
        {
            SetupMultiPlayerManager();
            SetupUI();
            
            // Create first player automatically
            CreatePlayer();
        }

        private void Update()
        {
            HandleAllPlayersInput();
            UpdateStatusDisplay();
        }

        private void SetupMultiPlayerManager()
        {
            if (multiPlayerManager == null)
            {
                multiPlayerManager = FindFirstObjectByType<MultiPlayerInputManager>();
                if (multiPlayerManager == null)
                {
                    Debug.LogError("MultiPlayerInputManager not found! Please add one to the scene.");
                    return;
                }
            }

            // Subscribe to events
            multiPlayerManager.OnPlayerJoined += OnPlayerJoined;
            multiPlayerManager.OnPlayerLeft += OnPlayerLeft;
        }

        private void SetupUI()
        {
            if (addPlayerButton != null)
            {
                addPlayerButton.onClick.AddListener(CreatePlayer);
            }
            
            if (removePlayerButton != null)
            {
                removePlayerButton.onClick.AddListener(RemoveLastPlayer);
            }
        }

        #region Player Management

        public void CreatePlayer()
        {
            int playerId = multiPlayerManager.CreatePlayer();
            if (playerId >= 0)
            {
                Debug.Log($"Player {playerId} created successfully!");
            }
            else
            {
                Debug.LogWarning("Failed to create player (max players reached?)");
            }
        }

        public void RemoveLastPlayer()
        {
            var activePlayerIds = multiPlayerManager.GetActivePlayerIds();
            if (activePlayerIds.Length > 0)
            {
                int lastPlayerId = activePlayerIds[activePlayerIds.Length - 1];
                if (multiPlayerManager.RemovePlayer(lastPlayerId))
                {
                    Debug.Log($"Player {lastPlayerId} removed successfully!");
                }
            }
        }

        private void OnPlayerJoined(int playerId, PlayerInputController controller)
        {
            Debug.Log($"Player {playerId} joined the game!");
            
            // Show player cube if available
            if (playerCubes != null && playerId < playerCubes.Length)
            {
                playerCubes[playerId].SetActive(true);
                
                // Set different colors for different players
                var renderer = playerCubes[playerId].GetComponent<Renderer>();
                if (renderer != null)
                {
                    Color[] colors = { Color.red, Color.blue, Color.green, Color.yellow };
                    renderer.material.color = colors[playerId % colors.Length];
                }
            }
        }

        private void OnPlayerLeft(int playerId, PlayerInputController controller)
        {
            Debug.Log($"Player {playerId} left the game!");
            
            // Hide player cube
            if (playerCubes != null && playerId < playerCubes.Length)
            {
                playerCubes[playerId].SetActive(false);
            }
        }

        #endregion

        #region Input Handling

        private void HandleAllPlayersInput()
        {
            var activePlayerIds = multiPlayerManager.GetActivePlayerIds();
            
            foreach (int playerId in activePlayerIds)
            {
                HandlePlayerInput(playerId);
            }
        }

        private void HandlePlayerInput(int playerId)
        {
            var inputManager = multiPlayerManager.GetPlayerInputManager(playerId);
            if (inputManager == null) return;

            // Get movement input
            Vector2 movement = inputManager.GetMove();
            
            // Move player cube
            if (playerCubes != null && playerId < playerCubes.Length && playerCubes[playerId].activeInHierarchy)
            {
                var cube = playerCubes[playerId];
                if (movement != Vector2.zero)
                {
                    Vector3 move = new Vector3(movement.x, 0, movement.y) * Time.deltaTime * 5f;
                    cube.transform.Translate(move);
                }
            }

            // Handle action inputs
            if (inputManager.GetFire())
            {
                Debug.Log($"Player {playerId} fired!");
                // Could trigger particle effect, sound, etc.
            }

            if (inputManager.GetJump())
            {
                Debug.Log($"Player {playerId} jumped!");
                // Could make cube jump up
                if (playerCubes != null && playerId < playerCubes.Length)
                {
                    var rb = playerCubes[playerId].GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.AddForce(Vector3.up * 5f, ForceMode.Impulse);
                    }
                }
            }

            if (inputManager.GetInteract())
            {
                Debug.Log($"Player {playerId} interacted!");
            }
        }

        #endregion

        #region UI Updates

        private void UpdateStatusDisplay()
        {
            if (statusText == null) return;

            var activePlayerIds = multiPlayerManager.GetActivePlayerIds();
            statusText.text = $"Active Players: {activePlayerIds.Length}/{multiPlayerManager.MaxPlayers}\n\n";

            foreach (int playerId in activePlayerIds)
            {
                var device = multiPlayerManager.GetPlayerDevice(playerId);
                string deviceName = device?.displayName ?? "No Device";
                statusText.text += $"Player {playerId}: {deviceName}\n";
            }

            // Update button states
            if (addPlayerButton != null)
            {
                addPlayerButton.interactable = multiPlayerManager.PlayerCount < multiPlayerManager.MaxPlayers;
            }

            if (removePlayerButton != null)
            {
                removePlayerButton.interactable = multiPlayerManager.PlayerCount > 0;
            }
        }

        #endregion

        #region Public Methods (for UI buttons)

        /// <summary>
        /// Method that can be called from UI buttons to create a player.
        /// </summary>
        public void OnCreatePlayerButtonClicked()
        {
            CreatePlayer();
        }

        /// <summary>
        /// Method that can be called from UI buttons to remove a player.
        /// </summary>
        public void OnRemovePlayerButtonClicked()
        {
            RemoveLastPlayer();
        }

        /// <summary>
        /// Example of assigning a specific device to a player.
        /// </summary>
        public void AssignGamepadToPlayer(int playerId)
        {
            var gamepads = UnityEngine.InputSystem.Gamepad.all;
            if (gamepads.Count > 0)
            {
                var gamepad = gamepads[0]; // Use first available gamepad
                if (multiPlayerManager.AssignDeviceToPlayer(playerId, gamepad))
                {
                    Debug.Log($"Assigned gamepad to Player {playerId}");
                }
            }
            else
            {
                Debug.LogWarning("No gamepads found!");
            }
        }

        #endregion

        private void OnDestroy()
        {
            // Clean up event subscriptions
            if (multiPlayerManager != null)
            {
                multiPlayerManager.OnPlayerJoined -= OnPlayerJoined;
                multiPlayerManager.OnPlayerLeft -= OnPlayerLeft;
            }
        }
    }
}
