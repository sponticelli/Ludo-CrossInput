using UnityEngine;
using UnityEngine.EventSystems;

namespace Ludo.CrossInput.Joysticks
{
    public class FixedJoystick : Joystick
    {
        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            handle.anchoredPosition = Vector2.zero;
        }
    }
}