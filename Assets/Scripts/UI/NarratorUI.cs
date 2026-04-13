using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// 나레이터 텍스트 박스.
/// - 타이핑 애니메이션
/// - 스탯 호버 시 설명 교체
/// - ShowText()로 RoomScene 등 모든 씬에서 재사용 가능
///
/// [Inspector 연결 목록]
///   - narratorTMP  : TextMeshPro
///   - charInterval : 타이핑 간격 (초)
///   - introText    : Phase 01 텍스트
///   - confirmText  : Phase 04 텍스트
/// </summary>
public class NarratorUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI narratorTMP;
    [SerializeField] private float charInterval = 0.035f;

    [Header("Texts")]
    [TextArea, SerializeField] private string introText = "당신은 이 공간에 있습니다.\n스탯을 분배하세요.";
    [TextArea, SerializeField] private string confirmText = "이걸로 정해졌습니다.";

    private Coroutine typingCoroutine;

    // ── Phase 01 / 04 ────────────────────────────────────

    public IEnumerator PlayIntro(float maxDuration)
    {
        yield return TypeText(introText, maxDuration);
    }

    public IEnumerator PlayConfirm()
    {
        yield return TypeText(confirmText, 0f);
    }

    // ── 범용 텍스트 출력 (RoomScene 등에서 사용) ──────────

    /// <summary>임의 텍스트를 타이핑 애니메이션으로 출력. 완료까지 대기.</summary>
    public IEnumerator ShowText(string text)
    {
        yield return TypeText(text, 0f);
    }

    /// <summary>타이핑 없이 즉시 교체.</summary>
    public void SetTextImmediate(string text)
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        if (narratorTMP) narratorTMP.text = text;
    }

    // ── 스탯 설명 (호버) ────────────────────────────────

    public void ShowStatDescription(string desc)
    {
        if (narratorTMP) narratorTMP.text = desc;
    }

    // ── 타이핑 코루틴 ────────────────────────────────────

    /// <param name="maxDuration">0이면 끝날 때까지. 양수면 그 시간 후 강제 완성.</param>
    private IEnumerator TypeText(string text, float maxDuration)
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        if (narratorTMP == null) yield break;

        narratorTMP.text = "";
        float elapsed = 0f;

        foreach (char c in text)
        {
            narratorTMP.text += c;

            // 미세 흔들림 (공책 필기 느낌 — 에셋 완성 시 Animator로 교체 가능)
            narratorTMP.rectTransform.anchoredPosition +=
                new Vector2(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f));

            yield return new WaitForSeconds(charInterval);
            elapsed += charInterval;

            if (maxDuration > 0 && elapsed >= maxDuration)
            {
                narratorTMP.text = text;
                yield break;
            }
        }
    }
}
