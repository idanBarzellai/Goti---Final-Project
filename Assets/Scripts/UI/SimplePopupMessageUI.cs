using System.Collections;
using TMPro;
using UnityEngine;

public class SimplePopupMessageUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text messageText;

    [SerializeField] private float visibleDuration = 1.5f;
    [SerializeField] private float fadeDuration = 0.25f;

    private Coroutine currentRoutine;

    private void Awake()
    {
        HideImmediate();
    }

    public void ShowMessage(string message)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(ShowRoutine(message));
    }
private IEnumerator ShowRoutine(string message)
{
    if (messageText != null)
        messageText.text = message;

    if (canvasGroup != null)
    {
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    yield return Fade(0f, 1f);

    yield return new WaitForSeconds(visibleDuration);

    yield return Fade(1f, 0f);
}

    private IEnumerator Fade(float from, float to)
    {
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;

            float t = Mathf.Clamp01(time / fadeDuration);

            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(from, to, t);

            yield return null;
        }

        if (canvasGroup != null)
            canvasGroup.alpha = to;
    }

    private void HideImmediate()
{
    if (canvasGroup != null)
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }
}
}