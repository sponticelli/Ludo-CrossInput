using UnityEngine;
using UnityEngine.EventSystems;

namespace Ludo.CrossInput.Joysticks
{
    public class VariableJoystick : Joystick
    {
        private float moveThreshold = 1f;
        private Vector2 radius;

        protected override void Start()
        {
            base.Start();
            radius = background.sizeDelta * 0.5f;
        }

        public override void OnDrag(PointerEventData eventData)
        {
            Vector2 backgroundPosition = RectTransformUtility.WorldToScreenPoint(cam, background.position);
            Vector2 rawInput = (eventData.position - backgroundPosition) / (radius * canvas.scaleFactor);

            if (rawInput.magnitude > moveThreshold)
            {
                Vector2 offset = rawInput.normalized * (rawInput.magnitude - moveThreshold) * radius;
                background.anchoredPosition += offset;
            }

            base.OnDrag(eventData);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            background.anchoredPosition = Vector2.zero;
        }
    }
}