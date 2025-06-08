using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Ludo.CrossInput
{
    /// <summary>
    /// Multi-player input manager that supports multiple players on the same device.
    /// Each player can have their own input configuration, controls, and state.
    /// </summary>
    public class MultiPlayerInputManager : MonoBehaviour
    {
        [Header("Multi-Player Configuration")]
        [SerializeField] private int maxPlayers = 4;
        [SerializeField] private bool autoCreatePlayers = true;
        [SerializeField] private PlayerInputManager playerInputManager;
        
        [Header("Player Prefabs")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private InputActionAsset defaultInputActions;
        
        [Header("Split Screen Support")]
        [SerializeField] private bool enableSplitScreen = false;
        [SerializeField] private Camera[] playerCameras;
        
        // Player management
        private readonly Dictionary<int, PlayerInputController> players = new Dictionary<int, PlayerInputController>();
        private readonly Dictionary<PlayerInput, int> playerInputToId = new Dictionary<PlayerInput, int>();
        
        // Events
        public event Action<int, PlayerInputController> OnPlayerJoined;
        public event Action<int, PlayerInputController> OnPlayerLeft;
        public event Action<int, InputDevice> OnPlayerDeviceChanged;

        public IReadOnlyDictionary<int, PlayerInputController> Players => players;
        public int PlayerCount => players.Count;
        public int MaxPlayers => maxPlayers;

        private void Awake()
        {
            InitializePlayerInputManager();
        }

        private void Start()
        {
            if (autoCreatePlayers)
            {
                // Create first player automatically
                CreatePlayer();
            }
        }

        private void InitializePlayerInputManager()
        {
            if (playerInputManager == null)
            {
                playerInputManager = GetComponent<PlayerInputManager>();
                if (playerInputManager == null)
                {
                    playerInputManager = gameObject.AddComponent<PlayerInputManager>();
                }
            }

            // Configure PlayerInputManager
            if (defaultInputActions != null)
            {
                playerInputManager.playerPrefab = playerPrefab;
                // Note: maxPlayerCount is read-only, managed internally by PlayerInputManager

                // Subscribe to player events
                playerInputManager.onPlayerJoined += OnPlayerInputJoined;
                playerInputManager.onPlayerLeft += OnPlayerInputLeft;
            }
        }

        /// <summary>
        /// Creates a new player and returns their ID.
        /// Uses Unity's PlayerInputManager for proper device handling.
        /// </summary>
        public int CreatePlayer(InputDevice device = null)
        {
            if (players.Count >= maxPlayers)
            {
                Debug.LogWarning($"Cannot create player: maximum players ({maxPlayers}) reached");
                return -1;
            }

            try
            {
                // Use Unity's PlayerInputManager to create player
                if (playerInputManager != null && playerPrefab != null)
                {
                    // Let Unity's PlayerInputManager handle player creation
                    var playerInstance = PlayerInput.Instantiate(playerPrefab, playerIndex: -1, controlScheme: null,
                        splitScreenIndex: enableSplitScreen ? players.Count : -1, device);

                    if (playerInstance != null)
                    {
                        // The OnPlayerInputJoined callback will handle the rest
                        return GetPlayerId(playerInstance);
                    }
                }
                else
                {
                    // Manual creation if no PlayerInputManager setup
                    return CreatePlayerManually(device);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error creating player: {ex.Message}");
            }

            return -1;
        }

        /// <summary>
        /// Creates a player manually without using PlayerInputManager.
        /// </summary>
        private int CreatePlayerManually(InputDevice device = null)
        {
            int playerId = GetNextPlayerId();

            try
            {
                // Create PlayerInput for this player
                var playerInputGO = new GameObject($"Player{playerId}_Input");
                playerInputGO.transform.SetParent(transform);

                var playerInput = playerInputGO.AddComponent<PlayerInput>();
                if (defaultInputActions != null)
                {
                    playerInput.actions = Instantiate(defaultInputActions);
                }

                // Assign device if specified
                if (device != null)
                {
                    try
                    {
                        playerInput.SwitchCurrentControlScheme(device);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"Could not assign device {device.displayName} to player {playerId}: {ex.Message}");
                    }
                }

                // Create PlayerInputController
                var controller = new PlayerInputController(playerId, playerInput, this);

                // Setup camera for split screen
                if (enableSplitScreen && playerId < playerCameras.Length)
                {
                    controller.SetCamera(playerCameras[playerId]);
                }

                // Register player
                players[playerId] = controller;
                playerInputToId[playerInput] = playerId;

                OnPlayerJoined?.Invoke(playerId, controller);

                Debug.Log($"Player {playerId} created manually with device: {device?.displayName ?? "None"}");
                return playerId;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error creating player {playerId} manually: {ex.Message}");
                return -1;
            }
        }

        /// <summary>
        /// Removes a player by ID.
        /// </summary>
        public bool RemovePlayer(int playerId)
        {
            if (!players.TryGetValue(playerId, out var controller))
            {
                Debug.LogWarning($"Player {playerId} not found");
                return false;
            }

            try
            {
                // Clean up
                playerInputToId.Remove(controller.PlayerInput);
                controller.Dispose();
                players.Remove(playerId);

                OnPlayerLeft?.Invoke(playerId, controller);
                
                Debug.Log($"Player {playerId} removed");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error removing player {playerId}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets the InputManager for a specific player.
        /// </summary>
        public IInputManager GetPlayerInputManager(int playerId)
        {
            return players.TryGetValue(playerId, out var controller) ? controller : null;
        }

        /// <summary>
        /// Gets the player ID for a given PlayerInput component.
        /// </summary>
        public int GetPlayerId(PlayerInput playerInput)
        {
            return playerInputToId.TryGetValue(playerInput, out int id) ? id : -1;
        }

        /// <summary>
        /// Assigns a device to a specific player.
        /// </summary>
        public bool AssignDeviceToPlayer(int playerId, InputDevice device)
        {
            if (!players.TryGetValue(playerId, out var controller))
            {
                Debug.LogWarning($"Player {playerId} not found");
                return false;
            }

            try
            {
                // Try to switch control scheme with the device
                var playerInput = controller.PlayerInput;
                if (playerInput != null && device != null)
                {
                    // Find a control scheme that supports this device
                    var controlSchemes = playerInput.actions?.controlSchemes;
                    if (controlSchemes != null)
                    {
                        foreach (var scheme in controlSchemes)
                        {
                            if (scheme.SupportsDevice(device))
                            {
                                playerInput.SwitchCurrentControlScheme(scheme.name, device);
                                OnPlayerDeviceChanged?.Invoke(playerId, device);
                                Debug.Log($"Device {device.displayName} assigned to Player {playerId} using scheme '{scheme.name}'");
                                return true;
                            }
                        }
                    }

                    // Fallback: try to switch without specifying scheme
                    playerInput.SwitchCurrentControlScheme(device);
                    OnPlayerDeviceChanged?.Invoke(playerId, device);
                    Debug.Log($"Device {device.displayName} assigned to Player {playerId}");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error assigning device to player {playerId}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Sets up mobile controls for a specific player.
        /// </summary>
        public bool SetupMobileControls(int playerId, MobileControlsConfig config)
        {
            if (!players.TryGetValue(playerId, out var controller))
            {
                Debug.LogWarning($"Player {playerId} not found");
                return false;
            }

            return controller.SetupMobileControls(config);
        }

        private void OnPlayerInputJoined(PlayerInput playerInput)
        {
            // This is called when PlayerInputManager creates a player
            int playerId = GetNextPlayerId();
            var controller = new PlayerInputController(playerId, playerInput, this);
            
            players[playerId] = controller;
            playerInputToId[playerInput] = playerId;
            
            OnPlayerJoined?.Invoke(playerId, controller);
        }

        private void OnPlayerInputLeft(PlayerInput playerInput)
        {
            if (playerInputToId.TryGetValue(playerInput, out int playerId))
            {
                RemovePlayer(playerId);
            }
        }

        private int GetNextPlayerId()
        {
            for (int i = 0; i < maxPlayers; i++)
            {
                if (!players.ContainsKey(i))
                {
                    return i;
                }
            }
            return players.Count; // Fallback
        }

        private void OnDestroy()
        {
            // Clean up all players
            foreach (var controller in players.Values)
            {
                controller?.Dispose();
            }
            players.Clear();
            playerInputToId.Clear();

            // Unsubscribe from events
            if (playerInputManager != null)
            {
                playerInputManager.onPlayerJoined -= OnPlayerInputJoined;
                playerInputManager.onPlayerLeft -= OnPlayerInputLeft;
            }
        }

        /// <summary>
        /// Configuration for mobile controls per player.
        /// </summary>
        [Serializable]
        public class MobileControlsConfig
        {
            public Joystick joystick;
            public Button fireButton;
            public Button jumpButton;
            public Button crouchButton;
            public Button sprintButton;
            public Button reloadButton;
            public Button inventoryButton;
            public Button interactButton;
            public Button mapButton;
            public Button previousButton;
            public Button nextButton;
            public Button backButton;
            public Button pauseButton;
            public RectTransform uiContainer; // Container for this player's UI
        }

        /// <summary>
        /// Gets all active player IDs.
        /// </summary>
        public int[] GetActivePlayerIds()
        {
            var ids = new int[players.Count];
            int index = 0;
            foreach (var playerId in players.Keys)
            {
                ids[index++] = playerId;
            }
            return ids;
        }

        /// <summary>
        /// Checks if a player exists.
        /// </summary>
        public bool HasPlayer(int playerId)
        {
            return players.ContainsKey(playerId);
        }

        /// <summary>
        /// Gets the device assigned to a player.
        /// </summary>
        public InputDevice GetPlayerDevice(int playerId)
        {
            if (players.TryGetValue(playerId, out var controller))
            {
                var playerInput = controller.PlayerInput;
                if (playerInput != null && playerInput.devices.Count > 0)
                {
                    return playerInput.devices[0]; // Return first device
                }
            }
            return null;
        }
    }
}
