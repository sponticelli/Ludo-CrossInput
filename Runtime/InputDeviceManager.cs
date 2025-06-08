using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ludo.CrossInput
{
    /// <summary>
    /// Manages input device detection, connection, and disconnection events.
    /// Provides information about available input devices and their capabilities.
    /// </summary>
    public class InputDeviceManager : MonoBehaviour
    {
        [Header("Device Management Settings")]
        [Tooltip("Enable automatic device detection")]
        [SerializeField] private bool enableDeviceDetection = true;
        
        [Tooltip("Log device changes to console")]
        [SerializeField] private bool logDeviceChanges = true;

        // Events
        public event Action<InputDevice> OnDeviceAdded;
        public event Action<InputDevice> OnDeviceRemoved;
        public event Action<InputDevice> OnDeviceConfigurationChanged;
        public event Action<InputDevice> OnDeviceEnabled;
        public event Action<InputDevice> OnDeviceDisabled;

        // Properties
        public IReadOnlyList<InputDevice> ConnectedDevices => InputSystem.devices;
        public bool HasKeyboard => Keyboard.current != null;
        public bool HasMouse => Mouse.current != null;
        public bool HasGamepad => Gamepad.current != null;
        public bool HasTouchscreen => Touchscreen.current != null;

        private readonly Dictionary<InputDevice, DeviceInfo> deviceInfoCache = new Dictionary<InputDevice, DeviceInfo>();

        [Serializable]
        public class DeviceInfo
        {
            public string deviceName;
            public string deviceType;
            public bool isConnected;
            public bool isEnabled;
            public DateTime connectionTime;
            public string[] capabilities;

            public DeviceInfo(InputDevice device)
            {
                deviceName = device.displayName;
                deviceType = device.GetType().Name;
                isConnected = device.added;
                isEnabled = device.enabled;
                connectionTime = DateTime.Now;
                capabilities = GetDeviceCapabilities(device);
            }

            private string[] GetDeviceCapabilities(InputDevice device)
            {
                var caps = new List<string>();
                
                if (device is Keyboard) caps.Add("Keyboard Input");
                if (device is Mouse) caps.Add("Mouse Input");
                if (device is Gamepad) caps.Add("Gamepad Input");
                if (device is Touchscreen) caps.Add("Touch Input");
                if (device.children.Count > 0) caps.Add("Composite Device");
                
                return caps.ToArray();
            }
        }

        private void Awake()
        {
            if (enableDeviceDetection)
            {
                InitializeDeviceDetection();
            }
        }

        private void Start()
        {
            // Cache information about currently connected devices
            foreach (var device in InputSystem.devices)
            {
                CacheDeviceInfo(device);
            }
        }

        private void OnDestroy()
        {
            if (enableDeviceDetection)
            {
                InputSystem.onDeviceChange -= OnDeviceChange;
            }
        }

        private void InitializeDeviceDetection()
        {
            try
            {
                InputSystem.onDeviceChange += OnDeviceChange;
                
                if (logDeviceChanges)
                {
                    Debug.Log("InputDeviceManager: Device detection initialized");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error initializing device detection: {ex.Message}");
            }
        }

        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            try
            {
                switch (change)
                {
                    case InputDeviceChange.Added:
                        HandleDeviceAdded(device);
                        break;
                    case InputDeviceChange.Removed:
                        HandleDeviceRemoved(device);
                        break;
                    case InputDeviceChange.ConfigurationChanged:
                        HandleDeviceConfigurationChanged(device);
                        break;
                    case InputDeviceChange.Enabled:
                        HandleDeviceEnabled(device);
                        break;
                    case InputDeviceChange.Disabled:
                        HandleDeviceDisabled(device);
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error handling device change for {device?.displayName}: {ex.Message}");
            }
        }

        private void HandleDeviceAdded(InputDevice device)
        {
            CacheDeviceInfo(device);
            
            if (logDeviceChanges)
            {
                Debug.Log($"Device Added: {device.displayName} ({device.GetType().Name})");
            }
            
            OnDeviceAdded?.Invoke(device);
        }

        private void HandleDeviceRemoved(InputDevice device)
        {
            deviceInfoCache.Remove(device);
            
            if (logDeviceChanges)
            {
                Debug.Log($"Device Removed: {device.displayName} ({device.GetType().Name})");
            }
            
            OnDeviceRemoved?.Invoke(device);
        }

        private void HandleDeviceConfigurationChanged(InputDevice device)
        {
            UpdateDeviceInfo(device);
            
            if (logDeviceChanges)
            {
                Debug.Log($"Device Configuration Changed: {device.displayName}");
            }
            
            OnDeviceConfigurationChanged?.Invoke(device);
        }

        private void HandleDeviceEnabled(InputDevice device)
        {
            UpdateDeviceInfo(device);
            
            if (logDeviceChanges)
            {
                Debug.Log($"Device Enabled: {device.displayName}");
            }
            
            OnDeviceEnabled?.Invoke(device);
        }

        private void HandleDeviceDisabled(InputDevice device)
        {
            UpdateDeviceInfo(device);
            
            if (logDeviceChanges)
            {
                Debug.Log($"Device Disabled: {device.displayName}");
            }
            
            OnDeviceDisabled?.Invoke(device);
        }

        private void CacheDeviceInfo(InputDevice device)
        {
            if (device != null)
            {
                deviceInfoCache[device] = new DeviceInfo(device);
            }
        }

        private void UpdateDeviceInfo(InputDevice device)
        {
            if (device != null && deviceInfoCache.ContainsKey(device))
            {
                var info = deviceInfoCache[device];
                info.isConnected = device.added;
                info.isEnabled = device.enabled;
            }
        }

        /// <summary>
        /// Gets cached information about a specific device.
        /// </summary>
        public DeviceInfo GetDeviceInfo(InputDevice device)
        {
            return deviceInfoCache.TryGetValue(device, out var info) ? info : null;
        }

        /// <summary>
        /// Gets all cached device information.
        /// </summary>
        public Dictionary<InputDevice, DeviceInfo> GetAllDeviceInfo()
        {
            return new Dictionary<InputDevice, DeviceInfo>(deviceInfoCache);
        }

        /// <summary>
        /// Checks if a specific device type is available.
        /// </summary>
        public bool IsDeviceTypeAvailable<T>() where T : InputDevice
        {
            foreach (var device in InputSystem.devices)
            {
                if (device is T && device.enabled)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the primary device of a specific type.
        /// </summary>
        public T GetPrimaryDevice<T>() where T : InputDevice
        {
            foreach (var device in InputSystem.devices)
            {
                if (device is T && device.enabled)
                    return device as T;
            }
            return null;
        }

        /// <summary>
        /// Gets all devices of a specific type.
        /// </summary>
        public List<T> GetDevicesOfType<T>() where T : InputDevice
        {
            var devices = new List<T>();
            foreach (var device in InputSystem.devices)
            {
                if (device is T)
                    devices.Add(device as T);
            }
            return devices;
        }
    }
}
