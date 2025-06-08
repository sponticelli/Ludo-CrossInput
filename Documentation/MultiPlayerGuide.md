# Multi-Player Input Management Guide

The enhanced InputManager fully supports **multiple players on the same device** with individual input states, controls, and configurations.

## 🎮 **Multi-Player Capabilities**

### ✅ **What's Supported:**
- **Multiple PlayerInput components** - Each player has their own input state
- **Device separation** - Different controllers/keyboards for different players  
- **Individual mobile controls** - Separate joysticks and buttons per player
- **Split-screen support** - Individual cameras and UI for each player
- **Per-player action mappings** - Different control schemes per player
- **Independent input buffering** - Separate input timing per player
- **Cross-platform** - Works on PC, console, and mobile

### 🎯 **Use Cases:**
- **Local co-op games** (2-4 players on same device)
- **Fighting games** (multiple controllers)
- **Party games** (shared screen, individual controls)
- **Split-screen multiplayer** (racing, FPS, etc.)
- **Mobile multiplayer** (multiple touch areas)

## 🚀 **Quick Setup**

### **Step 1: Add MultiPlayerInputManager**

```csharp
// Create GameObject with MultiPlayerInputManager
var multiPlayerGO = new GameObject("MultiPlayerInputManager");
var multiPlayerManager = multiPlayerGO.AddComponent<MultiPlayerInputManager>();

// Configure settings in Inspector:
// - Max Players: 4
// - Auto Create Players: true
// - Default Input Actions: (assign your Input Actions asset)
// - Player Prefab: (optional, for automatic player creation)
```

### **Step 2: Create Players**

```csharp
// Create players programmatically
int player1Id = multiPlayerManager.CreatePlayer();
int player2Id = multiPlayerManager.CreatePlayer();

// Or let Unity's PlayerInputManager handle it automatically
// (players join when they press buttons on new devices)
```

### **Step 3: Get Player Input**

```csharp
// Get input manager for specific player
var player1Input = multiPlayerManager.GetPlayerInputManager(player1Id);
var player2Input = multiPlayerManager.GetPlayerInputManager(player2Id);

// Use like normal InputManager
Vector2 p1Movement = player1Input.GetMove();
Vector2 p2Movement = player2Input.GetMove();

if (player1Input.GetFire()) { /* Player 1 fired */ }
if (player2Input.GetJump()) { /* Player 2 jumped */ }
```

## 📋 **Detailed Setup Guide**

### **1. Scene Setup**

#### **Basic Multi-Player Scene:**
```csharp
// 1. Create MultiPlayerInputManager GameObject
// 2. Assign Input Actions asset
// 3. Configure max players
// 4. Setup player spawn points
// 5. Configure split-screen cameras (optional)
```

#### **Inspector Configuration:**
```csharp
[Header("Multi-Player Configuration")]
[SerializeField] private int maxPlayers = 4;                    // Configure in Inspector
[SerializeField] private bool autoCreatePlayers = true;         // Configure in Inspector
[SerializeField] private InputActionAsset defaultInputActions;  // Assign in Inspector

[Header("Split Screen")]
[SerializeField] private bool enableSplitScreen = true;         // Configure in Inspector
[SerializeField] private Camera[] playerCameras;                // Assign cameras in Inspector

// Note: These are configured in the Inspector, not set via code
// Unity's PlayerInputManager handles the internal player management
```

### **2. Player Management**

#### **Creating Players:**
```csharp
public class GameManager : MonoBehaviour
{
    [SerializeField] private MultiPlayerInputManager multiPlayerManager;
    
    public void AddPlayer()
    {
        int playerId = multiPlayerManager.CreatePlayer();
        if (playerId >= 0)
        {
            Debug.Log($"Player {playerId} joined!");
            SetupPlayerCharacter(playerId);
        }
    }
    
    public void RemovePlayer(int playerId)
    {
        if (multiPlayerManager.RemovePlayer(playerId))
        {
            Debug.Log($"Player {playerId} left!");
            CleanupPlayerCharacter(playerId);
        }
    }
}
```

#### **Player Events:**
```csharp
private void Start()
{
    multiPlayerManager.OnPlayerJoined += OnPlayerJoined;
    multiPlayerManager.OnPlayerLeft += OnPlayerLeft;
    multiPlayerManager.OnPlayerDeviceChanged += OnDeviceChanged;
}

private void OnPlayerJoined(int playerId, PlayerInputController controller)
{
    Debug.Log($"Player {playerId} joined with device: {controller.PlayerInput.GetDevice<InputDevice>()?.displayName}");
    
    // Setup player character, UI, camera, etc.
    CreatePlayerCharacter(playerId, controller);
}
```

### **3. Device Assignment**

#### **Automatic Device Assignment:**
```csharp
// Unity automatically assigns devices when players join
// First available device is assigned to new players
```

