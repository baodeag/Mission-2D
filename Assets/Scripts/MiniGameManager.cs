using UnityEngine;
using System;

public class MiniGameManager : MonoBehaviour
{
    // === Singleton ===
    public static MiniGameManager Instance { get; private set; }
    // Alias cho code cũ hay dùng I/Exists
    public static MiniGameManager I => Instance;
    public static bool Exists => Instance != null;

    [Header("Refs (kéo trong Canvas/UI)")]
    [SerializeField] GameObject menu;                 // UI menu (tuỳ chọn, có thể để trống)
    [SerializeField] GameObject miniGameLayer;        // Panel mờ full-screen (inactive mặc định)
    [SerializeField] RectTransform miniGameParent;    // Nơi chứa prefab mini-game (RectTransform stretch full)

    [Header("Tuỳ chọn hành vi")]
    [SerializeField] bool hideMenuWhileOpen = true; // ẩn menu khi mở mini-game
    [SerializeField] bool setLayerLastSibling = true; // đưa layer lên trên cùng
    [SerializeField] bool allowEscToClose = true; // cho phép ESC để đóng mini-game
    [SerializeField] bool pauseTimeWhileOpen = false; // dừng Time.timeScale khi đang mở

    // Runtime
    MiniGameBase current;
    Action<bool> onFinishedOnce;

    // Lưu trạng thái để khôi phục
    CursorLockMode prevCursorLock;
    bool prevCursorVisible;
    float prevTimeScale = 1f;

    public bool IsOpen => current != null;
    public bool IsBusy => current != null;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (miniGameLayer) miniGameLayer.SetActive(false);
    }

    /// <summary>
    /// Mở mini-game với callback (tuỳ chọn).
    /// </summary>
    public void Open(MiniGameBase prefab, Action<bool> onFinished = null)
    {
        if (!prefab) { Debug.LogWarning("[MiniGameManager] Prefab null."); return; }
        if (current) { Debug.Log("[MiniGameManager] Đã có mini-game đang mở."); return; }
        if (!miniGameLayer || !miniGameParent)
            Debug.LogWarning("[MiniGameManager] Chưa gán miniGameLayer/miniGameParent.");

        if (setLayerLastSibling && miniGameLayer)
            miniGameLayer.transform.SetAsLastSibling();

        if (hideMenuWhileOpen && menu)
            menu.SetActive(false);

        if (miniGameLayer)
            miniGameLayer.SetActive(true);

        // Lưu & set trạng thái
        prevCursorLock = Cursor.lockState;
        prevCursorVisible = Cursor.visible;
        prevTimeScale = Time.timeScale;

        if (pauseTimeWhileOpen) Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Tạo mini-game trong UI
        current = Instantiate(prefab, miniGameParent);

        // Đảm bảo RectTransform fill parent
        if (current.transform is RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.localScale = Vector3.one;
            rt.localRotation = Quaternion.identity;
        }

        onFinishedOnce = onFinished;
        current.OnFinished += HandleFinished;
        current.Open();
    }

    /// <summary>
    /// Giữ API cũ cho tiện.
    /// </summary>
    public void OpenMiniGame(MiniGameBase prefab) => Open(prefab, null);

    void HandleFinished(bool ok)
    {
        if (current)
        {
            current.OnFinished -= HandleFinished;
            Destroy(current.gameObject);
            current = null;
        }

        if (miniGameLayer) miniGameLayer.SetActive(false);
        if (hideMenuWhileOpen && menu) menu.SetActive(true);

        // Khôi phục trạng thái
        if (pauseTimeWhileOpen) Time.timeScale = prevTimeScale;
        Cursor.lockState = prevCursorLock;
        Cursor.visible = prevCursorVisible;

        // Gọi callback một lần
        var cb = onFinishedOnce; onFinishedOnce = null;
        cb?.Invoke(ok);
    }

    public void ForceClose(bool completed = false)
    {
        if (!current) return;
        current.Close(completed); // HandleFinished sẽ dọn dẹp
    }

    void Update()
    {
        if (current != null && allowEscToClose && Input.GetKeyDown(KeyCode.Escape))
            current.Close(false);
    }
}
