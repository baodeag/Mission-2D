using UnityEngine;
using UnityEngine.EventSystems;

public class DropSlot : MonoBehaviour, IDropHandler
{
    [Header("ID mong đợi (khớp với DragItem.itemId)")]
    public string expectedId; // ví dụ: "circle", "square", ...

    public bool IsFilled => filledItem != null;
    public bool IsCorrectlyFilled { get; private set; }

    DragItem filledItem;
    SortShapesTask task;

    void Awake()
    {
        task = GetComponentInParent<SortShapesTask>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (IsFilled) return; // đơn giản: không cho đè lên

        var go = eventData.pointerDrag;
        if (!go) return;

        var item = go.GetComponent<DragItem>();
        if (!item) return;

        bool correct = (item.itemId == expectedId);

        if (correct)
        {
            filledItem = item;
            IsCorrectlyFilled = true;
            item.SnapToSlot(this);
            task?.NotifySlotChanged();
        }
        else
        {
            // Sai thì để item tự quay về trong OnEndDrag
            IsCorrectlyFilled = false;
        }
    }

    public void ClearSlot(bool silent = false)
    {
        if (filledItem)
        {
            // Không gọi ReturnToStart ở đây (để DragItem tự xử lý nếu cần)
            filledItem = null;
        }
        IsCorrectlyFilled = false;
        if (!silent) task?.NotifySlotChanged();
    }
}
