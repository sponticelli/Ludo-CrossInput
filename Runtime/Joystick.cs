using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Ludo.CrossInput
{
    /// <summary>
    /// Enhanced joystick implementation with improved error handling and performance.
    /// </summary>
    public class Joystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Header("Joystick Settings")] 
        [Tooltip("Defines the axis of movement for the joystick.")] 
        [SerializeField]
        private Axis axis = Axis.Both;

        [Tooltip("Enables snapping of the joystick to predefined angles.")] 
        [SerializeField]
        private Snap snapping = Snap.None;

        [Tooltip("Determines how far the handle can move relative to the background.")]
        [SerializeField] [Range(0.1f, 1.0f)]
        private float flexibility = 1f;

        [Tooltip("Controls the smoothness of the joystick's movement.")] 
        [SerializeField] [Range(0.0f, 1.0f)]
        private float smoothness = 0.5f;

        [Space(25)] 
        [Tooltip("The background of the joystick.")] 
        [SerializeField]
        protected RectTransform background = null;

        [Tooltip("The handle of the joystick.")] 
        [SerializeField]
        protected RectTransform handle = null;

        public Vector2 Direction => snapping != Snap.None ? SnapVector(SmoothVector(input)) : SmoothVector(input);

        public float Flexibility
        {
            get => flexibility;
            set => flexibility = Mathf.Abs(value);
        }

        public Axis Axis
        {
            get => axis;
            set => axis = value;
        }

        public Snap Snapping
        {
            get => snapping;
            set => snapping = value;
        }

        public float Smoothness
        {
            get => smoothness;
            set => smoothness = value;
        }

        protected Canvas canvas;
        protected Camera cam;
        private Vector2 input = Vector2.zero;

        protected virtual void Start()
        {
            try
            {
                canvas = GetComponentInParent<Canvas>();
                if (canvas == null)
                {
                    Debug.LogError("The Joystick is not placed inside a canvas");
                    return;
                }

                ConfigureHandle();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error initializing joystick: {ex.Message}");
            }
        }

        private void ConfigureHandle()
        {
            try
            {
                if (background == null || handle == null)
                {
                    Debug.LogError("Background or handle RectTransform is not assigned");
                    return;
                }

                Vector2 center = new Vector2(0.5f, 0.5f);
                background.pivot = center;
                handle.anchorMin = center;
                handle.anchorMax = center;
                handle.pivot = center;
                handle.anchoredPosition = Vector2.zero;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error configuring joystick handle: {ex.Message}");
            }
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            try
            {
                OnDrag(eventData);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error handling pointer down: {ex.Message}");
            }
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            try
            {
                if (canvas == null || background == null || handle == null)
                    return;

                cam = canvas.renderMode == RenderMode.ScreenSpaceCamera ? canvas.worldCamera : null;

                Vector2 position = RectTransformUtility.WorldToScreenPoint(cam, background.position);
                Vector2 radius = background.sizeDelta / 2;

                input = (eventData.position - position) / (radius * canvas.scaleFactor);
                FormatInput();
                HandleInput(input.magnitude, input.normalized, radius);
                handle.anchoredPosition = input * radius * flexibility;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error handling drag: {ex.Message}");
            }
        }

        protected virtual void HandleInput(float magnitude, Vector2 normalized, Vector2 radius) =>
            input = magnitude > 1f ? normalized : input;

        private void FormatInput()
        {
            switch (axis)
            {
                case Axis.Horizontal:
                    input.y = 0f;
                    break;
                case Axis.Vertical:
                    input.x = 0f;
                    break;
            }
        }

        private Vector2 SnapVector(Vector2 value)
        {
            if (value == Vector2.zero) return value;

            float angle = Mathf.Atan2(value.y, value.x) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360;

            float[] snapAngles = snapping switch
            {
                Snap.Four => new float[] { 0, 90, 180, 270 },
                Snap.Eight => new float[] { 0, 45, 90, 135, 180, 225, 270, 315 },
                _ => new float[] { angle }
            };

            float closestAngle = snapAngles[0];
            float minDiff = Mathf.Abs(angle - closestAngle);

            for (int i = 1; i < snapAngles.Length; i++)
            {
                float diff = Mathf.Abs(angle - snapAngles[i]);
                if (diff < minDiff)
                {
                    minDiff = diff;
                    closestAngle = snapAngles[i];
                }
            }

            return AngleToVector2(closestAngle);
        }

        private Vector2 SmoothVector(Vector2 value)
        {
            if (value == Vector2.zero) return value;
            return Vector2.Lerp(value, snapping != Snap.None ? SnapVector(value) : value, 1 - smoothness);
        }

        private Vector2 AngleToVector2(float angle)
        {
            float rad = angle * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            try
            {
                input = Vector2.zero;
                if (handle != null)
                {
                    handle.anchoredPosition = Vector2.zero;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error handling pointer up: {ex.Message}");
            }
        }
    }
}