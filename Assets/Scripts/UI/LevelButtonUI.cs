using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelButtonUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text label;

    private int levelIndex;

    public void Initialize(int levelIndex)
    {
        this.levelIndex = levelIndex;

        if (label != null)
            label.text = $"Level {levelIndex + 1}";

        if (button != null)
            button.onClick.AddListener(() => SceneFlowManager.StartLevel(levelIndex));
    }
}