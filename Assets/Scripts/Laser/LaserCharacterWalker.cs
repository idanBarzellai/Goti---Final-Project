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

    private Coroutine walkRoutine;

    private void Awake()
    {
        if (characterRenderer == null)
            characterRenderer = GetComponentInChildren<SpriteRenderer>();

        gameObject.SetActive(false);
    }

    public void WalkPaths(List<List<Vector3>> paths, bool shouldPlayWinAnimation, Action onComplete)
    {
        gameObject.SetActive(true);

        if (walkRoutine != null)
            StopCoroutine(walkRoutine);

        walkRoutine = StartCoroutine(WalkPathsRoutine(paths, shouldPlayWinAnimation, onComplete));
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

    private IEnumerator WalkPathsRoutine(List<List<Vector3>> paths, bool shouldPlayWinAnimation, Action onComplete)
    {
        if (paths == null || paths.Count == 0)
        {
            Clear();
            onComplete?.Invoke();
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

            StartTrail();

            for (int i = 1; i < path.Count; i++)
            {
                yield return MoveTo(path[i]);
            }

            StopTrail();
        }

        if (shouldPlayWinAnimation)
        {
            yield return new WaitForSeconds(winPauseDuration);
        }
        else
        {
            yield return new WaitForSeconds(winPauseDuration);
            gameObject.SetActive(false);
        }

        walkRoutine = null;
        onComplete?.Invoke();
    }

    private IEnumerator MoveTo(Vector3 target)
    {
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