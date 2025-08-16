using UnityEngine;
using UnityEngine.EventSystems;

public class DropSlot : MonoBehaviour, IDropHandler
{
    [Header("ID slot này (khớp với Piece)")]
    public int id; // 0=Circle, 1=Square, 2=Triangle

    [Header("Trạng thái")]
    public bool occupied;
    public bool isCorrect;

    [Header("Thông báo về Task")]
    public SortShapesTask owner; // gán SortShapesTask (trên gốc prefab)

    public void OnDrop(PointerEventData eventData)
    {
        if (occupied) return;

        var go = eventData.pointerDrag;
        if (!go) return;

        var item = go.GetComponent<DragItem>();
        if (!item) return;

        // chấp nhận mọi mảnh, nhưng chỉ đúng khi id khớp
        occupied = true;
        isCorrect = (item.id == id);

        item.SetDroppedOnSlot(this);
        owner?.OnAnySlotChanged();
    }
}
