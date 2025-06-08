# Using InputManager with Existing Input Actions

This guide shows how to use the enhanced InputManager with your existing Input Actions files without having to rename your actions.

## 🎯 **The Problem**

You have an existing Input Actions file with actions named like:
- `Attack` (instead of `Fire`)
- `Run` (instead of `Sprint`)
- `Use` (instead of `Interact`)
- `Movement` (instead of `Move`)

The InputManager expects specific action names, but you don't want to change your existing setup.

## ✅ **The Solution: Action Mapping**

The InputManager supports **Action Mapping** - a system that translates between your existing action names and the InputManager's expected names.

## 🚀 **Quick Setup (Recommended)**

### **Step 1: Use the Setup Wizard**

1. Go to `Tools > Ludo > CrossInput > Setup Wizard`
2. Select your existing Input Actions asset
3. The wizard will automatically suggest mappings
4. Review and adjust the mappings
5. Click "Apply Mappings"

### **Step 2: Configure InputManager**

```csharp
[SerializeField] private InputManager inputManager;
[SerializeField] private InputActionMapping actionMapping; // Created by wizard

private void Start()
{
    inputManager.SetActionMapping(actionMapping);
}
```

### **Step 3: Use InputManager Normally**

```csharp
private void Update()
{
    // These automatically use your mapped action names
    Vector2 movement = inputManager.GetMove(); // Uses your "Movement" action
    
    if (inputManager.GetFire()) // Uses your "Attack" action
    {
        Attack();
    }
    
    if (inputManager.GetSprint()) // Uses your "Run" action
    {
        StartRunning();
    }
}
```

## 🔧 **Manual Setup**

### **Step 1: Create Action Mapping Asset**

1. Right-click in Project window
2. Select `Create > Ludo > CrossInput > Action Mapping`
3. Name it "MyInputActionMapping"

### **Step 2: Configure Mappings**

In the Inspector, map your action names:

| Expected Name | Your Action Name |
|---------------|------------------|
| Move          | Movement         |
| Fire          | Attack           |
| Jump          | Jump             |
| Sprint        | Run              |
| Interact      | Use              |
| Inventory     | Bag              |
| Pause         | Menu             |

### **Step 3: Assign to InputManager**

```csharp
public class PlayerController : MonoBehaviour
{
    [SerializeField] private InputManager inputManager;
    [SerializeField] private InputActionMapping actionMapping;
    
    private void Start()
    {
        inputManager.SetActionMapping(actionMapping);
    }
}
```

## 📋 **Common Mapping Examples**

### **FPS Game Example**

Your Input Actions:
```
- WASD (Vector2) → Maps to "Move"
- LeftClick (Button) → Maps to "Fire"  
- Space (Button) → Maps to "Jump"
- LeftShift (Button) → Maps to "Sprint"
- R (Button) → Maps to "Reload"
- E (Button) → Maps to "Interact"
- Tab (Button) → Maps to "Inventory"
- Escape (Button) → Maps to "Pause"
```

### **Platformer Game Example**

Your Input Actions:
```
- Movement (Vector2) → Maps to "Move"
- A_Button (Button) → Maps to "Jump"
- X_Button (Button) → Maps to "Fire"
- B_Button (Button) → Maps to "Sprint"
- Y_Button (Button) → Maps to "Interact"
- Start (Button) → Maps to "Pause"
```

### **Mobile Game Example**

Your Input Actions:
```
- TouchMove (Vector2) → Maps to "Move"
- TouchTap (Button) → Maps to "Fire"
- SwipeUp (Button) → Maps to "Jump"
- TouchHold (Button) → Maps to "Sprint"
- DoubleTap (Button) → Maps to "Interact"
```

## 🔍 **Validation and Debugging**

### **Validate Your Mapping**

```csharp
private void ValidateMapping()
{
    var playerInput = GetComponent<PlayerInput>();
    var result = actionMapping.ValidateMapping(playerInput.actions);
    
    if (!result.IsValid)
    {
        Debug.LogWarning(result.GetReport());
    }
}
```

### **Debug Current Mappings**

```csharp
private void ShowCurrentMappings()
{
    Debug.Log($"Fire maps to: {actionMapping.GetActualActionName('Fire')}");
    Debug.Log($"Jump maps to: {actionMapping.GetActualActionName('Jump')}");
    Debug.Log($"Move maps to: {actionMapping.GetActualActionName('Move')}");
}
```

