using UnityEngine;
using UnityEngine.UI;

public class SimonPad : MonoBehaviour
{
    [Header("Runtime")]
    public SimonTask owner;   // gán runtime
    public int id;            // 0..3

    [Header("Refs")]
    [SerializeField] Image img;

    [Header("Colors")]
    [SerializeField] Color baseColor = Color.gray;
    [SerializeField] Color activeColor = Color.white;

    void Reset()
    {
        if (!img) img = GetComponent<Image>();
    }

    public void Setup(int _id, Color baseCol, Color activeCol)
    {
        id = _id;
        baseColor = baseCol;
        activeColor = activeCol;
        SetActive(false);
    }

    public void SetActive(bool on)
    {
        if (img) img.color = on ? activeColor : baseColor;
    }

    public void Click()
    {
        owner?.OnPadPressed(this);
    }
}
