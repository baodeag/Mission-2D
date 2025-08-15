using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderTask : MiniGameBase
{
    [Header("UI")]
    [SerializeField] Slider slider;
    [SerializeField] TextMeshProUGUI label;
    [SerializeField] Button confirmButton;
    [SerializeField] TextMeshProUGUI hintText; // optional: báo “Đưa thanh vào vùng xanh…”

    [Header("Logic")]
    [Range(0f, 1f)][SerializeField] float target = 0.65f;
    [Range(0.01f, 0.5f)][SerializeField] float tolerance = 0.05f;

    bool inRange;

    protected override void OnOpened()
    {
        if (slider) slider.wholeNumbers = false;
        if (slider) slider.value = Random.Range(0f, 1f);
        UpdateUI();
    }

    public void OnSliderChanged(float _)
    {
        UpdateUI();
    }

    void UpdateUI()
    {
        float v = slider ? slider.value : 0f;
        inRange = Mathf.Abs(v - target) <= tolerance;

        if (label)
        {
            int pct = Mathf.RoundToInt(v * 100f);
            int tgt = Mathf.RoundToInt(target * 100f);
            int tol = Mathf.RoundToInt(tolerance * 100f);
            label.text = $"Value: {pct}%   •   Target: {tgt}% ± {tol}%";

            // Đổi màu chữ để feedback nhanh
            label.color = inRange ? new Color(0.1f, 0.7f, 0.2f) : new Color(0.9f, 0.3f, 0.2f);
        }

        if (confirmButton) confirmButton.interactable = inRange;
        if (hintText) hintText.text = inRange ? "✔ Trong vùng mục tiêu" : "Kéo thanh vào vùng mục tiêu để xác nhận";
    }

    public void OnConfirm()
    {
        if (!inRange) return;    // CHỐT: ngoài vùng thì không làm gì
        Close(true);             // chỉ đóng khi đạt điều kiện
    }

    public void OnCancel() => Close(false);
}
