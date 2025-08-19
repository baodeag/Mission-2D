using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardSwipeTask : MiniGameBase
{
    [Header("Refs (auto-find nếu quên)")]
    [SerializeField] Canvas canvas;                 // auto-wire trong Awake
    [SerializeField] RectTransform trackRect;
    [SerializeField] RectTransform startZoneRect;
    [SerializeField] RectTransform endZoneRect;
    [SerializeField] RectTransform cardRect;
    [SerializeField] Button confirmButton;
    [SerializeField] Button cancelButton;
    [SerializeField] TextMeshProUGUI statusText;

    [Header("Config")]
    [SerializeField] float minTime = 0.6f;   // giây
    [SerializeField] float maxTime = 1.0f;   // giây
    [SerializeField] float verticalTolerance = 24f; // cho phép lệch dọc trong rãnh

    protected override void OnOpened()
    {
        if (!canvas) canvas = GetComponentInParent<Canvas>(true);
        if (!canvas) canvas = FindFirstObjectByType<Canvas>();

        if (!trackRect) trackRect = transform.Find("Track")?.GetComponent<RectTransform>();
        if (!startZoneRect) startZoneRect = transform.Find("Track/StartZone")?.GetComponent<RectTransform>();
        if (!endZoneRect) endZoneRect = transform.Find("Track/EndZone")?.GetComponent<RectTransform>();
        if (!cardRect) cardRect = transform.Find("Track/Card")?.GetComponent<RectTransform>();
        if (!confirmButton) confirmButton = transform.Find("Footer/BtnConfirm")?.GetComponent<Button>();
        if (!cancelButton) cancelButton = transform.Find("Footer/BtnCancel")?.GetComponent<Button>();
        if (!statusText) statusText = transform.Find("StatusText")?.GetComponent<TextMeshProUGUI>();

        if (confirmButton) confirmButton.interactable = false;
        if (statusText) statusText.text = $"Kéo thẻ từ trái sang phải trong {minTime:0.##}–{maxTime:0.##}s.";

        // Đặt thẻ về tâm StartZone
        PlaceCardToStart();

        // gắn owner/config cho SwipeCard
        var swipe = cardRect.GetComponent<SwipeCard>();
        if (!swipe) swipe = cardRect.gameObject.AddComponent<SwipeCard>();
        swipe.owner = this;
        swipe.canvas = canvas;
        swipe.trackRect = trackRect;
        swipe.startZoneRect = startZoneRect;
        swipe.endZoneRect = endZoneRect;
        swipe.verticalTolerance = verticalTolerance;
        swipe.minTime = minTime;
        swipe.maxTime = maxTime;
    }

    void PlaceCardToStart()
    {
        var cam = canvas ? canvas.worldCamera : null;
        var startWorld = startZoneRect.TransformPoint(new Vector3(startZoneRect.rect.width * 0.5f, 0f, 0f));
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(cam, startWorld);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(trackRect, screen, cam, out var local);
        cardRect.SetParent(trackRect, false);
        cardRect.anchoredPosition = local;
        cardRect.localRotation = Quaternion.identity;
        cardRect.localScale = Vector3.one;
    }

    public void OnSwipeFinished(bool success, float time, bool endInEndZone)
    {
        if (success)
        {
            if (statusText) statusText.text = $"✔ Tốc độ chuẩn ({time:0.00}s). Nhấn Confirm để hoàn thành.";
            if (confirmButton) confirmButton.interactable = true;
        }
        else
        {
            string reason = endInEndZone ?
                (time < minTime ? "quá nhanh" : (time > maxTime ? "quá chậm" : "tốc độ chưa chuẩn")) :
                "chưa kết thúc trong vùng đích";
            if (statusText) statusText.text = $"❌ {reason}. Hãy thử lại ({time:0.00}s).";
            if (confirmButton) confirmButton.interactable = false;
        }
    }

    public void OnClickConfirm() => Close(true);
    public void OnClickCancel() => Close(false);
}
