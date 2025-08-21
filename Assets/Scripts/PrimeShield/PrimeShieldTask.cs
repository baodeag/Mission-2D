using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PrimeShieldsTask : MiniGameBase
{
    [Header("Refs (auto-find nếu quên)")]
    [SerializeField] RectTransform targetGrid;    // Columns/TargetBox/TargetGrid
    [SerializeField] RectTransform playGrid;      // Columns/PlayBox/PlayGrid
    [SerializeField] GameObject shieldCellPrefab; // Prefab button
    [SerializeField] Button confirmButton;
    [SerializeField] Button cancelButton;
    [SerializeField] TextMeshProUGUI statusText;

    [Header("Config")]
    [SerializeField] int rows = 3;
    [SerializeField] int cols = 3;                  // 3x3 = 9 (đổi 4x3 nếu thích)
    [SerializeField, Range(0.1f, 0.9f)]
    float fillProbability = 0.45f;                  // xác suất ON trong mẫu
    [SerializeField] bool reshuffleOnOpen = true;   // random mỗi lần mở
    [SerializeField] Vector2 spacing = new Vector2(10f, 10f);

    // Runtime
    readonly List<ShieldCell> targetCells = new();
    readonly List<ShieldCell> playCells = new();

    protected override void OnOpened()
    {
        // Auto-find theo tên nếu trống
        if (!targetGrid) targetGrid = transform.Find("Columns/TargetBox/TargetGrid")?.GetComponent<RectTransform>();
        if (!playGrid) playGrid = transform.Find("Columns/PlayBox/PlayGrid")?.GetComponent<RectTransform>();
        if (!confirmButton) confirmButton = transform.Find("Footer/BtnConfirm")?.GetComponent<Button>();
        if (!cancelButton) cancelButton = transform.Find("Footer/BtnCancel")?.GetComponent<Button>();
        if (!statusText) statusText = transform.Find("StatusText")?.GetComponent<TextMeshProUGUI>();

        // Kiểm tra prefab
        if (!shieldCellPrefab)
        {
            Debug.LogError("[PrimeShields] Chưa gán ShieldCellPrefab!");
            return;
        }

        if (confirmButton) confirmButton.interactable = false;

        // BẮT BUỘC: set up GridLayoutGroup chuẩn cho cả 2 lưới
        EnsureGrid(targetGrid, rows, cols, spacing);
        EnsureGrid(playGrid, rows, cols, spacing);

        // Xây grid & mẫu
        BuildGrids();
        GenerateTargetPattern();

        // Rebuild layout để tránh “dồn góc trái”
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(targetGrid);
        LayoutRebuilder.ForceRebuildLayoutImmediate(playGrid);

        // Hiển thị trạng thái (hiện “Bật đúng” để khỏi hiểu lầm)
        SyncStatus();
    }

    // === GRID SETUP ===
    void EnsureGrid(RectTransform gridRT, int r, int c, Vector2 sp)
    {
        if (!gridRT)
        {
            Debug.LogError("[PrimeShields] Thiếu RectTransform grid!");
            return;
        }

        // Đưa anchor/pivot về giữa để tính kích thước ổn định
        gridRT.anchorMin = gridRT.anchorMax = new Vector2(0.5f, 0.5f);
        gridRT.pivot = new Vector2(0.5f, 0.5f);

        var gl = gridRT.GetComponent<GridLayoutGroup>();
        if (!gl) gl = gridRT.gameObject.AddComponent<GridLayoutGroup>();

        gl.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gl.startAxis = GridLayoutGroup.Axis.Horizontal;
        gl.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gl.constraintCount = c;
        gl.spacing = sp;
        gl.childAlignment = TextAnchor.MiddleCenter;

        // Tính cellSize để chắc chắn fit r x c vào rect của grid
        // (Nếu bạn muốn cố định cellSize, hãy set trực tiếp và bỏ đoạn tính này.)
        var rect = gridRT.rect;
        float w = Mathf.Max(0f, rect.width - sp.x * (c - 1));
        float h = Mathf.Max(0f, rect.height - sp.y * (r - 1));
        float cellW = Mathf.Floor(w / c);
        float cellH = Mathf.Floor(h / r);
        gl.cellSize = new Vector2(cellW, cellH);

        // Cảnh báo nếu grid quá nhỏ
        if (rect.width < (gl.cellSize.x * c + sp.x * (c - 1)) - 1f ||
            rect.height < (gl.cellSize.y * r + sp.y * (r - 1)) - 1f)
        {
            Debug.LogWarning($"[PrimeShields] Grid {gridRT.name} hơi nhỏ ({rect.size}), cell {gl.cellSize}, rows={r}, cols={c}.");
        }
    }

    // === BUILD ===
    void BuildGrids()
    {
        // Xoá con cũ
        foreach (Transform c in targetGrid) Destroy(c.gameObject);
        foreach (Transform c in playGrid) Destroy(c.gameObject);
        targetCells.Clear(); playCells.Clear();

        int total = rows * cols;

        for (int i = 0; i < total; i++)
        {
            // Target cell (không tương tác)
            var tgo = Instantiate(shieldCellPrefab, targetGrid);
            var tcell = tgo.GetComponent<ShieldCell>();
            tcell.Setup(this, i, false, false);
            tcell.SetInteractable(false); // tắt Button trên mẫu
            targetCells.Add(tcell);

            // Play cell (có tương tác)
            var pgo = Instantiate(shieldCellPrefab, playGrid);
            var pcell = pgo.GetComponent<ShieldCell>();
            pcell.Setup(this, i, false, true);
            playCells.Add(pcell);

            // Bảo đảm OnClick chỉ trỏ về chính pcell
            var btn = pgo.GetComponent<Button>();
            if (btn)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(pcell.OnClick);
            }
        }
    }

    // === RANDOM MẪU ===
    void GenerateTargetPattern()
    {
        int total = rows * cols;
        var rnd = new System.Random(reshuffleOnOpen ? System.Environment.TickCount : 12345);

        int onCount = 0;
        for (int i = 0; i < total; i++)
        {
            bool on = rnd.NextDouble() < fillProbability;
            targetCells[i].SetState(on);
            if (on) onCount++;
        }

        // Tránh all-OFF hoặc all-ON (nhàm chán)
        if (onCount == 0) targetCells[rnd.Next(0, total)].SetState(true);
        else if (onCount == total) targetCells[rnd.Next(0, total)].SetState(false);

        // Reset play về OFF
        foreach (var c in playCells) c.SetState(false);
    }

    // === TƯƠNG TÁC ===
    public void OnPlayCellToggled(ShieldCell cell)
    {
        SyncStatus();
    }

    void SyncStatus()
    {
        int total = rows * cols;
        int matchAll = 0;
        int totalOn = 0;
        int matchOn = 0;

        for (int i = 0; i < total; i++)
        {
            bool t = targetCells[i].IsOn();
            bool p = playCells[i].IsOn();

            if (t) totalOn++;
            if (t && p) matchOn++;
            if (t == p) matchAll++;
        }

        bool done = (matchAll == total);

        // Hiển thị 2 chỉ số để dễ hiểu:
        // - Bật đúng: số ô BẬT trùng (bắt đầu từ 0)
        // - Khớp toàn bộ: tính cả ô tắt (chỉ để check hoàn thành)
        if (!done)
        {
            if (statusText)
                statusText.text = $"Bật đúng: {matchOn}/{totalOn} • Khớp toàn bộ: {matchAll}/{total}";
        }
        else
        {
            if (statusText)
                statusText.text = "✔ Đã khớp 100%! Nhấn Confirm để hoàn thành.";
        }

        if (confirmButton) confirmButton.interactable = done;
    }

    public void OnClickConfirm() => Close(true);
    public void OnClickCancel() => Close(false);
}
