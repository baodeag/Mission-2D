using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

public class WiresTask : MiniGameBase
{
    [Header("Refs (để trống cũng được, sẽ auto-fill)")]
    [SerializeField] Canvas canvas;                 // KHÔNG kéo tay khi là Prefab
    [SerializeField] RectTransform linesContainer;  // Có thể kéo con "LinesContainer", nếu quên sẽ auto-find
    [SerializeField] GameObject wireLinePrefab;     // Prefab Image đường dây (cần gán sẵn trong prefab!)
    [SerializeField] Button confirmButton;
    [SerializeField] Button cancelButton;
    [SerializeField] TextMeshProUGUI statusText;

    [Header("Nodes")]
    [SerializeField] WireNode[] leftNodes;          // size = 4 (gán trong prefab)
    [SerializeField] WireNode[] rightNodes;         // size = 4 (gán trong prefab)

    [Header("Vẽ dây")]
    [SerializeField] float lineThickness = 6f;

    class Connection { public WireNode a, b; public RectTransform line; }
    Dictionary<WireNode, Connection> nodeConn = new();
    List<Connection> connections = new();

    WireNode dragStart;
    RectTransform tempLine;

    void Awake()
    {
        // ★ Auto-wire Canvas: khi prefab được Instantiate dưới Canvas trong scene
        if (!canvas) canvas = GetComponentInParent<Canvas>(true);
        if (!canvas) canvas = FindFirstObjectByType<Canvas>(); // fallback an toàn

        // ★ Auto-wire LinesContainer (nếu quên kéo)
        if (!linesContainer)
        {
            var t = transform.Find("LinesContainer");
            if (t) linesContainer = t as RectTransform;
        }

        // Cảnh báo nếu thiếu prefab line (cái này nên gán sẵn trong prefab)
        if (!wireLinePrefab)
            Debug.LogError("[WiresTask] Chưa gán wireLinePrefab trong prefab MiniGame_Wires!");
    }

    protected override void OnOpened()
    {
        if (confirmButton) confirmButton.interactable = false;
        if (statusText) statusText.text = "Kéo dây từ TRÁI sang PHẢI theo đúng màu/ID.";

        foreach (var n in leftNodes) { if (n) { n.owner = this; n.SetConnectedVisual(false); } }
        foreach (var n in rightNodes) { if (n) { n.owner = this; n.SetConnectedVisual(false); } }

        foreach (var c in connections) if (c.line) Destroy(c.line.gameObject);
        connections.Clear();
        nodeConn.Clear();
    }

    public void OnClickConfirm() => Close(true);
    public void OnClickCancel() => Close(false);

    // ====== Drag API từ WireNode ======
    public void BeginDrag(WireNode node, PointerEventData ev)
    {
        if (!node.isLeft) return;              // kéo từ bên trái
        RemoveConnection(node);                // nếu node đã có dây, gỡ trước
        dragStart = node;
        tempLine = CreateLine(node.dot ? node.dot.color : Color.white);
        var p = NodeCenterLocal(node);
        SetLine(tempLine, p, p);
    }

    public void UpdateDrag(WireNode node, PointerEventData ev)
    {
        if (!tempLine) return;
        var start = NodeCenterLocal(dragStart);
        var end = ScreenToLocal(ev.position, canvas ? canvas.worldCamera : null);
        SetLine(tempLine, start, end);
    }

    public void EndDrag(WireNode node, PointerEventData ev)
    {
        if (!tempLine) { dragStart = null; return; }

        var target = RaycastForNode(ev);
        bool ok = false;

        if (target && !target.isLeft && !HasConnection(target) && target.id == dragStart.id)
        {
            CreateConnection(dragStart, target, tempLine);
            ok = true;
        }

        if (!ok) Destroy(tempLine.gameObject);
        tempLine = null;
        dragStart = null;
        RecomputeState();
    }

    // ====== Kết nối ======
    void CreateConnection(WireNode a, WireNode b, RectTransform line)
    {
        var c = new Connection { a = a, b = b, line = line };
        nodeConn[a] = c; nodeConn[b] = c; connections.Add(c);
        var pA = NodeCenterLocal(a); var pB = NodeCenterLocal(b);
        SetLine(line, pA, pB);
        a.SetConnectedVisual(true); b.SetConnectedVisual(true);
    }

    void RemoveConnection(WireNode n)
    {
        if (!nodeConn.TryGetValue(n, out var c)) return;
        nodeConn.Remove(c.a); nodeConn.Remove(c.b); connections.Remove(c);
        if (c.line) Destroy(c.line.gameObject);
        c.a.SetConnectedVisual(false); c.b.SetConnectedVisual(false);
    }

    bool HasConnection(WireNode n) => nodeConn.ContainsKey(n);

    void RecomputeState()
    {
        if (connections.Count < leftNodes.Length)
        {
            if (statusText) statusText.text = "Chưa đủ cặp. Hãy nối tất cả dây.";
            if (confirmButton) confirmButton.interactable = false;
            return;
        }
        foreach (var c in connections)
        {
            if (c.a.id != c.b.id)
            {
                if (statusText) statusText.text = "Có dây sai cặp. Gỡ ra và nối lại.";
                if (confirmButton) confirmButton.interactable = false;
                return;
            }
        }
        if (statusText) statusText.text = "✔ Tất cả đúng! Nhấn Confirm để hoàn thành.";
        if (confirmButton) confirmButton.interactable = true;
    }

    // ====== Vẽ line UI ======
    RectTransform CreateLine(Color color)
    {
        var go = Instantiate(wireLinePrefab, linesContainer);
        var rt = go.transform as RectTransform;
        if (go.TryGetComponent<Image>(out var img)) img.color = color;
        var sz = rt.sizeDelta; rt.sizeDelta = new Vector2(sz.x, lineThickness);
        rt.pivot = new Vector2(0f, 0.5f);
        return rt;
    }

    void SetLine(RectTransform line, Vector2 startLocal, Vector2 endLocal)
    {
        Vector2 dir = endLocal - startLocal;
        float len = dir.magnitude;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        line.anchoredPosition = startLocal;
        line.sizeDelta = new Vector2(len, lineThickness);
        line.localRotation = Quaternion.Euler(0, 0, angle);
    }

    // ====== Toạ độ ======
    Vector2 NodeCenterLocal(WireNode node)
    {
        var cam = canvas ? canvas.worldCamera : null;
        var screen = RectTransformUtility.WorldToScreenPoint(cam, node.Rect.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(linesContainer, screen, cam, out var local);
        return local;
    }

    Vector2 ScreenToLocal(Vector2 screen, Camera cam)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(linesContainer, screen, cam, out var local);
        return local;
    }

    WireNode RaycastForNode(PointerEventData ev)
    {
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(ev, results);
        foreach (var r in results)
        {
            var node = r.gameObject.GetComponentInParent<WireNode>();
            if (node) return node;
        }
        return null;
    }
}