#### **Manual Device Assignment:**
```csharp
// Assign specific device to player
var gamepad = Gamepad.all[0]; // First gamepad
multiPlayerManager.AssignDeviceToPlayer(playerId, gamepad);

// Check player's current device
var device = multiPlayerManager.GetPlayerDevice(playerId);
Debug.Log($"Player {playerId} using: {device?.displayName}");
```

### **4. Split-Screen Setup**

#### **Automatic Split-Screen:**
```csharp
[Header("Split Screen Cameras")]
[SerializeField] private Camera[] playerCameras = new Camera[4];

private void SetupSplitScreen(int playerCount)
{
    for (int i = 0; i < playerCount; i++)
    {
        var camera = playerCameras[i];
        
        switch (playerCount)
        {
            case 1:
                camera.rect = new Rect(0, 0, 1, 1); // Full screen
                break;
            case 2:
                // Horizontal split
                camera.rect = i == 0 ? 
                    new Rect(0, 0.5f, 1, 0.5f) : // Top half
                    new Rect(0, 0, 1, 0.5f);     // Bottom half
                break;
            case 3:
            case 4:
                // Quad split
                float x = (i % 2) * 0.5f;
                float y = (i < 2) ? 0.5f : 0f;
                camera.rect = new Rect(x, y, 0.5f, 0.5f);
                break;
        }
    }
}
```

### **5. Mobile Multi-Player**

#### **Multiple Touch Areas:**
```csharp
[Serializable]
public class MobilePlayerConfig
{
    public RectTransform uiContainer;  // Player's UI area
    public Joystick joystick;         // Player's joystick
    public Button[] actionButtons;     // Player's buttons
}

[SerializeField] private MobilePlayerConfig[] mobileConfigs;

private void SetupMobilePlayer(int playerId)
{
    if (playerId < mobileConfigs.Length)
    {
        var config = mobileConfigs[playerId];
        
        // Create mobile controls config
        var mobileConfig = new MultiPlayerInputManager.MobileControlsConfig
        {
            uiContainer = config.uiContainer,
            joystick = config.joystick,
            fireButton = config.actionButtons[0],
            jumpButton = config.actionButtons[1],
            // ... other buttons
        };
        
        multiPlayerManager.SetupMobileControls(playerId, mobileConfig);
    }
}
```

## 🎮 **Common Scenarios**

### **Scenario 1: Local Co-op Game**

```csharp
public class CoopGameManager : MonoBehaviour
{
    [SerializeField] private MultiPlayerInputManager multiPlayerManager;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform[] spawnPoints;
    
    private Dictionary<int, GameObject> playerCharacters = new Dictionary<int, GameObject>();
    
    private void Start()
    {
        // Create 2 players for co-op
        CreatePlayer(); // Player 0
        CreatePlayer(); // Player 1
    }
    
    private void CreatePlayer()
    {
        int playerId = multiPlayerManager.CreatePlayer();
        
        // Spawn character
        var spawnPoint = spawnPoints[playerId % spawnPoints.Length];
        var character = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        
        // Setup character controller
        var controller = character.GetComponent<PlayerCharacterController>();
        var inputManager = multiPlayerManager.GetPlayerInputManager(playerId);
        controller.Initialize(inputManager);
        
        playerCharacters[playerId] = character;
    }
}
```

### **Scenario 2: Fighting Game**

```csharp
public class FightingGameManager : MonoBehaviour
{
    [SerializeField] private MultiPlayerInputManager multiPlayerManager;
    [SerializeField] private FighterController[] fighters;
    
    private void Start()
    {
        // Create exactly 2 players
        int player1Id = multiPlayerManager.CreatePlayer();
        int player2Id = multiPlayerManager.CreatePlayer();
        
        // Assign to fighters
        var p1Input = multiPlayerManager.GetPlayerInputManager(player1Id);
        var p2Input = multiPlayerManager.GetPlayerInputManager(player2Id);
        
        fighters[0].Initialize(p1Input);
        fighters[1].Initialize(p2Input);
    }
}

public class FighterController : MonoBehaviour
{
    private IInputManager inputManager;
    
    public void Initialize(IInputManager input)
    {
        inputManager = input;
    }
    
    private void Update()
    {
        // Fighting game input
        Vector2 movement = inputManager.GetMove();
        
        if (inputManager.GetFire()) { /* Light attack */ }
        if (inputManager.GetJump()) { /* Heavy attack */ }
        if (inputManager.GetCrouch()) { /* Block */ }
        
        // Special moves based on input combinations
        HandleSpecialMoves();
    }
}
```

### **Scenario 3: Party Game with Mobile**

