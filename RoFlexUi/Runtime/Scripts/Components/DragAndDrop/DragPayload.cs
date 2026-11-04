using UnityEngine;

namespace RO_Flex_UI.Components.DragAndDrop
{
    public class DragPayload
    {
        public readonly IDragSource source;
        public readonly Sprite sprite;
        public readonly string text;

        public DragPayload(IDragSource source, Sprite sprite, string text)
        {
            this.source = source;
            this.sprite = sprite;
            this.text = text ?? string.Empty;
        }
    }

    public class DragPayload<T> : DragPayload
    {
        public readonly T data;

        public DragPayload(IDragSource source, Sprite sprite, string text, T data) : base(source, sprite, text)
        {
            this.data = data;
        }
    }
}