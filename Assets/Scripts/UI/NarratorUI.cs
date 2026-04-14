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
///   - narratorTMP          : TextMeshPro
///   - charInterval         : 타이핑 간격 (초)
///   - pauseBetweenSegments : 빈 줄 구분 덩어리 사이 텀 (초)
/// </summary>
public class NarratorUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI narratorTMP;
    [SerializeField] private float charInterval = 0.035f;

    [Header("분할 출력")]
    [SerializeField] private float pauseBetweenSegments = 0.8f;

    private Coroutine typingCoroutine;
    private Coroutine restoreCoroutine;
    private string currentText = ""; // 호버 복원용

    // ── Phase 01 / 04 ────────────────────────────────────

    /// <summary>TitleData.introBlocks 를 받아 블록 순서대로 출력. maxDuration은 블록 전체 제한.</summary>
    public IEnumerator PlayIntro(string[] blocks, float maxDuration)
    {
        if (blocks == null) yield break;
        float elapsed = 0f;
        foreach (string block in blocks)
        {
            if (string.IsNullOrEmpty(block)) continue;
            yield return TypeText(block, 0f);
            elapsed += pauseBetweenSegments;
            if (maxDuration > 0 && elapsed >= maxDuration) yield break;
            yield return new WaitForSeconds(pauseBetweenSegments);
        }
    }

    /// <summary>TitleData.confirmBlocks 를 받아 블록 순서대로 출력.</summary>
    public IEnumerator PlayConfirm(string[] blocks)
    {
        yield return ShowBlocks(blocks);
    }

    // ── 범용 텍스트 출력 (RoomScene 등에서 사용) ──────────

    /// <summary>
    /// 임의 텍스트를 타이핑 애니메이션으로 출력.
    /// 빈 줄(\n\n) 기준으로 덩어리 분할 후 pauseBetweenSegments 텀을 둠.
    /// 완료까지 대기.
    /// </summary>
    public IEnumerator ShowText(string text)
    {
        // 빈 줄 1줄 이상(\n 2개 이상)을 구분자로 덩어리 분할
        string[] segments = System.Text.RegularExpressions.Regex.Split(
            text.Trim(), @"\n\s*\n"
        );

        foreach (string seg in segments)
        {
            string trimmed = seg.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;

            yield return TypeText(trimmed, 0f);
            yield return new WaitForSeconds(pauseBetweenSegments);
        }
    }

    /// <summary>배열 단위로 블록 순서대로 출력. 블록마다 화면 지우고 새로 타이핑.</summary>
    public IEnumerator ShowBlocks(string[] blocks)
    {
        if (blocks == null) yield break;
        foreach (string block in blocks)
        {
            if (string.IsNullOrEmpty(block)) continue;
            yield return TypeText(block, 0f);
            yield return new WaitForSeconds(pauseBetweenSegments);
        }
    }

    /// <summary>나레이터 텍스트 즉시 클리어.</summary>
    public void Clear()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        if (narratorTMP) narratorTMP.text = "";
        currentText = "";
    }

    /// <summary>타이핑 없이 즉시 교체.</summary>
    public void SetTextImmediate(string text)
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        if (narratorTMP) narratorTMP.text = text;
    }

    // ── 스탯 설명 (호버) ────────────────────────────────

    /// <summary>스탯 호버 시 설명 출력. 복원 대기 중이면 취소.</summary>
    public void ShowStatDescription(string desc)
    {
        if (restoreCoroutine != null) StopCoroutine(restoreCoroutine);
        if (narratorTMP) narratorTMP.text = desc;
    }

    /// <summary>호버 해제 시 호출 — 딜레이 후 currentText로 복원.</summary>
    public void RestoreText()
    {
        if (restoreCoroutine != null) StopCoroutine(restoreCoroutine);
        restoreCoroutine = StartCoroutine(RestoreDelay());
    }

    private IEnumerator RestoreDelay()
    {
        yield return new WaitForSeconds(0.2f);
        if (narratorTMP) narratorTMP.text = currentText;
        restoreCoroutine = null;
    }

    // ── 타이핑 코루틴 ────────────────────────────────────

    /// <param name="maxDuration">0이면 끝날 때까지. 양수면 그 시간 후 강제 완성.</param>
    private IEnumerator TypeText(string text, float maxDuration)
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        if (narratorTMP == null) yield break;

        narratorTMP.text = "";
        currentText = text;
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