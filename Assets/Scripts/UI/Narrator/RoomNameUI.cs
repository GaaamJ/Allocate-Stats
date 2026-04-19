using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Tab 키를 누르면 좌하단에 방 이름이 데스티니 스타일로 페이드인.
/// Tab을 다시 누르거나 떼면 페이드아웃.
///
/// RoomSceneController.Start() 후 Init()을 호출해서 방 이름을 주입.
///
/// [Inspector 연결]
///   roomNameTMP   : 방 이름 TMP
///   canvasGroup   : 페이드 제어용 CanvasGroup
///   toggleKey     : PaperNarrator와 동일한 키 (Tab)
///   fadeInDuration : 페이드인 시간
///   fadeOutDuration: 페이드아웃 시간
/// </summary>
public class RoomNameUI : MonoBehaviour
{
    [Header("방 이름 TMP")]
    [SerializeField] private TextMeshProUGUI roomNameTMP;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("토글 설정 — PaperNarrator와 동일 키")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.2f;

    private bool isVisible = false;
    private Coroutine fadeCoroutine;

    // ── 초기화 ────────────────────────────────────────────

    private void Awake()
    {
        if (canvasGroup) canvasGroup.alpha = 0f;
    }

    /// <summary>RoomSceneController에서 씬 시작 시 방 이름 주입.</summary>
    public void Init(string roomName)
    {
        if (roomNameTMP) roomNameTMP.text = roomName;
    }

    // ── 토글 ─────────────────────────────────────────────

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            isVisible = !isVisible;
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(isVisible
                ? Fade(0f, 1f, fadeInDuration)
                : Fade(1f, 0f, fadeOutDuration));
        }
    }

    // ── 페이드 ────────────────────────────────────────────

    private IEnumerator Fade(float from, float to, float duration)
    {
        if (canvasGroup == null) yield break;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to,
                Mathf.SmoothStep(0f, 1f, elapsed / duration));
            yield return null;
        }
        canvasGroup.alpha = to;
        fadeCoroutine = null;
    }
}
