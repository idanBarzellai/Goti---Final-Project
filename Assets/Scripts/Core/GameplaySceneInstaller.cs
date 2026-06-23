using UnityEngine;

public class GameplaySceneInstaller : MonoBehaviour
{
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private InventoryBarUI inventoryBarUI;
    [SerializeField] private LevelTitleUI levelTitleUI;

    private void Start()
    {
        if (LevelManager.Instance == null)
        {
            Debug.LogError("GameplaySceneInstaller: No LevelManager found.");
            return;
        }

        LevelManager.Instance.RegisterGameplaySceneReferences(
            boardManager,
            inventoryBarUI,
            levelTitleUI
        );
    }
}
