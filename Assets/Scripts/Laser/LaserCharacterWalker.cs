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

    [Header("Win Animation Placeholder")]
    [SerializeField] private float winPauseDuration = 0.5f;

    private Coroutine walkRoutine;

    private void Awake()
    {
        if (characterRenderer == null)
            characterRenderer = GetComponentInChildren<SpriteRenderer>();

        gameObject.SetActive(false);
    }

    public void WalkPath(List<Vector3> path, bool shouldPlayWinAnimation, Action onComplete)
    {
        gameObject.SetActive(true);
        if (walkRoutine != null)
            StopCoroutine(walkRoutine);

        walkRoutine = StartCoroutine(WalkRoutine(path, shouldPlayWinAnimation, onComplete));
    }

public void Clear()
{
    if (walkRoutine != null)
    {
        StopCoroutine(walkRoutine);
        walkRoutine = null;
    }

    if (trailRenderer != null)
    {
        trailRenderer.emitting = false;
        trailRenderer.Clear();
    }

    gameObject.SetActive(false);
}

  private IEnumerator WalkRoutine(List<Vector3> path, bool shouldPlayWinAnimation, Action onComplete)
{
    if (path == null || path.Count == 0)
    {
        Clear();
        onComplete?.Invoke();
        yield break;
    }

    gameObject.SetActive(true);

    if (trailRenderer != null)
    {
        trailRenderer.emitting = false;
        trailRenderer.Clear();
    }

    transform.position = path[0];

    // Wait one frame so Unity fully moves the object before trail starts again
    yield return null;

    if (trailRenderer != null)
    {
        trailRenderer.Clear();
        trailRenderer.emitting = true;
    }

    for (int i = 1; i < path.Count; i++)
    {
        Vector3 target = path[i];

        while (Vector3.Distance(transform.position, target) > arriveDistance)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        transform.position = target;
    }

    if (shouldPlayWinAnimation)
{
    yield return new WaitForSeconds(winPauseDuration);
}
else
{
    // Hide character on failed attempt
    yield return new WaitForSeconds(winPauseDuration);
    gameObject.SetActive(false);
}

    walkRoutine = null;
    onComplete?.Invoke();
}

}