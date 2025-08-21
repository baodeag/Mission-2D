using UnityEngine;
using UnityEngine.UI;

public class ShieldCell : MonoBehaviour
{
    [Header("Runtime")]
    public PrimeShieldsTask owner;  // gán khi spawn
    public int index;               // 0..(rows*cols-1)

    [Header("Refs")]
    [SerializeField] Image background;   // Image trên Button
    [SerializeField] Image icon;         // (optional) hình phía trên

    [Header("Colors")]
    [SerializeField] Color offColor = new Color32(0x3A, 0x3A, 0x3A, 0xFF);
    [SerializeField] Color onColor = new Color32(0xF5, 0xD4, 0x3D, 0xFF); // vàng

    bool isOn;
    bool interactable = true;

    void Reset()
    {
        if (!background) background = GetComponent<Image>();
        if (!icon && transform.childCount > 0)
            icon = transform.GetChild(0).GetComponent<Image>();
    }

    public void Setup(PrimeShieldsTask _owner, int _index, bool startOn, bool canInteract)
    {
        owner = _owner;
        index = _index;
        interactable = canInteract;
        SetState(startOn, true);
    }

    public void OnClick()
    {
        if (!interactable) return;
        Toggle();
        owner?.OnPlayCellToggled(this);
    }

    public void Toggle() => SetState(!isOn);

    public void SetState(bool on, bool immediate = true)
    {
        isOn = on;
        var c = on ? onColor : offColor;
        if (background) background.color = c;
        if (icon) icon.enabled = on; // nếu có icon, chỉ bật khi ON
    }

    public bool IsOn() => isOn;

    public void SetInteractable(bool on)
    {
        interactable = on;
        var btn = GetComponent<Button>();
        if (btn) btn.interactable = on;
    }
}
