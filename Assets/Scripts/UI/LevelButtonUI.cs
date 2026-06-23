using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelButtonUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text label;

    [Header("Visual")]
    [SerializeField] private Image objectImage;

    [SerializeField] private Sprite unlockedSprite;
    [SerializeField] private Sprite lockedSprite;
    private int levelIndex;

    public void Initialize(int levelIndex, bool isUnlocked)
    {
        this.levelIndex = levelIndex;
        Debug.Log($"Initialized level button {levelIndex + 1} on {gameObject.name}");

        if (objectImage != null)
        {
            objectImage.sprite = isUnlocked ? unlockedSprite : lockedSprite;
            Vector3 imageScale = objectImage.rectTransform.localScale;
            imageScale.x = Mathf.Abs(imageScale.x);
            objectImage.rectTransform.localScale = imageScale;
            objectImage.rectTransform.localRotation = Quaternion.identity;
        }

        if (label != null)
        {
            label.gameObject.SetActive(isUnlocked);
            label.text = $"{levelIndex + 1}";
            label.rectTransform.localScale = Vector3.one;
            label.rectTransform.localRotation = Quaternion.identity;
        }

        if (button == null)
            return;

        button.interactable = isUnlocked;
        button.onClick.RemoveAllListeners();

        if (isUnlocked)
        {
            button.onClick.AddListener(() =>
            {
                LevelManager.Instance.SelectLevelAndLoadGame(levelIndex);
            });
        }
    }
}
