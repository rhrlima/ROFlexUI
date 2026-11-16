using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RO_Flex_UI.Components
{
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler
    {
        [SerializeField] private bool tooltipEnabled = true;
        [SerializeField] private TooltipBox tooltipComponent;
        [SerializeField] private string tooltipText;
        public bool Enabled
        {
            get => tooltipEnabled;
            set => tooltipEnabled = value;
        }
        public string TooltipText => tooltipText;
        public Action OnTrigger;

        private void OnDisable()
        {
            HideTooltip();
        }

        public void SetText(string text)
        {
            tooltipText = text;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnTrigger?.Invoke();

            if (!CanShowTooltip(eventData))
            {
                HideTooltip();
                return;
            }

            tooltipComponent.SetText(tooltipText);
            tooltipComponent.ShowTooltip(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            HideTooltip();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            HideTooltip();
        }

        private void HideTooltip()
        {
            if (tooltipComponent != null)
                tooltipComponent.HideTooltip(this);
        }

        private bool CanShowTooltip(PointerEventData eventData)
        {
            if (!tooltipEnabled || tooltipComponent == null)
                return false;

            return true;
        }
    }
}
