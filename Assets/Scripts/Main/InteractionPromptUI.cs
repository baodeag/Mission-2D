using UnityEngine;
using TMPro;

public class InteractionPromptUI : MonoBehaviour
{
    public Canvas canvas;
    public RectTransform root;
    public TextMeshProUGUI label;

    Transform follow;

    void Awake()
    {
        if (!canvas) canvas = GetComponentInParent<Canvas>(true);
        if (!root) root = transform as RectTransform;
        gameObject.SetActive(false);
    }

    public void Show(string text, Transform anchor)
    {
        label.text = text;
        follow = anchor;
        gameObject.SetActive(true);
        UpdatePosition();
    }

    public void Hide()
    {
        follow = null;
        gameObject.SetActive(false);
    }

    void LateUpdate() => UpdatePosition();

    void UpdatePosition()
    {
        if (!follow || !root) return;
        Vector3 screen = Camera.main.WorldToScreenPoint(follow.position + Vector3.up * 1.7f);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform, screen, canvas.worldCamera, out var local);
        root.anchoredPosition = local;
    }
}
