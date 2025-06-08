using UnityEngine;
using UnityEngine.EventSystems;

namespace Ludo.CrossInput.Joysticks
{
    public class FloatingJoystick : Joystick
    {
        public override void OnPointerDown(PointerEventData eventData)
        {
            background.gameObject.SetActive(true);
            background.position = eventData.position;
            handle.anchoredPosition = Vector2.zero;
            base.OnPointerDown(eventData);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            background.gameObject.SetActive(false);
            base.OnPointerUp(eventData);
        }

        protected override void Start()
        {
            base.Start();
            background.gameObject.SetActive(false);
        }
    }
}