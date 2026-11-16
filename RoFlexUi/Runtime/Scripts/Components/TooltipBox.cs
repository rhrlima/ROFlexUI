using RO_Flex_UI.Utils;
using TMPro;
using UnityEngine;

namespace RO_Flex_UI.Components
{
    public class TooltipBox : MonoBehaviour, IComponent
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private RectTransform background;
        [SerializeField, Min(0f)] private Vector2 maxSize = new(300f, 200f);
        private CanvasGroup canvasGroup;
        private TooltipTrigger activeTrigger;

        void Start()
        {
            if (!EnsureReferences()) return;

            HideTooltip();

            canvasGroup.blocksRaycasts = false;
            var rectTransform = transform as RectTransform;
            rectTransform.pivot = Vector2.zero;
        }

        public bool EnsureReferences()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                Tools.LogMissingReference(this, nameof(canvasGroup));
                return false;
            }

            if (text == null)
            {
                Tools.LogMissingReference(this, nameof(text));
                return false;
            }

            if (background == null)
            {
                Tools.LogMissingReference(this, nameof(background));
                return false;
            }

            return true;
        }

        void Update()
        {
            background.position = Input.mousePosition + Vector3.one;
        }

        public void SetText(string tooltipText)
        {
            text.text = tooltipText;
            RefreshSize();
        }

        internal void ShowTooltip(TooltipTrigger trigger)
        {
            ShowTooltip();
            activeTrigger = trigger;
        }

        internal void HideTooltip(TooltipTrigger trigger)
        {
            // Multiple triggers share this proxy; stale events must not hide its current owner.
            if (activeTrigger == trigger)
                HideTooltip();
        }

        private void OnDisable()
        {
            activeTrigger = null;
        }

        public void ShowTooltip()
        {
            activeTrigger = null;
            RefreshSize();
            gameObject.SetActive(true);
        }

        public void HideTooltip()
        {
            activeTrigger = null;
            gameObject.SetActive(false);
        }

        private void RefreshSize()
        {
            var preferred = text.GetPreferredValues(
                text.text,
                maxSize.x,
                Mathf.Infinity);

            background.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                Mathf.Min(preferred.x, maxSize.x));

            background.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Vertical,
                Mathf.Min(preferred.y, maxSize.y));
        }
    }
}