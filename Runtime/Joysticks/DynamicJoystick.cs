using UnityEngine;
using UnityEngine.EventSystems;

namespace Ludo.CrossInput.Joysticks
{
    public class DynamicJoystick : Joystick
    {
        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            handle.anchoredPosition = Vector2.zero;
        }

        public override void OnDrag(PointerEventData eventData)
        {
            base.OnDrag(eventData);

            Vector2 position = RectTransformUtility.WorldToScreenPoint(cam, background.position);
            Vector2 moveDirection = eventData.position - position;
            float maxRadius = background.sizeDelta.x * 0.5f;

            if (moveDirection.magnitude > maxRadius) moveDirection = moveDirection.normalized * maxRadius;

            handle.anchoredPosition = moveDirection;
        }
    }
}