using System.Collections.Generic;
using RO_Flex_UI.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RO_Flex_UI.Components.DragAndDrop
{
    public class DraggableManager : Singleton<DraggableManager>
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private IconAmount proxy;
        private RectTransform proxyTransform;
        private IDragSource activeSource;
        private DragPayload payload;
        private bool dragging;
        private bool dropAttempted;
        private PointerEventData dragEvent;
        private readonly List<RaycastResult> raycastResults = new();
        public bool Dragging => dragging;

        public void Start()
        {
            if (canvas == null)
            {
                canvas = GetComponentInParent<Canvas>();
                if (canvas == null)
                    Debug.LogError($"[{name}] No Canvas found.");
            }

            proxyTransform = proxy.transform as RectTransform;

            foreach (var graphic in proxy.GetComponentsInChildren<Graphic>(true))
                graphic.raycastTarget = false;
        }

        public bool BeginDragSession(IDragSource source, PointerEventData eventData)
        {
            if (dragging) return false;
            if (payload != null) return false;
            if (source == null) return false;
            if (eventData.button != PointerEventData.InputButton.Left) return false;
            if (!source.CanDrag()) return false;

            payload = source.CreatePayload();

            if (payload == null)
            {
                Debug.LogError("Payload cannot be null.");
                return false;
            }

            activeSource = source;
            dragEvent = eventData;
            dropAttempted = false;
            dragging = true;

            proxy.Assign(payload.sprite, payload.text);
            proxy.SetActive(true);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                eventData.position,
                canvas.worldCamera,
                out var localPoint
            );

            proxyTransform.anchoredPosition = localPoint;

            return true;
        }

        public void OnEndDragSession(IDragSource source, PointerEventData eventData)
        {
            CancelDragSession(source);
        }

        public void CancelDragSession(IDragSource source)
        {
            if (!dragging) return;
            if (!ReferenceEquals(activeSource, source)) return;

            ClearDragSession();
        }

        private void ClearDragSession()
        {
            dragging = false;
            payload = null;
            activeSource = null;
            dragEvent = null;

            if (proxy != null)
            {
                proxy.Clear();
                proxy.SetActive(false);
            }
        }

        private void OnDisable()
        {
            if (dragging)
                ClearDragSession();
        }

        public void OnDrag(IDragSource source, PointerEventData eventData)
        {
            if (!dragging) return;
            if (!ReferenceEquals(activeSource, source)) return;

            MoveProxy(eventData.position);
        }

        public bool TryDrop(IDragSource targetSource, IDragTarget target)
        {
            if (target == null) return false;
            if (dropAttempted) return false;
            if (payload == null) return false;
            var currentPayload = payload;
            dropAttempted = true;
            try
            {
                if (targetSource != null && ReferenceEquals(currentPayload.source, targetSource)) return false;
                if (!target.CanDrop(currentPayload)) return false;

                target.OnDropComplete(currentPayload);
                currentPayload.source.OnDragComplete();
                return true;
            }
            catch
            {
                ClearDragSession();
                throw;
            }
        }

        private void LateUpdate()
        {
            if (!dragging) return;

            var position = (Vector2)Input.mousePosition;
            MoveProxy(position);
            if (Input.GetMouseButton(0)) return;

            // Run after EventSystem.Update so normal drop dispatch gets first chance.
            try
            {
                var eventSystem = EventSystem.current;
                if (!dropAttempted && eventSystem != null)
                {
                    dragEvent.position = position;
                    raycastResults.Clear();
                    eventSystem.RaycastAll(dragEvent, raycastResults);
                    foreach (var hit in raycastResults)
                    {
                        if (hit.gameObject == null) continue;
                        dragEvent.pointerCurrentRaycast = hit;
                        ExecuteEvents.ExecuteHierarchy(hit.gameObject, dragEvent, ExecuteEvents.dropHandler);
                        break;
                    }
                }
            }
            finally
            {
                raycastResults.Clear();
                ClearDragSession();
            }
        }

        private void MoveProxy(Vector2 position)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform, position, canvas.worldCamera, out var localPoint);
            proxyTransform.anchoredPosition = localPoint;
        }
    }
}
