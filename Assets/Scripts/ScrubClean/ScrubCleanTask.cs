using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScrubCleanTask : MiniGameBase
{
    [Header("Refs (auto-find nếu quên)")]
    [SerializeField] Canvas canvas;
    [SerializeField] RectTransform areaRect;
    [SerializeField] Image dirtOverlay;
    [SerializeField] Slider progressSlider;
    [SerializeField] Button confirmButton;
    [SerializeField] Button cancelButton;
    [SerializeField] TextMeshProUGUI statusText;

    [Header("Config")]
    [Tooltip("Tổng quãng đường chuột (pixel UI) cần chà để đạt 100%")]
    [SerializeField] float targetDistance = 2000f;      // chỉnh tuỳ độ khó
    [SerializeField] float gainMultiplier = 1f;         // hệ số khuếch đại tiến độ
    [SerializeField] float maxDeltaPerEvent = 80f;      // chặn delta lớn bất thường

    float scrubAccum;    // tích luỹ quãng đường (đơn vị pixel local)
    float Progress01 => Mathf.Clamp01(scrubAccum / Mathf.Max(1f, targetDistance));

    void Awake()
    {
        if (!canvas) canvas = GetComponentInParent<Canvas>(true);
        if (!canvas) canvas = FindFirstObjectByType<Canvas>();
        if (!areaRect)
        {
            var t = transform.Find("Area");
            if (t) areaRect = t as RectTransform;
        }
        if (!dirtOverlay) dirtOverlay = transform.Find("Area/DirtOverlay")?.GetComponent<Image>();
        if (!progressSlider) progressSlider = transform.Find("Progress")?.GetComponent<Slider>();
        if (!confirmButton) confirmButton = transform.Find("Footer/BtnConfirm")?.GetComponent<Button>();
        if (!cancelButton) cancelButton = transform.Find("Footer/BtnCancel")?.GetComponent<Button>();
        if (!statusText) statusText = transform.Find("StatusText")?.GetComponent<TextMeshProUGUI>();
    }

    protected override void OnOpened()
    {
        scrubAccum = 0f;
        if (progressSlider)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = 1f;
            progressSlider.value = 0f;
        }
        if (confirmButton) confirmButton.interactable = false;
        if (statusText) statusText.text = "Nhấn giữ và chà trong vùng để làm sạch 100%.";

        // Gắn ScrubArea lên Area (nếu chưa có)
        var sa = areaRect.GetComponent<ScrubArea>();
        if (!sa) sa = areaRect.gameObject.AddComponent<ScrubArea>();
        sa.owner = this;
        sa.canvas = canvas;
        sa.maxDeltaPerEvent = Mathf.Max(10f, maxDeltaPerEvent);

        // Hiện overlay bẩn
        if (dirtOverlay)
        {
            var c = dirtOverlay.color;
            c.a = 0.86f; // ~220/255
            dirtOverlay.color = c;
        }
    }

    public void AddScrub(float localDistance)
    {
        // chặn delta quá lớn (giật khung)
        float d = Mathf.Min(localDistance, maxDeltaPerEvent) * Mathf.Max(0.01f, gainMultiplier);
        scrubAccum += d;

        float p = Progress01;
        if (progressSlider) progressSlider.value = p;

        if (dirtOverlay)
        {
            var c = dirtOverlay.color;
            c.a = Mathf.Lerp(0.86f, 0f, p);  // sạch dần
            dirtOverlay.color = c;
        }

        if (p >= 1f)
        {
            if (statusText) statusText.text = "✔ Đã sạch 100%! Nhấn Confirm để hoàn thành.";
            if (confirmButton) confirmButton.interactable = true;
        }
        else
        {
            if (statusText) statusText.text = $"Đang làm sạch… {(int)(p * 100f)}%";
        }
    }

    public void OnClickConfirm() => Close(true);
    public void OnClickCancel() => Close(false);
}
