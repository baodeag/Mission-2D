using UnityEngine;
using UnityEngine.Events;

public class InteractableMinigame : MonoBehaviour, IInteractable
{
    [Header("UI")]
    [TextArea][SerializeField] string prompt = "Nhấn E để bắt đầu mini-game";
    [SerializeField] Transform worldAnchor;

    [Header("Mini-game")]
    [SerializeField] MiniGameBase miniGamePrefab;
    [SerializeField] bool repeatable = true;

    [Header("Feedback khi hoàn thành (tuỳ chọn)")]
    [SerializeField] Renderer[] toTint;
    [SerializeField] Color completedColor = new Color(0.3f, 0.9f, 0.4f);

    [Header("Sự kiện (kéo nối trong Inspector nếu cần)")]
    public UnityEvent onOpened;
    public UnityEvent onCompleted;
    public UnityEvent onCancelled;

    bool completed;

    // IInteractable
    public string Prompt => (completed && !repeatable) ? "(Đã hoàn thành)" : prompt;
    public bool CanInteract => (miniGamePrefab != null) && (repeatable || !completed);
    public Transform WorldAnchor => worldAnchor ? worldAnchor : transform;

    public void Interact(PlayerInteractor who)
    {
        if (!CanInteract) return;

        onOpened?.Invoke();

        MiniGameManager.Instance.Open(miniGamePrefab, ok =>
        {
            if (ok)
            {
                completed = true;
                onCompleted?.Invoke();
                // Đổi màu máy cho dễ nhìn
                foreach (var r in toTint) if (r) r.material.color = completedColor;

                // Khoá tương tác nếu one-shot
                if (!repeatable)
                {
                    var col = GetComponent<Collider>();
                    if (col) col.enabled = false;
                }
            }
            else
            {
                onCancelled?.Invoke();
            }
        });
    }
}
