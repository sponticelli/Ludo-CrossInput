namespace Ludo.CrossInput
{
    /// <summary>
    /// Constants for input action names to avoid hardcoded strings and enable refactoring.
    /// </summary>
    public static class InputActionNames
    {
        // Movement Actions
        public const string MOVE = "Move";
        public const string LEFT = "Left";
        public const string RIGHT = "Right";
        
        // Interaction Actions
        public const string PRESS = "Press";
        public const string POSITION = "Position";
        public const string INTERACT = "Interact";
        
        // Combat Actions
        public const string FIRE = "Fire";
        public const string RELOAD = "Reload";
        
        // Character Actions
        public const string JUMP = "Jump";
        public const string CROUCH = "Crouch";
        public const string SPRINT = "Sprint";
        
        // UI Actions
        public const string INVENTORY = "Inventory";
        public const string MAP = "Map";
        public const string PREVIOUS = "Previous";
        public const string NEXT = "Next";
        public const string PAUSE = "Pause";
        public const string BACK = "Back";
    }
}
