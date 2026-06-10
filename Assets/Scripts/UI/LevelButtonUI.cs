using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelButtonUI : MonoBehaviour
{
    [SerializeField] private Button button;
  [SerializeField] private TMP_Text label;

[Header("Visual")]
[SerializeField] private Image objectImage;

[SerializeField] private Sprite normalSprite;
[SerializeField] private Sprite finalLevelSprite;

[SerializeField] private bool flipObjectImage;

[SerializeField] private float flippedTextRotation = -4f;
    private int levelIndex;

    public void Initialize(
    int levelIndex,
    bool isNextAvailableLevel)
{
    this.levelIndex = levelIndex;
    Debug.Log($"Initialized level button {levelIndex + 1} on {gameObject.name}");

if (objectImage != null)
{
    objectImage.sprite =
        isNextAvailableLevel
        ? finalLevelSprite
        : normalSprite;

    // RectTransform imageRect = objectImage.rectTransform;

    // Vector3 imageScale = Vector3.one;

    // // Flip image only
    // imageScale.x = flipObjectImage ? -1f : 1f;

    // imageRect.localScale = imageScale;

}

if (label != null)
{
    label.text = $"{levelIndex + 1}";
    // label.rectTransform.localRotation = Quaternion.Euler(
    //     0f,
    //     0f,
    //     flipObjectImage ? flippedTextRotation : flippedTextRotation * -1f
    // );
}

    button.onClick.RemoveAllListeners();
    button.onClick.AddListener(() =>
    {
        LevelManager.Instance.SelectLevelAndLoadGame(levelIndex);
    });
}
}