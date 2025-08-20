using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class NumberOrderTask : MiniGameBase
{
    [Header("Refs (auto-find nếu quên)")]
    [SerializeField] RectTransform grid;       // Grid (có GridLayoutGroup)
    [SerializeField] GameObject numberButtonPrefab; // Prefab nút số
    [SerializeField] Button confirmButton;
    [SerializeField] Button cancelButton;
    [SerializeField] TextMeshProUGUI statusText;

    [Header("Config")]
    [SerializeField] int totalNumbers = 10; // 1..10
    [SerializeField] bool reshuffleOnOpen = true; // xáo trộn mỗi lần mở

    int nextExpected; // số tiếp theo cần bấm
    readonly List<NumberButton> buttons = new();

    void Awake()
    {
        if (!grid)
        {
            var t = transform.Find("Grid");
            if (t) grid = t as RectTransform;
        }
        if (!confirmButton) confirmButton = transform.Find("Footer/BtnConfirm")?.GetComponent<Button>();
        if (!cancelButton) cancelButton = transform.Find("Footer/BtnCancel")?.GetComponent<Button>();
        if (!statusText) statusText = transform.Find("StatusText")?.GetComponent<TextMeshProUGUI>();
    }

    protected override void OnOpened()
    {
        if (confirmButton) confirmButton.interactable = false;
        BuildGrid();
        ResetProgress();
        UpdateStatus("Bắt đầu từ số 1");
    }

    void BuildGrid()
    {
        // xoá con cũ
        foreach (Transform c in grid) Destroy(c.gameObject);
        buttons.Clear();

        // tạo mảng số 1..N
        var values = new List<int>(totalNumbers);
        for (int i = 1; i <= totalNumbers; i++) values.Add(i);

        // xáo
        if (reshuffleOnOpen)
        {
            for (int i = values.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (values[i], values[j]) = (values[j], values[i]);
            }
        }

        // spawn
        for (int k = 0; k < values.Count; k++)
        {
            var go = Instantiate(numberButtonPrefab, grid);
            var nb = go.GetComponent<NumberButton>();
            nb.owner = this;
            nb.Setup(values[k]);

            // gắn sự kiện OnClick qua Button (nếu bạn không dùng UnityEvent)
            var btn = go.GetComponent<Button>();
            if (btn) btn.onClick.AddListener(nb.OnClick);

            buttons.Add(nb);
        }
    }

    void ResetProgress()
    {
        nextExpected = 1;
        foreach (var b in buttons) b.SetNormal();
        HighlightNext();
        if (confirmButton) confirmButton.interactable = false;
    }

    void HighlightNext()
    {
        foreach (var b in buttons)
        {
            if (b.value == nextExpected) b.SetNext();
        }
    }

    void UpdateStatus(string s)
    {
        if (statusText) statusText.text = s;
    }

    public void OnClickNumber(NumberButton btn)
    {
        if (btn.value != nextExpected)
        {
            // Sai → flash đỏ nút vừa bấm, reset tiến độ
            btn.FlashWrong();
            UpdateStatus("Sai số! Bắt đầu lại từ 1.");
            ResetProgress();
            return;
        }

        // Đúng
        btn.SetCorrect();
        nextExpected++;

        if (nextExpected > totalNumbers)
        {
            UpdateStatus("✔ Hoàn thành! Nhấn Confirm để kết thúc.");
            if (confirmButton) confirmButton.interactable = true;
        }
        else
        {
            UpdateStatus($"Tốt! Tiếp theo: {nextExpected}");
            HighlightNext();
        }
    }

    public void OnClickConfirm() => Close(true);
    public void OnClickCancel() => Close(false);
}