```csharp
public class PartyGameManager : MonoBehaviour
{
    [SerializeField] private MultiPlayerInputManager multiPlayerManager;
    [SerializeField] private RectTransform[] playerUIAreas; // 4 corners of screen
    [SerializeField] private GameObject joystickPrefab;
    [SerializeField] private GameObject buttonPrefab;
    
    private void Start()
    {
        SetupMobileMultiPlayer();
    }
    
    private void SetupMobileMultiPlayer()
    {
        for (int i = 0; i < 4; i++)
        {
            int playerId = multiPlayerManager.CreatePlayer();
            
            // Create mobile controls in player's UI area
            var uiArea = playerUIAreas[i];
            var joystick = Instantiate(joystickPrefab, uiArea).GetComponent<Joystick>();
            var fireButton = Instantiate(buttonPrefab, uiArea).GetComponent<Button>();
            
            // Setup mobile config
            var config = new MultiPlayerInputManager.MobileControlsConfig
            {
                uiContainer = uiArea,
                joystick = joystick,
                fireButton = fireButton
            };
            
            multiPlayerManager.SetupMobileControls(playerId, config);
        }
    }
}
```

## 🔧 **Advanced Features**

### **Per-Player Action Mappings**

```csharp
// Different control schemes per player
[SerializeField] private InputActionMapping player1Mapping; // WASD + Mouse
[SerializeField] private InputActionMapping player2Mapping; // Arrow Keys + Ctrl/Shift

private void AssignPlayerMappings()
{
    var p1Controller = multiPlayerManager.GetPlayerInputManager(0) as PlayerInputController;
    var p2Controller = multiPlayerManager.GetPlayerInputManager(1) as PlayerInputController;
    
    p1Controller?.SetActionMapping(player1Mapping);
    p2Controller?.SetActionMapping(player2Mapping);
}
```

### **Dynamic Player Joining**

```csharp
private void Update()
{
    // Allow players to join by pressing any button on unused devices
    foreach (var device in InputSystem.devices)
    {
        if (device.wasUpdatedThisFrame && !IsDeviceAssigned(device))
        {
            // Device input detected, create new player
            int playerId = multiPlayerManager.CreatePlayer(device);
            if (playerId >= 0)
            {
                Debug.Log($"Player {playerId} joined with {device.displayName}");
            }
        }
    }
}
```

### **Player-Specific UI**

```csharp
public class PlayerUI : MonoBehaviour
{
    [SerializeField] private Text playerIdText;
    [SerializeField] private Text deviceText;
    [SerializeField] private Slider healthBar;
    
    private int playerId;
    private MultiPlayerInputManager multiPlayerManager;
    
    public void Initialize(int id, MultiPlayerInputManager manager)
    {
        playerId = id;
        multiPlayerManager = manager;
        
        playerIdText.text = $"Player {playerId}";
        
        var device = multiPlayerManager.GetPlayerDevice(playerId);
        deviceText.text = device?.displayName ?? "No Device";
    }
}
```

## 🛠️ **Best Practices**

### **1. Resource Management**
```csharp
// Properly clean up when players leave
private void OnPlayerLeft(int playerId, PlayerInputController controller)
{
    // Destroy player objects
    if (playerCharacters.TryGetValue(playerId, out var character))
    {
        Destroy(character);
        playerCharacters.Remove(playerId);
    }
    
    // Update UI layout
    UpdateSplitScreenLayout();
}
```

### **2. Performance Optimization**
```csharp
// Cache player input managers
private readonly Dictionary<int, IInputManager> cachedInputManagers = new Dictionary<int, IInputManager>();

private IInputManager GetCachedInputManager(int playerId)
{
    if (!cachedInputManagers.TryGetValue(playerId, out var inputManager))
    {
        inputManager = multiPlayerManager.GetPlayerInputManager(playerId);
        cachedInputManagers[playerId] = inputManager;
    }
    return inputManager;
}
```

### **3. Error Handling**
```csharp
private void HandlePlayerInput(int playerId)
{
    var inputManager = multiPlayerManager.GetPlayerInputManager(playerId);
    if (inputManager == null)
    {
        Debug.LogWarning($"No input manager found for player {playerId}");
        return;
    }
    
    try
    {
        Vector2 movement = inputManager.GetMove();
        // Handle movement...
    }
    catch (System.Exception ex)
    {
        Debug.LogError($"Error handling input for player {playerId}: {ex.Message}");
    }
}
```

## 📊 **Summary**

### ✅ **Multi-Player Features:**
- **Individual player input states**
- **Device separation and assignment**  
- **Split-screen camera support**
- **Mobile multi-touch areas**
- **Per-player action mappings**
- **Dynamic player joining/leaving**
- **Cross-platform compatibility**

### 🎮 **Supported Scenarios:**
- **Local co-op** (2-4 players, shared screen)
- **Fighting games** (2 players, individual controls)
- **Party games** (multiple players, mobile touch)
- **Racing games** (split-screen, multiple controllers)
- **Puzzle games** (turn-based, shared controls)

The multi-player system provides everything needed for local multiplayer games while maintaining the same easy-to-use API as the single-player InputManager! 🚀
