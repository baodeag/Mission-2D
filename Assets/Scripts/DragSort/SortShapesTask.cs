using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SortShapesTask : MiniGameBase
{
    [Header("UI")]
    [SerializeField] Button confirmButton;
    [SerializeField] Button cancelButton;
    [SerializeField] TextMeshProUGUI statusText;

    [Header("Slots cần kiểm tra")]
    [SerializeField] DropSlot[] slots;

    protected override void OnOpened()
    {
        if (confirmButton) confirmButton.interactable = false;
        if (statusText) statusText.text = "Kéo các hình vào đúng ô đích.";
    }

    public void OnAnySlotChanged()
    {
        bool allOccupied = true;
        bool allCorrect = true;

        foreach (var s in slots)
        {
            if (!s.occupied) { allOccupied = false; allCorrect = false; break; }
            if (!s.isCorrect) allCorrect = false;
        }

        if (!allOccupied)
        {
            if (statusText) statusText.text = "Chưa đủ mảnh. Hãy kéo hết vào các ô.";
            if (confirmButton) confirmButton.interactable = false;
            return;
        }

        if (allCorrect)
        {
            if (statusText) statusText.text = "✔ Tất cả chính xác! Nhấn Confirm để hoàn thành.";
            if (confirmButton) confirmButton.interactable = true;
        }
        else
        {
            if (statusText) statusText.text = "❌ Có mảnh sai ô. Kéo lại cho đúng.";
            if (confirmButton) confirmButton.interactable = false;
        }
    }

    public void OnClickConfirm() => Close(true);
    public void OnClickCancel() => Close(false);
}
