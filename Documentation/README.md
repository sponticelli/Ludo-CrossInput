# Ludo CrossInput Enhanced

A robust, production-ready cross-platform input system for Unity that provides a unified interface for handling input across different platforms and input methods.

## 🚀 Features

### Core Features
- **Unified Input API**: Single interface for keyboard, mouse, gamepad, touch, and mobile joystick input
- **Cross-Platform Support**: Works seamlessly across PC, mobile, console, and VR platforms
- **Unity Input System Integration**: Built on Unity's new Input System with proper event handling
- **Memory Management**: Proper event cleanup and disposal to prevent memory leaks
- **Error Handling**: Comprehensive error handling with detailed logging

### Advanced Features
- **Input Buffering**: Configurable input buffering for more forgiving input timing
- **Device Management**: Real-time device detection and management
- **Input Rebinding**: Runtime key/button remapping with save/load functionality
- **Performance Monitoring**: Built-in performance metrics and optimization tools
- **Configuration System**: Runtime input action configuration and templates
- **Thread Safety**: Thread-safe input flag management

### Mobile Features
- **Multiple Joystick Types**: Fixed, Dynamic, Floating, and Variable joysticks
- **Touch Gesture Support**: Swipe detection with configurable thresholds
- **Mobile Button Integration**: Seamless UI button to input action mapping
- **Responsive Design**: Automatic scaling and positioning for different screen sizes

## 📦 Installation

### Via Unity Package Manager
1. Open Unity Package Manager
2. Click "+" and select "Add package from git URL"
3. Enter: `https://github.com/sponticelli/Ludo-CrossInput.git`

### Via Package Manager (Local)
1. Download or clone this repository
2. In Unity, go to Window > Package Manager
3. Click "+" and select "Add package from disk"
4. Select the `package.json` file

## 🎯 Quick Start

### Basic Setup

1. **Add InputManager to your scene:**
```csharp
// Create a GameObject and add InputManager component
var inputManagerGO = new GameObject("InputManager");
var inputManager = inputManagerGO.AddComponent<InputManager>();

// Configure using fluent interface
inputManager
    .SetMainCamera(Camera.main)
    .SetJoystick(FindFirstObjectByType<Joystick>());
```

2. **Use input in your scripts:**
```csharp
public class PlayerController : MonoBehaviour
{
    [SerializeField] private InputManager inputManager;

    private void Update()
    {
        // Get movement input (works with WASD, gamepad stick, or mobile joystick)
        Vector2 movement = inputManager.GetMove();

        // Handle action inputs
        if (inputManager.GetJump())
        {
            Jump();
        }

        if (inputManager.GetFire())
        {
            Fire();
        }
    }
}
```

### Mobile Setup

1. **Create a mobile joystick:**
```csharp
// Add joystick prefab to Canvas
var joystick = Instantiate(joystickPrefab, canvas.transform);
inputManager.SetJoystick(joystick);
```

2. **Setup mobile buttons:**
```csharp
inputManager.SetMobileButton(
    fireBtn: fireButton,
    jumpBtn: jumpButton,
    crouchBtn: crouchButton,
    // ... other buttons
);
```

## 📚 API Reference

### Core Input Methods

#### Movement Input
```csharp
Vector2 GetMove()                    // Get movement input (WASD, gamepad, joystick)
bool GetLeft()                       // Left button/key pressed
bool GetRight()                      // Right button/key pressed
```

#### Action Input
```csharp
bool GetFire()                       // Fire action
bool GetJump()                       // Jump action
bool GetCrouch()                     // Crouch action
bool GetSprint()                     // Sprint action
bool GetReload()                     // Reload action
bool GetInventory()                  // Inventory action
bool GetInteract()                   // Interact action
bool GetMap()                        // Map action
bool GetPrevious()                   // Previous item action
bool GetNext()                       // Next item action
bool GetBack()                       // Back/Cancel action
bool GetPause()                      // Pause action
```

#### Positional Input
```csharp
Vector2 GetScreenPosition()          // Current screen/touch position
Vector2 GetWorldPosition()           // Screen position converted to world space
SwipeDirection GetSwipe()            // Detect swipe gestures
```

#### Raycasting
```csharp
Collider2D GetCast2D()              // 2D physics raycast from input position
Collider GetCast3D()                // 3D physics raycast from input position
RectTransform GetCastRect()         // UI raycast from input position
```

#### Generic Input
```csharp
T GetKey<T>(string actionName)       // Get any input action by name
```

### Configuration Methods

#### Fluent Interface
```csharp
InputManager SetMainCamera(Camera camera)                    // Set main camera
InputManager SetGraphicRaycaster(GraphicRaycaster raycaster) // Set UI raycaster
InputManager SetJoystick(Joystick joystick)                  // Set mobile joystick
InputManager SetMobileButton(...)                            // Set mobile buttons
```

## 🔧 Advanced Usage

### Input Buffering

Enable input buffering for more forgiving input timing:

```csharp
// Configure in InputManager inspector
[SerializeField] private bool enableInputBuffering = true;
[SerializeField] private float inputBufferTime = 0.1f;

// Or configure at runtime
var inputBuffer = new InputBuffer(0.15f);
inputBuffer.BufferInput("Jump");

// Check buffered input
if (inputBuffer.ConsumeBufferedInput("Jump"))
{
    PerformJump();
}
```

### Device Management

Monitor input device changes:

