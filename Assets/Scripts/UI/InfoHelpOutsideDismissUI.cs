using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InfoHelpOutsideDismissUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private RectTransform instructionsPanel;
    private int enabledFrame;

    public void Configure(RectTransform panel)
    {
        instructionsPanel = panel;
    }

    private void OnEnable()
    {
        enabledFrame = Time.frameCount;
    }

    private void Update()
    {
        if (Time.frameCount <= enabledFrame)
            return;

        Pointer pointer = Pointer.current;
        if (pointer != null && pointer.press.wasPressedThisFrame)
            Dismiss();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Time.frameCount <= enabledFrame)
            return;

        Dismiss();
    }

    private void Dismiss()
    {
        AudioManager.Instance?.PlayButtonClick();
        gameObject.SetActive(false);
    }
}
