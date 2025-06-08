using System.Collections.Generic;
using UnityEngine;

namespace Ludo.CrossInput
{
    /// <summary>
    /// Manages input buffering to allow for more forgiving input timing.
    /// </summary>
    public class InputBuffer
    {
        private readonly Dictionary<string, Queue<float>> _inputBuffer = new Dictionary<string, Queue<float>>();
        private readonly float _bufferTime;

        public InputBuffer(float bufferTime = 0.1f)
        {
            _bufferTime = bufferTime;
        }

        /// <summary>
        /// Buffers an input action with the current timestamp.
        /// </summary>
        /// <param name="actionName">The name of the input action</param>
        /// <param name="timestamp">The timestamp when the input occurred (defaults to current time)</param>
        public void BufferInput(string actionName, float timestamp = 0)
        {
            if (string.IsNullOrEmpty(actionName))
                return;

            if (timestamp <= 0)
                timestamp = Time.time;

            if (!_inputBuffer.ContainsKey(actionName))
                _inputBuffer[actionName] = new Queue<float>();

            _inputBuffer[actionName].Enqueue(timestamp);
            CleanOldInputs(actionName);
        }

        /// <summary>
        /// Attempts to consume a buffered input if it's within the buffer time window.
        /// </summary>
        /// <param name="actionName">The name of the input action</param>
        /// <param name="currentTime">The current time (defaults to Time.time)</param>
        /// <returns>True if a buffered input was consumed, false otherwise</returns>
        public bool ConsumeBufferedInput(string actionName, float currentTime = 0)
        {
            if (string.IsNullOrEmpty(actionName))
                return false;

            if (currentTime <= 0)
                currentTime = Time.time;

            if (!_inputBuffer.ContainsKey(actionName) || _inputBuffer[actionName].Count == 0)
                return false;

            var inputTime = _inputBuffer[actionName].Dequeue();
            return currentTime - inputTime <= _bufferTime;
        }

        /// <summary>
        /// Checks if there's a buffered input without consuming it.
        /// </summary>
        /// <param name="actionName">The name of the input action</param>
        /// <param name="currentTime">The current time (defaults to Time.time)</param>
        /// <returns>True if there's a valid buffered input, false otherwise</returns>
        public bool HasBufferedInput(string actionName, float currentTime = 0)
        {
            if (string.IsNullOrEmpty(actionName))
                return false;

            if (currentTime <= 0)
                currentTime = Time.time;

            if (!_inputBuffer.ContainsKey(actionName) || _inputBuffer[actionName].Count == 0)
                return false;

            var inputTime = _inputBuffer[actionName].Peek();
            return currentTime - inputTime <= _bufferTime;
        }

        /// <summary>
        /// Clears all buffered inputs for a specific action.
        /// </summary>
        /// <param name="actionName">The name of the input action</param>
        public void ClearBuffer(string actionName)
        {
            if (_inputBuffer.ContainsKey(actionName))
                _inputBuffer[actionName].Clear();
        }

        /// <summary>
        /// Clears all buffered inputs.
        /// </summary>
        public void ClearAllBuffers()
        {
            foreach (var buffer in _inputBuffer.Values)
                buffer.Clear();
        }

        /// <summary>
        /// Removes old inputs that are outside the buffer time window.
        /// </summary>
        private void CleanOldInputs(string actionName)
        {
            if (!_inputBuffer.ContainsKey(actionName))
                return;

            var currentTime = Time.time;
            var buffer = _inputBuffer[actionName];

            while (buffer.Count > 0 && currentTime - buffer.Peek() > _bufferTime)
            {
                buffer.Dequeue();
            }
        }

        /// <summary>
        /// Updates the buffer by cleaning old inputs for all actions.
        /// Should be called regularly (e.g., in Update).
        /// </summary>
        public void Update()
        {
            foreach (var actionName in _inputBuffer.Keys)
            {
                CleanOldInputs(actionName);
            }
        }
    }
}
