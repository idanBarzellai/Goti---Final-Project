using UnityEngine;

public class LevelSelectUI : MonoBehaviour
{
    [SerializeField] private Transform levelButtonContainer;
    [SerializeField] private LevelButtonUI levelButtonPrefab;

    [SerializeField] private int levelCount = 5;

    private void Start()
    {
        BuildLevelButtons();
    }

    private void BuildLevelButtons()
    {
        for (int i = levelButtonContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(levelButtonContainer.GetChild(i).gameObject);
        }

        for (int i = 0; i < levelCount; i++)
        {
            LevelButtonUI button = Instantiate(levelButtonPrefab, levelButtonContainer);
            button.Initialize(i);
        }
    }
}