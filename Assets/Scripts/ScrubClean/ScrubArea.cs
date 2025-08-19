using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Bắt drag trong vùng Area, tính quãng đường chuột theo local of Area,
/// gọi AddScrub(distance) lên ScrubCleanTask.
/// </summary>
public class ScrubArea : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IPointerExitHandler
{
    [HideInInspector] public ScrubCleanTask owner;
    [HideInInspector] public Canvas canvas;
    public float maxDeltaPerEvent = 80f;

    RectTransform rt;
    bool pressed;
    Vector2 lastLocal;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        // đảm bảo object có Graphic để nhận raycast (Image…)
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pressed = true;
        lastLocal = ScreenToLocal(eventData.position, eventData.pressEventCamera);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!pressed) return;

        Vector2 local = ScreenToLocal(eventData.position, eventData.pressEventCamera);
        float dist = Vector2.Distance(local, lastLocal);
        if (dist > 0f)
        {
            dist = Mathf.Min(dist, maxDeltaPerEvent); // chặn delta bất thường
            owner?.AddScrub(dist);
            lastLocal = local;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pressed = false;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // giữ pressed để người chơi có thể chà sát mép; nếu muốn chặt hơn thì pressed=false;
    }

    Vector2 ScreenToLocal(Vector2 screen, Camera cam)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, screen, cam, out var local);
        return local;
    }
}
