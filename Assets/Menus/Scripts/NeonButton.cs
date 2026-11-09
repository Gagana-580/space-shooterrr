using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SpaceShooterMenus
{
    /// <summary>
    /// Neon Arcade button: outlined when idle, filled with the accent colour + glow when selected
    /// (keyboard/gamepad selection or mouse hover). Works with Time.timeScale = 0.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class NeonButton : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler
    {
        [Header("Parts")]
        public Image fill;
        public Image border;
        public Image glow;
        public TMP_Text label;

        [Header("Colours")]
        public Color accent = new Color32(0x22, 0xD3, 0xEE, 0xFF);
        public Color idleFill = new Color32(0x07, 0x0A, 0x1A, 0x99);
        public Color idleBorder = new Color(0.133f, 0.827f, 0.933f, 0.55f);
        public Color idleText = new Color32(0xCF, 0xF7, 0xFF, 0xFF);
        public Color activeText = new Color32(0x04, 0x12, 0x1A, 0xFF);

        [Header("Text")]
        public TMP_FontAsset idleFont;
        public TMP_FontAsset activeFont;
        public float idleSize = 24f;
        public float activeSize = 26f;

        void OnEnable()
        {
            bool isSelected = EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject;
            Apply(isSelected);
        }

        public void OnSelect(BaseEventData eventData) => Apply(true);

        public void OnDeselect(BaseEventData eventData) => Apply(false);

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(gameObject);
        }

        public void Apply(bool on)
        {
            if (fill != null) fill.color = on ? accent : idleFill;
            if (border != null) border.color = on ? accent : idleBorder;
            if (glow != null) glow.enabled = on;
            if (label != null)
            {
                label.color = on ? activeText : idleText;
                TMP_FontAsset f = on ? activeFont : idleFont;
                if (f != null) label.font = f;
                label.fontSize = on ? activeSize : idleSize;
            }
        }
    }
}
