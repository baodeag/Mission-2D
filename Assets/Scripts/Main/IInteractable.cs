using UnityEngine;

public interface IInteractable
{
    string Prompt { get; }               // “Nhấn E để …”
    bool CanInteract { get; }            // có sẵn sàng không
    void Interact(PlayerInteractor who); // thực thi
    Transform WorldAnchor { get; }       // điểm hiển thị prompt
}
