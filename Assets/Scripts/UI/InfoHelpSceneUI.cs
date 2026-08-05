using UnityEngine;
using UnityEngine.UI;

public class InfoHelpSceneUI : MonoBehaviour
{
    [SerializeField] private Button openButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button outsideButton;
    [SerializeField] private GameObject popup;

    public void Configure(
        Button open,
        Button close,
        Button outside,
        GameObject popupObject)
    {
        openButton = open;
        closeButton = close;
        outsideButton = outside;
        popup = popupObject;
    }

    private void Awake()
    {
        if (popup != null)
        {
            if (closeButton == null)
            {
                Transform close = popup.transform.Find("Instructions/CLOSE");
                if (close != null)
                    closeButton = close.GetComponent<Button>();
            }

            if (outsideButton == null)
            {
                outsideButton = popup.GetComponent<Button>();
                if (outsideButton == null)
                {
                    outsideButton = popup.AddComponent<Button>();
                    outsideButton.transition = Selectable.Transition.None;
                }
            }

            Graphic outsideGraphic = popup.GetComponent<Graphic>();
            if (outsideGraphic != null)
            {
                outsideGraphic.raycastTarget = true;
                outsideButton.targetGraphic = outsideGraphic;
            }

            InfoHelpOutsideDismissUI outsideDismiss =
                popup.GetComponent<InfoHelpOutsideDismissUI>();
            if (outsideDismiss == null)
                outsideDismiss = popup.AddComponent<InfoHelpOutsideDismissUI>();

            Transform panel = popup.transform.Find("Instructions");
            outsideDismiss.Configure(panel as RectTransform);
        }

        if (openButton != null)
            openButton.onClick.AddListener(Open);
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
    }

    private void Open()
    {
        if (popup != null && popup.activeSelf)
        {
            Close();
            return;
        }

        AudioManager.Instance?.PlayButtonClick();
        if (popup == null)
            return;
        popup.SetActive(true);
        popup.transform.SetAsLastSibling();
        transform.SetAsLastSibling();
    }

    private void Close()
    {
        AudioManager.Instance?.PlayButtonClick();
        if (popup != null)
            popup.SetActive(false);
    }
}
