using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ludo.CrossInput;

namespace Ludo.CrossInput.Examples
{
    /// <summary>
    /// Example demonstrating multi-player input management on the same device.
    /// Shows how to handle multiple players with different control schemes.
    /// </summary>
    public class MultiPlayerExample : MonoBehaviour
    {
        [Header("Multi-Player Setup")]
        [SerializeField] private MultiPlayerInputManager multiPlayerManager;
        [SerializeField] private int maxPlayers = 4;
        
        [Header("Player Prefabs")]
        [SerializeField] private GameObject playerCharacterPrefab;
        [SerializeField] private Transform[] playerSpawnPoints;
        
        [Header("UI")]
        [SerializeField] private Text statusText;
        [SerializeField] private Button addPlayerButton;
        [SerializeField] private Button removePlayerButton;
        
        [Header("Split Screen Cameras")]
        [SerializeField] private Camera[] playerCameras;
        
        [Header("Mobile Controls (for each player)")]
        [SerializeField] private MultiPlayerInputManager.MobileControlsConfig[] mobileControlConfigs;

        // Player management
        private readonly Dictionary<int, GameObject> playerCharacters = new Dictionary<int, GameObject>();
        private readonly Dictionary<int, PlayerController> playerControllers = new Dictionary<int, PlayerController>();

        private void Start()
        {
            InitializeMultiPlayerManager();
            SetupUI();
            
            // Create first player automatically
            CreatePlayer();
        }

        private void Update()
        {
            UpdatePlayerInput();
            UpdateUI();
        }

        private void InitializeMultiPlayerManager()
        {
            if (multiPlayerManager == null)
            {
                multiPlayerManager = FindFirstObjectByType<MultiPlayerInputManager>();
                if (multiPlayerManager == null)
                {
                    Debug.LogError("MultiPlayerInputManager not found!");
                    return;
                }
            }

            // Subscribe to player events
            multiPlayerManager.OnPlayerJoined += OnPlayerJoined;
            multiPlayerManager.OnPlayerLeft += OnPlayerLeft;
            multiPlayerManager.OnPlayerDeviceChanged += OnPlayerDeviceChanged;
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
            if (multiPlayerManager.PlayerCount >= maxPlayers)
            {
                Debug.LogWarning("Maximum players reached!");
                return;
            }

            int playerId = multiPlayerManager.CreatePlayer();
            if (playerId >= 0)
            {
                Debug.Log($"Player {playerId} created successfully");
            }
        }

        public void RemoveLastPlayer()
        {
            var playerIds = multiPlayerManager.GetActivePlayerIds();
            if (playerIds.Length > 0)
            {
                int lastPlayerId = playerIds[playerIds.Length - 1];
                multiPlayerManager.RemovePlayer(lastPlayerId);
            }
        }

        private void OnPlayerJoined(int playerId, PlayerInputController inputController)
        {
            Debug.Log($"Player {playerId} joined the game");
            
            // Create player character
            CreatePlayerCharacter(playerId, inputController);
            
            // Setup mobile controls if available
            SetupPlayerMobileControls(playerId);
            
            // Setup split screen camera
            SetupPlayerCamera(playerId, inputController);
        }

        private void OnPlayerLeft(int playerId, PlayerInputController inputController)
        {
            Debug.Log($"Player {playerId} left the game");
            
            // Destroy player character
            if (playerCharacters.TryGetValue(playerId, out var character))
            {
                Destroy(character);
                playerCharacters.Remove(playerId);
            }
            
            // Remove controller reference
            playerControllers.Remove(playerId);
        }

        private void OnPlayerDeviceChanged(int playerId, UnityEngine.InputSystem.InputDevice device)
        {
            Debug.Log($"Player {playerId} device changed to: {device.displayName}");
        }

        #endregion

        #region Player Character Management

        private void CreatePlayerCharacter(int playerId, PlayerInputController inputController)
        {
            if (playerCharacterPrefab == null) return;

            // Determine spawn position
            Vector3 spawnPosition = Vector3.zero;
            if (playerSpawnPoints != null && playerId < playerSpawnPoints.Length)
            {
                spawnPosition = playerSpawnPoints[playerId].position;
            }
            else
            {
                // Default positioning for multiple players
                spawnPosition = new Vector3(playerId * 3f, 0, 0);
            }

            // Create character
            var character = Instantiate(playerCharacterPrefab, spawnPosition, Quaternion.identity);
            character.name = $"Player{playerId}_Character";
            
            // Setup player controller
            var controller = character.GetComponent<PlayerController>();
            if (controller == null)
            {
                controller = character.AddComponent<PlayerController>();
            }
            
            controller.Initialize(playerId, inputController);
            
            // Store references
            playerCharacters[playerId] = character;
            playerControllers[playerId] = controller;
        }

        private void SetupPlayerMobileControls(int playerId)
        {
            // Setup mobile controls if this is a mobile platform and config is available
            #if UNITY_ANDROID || UNITY_IOS
            if (mobileControlConfigs != null && playerId < mobileControlConfigs.Length)
            {
                var config = mobileControlConfigs[playerId];
                if (config.uiContainer != null)
                {
                    config.uiContainer.gameObject.SetActive(true);
                    multiPlayerManager.SetupMobileControls(playerId, config);
                }
            }
            #endif
        }

