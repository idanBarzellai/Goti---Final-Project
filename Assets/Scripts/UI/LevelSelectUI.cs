using UnityEngine;

public class LevelSelectUI : MonoBehaviour
{
    [SerializeField] private Transform levelButtonContainer;
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
            Debug.LogError("LevelSelectUI: LevelManager is missing.");
            return;
        }

        for (int i = levelButtonContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(levelButtonContainer.GetChild(i).gameObject);
        }

       for (int i = 0; i < LevelManager.Instance.LevelCount; i++)
{
    LevelButtonUI button = Instantiate(levelButtonPrefab, levelButtonContainer);
    button.Initialize(i);
}
    }
}