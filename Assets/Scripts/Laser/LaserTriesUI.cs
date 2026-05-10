using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LaserTriesUI : MonoBehaviour
{
    [SerializeField] private Image lifeImagePrefab;
    [SerializeField] private Transform lifeContainer;

    private readonly List<Image> spawnedLives = new List<Image>();

    public void SetTries(int remaining, int max)
    {
        BuildLives(max);
        Refresh(remaining);
    }

    public void SetTries(int remaining)
    {
        Refresh(remaining);
    }

    private void BuildLives(int max)
    {
        ClearLives();

        for (int i = 0; i < max; i++)
        {
            Image life = Instantiate(lifeImagePrefab, lifeContainer);
            life.gameObject.SetActive(true);
            spawnedLives.Add(life);
        }
    }

    private void Refresh(int remaining)
    {
        for (int i = 0; i < spawnedLives.Count; i++)
        {
            spawnedLives[i].gameObject.SetActive(i < remaining);
        }
    }

    private void ClearLives()
    {
        for (int i = spawnedLives.Count - 1; i >= 0; i--)
        {
            if (spawnedLives[i] != null)
                Destroy(spawnedLives[i].gameObject);
        }

        spawnedLives.Clear();
    }
}