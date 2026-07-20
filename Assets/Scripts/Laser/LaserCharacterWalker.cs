using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserCharacterWalker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer characterRenderer;
    [SerializeField] private TrailRenderer trailRenderer;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float arriveDistance = 0.02f;

    [Header("Portal")]
    [SerializeField] private float portalPauseDuration = 0.12f;

    [Header("Win Animation Placeholder")]
    [SerializeField] private float winPauseDuration = 0.5f;
    [SerializeField] private float winScale = 2f;
    [SerializeField] private float winScaleUpDuration = 0.2f;
    [SerializeField] private float winArrivalDelay = 0f;
    [SerializeField] private float winPostScaleDelay = 0.5f;
    [SerializeField] private float winDisappearDelay = 1f;
    [SerializeField] private float appearDisappearSpinDegrees = 360f;

    private Coroutine walkRoutine;
    private PieceSpriteLibrary animationLibrary;
    private BoardPieceView entryView;
    private int movementFrame;
    private float nextMovementFrameTime;
    private Direction? currentMovementDirection;
    private Quaternion entryRotation = Quaternion.identity;

    public void ConfigureFromEntry(BoardPiece entry)
    {
        entryView = entry != null ? entry.GetComponent<BoardPieceView>() : null;
        animationLibrary = entryView != null ? entryView.SpriteLibrary : null;
        entryRotation = entryView != null ? entryView.VisualWorldRotation : Quaternion.identity;
    }

    private void Awake()
    {
        if (characterRenderer == null)
            characterRenderer = GetComponentInChildren<SpriteRenderer>();

        StopTrail();
        gameObject.SetActive(false);
    }

    public void WalkPaths(List<List<Vector3>> paths, bool shouldPlayWinAnimation, bool wasBlocked, bool exitedBoard, Action<Vector3> onCellReached, Action onBump, Action onReachedGoal, Action onComplete)
    {
        gameObject.SetActive(true);
        entryView?.SetEntryAway(true);
        transform.localScale = Vector3.one;
        transform.rotation = Quaternion.identity;
        currentMovementDirection = null;

        if (walkRoutine != null)
            StopCoroutine(walkRoutine);

        walkRoutine = StartCoroutine(WalkPathsRoutine(paths, shouldPlayWinAnimation, wasBlocked, exitedBoard, onCellReached, onBump, onReachedGoal, onComplete));
    }

    public void Clear()
    {
        if (walkRoutine != null)
        {
            StopCoroutine(walkRoutine);
            walkRoutine = null;
        }

        StopTrail();

        gameObject.SetActive(false);
    }

    private IEnumerator WalkPathsRoutine(List<List<Vector3>> paths, bool shouldPlayWinAnimation, bool wasBlocked, bool exitedBoard, Action<Vector3> onCellReached, Action onBump, Action onReachedGoal, Action onComplete)
    {
        if (paths == null || paths.Count == 0)
        {
            StopTrail();
            walkRoutine = null;
            onComplete?.Invoke();
            gameObject.SetActive(false);
            yield break;
        }

        gameObject.SetActive(true);

        for (int pathIndex = 0; pathIndex < paths.Count; pathIndex++)
        {
            List<Vector3> path = paths[pathIndex];

            if (path == null || path.Count == 0)
                continue;

            bool isAfterPortal = pathIndex > 0;

            if (isAfterPortal)
            {
                StopTrail();

                if (characterRenderer != null)
                    characterRenderer.enabled = false;

                transform.position = path[0];

                yield return new WaitForSeconds(portalPauseDuration);

                if (characterRenderer != null)
                    characterRenderer.enabled = true;

                yield return null;
            }
            else
            {
                transform.position = path[0];
                yield return null;
            }

            onCellReached?.Invoke(path[0]);

            for (int i = 1; i < path.Count; i++)
            {
                bool blockedEnd = wasBlocked && pathIndex == paths.Count - 1 && i == path.Count - 1;
                Vector3 destination = blockedEnd ? Vector3.Lerp(path[i - 1], path[i], 0.48f) : path[i];
                yield return MoveTo(destination);
                if (blockedEnd)
                {
                    onBump?.Invoke();
                    yield return MoveTo(Vector3.Lerp(path[i - 1], path[i], 0.2f));
                }
                if (!blockedEnd)
                    onCellReached?.Invoke(path[i]);
            }

            StopTrail();
        }

        if (shouldPlayWinAnimation)
        {
            onReachedGoal?.Invoke();
            Sprite[] winFrames = animationLibrary != null ? animationLibrary.winFrames : null;
            if (winFrames != null && winFrames.Length > 0 && winFrames[0] != null)
                characterRenderer.sprite = winFrames[0];
            if (winArrivalDelay > 0f)
                yield return new WaitForSeconds(winArrivalDelay);
            yield return ScaleOnlyTo(winScale, winScaleUpDuration);
            yield return new WaitForSeconds(winPostScaleDelay);
            AudioManager.Instance?.PlayWin();
            yield return PlayFrames(winFrames, 1);
            yield return new WaitForSeconds(winDisappearDelay);
            yield return ScaleTo(0f, winPauseDuration);
        }
        else if (exitedBoard && paths.Count > 0 && paths[paths.Count - 1].Count > 1)
        {
            List<Vector3> last = paths[paths.Count - 1];
            Vector3 direction = (last[last.Count - 1] - last[last.Count - 2]).normalized;
            Camera camera = Camera.main;
            float exitDeadline = Time.time + 5f;
            while (camera != null && Time.time < exitDeadline)
            {
                Vector3 viewport = camera.WorldToViewportPoint(transform.position);
                if (viewport.x < -0.1f || viewport.x > 1.1f || viewport.y < -0.1f || viewport.y > 1.1f) break;
                yield return MoveTo(transform.position + direction * 0.5f);
            }
            yield return RespawnAtEntry(paths);
        }
        else
        {
            yield return RespawnAtEntry(paths);
        }

        walkRoutine = null;
        onComplete?.Invoke();
        gameObject.SetActive(false);
    }

    private IEnumerator MoveTo(Vector3 target)
    {
        Vector3 delta = target - transform.position;
        Direction direction = Mathf.Abs(delta.x) > Mathf.Abs(delta.y) ? (delta.x >= 0 ? Direction.Right : Direction.Left) : (delta.y >= 0 ? Direction.Up : Direction.Down);
        Sprite[] frames = animationLibrary != null ? animationLibrary.GetRollFrames(direction) : null;
        if (!currentMovementDirection.HasValue || currentMovementDirection.Value != direction)
        {
            currentMovementDirection = direction;
            movementFrame = 0;
            nextMovementFrameTime = 0f;
        }
        while (Vector3.Distance(transform.position, target) > arriveDistance)
        {
            if (frames != null && frames.Length > 0 && Time.time >= nextMovementFrameTime)
            {
                characterRenderer.sprite = frames[movementFrame++ % frames.Length];
                nextMovementFrameTime = Time.time + 0.06f;
            }
            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        transform.position = target;
    }

    private IEnumerator RespawnAtEntry(List<List<Vector3>> paths)
    {
        yield return ScaleTo(0f, winPauseDuration);
        if (paths.Count > 0 && paths[0] != null && paths[0].Count > 0) transform.position = paths[0][0];
        transform.rotation = entryRotation;
        yield return ScaleTo(1f, winPauseDuration);
        entryView?.SetEntryAway(false);
    }

    private IEnumerator PlayFrames(Sprite[] frames, int startIndex = 0)
    {
        if (frames == null) yield break;
        for (int i = Mathf.Clamp(startIndex, 0, frames.Length); i < frames.Length; i++)
        {
            if (frames[i] != null) characterRenderer.sprite = frames[i];
            yield return new WaitForSeconds(0.06f);
        }
    }

    private IEnumerator ScaleTo(float target, float duration)
    {
        Vector3 start = transform.localScale;
        Vector3 end = Vector3.one * target;
        float startAngle = transform.eulerAngles.z;
        float endAngle = startAngle + appearDisappearSpinDegrees;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, duration));
            transform.localScale = Vector3.Lerp(start, end, t);
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(startAngle, endAngle, t));
            yield return null;
        }
        transform.localScale = end;
        transform.rotation = Quaternion.Euler(0f, 0f, endAngle);
    }

    private IEnumerator ScaleOnlyTo(float target, float duration)
    {
        Vector3 start = transform.localScale;
        Vector3 end = Vector3.one * target;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(start, end, Mathf.Clamp01(elapsed / Mathf.Max(0.01f, duration)));
            yield return null;
        }
        transform.localScale = end;
    }

    private void StartTrail()
    {
        if (trailRenderer == null)
            return;

        trailRenderer.emitting = false;
        trailRenderer.Clear();
        trailRenderer.emitting = true;
    }

    private void StopTrail()
    {
        if (trailRenderer == null)
            return;

        trailRenderer.emitting = false;
        trailRenderer.Clear();
    }
}
