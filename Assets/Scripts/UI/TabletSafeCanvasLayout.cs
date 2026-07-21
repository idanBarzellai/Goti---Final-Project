using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Keeps authored UI inside a centered portrait design frame while allowing
/// full-screen backgrounds to fill wider phone and tablet displays.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Canvas), typeof(CanvasScaler))]
public sealed class TabletSafeCanvasLayout : MonoBehaviour
{
    [Header("Design frame")]
    [SerializeField] private Vector2 referenceResolution = new(1080f, 1920f);
    [SerializeField] private bool respectDeviceSafeArea = true;

    [Header("Children that should still fill the screen")]
    [Tooltip("Direct Canvas children with these names stay outside the portrait frame.")]
    [SerializeField] private List<string> fullScreenChildNames = new();

    private RectTransform canvasRect;
    private RectTransform portraitFrame;

    private void Awake()
    {
        CanvasScaler scaler = GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = referenceResolution;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasRect = transform as RectTransform;
        CreatePortraitFrame();
        MoveAuthoredContentIntoFrame();
    }

    private void OnEnable() => Canvas.willRenderCanvases += RefreshLayout;

    private void OnDisable() => Canvas.willRenderCanvases -= RefreshLayout;

    private void CreatePortraitFrame()
    {
        GameObject frameObject = new("PortraitContentRoot", typeof(RectTransform));
        frameObject.layer = gameObject.layer;
        portraitFrame = frameObject.GetComponent<RectTransform>();
        portraitFrame.SetParent(transform, false);
        portraitFrame.anchorMin = new Vector2(0.5f, 0.5f);
        portraitFrame.anchorMax = new Vector2(0.5f, 0.5f);
        portraitFrame.pivot = new Vector2(0.5f, 0.5f);
        portraitFrame.sizeDelta = referenceResolution;
    }

    private void MoveAuthoredContentIntoFrame()
    {
        // Copy first because reparenting changes transform.childCount.
        List<RectTransform> content = new();
        int frameSiblingIndex = transform.childCount - 1;

        for (int index = 0; index < transform.childCount; index++)
        {
            RectTransform child = transform.GetChild(index) as RectTransform;
            if (child == null || child == portraitFrame || fullScreenChildNames.Contains(child.name))
                continue;

            content.Add(child);
            frameSiblingIndex = Mathf.Min(frameSiblingIndex, index);
        }

        portraitFrame.SetSiblingIndex(frameSiblingIndex);
        foreach (RectTransform child in content)
            child.SetParent(portraitFrame, false);
    }

    private void RefreshLayout()
    {
        if (canvasRect == null || portraitFrame == null || Screen.width <= 0 || Screen.height <= 0)
            return;

        Rect safeArea = respectDeviceSafeArea ? Screen.safeArea : new Rect(0f, 0f, Screen.width, Screen.height);
        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;
        float safeWidth = canvasWidth * safeArea.width / Screen.width;
        float safeHeight = canvasHeight * safeArea.height / Screen.height;
        float scale = Mathf.Min(safeWidth / referenceResolution.x, safeHeight / referenceResolution.y);

        portraitFrame.localScale = new Vector3(scale, scale, 1f);
        Vector2 safeCenterPixels = safeArea.center - new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        portraitFrame.anchoredPosition = new Vector2(
            safeCenterPixels.x * canvasWidth / Screen.width,
            safeCenterPixels.y * canvasHeight / Screen.height);

    }
}
