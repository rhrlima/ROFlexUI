using UnityEngine;
using UnityEngine.EventSystems;

namespace RO_Flex_UI.Components.DragAndDrop
{
    public class DragTrigger : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        protected IDragSource source;
        private DraggableManager sessionManager;

        protected virtual void Start()
        {
            source = GetComponentInParent<IDragSource>();

            if (source == null)
                Debug.LogError($"[{name}] Could not find an IDragSource presenter.");
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (source == null) return;

            var manager = DraggableManager.Instance;
            if (manager != null && manager.BeginDragSession(source, eventData))
                sessionManager = manager;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (source == null) return;

            if (sessionManager != null)
                sessionManager.OnEndDragSession(source, eventData);
            sessionManager = null;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (source == null) return;

            if (sessionManager != null)
                sessionManager.OnDrag(source, eventData);
        }

    }
}
