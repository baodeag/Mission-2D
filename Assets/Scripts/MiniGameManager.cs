using UnityEngine;

public class MiniGameManager : MonoBehaviour
{
    public static MiniGameManager I;

    [Header("Refs")]
    [SerializeField] GameObject menu;
    [SerializeField] GameObject miniGameLayer; // gán MiniGameLayer
    [SerializeField] Transform miniGameParent; // gán MiniGameParent

    MiniGameBase current;

    void Awake() => I = this;

    public bool IsOpen => current != null;

    public void OpenMiniGame(MiniGameBase prefab)
    {
        if (current) return;

        // (Tùy chọn) đảm bảo vẽ trên cùng
        miniGameLayer.transform.SetAsLastSibling();

        if (menu) menu.SetActive(false);
        miniGameLayer.SetActive(true);

        current = Instantiate(prefab, miniGameParent);
        current.OnFinished += HandleFinished;
        current.Open();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void HandleFinished(bool ok)
    {
        current.OnFinished -= HandleFinished;
        Destroy(current.gameObject);
        current = null;

        miniGameLayer.SetActive(false);
        if (menu) menu.SetActive(true);
    }

    void Update()
    {
        if (current != null && Input.GetKeyDown(KeyCode.Escape))
            current.Close(false); // nhấn ESC để thoát mini-game
    }
}
