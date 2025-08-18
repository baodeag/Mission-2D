using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class MemoryTask : MiniGameBase
{
    [Header("Refs (auto-find nếu quên)")]
    [SerializeField] RectTransform cardGrid;   // gán CardGrid
    [SerializeField] GameObject cardPrefab;    // gán CardPrefab (Button)
    [SerializeField] Button confirmButton;
    [SerializeField] Button cancelButton;
    [SerializeField] TextMeshProUGUI statusText;

    [Header("Sprites")]
    [SerializeField] Sprite[] faceSprites;     // 6 sprite cho 6 cặp (0..5)
    [SerializeField] Sprite backSprite;        // sprite mặt sau

    [Header("Config")]
    [SerializeField] int rows = 3;
    [SerializeField] int cols = 4;
    [SerializeField] float mismatchDelay = 0.6f;

    // Runtime
    List<CardItem> cards = new List<CardItem>();
    CardItem first, second;
    bool lockInput;
    int matchedPairs;
    int moves;

    void Awake()
    {
        if (!cardGrid)
        {
            var t = transform.Find("CardGrid");
            if (t) cardGrid = t as RectTransform;
        }
        if (!confirmButton) confirmButton = transform.Find("Footer/BtnConfirm")?.GetComponent<Button>();
        if (!cancelButton) cancelButton = transform.Find("Footer/BtnCancel")?.GetComponent<Button>();
        if (!statusText) statusText = transform.Find("StatusText")?.GetComponent<TextMeshProUGUI>();
    }

    protected override void OnOpened()
    {
        if (confirmButton) confirmButton.interactable = false;
        matchedPairs = 0;
        moves = 0;
        if (statusText) statusText.text = "Chọn 2 thẻ để lật";

        // Dọn cũ
        foreach (Transform c in cardGrid) Destroy(c.gameObject);
        cards.Clear();
        first = second = null;
        lockInput = false;

        // Tạo list ID: 0,0,1,1,...,5,5 (12 thẻ)
        int pairCount = rows * cols / 2; // = 6
        var ids = new List<int>(rows * cols);
        for (int i = 0; i < pairCount; i++) { ids.Add(i); ids.Add(i); }

        // Shuffle Fisher–Yates
        for (int i = ids.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (ids[i], ids[j]) = (ids[j], ids[i]);
        }

        // Instantiate 12 thẻ
        for (int k = 0; k < ids.Count; k++)
        {
            int id = ids[k];
            Sprite face = (faceSprites != null && id < faceSprites.Length) ? faceSprites[id] : null;

            var go = Instantiate(cardPrefab, cardGrid);
            var card = go.GetComponent<CardItem>();
            card.owner = this;
            card.Setup(id, face, backSprite);
            card.FlipDownImmediate(); // đảm bảo úp lúc đầu

            cards.Add(card);
        }
    }

    public void OnClickConfirm() => Close(true);
    public void OnClickCancel() => Close(false);

    public void OnCardClicked(CardItem card)
    {
        if (lockInput) return;
        if (card.isMatched || card.isFaceUp) return;

        card.FlipUpImmediate();

        if (first == null)
        {
            first = card;
            UpdateStatus("Chọn thêm 1 thẻ…");
            return;
        }

        // Đang lật tấm thứ 2
        second = card;
        moves++;

        if (first.id == second.id)
        {
            // Khớp
            first.MarkMatched();
            second.MarkMatched();
            matchedPairs++;

            first = second = null;

            if (matchedPairs >= (rows * cols) / 2)
            {
                UpdateStatus($"✔ Hoàn thành! (Moves: {moves})");
                if (confirmButton) confirmButton.interactable = true;
            }
            else
            {
                UpdateStatus($"Khớp! (Moves: {moves}). Tiếp tục…");
            }
        }
        else
        {
            // Sai → úp lại sau delay
            lockInput = true;
            UpdateStatus($"Sai cặp! (Moves: {moves}). Sẽ úp lại…");
            StartCoroutine(HidePairAfterDelay(first, second));
        }
    }

    IEnumerator HidePairAfterDelay(CardItem a, CardItem b)
    {
        yield return a.StartCoroutine(a.FlipMismatch(mismatchDelay));
        yield return b.StartCoroutine(b.FlipMismatch(0f));
        first = second = null;
        lockInput = false;
        UpdateStatus("Chọn 2 thẻ để lật");
    }

    void UpdateStatus(string s)
    {
        if (statusText) statusText.text = s;
    }
}
