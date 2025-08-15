using UnityEngine;

public class OpenMiniGameButton : MonoBehaviour
{
    [SerializeField] MiniGameBase miniGamePrefab;

    public void Open()
    {
        MiniGameManager.I.OpenMiniGame(miniGamePrefab);
    }
}
