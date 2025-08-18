using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CardItem : MonoBehaviour
{
    [Header("Runtime")]
    public MemoryTask owner;         // gán khi Instantiate
    public int id;                   // ID cặp (0..5)

    [Header("Refs")]
    [SerializeField] Image front;    // hình mặt trước (hiển thị khi lật)
    [SerializeField] Image back;     // hình mặt sau (luôn có)
    [SerializeField] CanvasGroup cg; // khoá tương tác tạm thời

    [Header("State")]
    public bool isFaceUp;
    public bool isMatched;

    Sprite faceSprite;

    void Reset()
    {
        front = transform.Find("Front")?.GetComponent<Image>();
        back = GetComponent<Image>();
        cg = GetComponent<CanvasGroup>();
        if (!cg) cg = gameObject.AddComponent<CanvasGroup>();
    }

    public void Setup(int id, Sprite face, Sprite backSprite = null)
    {
        this.id = id;
        faceSprite = face;
        if (front) { front.sprite = faceSprite; front.color = new Color(1, 1, 1, 0f); } // ẩn
        if (back && backSprite) back.sprite = backSprite;

        isFaceUp = false;
        isMatched = false;
        SetInteractable(true);
    }

    public void OnClick()
    {
        if (isMatched || isFaceUp) return;
        owner?.OnCardClicked(this);
    }

    public void FlipUpImmediate()
    {
        isFaceUp = true;
        if (front) front.color = Color.white;
        if (back) back.color = new Color(1, 1, 1, 0f);
    }

    public void FlipDownImmediate()
    {
        isFaceUp = false;
        if (front) front.color = new Color(1, 1, 1, 0f);
        if (back) back.color = Color.white;
    }

    public IEnumerator FlipMismatch(float delay = 0.6f)
    {
        yield return new WaitForSecondsRealtime(delay);
        FlipDownImmediate();
    }

    public void MarkMatched()
    {
        isMatched = true;
        SetInteractable(false);
        // Optional: đổi tint để biết là matched
        if (front) front.color = new Color(0.8f, 1f, 0.8f);
    }

    public void SetInteractable(bool on)
    {
        if (!cg) return;
        cg.interactable = on;
        cg.blocksRaycasts = on;
        cg.alpha = on ? 1f : 0.9f;
    }
}
