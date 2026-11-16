using RO_Flex_UI.Utils;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace RO_Flex_UI.Components
{
    [RequireComponent(typeof(Image))]
    public class RoButton : Button, IComponent
    {
        [System.Serializable]
        public class PointerClickEvent : UnityEvent<PointerEventData> { }

        [SerializeField] private PointerClickEvent pointerClick = new();
        public PointerClickEvent onPointerClick => pointerClick;

        [SerializeField] private RoText label;
        private Image sprite;

        protected override void Awake()
        {
            base.Awake();
            if (!EnsureReferences()) return;
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (!IsActive() || !IsInteractable()) return;

            base.OnPointerClick(eventData);

            // Ordinary activation may disable or destroy this button.
            if (this != null && IsActive() && IsInteractable())
                pointerClick.Invoke(eventData);
        }

        public virtual bool EnsureReferences()
        {
            sprite = GetComponent<Image>();

            if (!Tools.IsValid(this, sprite)) return false;

            return true;
        }

        public void Configure(Sprite sprite, string label = "")
        {
            Sprite = sprite;
            Label = label;
        }

        #region Getters & Setters
        public string Label
        {
            get
            {
                if (label != null) return label.text;
                return string.Empty;
            }
            set
            {
                if (label != null)
                    label.text = value;
                else
                    Debug.LogWarning("RoButton: Label reference is missing.");
            }
        }
        public Sprite Sprite
        {
            get
            {
                if (sprite != null) return sprite.sprite;
                return null;
            }
            set
            {
                if (sprite != null)
                    sprite.sprite = value;
            }
        }
        #endregion
    }
}