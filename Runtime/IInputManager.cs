using UnityEngine;
using UnityEngine.UI;

namespace Ludo.CrossInput
{
    /// <summary>
    /// Interface for input management providing a unified API for cross-platform input handling.
    /// </summary>
    public interface IInputManager
    {
        /// <summary>
        /// Sets the main camera for input calculations. Returns this instance for method chaining.
        /// </summary>
        InputManager SetMainCamera(Camera newMainCamera);

        /// <summary>
        /// Sets the graphic raycaster for UI input detection. Returns this instance for method chaining.
        /// </summary>
        InputManager SetGraphicRaycaster(GraphicRaycaster newGraphicRaycaster);

        /// <summary>
        /// Sets the mobile joystick for movement input. Returns this instance for method chaining.
        /// </summary>
        InputManager SetJoystick(Joystick newJoystick);

        /// <summary>
        /// Sets the action mapping to use existing Input Actions with different names. Returns this instance for method chaining.
        /// </summary>
        InputManager SetActionMapping(InputActionMapping mapping);

        /// <summary>
        /// Sets all mobile buttons for touch input. Returns this instance for method chaining.
        /// </summary>
        InputManager SetMobileButton(Button fireBtn, Button jumpBtn, Button crouchBtn,
            Button sprintBtn, Button reloadBtn, Button inventoryBtn,
            Button interactBtn, Button mapBtn, Button previousBtn,
            Button nextBtn, Button backBtn, Button pauseBtn);

        /// <summary>
        /// Gets movement input from the appropriate method (new, old, or mobile).
        /// </summary>
        Vector2 GetMove();

        /// <summary>Returns true if left button is pressed.</summary>
        bool GetLeft();

        /// <summary>Returns true if right button is pressed.</summary>
        bool GetRight();

        /// <summary>Returns the current screen touch or pointer position.</summary>
        Vector2 GetScreenPosition();

        /// <summary>Converts the screen position to a world position using the main camera.</summary>
        Vector2 GetWorldPosition();

        /// <summary>Detects swipe direction based on input movement.</summary>
        SwipeDirection GetSwipe();

        /// <summary>Performs a Physics2D raycast and returns the hit Collider2D, if any.</summary>
        Collider2D GetCast2D();

        /// <summary>Performs a Physics raycast and returns the hit 3D Collider, if any.</summary>
        Collider GetCast3D();

        /// <summary>Performs a UI raycast and returns the first hit RectTransform, if any.</summary>
        RectTransform GetCastRect();

        /// <summary>Returns true if the input is pressed or held (mouse/touch/click).</summary>
        bool GetFire();

        /// <summary>Returns true if the input is pressed or held (mouse/touch/click).</summary>
        bool GetJump();

        /// <summary>Returns true if the input is pressed or held (mouse/touch/click).</summary>
        bool GetCrouch();

        /// <summary>Returns true if the input is pressed or held (mouse/touch/click).</summary>
        bool GetSprint();

        /// <summary>Returns true if the input is pressed or held (mouse/touch/click).</summary>
        bool GetReload();

        /// <summary>Returns true if the input is pressed or held (mouse/touch/click).</summary>
        bool GetInventory();

        /// <summary>Returns true if the input is pressed or held (mouse/touch/click).</summary>
        bool GetInteract();

        /// <summary>Returns true if the input is pressed or held (mouse/touch/click).</summary>
        bool GetMap();

        /// <summary>Returns true if the input is pressed or held (mouse/touch/click).</summary>
        bool GetPrevious();

        /// <summary>Returns true if the input is pressed or held (mouse/touch/click).</summary>
        bool GetNext();

        /// <summary>Returns true if the input is pressed or held (mouse/touch/click).</summary>
        bool GetBack();

        /// <summary>Returns true if the input is pressed or held (mouse/touch/click).</summary>
        bool GetPause();

        // Trigger-once input methods (consume input flag, return true only once per press)
        /// <summary>Returns true once when the fire input is pressed (trigger-once behavior).</summary>
        bool GetFirePressed();

        /// <summary>Returns true once when the jump input is pressed (trigger-once behavior).</summary>
        bool GetJumpPressed();

        /// <summary>Returns true once when the crouch input is pressed (trigger-once behavior).</summary>
        bool GetCrouchPressed();

        /// <summary>Returns true once when the sprint input is pressed (trigger-once behavior).</summary>
        bool GetSprintPressed();

        /// <summary>Returns true once when the reload input is pressed (trigger-once behavior).</summary>
        bool GetReloadPressed();

        /// <summary>Returns true once when the inventory input is pressed (trigger-once behavior).</summary>
        bool GetInventoryPressed();

        /// <summary>Returns true once when the interact input is pressed (trigger-once behavior).</summary>
        bool GetInteractPressed();

        /// <summary>Returns true once when the map input is pressed (trigger-once behavior).</summary>
        bool GetMapPressed();

        /// <summary>Returns true once when the previous input is pressed (trigger-once behavior).</summary>
        bool GetPreviousPressed();

        /// <summary>Returns true once when the next input is pressed (trigger-once behavior).</summary>
        bool GetNextPressed();

        /// <summary>Returns true once when the back input is pressed (trigger-once behavior).</summary>
        bool GetBackPressed();

        /// <summary>Returns true once when the pause input is pressed (trigger-once behavior).</summary>
        bool GetPausePressed();

        // Held input methods (return true while input is held down)
        /// <summary>Returns true while the fire input is held down (held behavior).</summary>
        bool GetFireHeld();

        /// <summary>Returns true while the jump input is held down (held behavior).</summary>
        bool GetJumpHeld();

        /// <summary>Returns true while the crouch input is held down (held behavior).</summary>
        bool GetCrouchHeld();

        /// <summary>Returns true while the sprint input is held down (held behavior).</summary>
        bool GetSprintHeld();

        /// <summary>Returns true while the reload input is held down (held behavior).</summary>
        bool GetReloadHeld();

        /// <summary>Returns true while the inventory input is held down (held behavior).</summary>
        bool GetInventoryHeld();

        /// <summary>Returns true while the interact input is held down (held behavior).</summary>
        bool GetInteractHeld();

        /// <summary>Returns true while the map input is held down (held behavior).</summary>
        bool GetMapHeld();

        /// <summary>Returns true while the previous input is held down (held behavior).</summary>
        bool GetPreviousHeld();

        /// <summary>Returns true while the next input is held down (held behavior).</summary>
        bool GetNextHeld();

        /// <summary>Returns true while the back input is held down (held behavior).</summary>
        bool GetBackHeld();

        /// <summary>Returns true while the pause input is held down (held behavior).</summary>
        bool GetPauseHeld();

        /// <summary>
        /// Gets the current value of an action (button, float, or vector) using the new Input System.
        /// </summary>
        /// <typeparam name="T">The type of input to return (bool, float, Vector2).</typeparam>
        /// <param name="actionName">The action name defined in Input Actions asset.</param>
        T GetKey<T>(string actionName) where T : struct;
    }
}