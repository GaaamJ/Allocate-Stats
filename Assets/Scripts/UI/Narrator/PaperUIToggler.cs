using System.Collections;
using UnityEngine;

public class PaperUIToggler : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private GameObject targetRoot;

    [Header("Input")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;
    [SerializeField] private bool startVisible = true;

    [Header("Fade")]
    [SerializeField] private float fadeInDuration = 0.2f;
    [SerializeField] private float fadeOutDuration = 0.2f;

    private bool isVisible;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        isVisible = startVisible;
        ApplyImmediate(isVisible);
    }

    private void Update()
    {
        if (!Input.GetKeyDown(toggleKey)) return;

        isVisible = !isVisible;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(Fade(isVisible));
    }

    private void ApplyImmediate(bool visible)
    {
        if (targetRoot != null)
            targetRoot.SetActive(visible);
        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }
    }

    private IEnumerator Fade(bool visible)
    {
        if (targetRoot != null && visible)
            targetRoot.SetActive(true);

        if (canvasGroup == null)
        {
            ApplyImmediate(visible);
            fadeCoroutine = null;
            yield break;
        }

        float from = canvasGroup.alpha;
        float to = visible ? 1f : 0f;
        float duration = visible ? fadeInDuration : fadeOutDuration;
        float elapsed = 0f;

        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, elapsed / duration));
            yield return null;
        }

        canvasGroup.alpha = to;
        if (targetRoot != null && !visible)
            targetRoot.SetActive(false);
        fadeCoroutine = null;
    }
}