## 🎮 **Advanced Usage**

### **Runtime Mapping Changes**

```csharp
public class InputConfigurationManager : MonoBehaviour
{
    [SerializeField] private InputActionMapping defaultMapping;
    [SerializeField] private InputActionMapping alternativeMapping;
    [SerializeField] private InputManager inputManager;
    
    public void SwitchToAlternativeControls()
    {
        inputManager.SetActionMapping(alternativeMapping);
    }
    
    public void ResetToDefaultControls()
    {
        inputManager.SetActionMapping(defaultMapping);
    }
}
```

### **Multiple Control Schemes**

```csharp
public class MultiSchemeController : MonoBehaviour
{
    [SerializeField] private InputActionMapping keyboardMapping;
    [SerializeField] private InputActionMapping gamepadMapping;
    [SerializeField] private InputActionMapping mobileMapping;
    [SerializeField] private InputManager inputManager;
    
    private void Start()
    {
        // Detect platform and apply appropriate mapping
        #if UNITY_STANDALONE
            inputManager.SetActionMapping(keyboardMapping);
        #elif UNITY_GAMEPAD
            inputManager.SetActionMapping(gamepadMapping);
        #elif UNITY_MOBILE
            inputManager.SetActionMapping(mobileMapping);
        #endif
    }
}
```

### **Conditional Mappings**

```csharp
public class ConditionalMappingExample : MonoBehaviour
{
    [SerializeField] private InputActionMapping beginnerMapping;
    [SerializeField] private InputActionMapping expertMapping;
    [SerializeField] private InputManager inputManager;
    
    public void SetDifficultyLevel(bool isExpert)
    {
        var mapping = isExpert ? expertMapping : beginnerMapping;
        inputManager.SetActionMapping(mapping);
    }
}
```

## 🛠️ **Best Practices**

### **1. Use Descriptive Names**

```csharp
// Good
actionMapping.name = "FPS_KeyboardMouse_Mapping";

// Bad  
actionMapping.name = "Mapping1";
```

### **2. Validate Early**

```csharp
private void Awake()
{
    // Validate mapping before using it
    if (actionMapping != null)
    {
        var result = actionMapping.ValidateMapping(playerInput.actions);
        if (!result.IsValid)
        {
            Debug.LogError($"Invalid input mapping: {result.GetReport()}");
        }
    }
}
```

### **3. Handle Missing Mappings**

```csharp
private void HandleInput()
{
    // Check if mapping exists before using
    if (actionMapping.HasMapping(InputActionNames.FIRE))
    {
        if (inputManager.GetFire())
        {
            Fire();
        }
    }
    else
    {
        Debug.LogWarning("No mapping found for Fire action");
    }
}
```

### **4. Use Fallbacks**

```csharp
private bool GetFireInput()
{
    // Try mapped action first, fallback to direct action name
    if (actionMapping != null && actionMapping.HasMapping(InputActionNames.FIRE))
    {
        return inputManager.GetFire();
    }
    else
    {
        // Fallback to checking your original action name directly
        return inputManager.GetKey<bool>("Attack");
    }
}
```

## 🔧 **Troubleshooting**

### **Common Issues**

1. **"Action not found" warnings**
   - Check that your action names in the mapping match exactly (case-sensitive)
   - Use the Setup Wizard to auto-detect action names

2. **Input not responding**
   - Validate your mapping using `ValidateMapping()`
   - Check that PlayerInput component has the correct Input Actions asset

3. **Performance issues**
   - Action mapping has minimal performance impact
   - Mappings are cached for efficiency

### **Debug Tools**

```csharp
// Check what action is actually being called
string actualAction = actionMapping.GetActualActionName(InputActionNames.FIRE);
Debug.Log($"Fire input is mapped to: {actualAction}");

// Validate entire mapping
var result = actionMapping.ValidateMapping(playerInput.actions);
Debug.Log(result.GetReport());
```

## 📝 **Summary**

With Action Mapping, you can:

✅ **Keep your existing Input Actions unchanged**
✅ **Use InputManager's unified API**  
✅ **Support multiple control schemes**
✅ **Validate configurations automatically**
✅ **Switch mappings at runtime**
✅ **Debug mapping issues easily**

The Action Mapping system provides a flexible bridge between your existing input setup and the InputManager's powerful features, giving you the best of both worlds!
