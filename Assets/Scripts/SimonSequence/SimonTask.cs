using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class SimonTask : MiniGameBase
{
    [Header("Refs (auto-find nếu quên)")]
    [SerializeField] Button confirmButton;
    [SerializeField] Button cancelButton;
    [SerializeField] TextMeshProUGUI statusText;

    [SerializeField] SimonPad padRed;
    [SerializeField] SimonPad padGreen;
    [SerializeField] SimonPad padBlue;
    [SerializeField] SimonPad padYellow;

    [Header("Config")]
    [Tooltip("Số vòng cần hoàn thành (dãy tăng dần từ 1..roundTarget)")]
    [SerializeField] int roundTarget = 5;
    [SerializeField] float flashTime = 0.35f;   // thời gian 1 pad sáng
    [SerializeField] float gapTime = 0.22f;   // nghỉ giữa 2 pad khi phát dãy
    [SerializeField] float beforePlayDelay = 0.5f; // nghỉ 0.5s trước khi phát dãy

    // runtime
    List<int> sequence;
    SimonPad[] pads;
    bool playingBack;   // đang phát dãy (khóa input)
    int currentRound;   // độ dài dãy hiện tại (1..roundTarget)
    int inputIndex;     // người chơi đang nhập vị trí thứ mấy trong vòng hiện tại

    void Awake()
    {
        // auto-find nếu chưa gán
        if (!statusText) statusText = transform.Find("StatusText")?.GetComponent<TextMeshProUGUI>();
        if (!confirmButton) confirmButton = transform.Find("Footer/BtnConfirm")?.GetComponent<Button>();
        if (!cancelButton) cancelButton = transform.Find("Footer/BtnCancel")?.GetComponent<Button>();

        // auto-find pads theo tên
        if (!padRed) padRed = transform.Find("PadGrid/Pad_Red")?.GetComponent<SimonPad>();
        if (!padGreen) padGreen = transform.Find("PadGrid/Pad_Green")?.GetComponent<SimonPad>();
        if (!padBlue) padBlue = transform.Find("PadGrid/Pad_Blue")?.GetComponent<SimonPad>();
        if (!padYellow) padYellow = transform.Find("PadGrid/Pad_Yellow")?.GetComponent<SimonPad>();
    }

    protected override void OnOpened()
    {
        if (confirmButton) confirmButton.interactable = false;

        // setup pad ids + màu (đọc từ Image hiện có)
        pads = new SimonPad[4] { padRed, padGreen, padBlue, padYellow };
        for (int i = 0; i < pads.Length; i++)
        {
            var img = pads[i].GetComponent<Image>();
            Color baseCol = img ? img.color : Color.gray;
            // active sáng hơn 35%
            Color activeCol = Color.Lerp(baseCol, Color.white, 0.35f);
            pads[i].owner = this;
            pads[i].Setup(i, baseCol, activeCol);

            // nối sự kiện button
            var btn = pads[i].GetComponent<Button>();
            if (btn)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(pads[i].Click);
            }
        }

        // tạo dãy ngẫu nhiên đủ dài
        sequence = new List<int>(roundTarget);
        for (int i = 0; i < roundTarget; i++)
            sequence.Add(Random.Range(0, pads.Length));

        currentRound = 1;
        StartCoroutine(PlayCurrentRound());
    }

    public void OnClickConfirm() => Close(true);
    public void OnClickCancel() => Close(false);

    IEnumerator PlayCurrentRound()
    {
        playingBack = true;
        inputIndex = 0;

        if (statusText) statusText.text = $"Vòng {currentRound}/{roundTarget}: hãy xem dãy…";
        yield return new WaitForSecondsRealtime(beforePlayDelay);

        for (int i = 0; i < currentRound; i++)
        {
            int id = sequence[i];
            pads[id].SetActive(true);
            yield return new WaitForSecondsRealtime(flashTime);
            pads[id].SetActive(false);
            yield return new WaitForSecondsRealtime(gapTime);
        }

        playingBack = false;
        if (statusText) statusText.text = $"Nhập lại dãy ({currentRound} bước)…";
    }

    public void OnPadPressed(SimonPad pad)
    {
        if (playingBack) return; // đang phát dãy => khóa input

        int expected = sequence[inputIndex];
        if (pad.id != expected)
        {
            // Sai → báo lỗi, phát lại dãy của vòng hiện tại
            StartCoroutine(HandleWrongInput());
            return;
        }

        // Đúng bước này: chớp nhanh để feedback
        StartCoroutine(FlashQuick(pad));

        inputIndex++;
        if (inputIndex >= currentRound)
        {
            // Hoàn thành vòng hiện tại
            if (currentRound >= roundTarget)
            {
                if (statusText) statusText.text = "✔ Hoàn thành tất cả vòng! Nhấn Confirm để kết thúc.";
                if (confirmButton) confirmButton.interactable = true;
            }
            else
            {
                currentRound++;
                StartCoroutine(PlayCurrentRound());
            }
        }
    }

    IEnumerator HandleWrongInput()
    {
        playingBack = true;
        if (statusText) statusText.text = "❌ Sai! Sẽ phát lại dãy…";
        // nhấp nháy tất cả pad một nhịp cho dễ thấy
        foreach (var p in pads) p.SetActive(true);
        yield return new WaitForSecondsRealtime(0.18f);
        foreach (var p in pads) p.SetActive(false);

        yield return new WaitForSecondsRealtime(0.35f);
        // phát lại cùng vòng, reset chỉ số nhập
        inputIndex = 0;
        playingBack = false;
        StartCoroutine(PlayCurrentRound());
    }

    IEnumerator FlashQuick(SimonPad pad)
    {
        pad.SetActive(true);
        yield return new WaitForSecondsRealtime(0.12f);
        pad.SetActive(false);
    }
}
