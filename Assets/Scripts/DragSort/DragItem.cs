using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("ID của mảnh này (khớp với Slot)")]
    public int id; // 0=Circle, 1=Square, 2=Triangle

    [Header("Tham chiếu")]
    public Canvas canvas;            // ★ KÉO Canvas (UI_Root) vào đây
    public RectTransform dragRoot;   // ★ KÉO MiniGame_DragSort vào đây (hoặc để trống => dùng chính canvas)

    RectTransform rt;
    CanvasGroup cg;
    Transform originalParent;
    Vector2 originalAnchoredPos;

    [HideInInspector] public DropSlot currentSlot;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        cg = gameObject.GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = rt.parent;
        originalAnchoredPos = rt.anchoredPosition;

        if (currentSlot != null)
        {
            currentSlot.occupied = false;
            currentSlot.isCorrect = false;
            currentSlot = null;
        }

        cg.blocksRaycasts = false; // cho slot nhận OnDrop
        transform.SetAsLastSibling();

        // ★ Đưa item lên một root ổn định để kéo (không bị layout/anchor khác can thiệp)
        var parentForDrag = (dragRoot ? dragRoot : canvas.transform) as RectTransform;
        rt.SetParent(parentForDrag, true);

        // Chuẩn hóa local transform khi bắt đầu kéo
        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;
    }

    public void OnDrag(PointerEventData eventData)
    {
        float scale = canvas ? canvas.scaleFactor : 1f;
        rt.anchoredPosition += eventData.delta / scale;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        cg.blocksRaycasts = true;

        // nếu không rơi vào slot nào, trả về vị trí cũ
        if (currentSlot == null)
        {
            rt.SetParent(originalParent, true);
            rt.anchoredPosition = originalAnchoredPos;
            rt.localScale = Vector3.one;
            rt.localRotation = Quaternion.identity;
        }
    }

    // được gọi bởi DropSlot khi chấp nhận mảnh này
    public void SetDroppedOnSlot(DropSlot slot)
    {
        currentSlot = slot;
        var parent = slot.transform as RectTransform;

        // Đưa về làm con của slot theo không gian local UI
        rt.SetParent(parent, false);

        // ★ Snap đúng tâm slot & reset mép
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;
    }
}
