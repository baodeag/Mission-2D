using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Gắn script này lên GameObject "Card" (UI Image/Button).
/// Bảo đảm Card là con trực tiếp của Track, và cả Track/Card đều có anchor & pivot = (0.5, 0.5).
/// </summary>
public class SwipeCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Refs")]
    public CardSwipeTask owner;             // gán runtime từ CardSwipeTask.OnOpened()
    public Canvas canvas;                   // auto-wire nếu để trống
    public RectTransform trackRect;         // panel rãnh quẹt
    public RectTransform startZoneRect;     // vùng xuất phát (bên trái)
    public RectTransform endZoneRect;       // vùng kết thúc (bên phải)

    [Header("Config")]
    public bool requireStartInStartZone = true; // true: bắt đầu kéo phải ở StartZone
    public float verticalTolerance = 24f;  // cho phép lệch trục dọc khi quẹt
    public float minTime = 0.6f; // thời gian tối thiểu (giây)
    public float maxTime = 1.0f; // thời gian tối đa (giây)

    // runtime
    RectTransform cardRect;
    bool dragging;
    float tStart;

    void Awake()
    {
        cardRect = GetComponent<RectTransform>();
        if (!canvas) canvas = GetComponentInParent<Canvas>(true);
        if (!canvas) canvas = FindFirstObjectByType<Canvas>();

        // Ép parent/anchor/pivot chuẩn để tránh lệch hệ trục
        if (trackRect && cardRect.parent != trackRect)
            cardRect.SetParent(trackRect, false);

        cardRect.anchorMin = cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        var cam = eventData.pressEventCamera;

        if (requireStartInStartZone)
        {
            bool inStart = RectTransformUtility.RectangleContainsScreenPoint(startZoneRect, eventData.position, cam);
            if (!inStart)
            {
                // Không cho bắt đầu ngoài StartZone
                dragging = false;
                return;
            }
        }

        dragging = true;
        tStart = Time.unscaledTime;

        // Đặt vị trí card theo chuột (local của track), giới hạn trong track và theo tolerance dọc
        Vector2 p = ScreenToTrackLocal(eventData.position, cam);
        p.y = Mathf.Clamp(p.y, -verticalTolerance, verticalTolerance);
        p = ClampToTrack(p);
        cardRect.anchoredPosition = p;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging) return;

        Vector2 p = ScreenToTrackLocal(eventData.position, eventData.pressEventCamera);
        p.y = Mathf.Clamp(p.y, -verticalTolerance, verticalTolerance);
        p = ClampToTrack(p);
        cardRect.anchoredPosition = p;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!dragging) return;
        dragging = false;

        float t = Time.unscaledTime - tStart;
        bool endInEndZone = RectTransformUtility.RectangleContainsScreenPoint(
            endZoneRect, eventData.position, eventData.pressEventCamera);

        bool timeOk = (t >= minTime && t <= maxTime);
        bool success = endInEndZone && timeOk;

        owner?.OnSwipeFinished(success, t, endInEndZone);

        // Trả card về tâm StartZone để thử lại/Confirm
        SnapToStartCenter();
    }

    // ===== Helpers =====

    Vector2 ScreenToTrackLocal(Vector2 screenPos, Camera cam)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(trackRect, screenPos, cam, out var local);
        return local;
    }

    Vector2 ClampToTrack(Vector2 local)
    {
        var r = trackRect.rect;
        float hw = cardRect.rect.width * 0.5f;
        float hh = cardRect.rect.height * 0.5f;

        local.x = Mathf.Clamp(local.x, r.xMin + hw, r.xMax - hw);
        local.y = Mathf.Clamp(local.y, r.yMin + hh, r.yMax - hh);
        return local;
    }

    void SnapToStartCenter()
    {
        // lấy tâm hình học của StartZone (theo chiều ngang)
        var cam = canvas ? canvas.worldCamera : null;
        var startWorld = startZoneRect.TransformPoint(new Vector3(startZoneRect.rect.width * 0.5f, 0f, 0f));

        Vector2 screen = RectTransformUtility.WorldToScreenPoint(cam, startWorld);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(trackRect, screen, cam, out var local);

        cardRect.SetParent(trackRect, false);
        cardRect.anchoredPosition = local;
        cardRect.localRotation = Quaternion.identity;
        cardRect.localScale = Vector3.one;
    }
}
