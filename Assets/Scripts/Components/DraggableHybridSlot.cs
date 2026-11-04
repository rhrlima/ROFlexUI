using RO_Flex_UI.Components;
using RO_Flex_UI.Components.DragAndDrop;
using UnityEngine;

public class DraggableHybridSlot : IconAmount, IDragSource, IDragTarget
{
    [SerializeField] protected ScriptableObject Data;
    [SerializeField] protected bool canReplaceOnDrop = false;

    protected override void Start()
    {
        base.Start();

        if (IsSupported(Data))
        {
            Assign(Sprite, GetSecondaryValue(Data));
        }
    }

    public DragPayload CreatePayload()
    {
        return Data switch
        {
            ItemData item => new DragPayload<ItemData>(this, Sprite, GetPayloadText(item), item),
            SkillData skill => new DragPayload<SkillData>(this, Sprite, GetPayloadText(skill), skill),
            _ => throw new System.NotSupportedException($"Unsupported data type: {Data.GetType()}"),
        };

    }

    public bool CanDrag()
    {
        return IsSupported(Data);
    }

    public bool CanDrop(DragPayload payload)
    {
        return payload != null
            && IsSupported(payload)
            && (canReplaceOnDrop || Data == null);
    }

    public void OnDropComplete(DragPayload payload)
    {
        if (!IsSupported(payload))
        {
            return;
        }

        switch (payload)
        {
            case DragPayload<ItemData> itemPayload:
                Data = itemPayload.data;
                break;
            case DragPayload<SkillData> skillPayload:
                Data = skillPayload.data;
                break;
        }

        Assign(payload.sprite, GetSecondaryValue(Data));
        SetActive(true);
    }

    public void OnDragComplete()
    {
        Data = null;
        Clear();
    }

    private static bool IsSupported(ScriptableObject data)
    {
        return data is ItemData or SkillData;
    }

    private static bool IsSupported(DragPayload payload)
    {
        return payload is DragPayload<ItemData> or DragPayload<SkillData>;
    }

    private static string GetSecondaryValue(object data)
    {
        return data switch
        {
            ItemData item => item.amount.ToString(),
            SkillData skill => skill.currLevel.ToString(),
            _ => string.Empty
        };
    }

    private static string GetPayloadText(object data)
    {
        return data switch
        {
            ItemData item => $"{item.name}: {item.amount} un.",
            SkillData skill => $"{skill.name}: {skill.currLevel}/{skill.maxLevel}",
            _ => string.Empty
        };
    }
}
