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
        ShowMessage(message, visibleDuration);
    }

    public void ShowMessage(string message, float duration)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(ShowRoutine(message, duration));
    }

    public void ShowPersistentMessage(string message)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(ShowPersistentRoutine(message));
    }

    public void HideMessage()
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = null;
        HideImmediate();
    }

    private IEnumerator ShowPersistentRoutine(string message)
    {
        if (messageText != null)
            messageText.text = message;

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        yield return Fade(0f, 1f);
    }
private IEnumerator ShowRoutine(string message, float duration)
{
    if (messageText != null)
        messageText.text = message;

    if (canvasGroup != null)
    {
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    yield return Fade(0f, 1f);

    yield return new WaitForSeconds(Mathf.Max(0f, duration));

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
