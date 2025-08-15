using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("ID phải khớp với DropSlot.expectedId")]
    public string itemId;

    [Header("Refs (kéo trong Inspector)")]
    [SerializeField] Canvas canvas;        // Kéo Canvas UI_Root vào (bắt buộc)
    [SerializeField] Transform dragRoot;   // Kéo MiniGameLayer hoặc 1 Empty "DragLayer" ở trong Canvas

    RectTransform rect;
    CanvasGroup cg;

    // Lưu trạng thái ban đầu để trả về
    Transform startParent;
    int startSiblingIndex;
    Vector2 startAnchoredPos;

    public DropSlot currentSlot { get; private set; }

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        cg = GetComponent<CanvasGroup>();
        if (!cg) cg = gameObject.AddComponent<CanvasGroup>();
        if (!canvas) canvas = GetComponentInParent<Canvas>(); // dự phòng, nhưng nên gán tay
        if (!dragRoot && canvas) dragRoot = canvas.transform; // nếu chưa gán, dùng gốc Canvas
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // (QUAN TRỌNG) Đảm bảo Graphic của chính object này là Raycast Target
        var img = GetComponent<Graphic>();
        if (img && !img.raycastTarget) img.raycastTarget = true;

        startParent = rect.parent;
        startSiblingIndex = rect.GetSiblingIndex();
        startAnchoredPos = rect.anchoredPosition;

        // Nếu đang ở Slot, thả ra trước
        if (currentSlot) { currentSlot.ClearSlot(); currentSlot = null; }

        // Re-parent ra dragRoot để không bị LayoutGroup giữ chặt
        rect.SetParent(dragRoot, worldPositionStays: false);

        // Cho phép pointer xuyên qua vật thể đang kéo, để Slot nhận OnDrop
        cg.blocksRaycasts = false;

        // Đặt lên trên cùng
        rect.SetAsLastSibling();

        UpdatePosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdatePosition(eventData);
    }

    void UpdatePosition(PointerEventData eventData)
    {
        // Chuyển screenPoint -> localPoint trong dragRoot
        RectTransform parentRect = dragRoot as RectTransform;
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect, eventData.position, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, out localPoint))
        {
            rect.anchoredPosition = localPoint;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        cg.blocksRaycasts = true;

        // Nếu KHÔNG thả vào Slot hợp lệ → trả về chỗ cũ
        if (!currentSlot)
        {
            rect.SetParent(startParent, worldPositionStays: false);
            rect.SetSiblingIndex(startSiblingIndex);
            rect.anchoredPosition = startAnchoredPos;
        }
    }

    public void SnapToSlot(DropSlot slot)
    {
        currentSlot = slot;
        rect.SetParent(slot.transform, worldPositionStays: false);
        rect.anchoredPosition = Vector2.zero;
    }

    public void ReturnToStart()
    {
        currentSlot = null;
        rect.SetParent(startParent, worldPositionStays: false);
        rect.SetSiblingIndex(startSiblingIndex);
        rect.anchoredPosition = startAnchoredPos;
    }
}
