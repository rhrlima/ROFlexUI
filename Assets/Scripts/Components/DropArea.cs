using RO_Flex_UI.Components.DragAndDrop;
using UnityEngine;

public class DropArea : MonoBehaviour, IDragTarget
{
    public bool ConsumeSource = false;
    public bool CanDrop(DragPayload payload)
    {
        return payload != null && (payload is DragPayload<ItemData> || payload is DragPayload<SkillData>);
    }

    public void OnDropComplete(DragPayload payload)
    {
        var text = string.Empty;
        switch (payload)
        {
            case DragPayload<ItemData> itemPayload:
                var itemData = itemPayload.data;
                text = $"{itemData.name}: {itemData.amount} un.";
                break;
            case DragPayload<SkillData> skillPayload:
                var skillData = skillPayload.data;
                text = $"{skillData.name}: {skillData.currLevel}/{skillData.maxLevel}";
                break;
            default:
                text = payload.text;
                break;
        }
        Debug.Log($"[{name}] Dropped payload: {text}");

        if (ConsumeSource)
        {
            if (payload.source is DraggableItem source)
            {
                source.Clear();
            }
        }
    }
}
