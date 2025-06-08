using System;
using UnityEngine;

namespace Ludo.CrossInput
{
    /// <summary>
    /// Represents an input event with timestamp and action information.
    /// </summary>
    [Serializable]
    public class InputEvent
    {
        public string actionName;
        public float timestamp;
        public bool isPressed;
        public Vector2 vector2Value;
        public float floatValue;
        public bool boolValue;

        public InputEvent()
        {
            Reset();
        }

        public InputEvent(string actionName, bool isPressed, float timestamp = 0)
        {
            this.actionName = actionName;
            this.isPressed = isPressed;
            this.timestamp = timestamp > 0 ? timestamp : Time.time;
            this.boolValue = isPressed;
        }

        public InputEvent(string actionName, Vector2 value, float timestamp = 0)
        {
            this.actionName = actionName;
            this.vector2Value = value;
            this.timestamp = timestamp > 0 ? timestamp : Time.time;
            this.isPressed = value != Vector2.zero;
        }

        public InputEvent(string actionName, float value, float timestamp = 0)
        {
            this.actionName = actionName;
            this.floatValue = value;
            this.timestamp = timestamp > 0 ? timestamp : Time.time;
            this.isPressed = value != 0;
        }

        /// <summary>
        /// Resets the event to default values for object pooling.
        /// </summary>
        public void Reset()
        {
            actionName = string.Empty;
            timestamp = 0;
            isPressed = false;
            vector2Value = Vector2.zero;
            floatValue = 0;
            boolValue = false;
        }

        /// <summary>
        /// Gets the value as the specified type.
        /// </summary>
        public T GetValue<T>() where T : struct
        {
            if (typeof(T) == typeof(bool))
                return (T)(object)boolValue;
            if (typeof(T) == typeof(float))
                return (T)(object)floatValue;
            if (typeof(T) == typeof(Vector2))
                return (T)(object)vector2Value;
            
            return default(T);
        }

        /// <summary>
        /// Sets the value from the specified type.
        /// </summary>
        public void SetValue<T>(T value) where T : struct
        {
            if (typeof(T) == typeof(bool))
                boolValue = (bool)(object)value;
            else if (typeof(T) == typeof(float))
                floatValue = (float)(object)value;
            else if (typeof(T) == typeof(Vector2))
                vector2Value = (Vector2)(object)value;
        }
    }
}
