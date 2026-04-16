using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Screen 채널 나레이터.
/// 화면을 크게 채우는 천체의 대사 출력.
///
/// 기존 NarratorUI의 스탯 호버(ShowStatDescription / RestoreText) 기능을 그대로 유지.
/// StatAllocatorUI는 이 컴포넌트를 직접 참조해서 호버 기능을 사용한다.
///
/// [Inspector 연결]
///   narratorTMP : TextMeshProUGUI (화면 중앙 또는 전체 채움)
/// </summary>
public class ScreenNarrator : BaseNarrator
{
    [Header("Screen 채널 TMP")]
    [SerializeField] private TextMeshProUGUI narratorTMP;

    // 호버 복원용 — 스탯 설명 표시 전 currentText 저장
    private string currentText = "";
    private Coroutine restoreCoroutine;

    // ── BaseNarrator 구현 ─────────────────────────────────

    protected override TextMeshProUGUI GetTMP() => narratorTMP;

    protected override void OnBlockStart(NarrationBlock block)
    {
        currentText = "";
    }

    protected override void OnBlockEnd(NarrationBlock block)
    {
        if (narratorTMP != null)
            currentText = narratorTMP.text;
    }

    // ── 스탯 호버 (StatAllocatorUI 전용) ─────────────────

    /// <summary>스탯 호버 시 설명 출력. 복원 대기 중이면 취소.</summary>
    public void ShowStatDescription(string desc)
    {
        if (restoreCoroutine != null) StopCoroutine(restoreCoroutine);
        if (narratorTMP) narratorTMP.text = desc;
    }

    /// <summary>호버 해제 시 호출 — 딜레이 후 currentText 복원.</summary>
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

    // ── 레거시 호환 (TitleSceneController 등에서 직접 호출하던 메서드) ──

    /// <summary>
    /// 타이핑 없이 텍스트 즉시 교체.
    /// TitleSceneController 내부 정리 후 제거 가능.
    /// </summary>
    public void SetTextImmediate(string text)
    {
        Clear();
        if (narratorTMP) narratorTMP.text = text;
        currentText = text;
    }
}
