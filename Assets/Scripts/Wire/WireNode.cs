using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WireNode : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int id;
    public bool isLeft;
    public Image dot;
    public Color baseColor = Color.white;
    public Color connectedColor = new Color(0.1f, 0.8f, 0.2f);

    [HideInInspector] public WiresTask owner;
    public RectTransform Rect => (RectTransform)transform;

    void Start()
    {
        if (dot) dot.color = baseColor;
    }

    public void SetConnectedVisual(bool connected)
    {
        if (dot) dot.color = connected ? connectedColor : baseColor;
    }

    public void OnBeginDrag(PointerEventData eventData) => owner?.BeginDrag(this, eventData);
    public void OnDrag(PointerEventData eventData) => owner?.UpdateDrag(this, eventData);
    public void OnEndDrag(PointerEventData eventData) => owner?.EndDrag(this, eventData);
}