```csharp
public class DeviceHandler : MonoBehaviour
{
    [SerializeField] private InputDeviceManager deviceManager;

    private void Start()
    {
        deviceManager.OnDeviceAdded += OnDeviceConnected;
        deviceManager.OnDeviceRemoved += OnDeviceDisconnected;
    }

    private void OnDeviceConnected(InputDevice device)
    {
        Debug.Log($"Device connected: {device.displayName}");
        // Update UI, enable device-specific features, etc.
    }

    private void OnDeviceDisconnected(InputDevice device)
    {
        Debug.Log($"Device disconnected: {device.displayName}");
        // Handle device loss gracefully
    }
}
```

### Input Rebinding

Allow players to customize their controls:

```csharp
public class RebindingUI : MonoBehaviour
{
    [SerializeField] private InputRebindingManager rebindingManager;
    [SerializeField] private Button rebindButton;
    [SerializeField] private Text bindingText;

    private void Start()
    {
        rebindingManager.OnRebindCompleted += OnRebindCompleted;
        UpdateBindingDisplay();
    }

    public void StartRebinding()
    {
        rebindingManager.StartRebinding("Jump", 0);
        rebindButton.interactable = false;
        bindingText.text = "Press any key...";
    }

    private void OnRebindCompleted(string actionName, int bindingIndex)
    {
        rebindButton.interactable = true;
        UpdateBindingDisplay();
    }

    private void UpdateBindingDisplay()
    {
        string displayString = rebindingManager.GetBindingDisplayString("Jump", 0);
        bindingText.text = displayString;
    }
}
```

### Performance Monitoring

Monitor input system performance:

```csharp
public class PerformanceDisplay : MonoBehaviour
{
    [SerializeField] private InputPerformanceMonitor performanceMonitor;
    [SerializeField] private Text performanceText;

    private void Update()
    {
        if (performanceMonitor.IsMonitoring)
        {
            performanceText.text = performanceMonitor.GetPerformanceReport();
        }
    }
}
```

### Custom Input Actions

Add custom input actions at runtime:

```csharp
public class CustomInputSetup : MonoBehaviour
{
    [SerializeField] private ConfigurableInputManager configurableInput;

    private void Start()
    {
        // Create a custom action configuration
        var customAction = InputConfiguration.CreateTemplate("CustomAction", InputTemplateType.KeyboardButton);

        // Add the action to the input system
        configurableInput.AddCustomAction(customAction);
    }
}
```

## 🎮 Joystick Types

### Fixed Joystick
- Joystick stays in a fixed position
- Handle moves within the background area
- Best for consistent UI layouts

### Dynamic Joystick
- Joystick appears where the user touches
- Handle moves relative to touch position
- Good for flexible positioning

### Floating Joystick
- Background appears at touch position
- Disappears when released
- Minimal UI footprint

### Variable Joystick
- Combines features of fixed and floating
- Configurable behavior based on needs

## ⚡ Performance Optimization

### Best Practices

1. **Use Input Buffering Wisely:**
```csharp
// Enable only for actions that need it
[SerializeField] private bool enableInputBuffering = true;
[SerializeField] private float inputBufferTime = 0.1f; // Keep it short
```

2. **Monitor Performance:**
```csharp
// Use performance monitoring in development
performanceMonitor.StartInputTiming();
// ... input processing ...
performanceMonitor.EndInputTiming(eventCount);
```

3. **Proper Cleanup:**
```csharp
private void OnDestroy()
{
    inputManager?.Dispose(); // Always dispose properly
}
```

4. **Efficient Raycasting:**
```csharp
// Only raycast when needed
if (inputManager.GetPress())
{
    var hit = inputManager.GetCast2D();
    if (hit != null)
    {
        // Handle hit
    }
}
```

## 🧪 Testing

The package includes comprehensive unit tests:

```csharp
// Run tests in Unity Test Runner
// Tests cover:
// - Input flag management
// - Memory leak prevention
// - Error handling
// - Buffer functionality
// - Device management
```

## 🔍 Troubleshooting

### Common Issues

1. **Input not working:**
   - Ensure PlayerInput component is present
   - Check that Input Actions asset is assigned
   - Verify action names match InputActionNames constants

2. **Memory leaks:**
   - Always call Dispose() on InputManager
   - Unsubscribe from events in OnDestroy()
   - Use the provided event handler references

3. **Performance issues:**
   - Enable performance monitoring
   - Check input processing time thresholds
   - Reduce input buffer time if not needed

4. **Mobile joystick not responding:**
   - Ensure joystick is child of Canvas
   - Check Canvas settings (Screen Space - Overlay recommended)
   - Verify GraphicRaycaster is present on Canvas

### Debug Information

Enable detailed logging:

```csharp
// In InputManager inspector
[SerializeField] private bool logPerformanceWarnings = true;
[SerializeField] private bool logDeviceChanges = true;
```

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Add tests for new functionality
4. Ensure all tests pass
5. Submit a pull request

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/sponticelli/Ludo-CrossInput/issues)
- **Discussions**: [GitHub Discussions](https://github.com/sponticelli/Ludo-CrossInput/discussions)
- **Documentation**: [Wiki](https://github.com/sponticelli/Ludo-CrossInput/wiki)

## 🔄 Changelog

### Version 2.0.0
- ✅ Fixed critical event handling bugs
- ✅ Added input buffering system
- ✅ Implemented device management
- ✅ Added input rebinding support
- ✅ Comprehensive error handling
- ✅ Performance monitoring tools
- ✅ Memory leak prevention
- ✅ Thread-safe input management
- ✅ Extensive unit test coverage
- ✅ Production-ready reliability

### Version 1.0.0
- Initial release with basic cross-input functionality