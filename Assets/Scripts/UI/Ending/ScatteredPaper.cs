using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
#if MM_FEEDBACKS
using MoreMountains.Feedbacks;
#endif

public class ScatteredPaper : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI contentTMP;
    [SerializeField] Button clickButton;

#if MM_FEEDBACKS
    [Header("FEEL")]
    [SerializeField] MMFeedbacks zoomInFeedbacks;
    [SerializeField] MMFeedbacks zoomOutFeedbacks;
#endif

    public bool IsZoomed { get; private set; } = false;

    private RectTransform rt;
    private Vector2 originalPosition;
    private Vector3 originalScale;
    private float originalRotation;

    public static ScatteredPaper CurrentZoomed { get; private set; }
    public static event Action<ScatteredPaper> OnZoomChanged;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
        if (clickButton != null)
            clickButton.onClick.AddListener(OnClick);
    }

    public void SetContent(string text)
    {
        if (contentTMP != null)
            contentTMP.text = text;
    }

    // EndingSceneController에서 랜덤 배치 후 반드시 호출
    public void StoreOriginalTransform()
    {
        originalPosition = rt.anchoredPosition;
        originalScale = rt.localScale;
        originalRotation = rt.localEulerAngles.z;
    }

    private void OnClick()
    {
        if (IsZoomed)
            ZoomOut();
        else
        {
            CurrentZoomed?.ZoomOut();
            ZoomIn();
        }
    }

    public void ZoomIn()
    {
        IsZoomed = true;
        CurrentZoomed = this;
        transform.SetAsLastSibling();
        OnZoomChanged?.Invoke(this);

#if MM_FEEDBACKS
        zoomInFeedbacks?.PlayFeedbacks();
#endif
    }

    public void ZoomOut()
    {
        IsZoomed = false;
        if (CurrentZoomed == this) CurrentZoomed = null;
        OnZoomChanged?.Invoke(null);

#if MM_FEEDBACKS
        zoomOutFeedbacks?.PlayFeedbacks();
#endif
    }

    private void OnDestroy()
    {
        if (CurrentZoomed == this) CurrentZoomed = null;
    }

    // FEEL MMFeedbackPosition이 원래 위치로 복귀할 때 참조할 수 있도록 공개
    public Vector2 OriginalPosition => originalPosition;
    public float OriginalRotation => originalRotation;
}
