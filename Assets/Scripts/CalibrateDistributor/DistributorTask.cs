using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DistributorTask : MiniGameBase
{
    [Header("Refs (auto-find nếu quên)")]
    [SerializeField] DialController dial1;
    [SerializeField] DialController dial2;
    [SerializeField] DialController dial3;

    [SerializeField] Button confirmButton;
    [SerializeField] Button cancelButton;
    [SerializeField] TextMeshProUGUI statusText;

    [Header("Config")]
    [Tooltip("Tốc độ 3 vòng (deg/s)")]
    [SerializeField] float speed1 = 90f;
    [SerializeField] float speed2 = 130f;
    [SerializeField] float speed3 = 170f;

    [SerializeField] float toleranceDeg = 10f;

    protected override void OnOpened()
    {
        // Auto-find
        if (!dial1) dial1 = transform.Find("DialsRow/Dial1")?.GetComponent<DialController>();
        if (!dial2) dial2 = transform.Find("DialsRow/Dial2")?.GetComponent<DialController>();
        if (!dial3) dial3 = transform.Find("DialsRow/Dial3")?.GetComponent<DialController>();
        if (!confirmButton) confirmButton = transform.Find("Footer/BtnConfirm")?.GetComponent<Button>();
        if (!cancelButton) cancelButton = transform.Find("Footer/BtnCancel")?.GetComponent<Button>();
        if (!statusText) statusText = transform.Find("StatusText")?.GetComponent<TextMeshProUGUI>();

        if (confirmButton) confirmButton.interactable = false;
        if (statusText) statusText.text = "Khóa từng vòng khi kim ở mốc 12 giờ.";

        // Cấu hình vòng
        SetupDial(dial1, speed1);
        SetupDial(dial2, speed2);
        SetupDial(dial3, speed3);

        // Gắn callback
        dial1.OnAttempt = OnDialAttempt;
        dial2.OnAttempt = OnDialAttempt;
        dial3.OnAttempt = OnDialAttempt;
    }

    void SetupDial(DialController d, float speed)
    {
        if (!d) return;
        d.Configure(speed, toleranceDeg);
        d.ForceUnlockAndReset();
    }

    void OnDialAttempt(DialController d, bool ok)
    {
        if (!ok)
        {
            if (statusText) statusText.text = "Chưa trúng! Đợi kim tới mốc 12 giờ rồi Lock lại.";
            return;
        }

        // Nếu cả 3 đã lock → bật Confirm
        if (dial1.IsLocked && dial2.IsLocked && dial3.IsLocked)
        {
            if (statusText) statusText.text = "✔ Đã khóa đủ 3 vòng! Nhấn Confirm để hoàn thành.";
            if (confirmButton) confirmButton.interactable = true;
        }
        else
        {
            // Gợi ý vòng tiếp theo (vòng chưa khóa)
            int remain = (dial1.IsLocked ? 0 : 1) + (dial2.IsLocked ? 0 : 1) + (dial3.IsLocked ? 0 : 1);
            if (statusText) statusText.text = $"Tốt! Còn {remain} vòng chưa khóa.";
        }
    }

    public void OnClickConfirm() => Close(true);
    public void OnClickCancel() => Close(false);
}
