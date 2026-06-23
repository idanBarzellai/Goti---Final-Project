using TMPro;
using UnityEngine;

public class LevelTitleUI : MonoBehaviour
{
    [SerializeField] private TMP_Text levelTitleText;
    [SerializeField] private string titleFormat = "LEVEL {0}";

    public void SetLevel(int levelIndex)
    {
        if (levelTitleText == null)
            return;

        levelTitleText.text = string.Format(titleFormat, levelIndex + 1);
    }
}