        private void SetupPlayerCamera(int playerId, PlayerInputController inputController)
        {
            if (playerCameras != null && playerId < playerCameras.Length)
            {
                var camera = playerCameras[playerId];
                inputController.SetCamera(camera);
                
                // Setup split screen viewport
                SetupSplitScreenViewport(playerId, camera);
            }
        }

        private void SetupSplitScreenViewport(int playerId, Camera camera)
        {
            int playerCount = multiPlayerManager.PlayerCount;
            
            switch (playerCount)
            {
                case 1:
                    camera.rect = new Rect(0, 0, 1, 1);
                    break;
                case 2:
                    // Horizontal split
                    camera.rect = playerId == 0 ? 
                        new Rect(0, 0.5f, 1, 0.5f) : 
                        new Rect(0, 0, 1, 0.5f);
                    break;
                case 3:
                case 4:
                    // Quad split
                    float x = (playerId % 2) * 0.5f;
                    float y = (playerId < 2) ? 0.5f : 0f;
                    camera.rect = new Rect(x, y, 0.5f, 0.5f);
                    break;
            }
        }

        #endregion

        #region Input Handling

        private void UpdatePlayerInput()
        {
            // Input is handled individually by each PlayerController
            // This method could be used for global input handling if needed
        }

        #endregion

        #region UI Updates

        private void UpdateUI()
        {
            if (statusText != null)
            {
                var playerIds = multiPlayerManager.GetActivePlayerIds();
                statusText.text = $"Players: {playerIds.Length}/{maxPlayers}\n";
                
                foreach (int playerId in playerIds)
                {
                    var device = multiPlayerManager.GetPlayerDevice(playerId);
                    statusText.text += $"Player {playerId}: {device?.displayName ?? "None"}\n";
                }
            }
            
            // Update button states
            if (addPlayerButton != null)
            {
                addPlayerButton.interactable = multiPlayerManager.PlayerCount < maxPlayers;
            }
            
            if (removePlayerButton != null)
            {
                removePlayerButton.interactable = multiPlayerManager.PlayerCount > 0;
            }
        }

        #endregion

        #region Player Controller Class

        /// <summary>
        /// Individual player controller that uses the multi-player input system.
        /// </summary>
        public class PlayerController : MonoBehaviour
        {
            [Header("Player Settings")]
            [SerializeField] private float moveSpeed = 5f;
            [SerializeField] private float jumpForce = 10f;
            [SerializeField] private Color playerColor = Color.white;
            
            private int playerId;
            private IInputManager inputManager;
            private Rigidbody rb;
            private Renderer playerRenderer;
            private bool isGrounded = true;

            public void Initialize(int id, IInputManager input)
            {
                playerId = id;
                inputManager = input;
                
                // Get components
                rb = GetComponent<Rigidbody>();
                if (rb == null)
                {
                    rb = gameObject.AddComponent<Rigidbody>();
                }
                
                playerRenderer = GetComponent<Renderer>();
                
                // Set player color
                SetPlayerColor();
                
                Debug.Log($"Player {playerId} controller initialized");
            }

            private void Update()
            {
                if (inputManager == null) return;
                
                HandleMovement();
                HandleActions();
            }

            private void HandleMovement()
            {
                Vector2 movement = inputManager.GetMove();
                
                if (movement != Vector2.zero)
                {
                    Vector3 move = new Vector3(movement.x, 0, movement.y) * moveSpeed * Time.deltaTime;
                    transform.Translate(move);
                }
            }

            private void HandleActions()
            {
                // Jump
                if (inputManager.GetJump() && isGrounded)
                {
                    rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                    isGrounded = false;
                    Debug.Log($"Player {playerId} jumped!");
                }
                
                // Fire
                if (inputManager.GetFire())
                {
                    Debug.Log($"Player {playerId} fired!");
                }
                
                // Interact
                if (inputManager.GetInteract())
                {
                    Debug.Log($"Player {playerId} interacted!");
                }
            }

            private void SetPlayerColor()
            {
                if (playerRenderer != null)
                {
                    // Set different colors for different players
                    Color[] colors = { Color.red, Color.blue, Color.green, Color.yellow };
                    playerColor = colors[playerId % colors.Length];
                    playerRenderer.material.color = playerColor;
                }
            }

            private void OnCollisionEnter(Collision collision)
            {
                if (collision.gameObject.CompareTag("Ground"))
                {
                    isGrounded = true;
                }
            }
        }

        #endregion

        private void OnDestroy()
        {
            // Cleanup
            if (multiPlayerManager != null)
            {
                multiPlayerManager.OnPlayerJoined -= OnPlayerJoined;
                multiPlayerManager.OnPlayerLeft -= OnPlayerLeft;
                multiPlayerManager.OnPlayerDeviceChanged -= OnPlayerDeviceChanged;
            }
        }
    }
}
