using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    public float radius = 2.0f;
    public LayerMask interactMask = ~0;
    public InteractionPromptUI promptUI;
    public Transform promptDefaultAnchor;

    IInteractable current;

    bool controlLocked => MiniGameManager.Exists && MiniGameManager.Instance.IsBusy;

    void Update()
    {
        if (controlLocked) { HidePrompt(); return; }

        // tìm interactable gần nhất
        Collider[] cols = Physics.OverlapSphere(transform.position, radius, interactMask);
        IInteractable nearest = null;
        float best = float.MaxValue;

        foreach (var c in cols)
        {
            var cand = c.GetComponentInParent<IInteractable>();
            if (cand == null || !cand.CanInteract) continue;
            float d = (c.transform.position - transform.position).sqrMagnitude;
            if (d < best) { best = d; nearest = cand; }
        }

        current = nearest;

        if (current != null)
        {
            ShowPrompt(current.Prompt, current.WorldAnchor ? current.WorldAnchor : (promptDefaultAnchor ? promptDefaultAnchor : transform));
            if (Input.GetKeyDown(KeyCode.E))
            {
                current.Interact(this);
                HidePrompt();
            }
        }
        else HidePrompt();
    }

    void ShowPrompt(string text, Transform anchor) { if (promptUI) promptUI.Show(text, anchor); }
    void HidePrompt() { if (promptUI) promptUI.Hide(); }
}
