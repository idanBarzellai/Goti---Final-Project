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
        {
            levelManager = LevelManager.Instance;
        }

        if (levelManager == null)
        {
            Debug.LogError("LevelSelectUI: LevelManager is missing.");
            return;
        }

        if (levelButtonSlotsContainer == null)
        {
            Debug.LogError("LevelSelectUI: Level button slots container is missing.");
            return;
        }

        if (levelButtonPrefab == null)
        {
            Debug.LogError("LevelSelectUI: Level button prefab is missing.");
            return;
        }

        int levelCount = levelManager.LevelCount;
        int slotCount = levelButtonSlotsContainer.childCount;

        for (int i = 0; i < slotCount; i++)
        {
            Transform slot = levelButtonSlotsContainer.GetChild(i);

            // Remove old runtime button if rebuilding
            for (int c = slot.childCount - 1; c >= 0; c--)
            {
                Destroy(slot.GetChild(c).gameObject);
            }

            if (i >= levelCount)
            {
                slot.gameObject.SetActive(false);
                continue;
            }

            slot.gameObject.SetActive(true);

            LevelButtonUI button = Instantiate(levelButtonPrefab, slot);
            button.transform.localPosition = Vector3.zero;
            button.transform.localRotation = Quaternion.identity;
            button.transform.localScale = Vector3.one;

            RectTransform rect = button.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }

            button.Initialize(i);
        }
    }
}