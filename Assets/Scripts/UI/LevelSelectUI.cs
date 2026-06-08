using UnityEngine;

public class LevelSelectUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform levelButtonSlotsContainer;
    [SerializeField] private LevelButtonUI levelButtonPrefab;
    [SerializeField] private LevelManager levelManager;

    private void Start()
    {
        BuildLevelButtons();
    }

    private void BuildLevelButtons()
{
    if (levelManager == null)
        levelManager = LevelManager.Instance;

    if (levelManager == null)
        return;

    int slotCount = levelButtonSlotsContainer.childCount;

    for (int i = 0; i < slotCount; i++)
    {
        Transform slot = levelButtonSlotsContainer.GetChild(i);

        bool hasLevel = i < levelManager.LevelCount;
        bool unlocked =
            hasLevel &&
            levelManager.IsLevelUnlocked(i);

        slot.gameObject.SetActive(unlocked);

        if (!unlocked)
            continue;

        LevelButtonUI button = slot.GetComponentInChildren<LevelButtonUI>(true);

        if (button == null)
        {
            Debug.LogWarning(
                $"Missing LevelButtonUI on {slot.name}");
            continue;
        }

        bool isNextAvailable =
            levelManager.IsNextAvailableLevel(i);

        button.Initialize(i, isNextAvailable);
    }
}
}