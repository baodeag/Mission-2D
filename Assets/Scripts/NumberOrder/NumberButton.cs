using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NumberButton : MonoBehaviour
{
    [Header("Runtime")]
    public NumberOrderTask owner;      // gán khi spawn
    public int value;

    [Header("Refs")]
    [SerializeField] Image background; // Image trên Button (nền)
    [SerializeField] TextMeshProUGUI label;

    [Header("Colors")]
    [SerializeField] Color normalColor = new Color32(0x3A, 0x3A, 0x3A, 0xFF);
    [SerializeField] Color nextColor = new Color32(0xF5, 0xD4, 0x3D, 0xFF); // vàng
    [SerializeField] Color correctColor = new Color32(0x31, 0xB5, 0x52, 0xFF); // xanh lá
    [SerializeField] Color wrongColor = new Color32(0xE8, 0x52, 0x52, 0xFF); // đỏ

    bool isLocked;

    void Reset()
    {
        if (!background) background = GetComponent<Image>();
        if (!label) label = GetComponentInChildren<TextMeshProUGUI>(true);
    }

    public void Setup(int v)
    {
        value = v;
        if (label) label.text = v.ToString();
        SetNormal();
        isLocked = false;
    }

    public void OnClick()
    {
        if (isLocked) return;
        owner?.OnClickNumber(this);
    }

    public void SetNormal()
    {
        if (background) background.color = normalColor;
    }

    public void SetNext()
    {
        if (background) background.color = nextColor;
    }

    public void SetCorrect()
    {
        if (background) background.color = correctColor;
        isLocked = true; // đã đúng thì khoá lại
    }

    public void FlashWrong(float duration = 0.15f)
    {
        if (!background) return;
        background.color = wrongColor;
        CancelInvoke(nameof(SetNormal)); // tránh chồng
        Invoke(nameof(SetNormal), duration);
    }
}
