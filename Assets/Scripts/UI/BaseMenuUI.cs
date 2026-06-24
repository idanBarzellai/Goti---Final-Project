using UnityEngine;

public abstract class BaseMenuUI : MonoBehaviour
{

    protected virtual void Start()
{
    ForceHiddenOnStart();
}

private void ForceHiddenOnStart() => gameObject.SetActive(false);

    public virtual void Show()
    {
        Debug.Log("Showing menu: " + gameObject.name);
        gameObject.SetActive(true);

        OnShown();
    }

    public virtual void Hide()
    {
        Debug.Log("Hiding menu: " + gameObject.name);
        gameObject.SetActive(false);

        OnHidden();
    }

    public void Toggle()
    {
        Debug.Log("Toggling menu: " + gameObject.name + " | Currently active: " + gameObject.activeSelf);
        AudioManager.Instance?.PlayButtonClick();

        if (!gameObject.activeSelf)
            Show();
        else
            Hide();
    }

    protected virtual void OnShown() { }

    protected virtual void OnHidden() { }

    protected void RestartLevel()
    {
        Hide();

        if (GameManager.Instance != null)
            GameManager.Instance.ReloadLevel();
    }

    protected void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneFlowManager.GoToMainMenu();
    }
}
