using System;
using UnityEngine;
using UnityEngine.UI;

public class LevelTimerManager : MonoBehaviour
{
    [SerializeField] private float levelDurationSeconds = 120f;
    [SerializeField] private bool startOnPlay = true;

    [Header("UI")]
    [SerializeField] private Image sunriseFillImage;

    private float remainingTime;
    private bool isRunning;

    public float RemainingTime => remainingTime;

    public float Progress01 => levelDurationSeconds <= 0f
        ? 1f
        : 1f - Mathf.Clamp01(remainingTime / levelDurationSeconds);

    public event Action OnTimerFinished;

    private void Start()
    {
        remainingTime = levelDurationSeconds;
        RefreshUI();

        if (startOnPlay)
            StartTimer();
    }

    private void Update()
    {
        if (!isRunning)
            return;

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            isRunning = false;
            RefreshUI();
            OnTimerFinished?.Invoke();
            return;
        }

        RefreshUI();
    }

    public void StartTimer()
    {
        isRunning = true;
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void ResetTimer()
    {
        remainingTime = levelDurationSeconds;
        isRunning = false;
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (sunriseFillImage != null)
            sunriseFillImage.fillAmount = Progress01;
    }
}