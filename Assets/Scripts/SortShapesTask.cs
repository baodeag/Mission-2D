using UnityEngine;
using TMPro;

public class SortShapesTask : MiniGameBase
{
    [Header("UI Refs")]
    [SerializeField] RectTransform itemsRoot;  // gán Transform "Items"
    [SerializeField] RectTransform slotsRoot;  // gán Transform "Slots"
    [SerializeField] TextMeshProUGUI hintText; // optional
    [SerializeField] UnityEngine.UI.Button cancelButton;
    [SerializeField] UnityEngine.UI.Button resetButton; // optional

    DropSlot[] slots;

    protected override void OnOpened()
    {
        slots = slotsRoot.GetComponentsInChildren<DropSlot>(true);
        UpdateHint();
        if (resetButton) resetButton.onClick.AddListener(ResetAll);
        if (cancelButton) cancelButton.onClick.AddListener(OnCancel);
    }

    void OnDestroy()
    {
        if (resetButton) resetButton.onClick.RemoveListener(ResetAll);
        if (cancelButton) cancelButton.onClick.RemoveListener(OnCancel);
    }

    public void NotifySlotChanged()
    {
        // Kiểm tra hoàn thành: tất cả slot filled & correct
        foreach (var s in slots)
        {
            if (!s.IsCorrectlyFilled) { UpdateHint(); return; }
        }
        // Done
        Close(true);
    }

    void UpdateHint()
    {
        if (!hintText) return;
        int correct = 0, total = slots.Length;
        foreach (var s in slots) if (s.IsCorrectlyFilled) correct++;
        hintText.text = $"Đã đúng: {correct}/{total} – Kéo mọi hình vào đúng khay để hoàn thành.";
    }

    public void OnCancel() => Close(false);

    public void ResetAll()
    {
        // Trả tất cả item về ItemsRoot
        var items = itemsRoot.GetComponentsInChildren<DragItem>(true);
        foreach (var s in slots) s.ClearSlot(silent: true);
        foreach (var it in items) it.ReturnToStart();
        UpdateHint();
    }
}
