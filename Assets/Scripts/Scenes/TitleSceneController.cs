using UnityEngine;
using System.Collections;

/// <summary>
/// TitleScene 총괄 컨트롤러.
///
/// Phase 흐름:
///   P00_Title   → 타이틀 화면 (게임 시작 버튼 대기)
///   P01_Intro   → 나레이터 인트로 + 공책 등장
///   P02_Allocate→ 스탯 분배 (타이머)
///   P03_Random  → 타이머 만료 시 남은 포인트 랜덤 배분
///   P04_Confirm → 최종 스탯 확인 (종이 클릭 → 게임 시작)
///
/// [Inspector 연결 목록]
///   - narratorUI         : NarratorUI
///   - statAllocator      : StatAllocatorUI
///   - timerUI            : TimerUI
///   - titleAnimator      : TitleAnimator (없으면 연출 스킵)
///   - titleScreenGroup   : Phase 00 타이틀 CanvasGroup (없으면 스킵)
///   - p01NarratorDuration: 인트로 나레이터 최대 지속 시간 (초)
///   - allocateTimerSeconds: 스탯 분배 타이머 (초)
/// </summary>
public class TitleSceneController : MonoBehaviour
{
    public enum Phase { P00_Title, P01_Intro, P02_Allocate, P03_Random, P04_Confirm }

    [Header("Sub-Controllers")]
    [SerializeField] private NarratorUI narratorUI;
    [SerializeField] private StatAllocatorUI statAllocator;
    [SerializeField] private TimerUI timerUI;
    [SerializeField] private TitleAnimator titleAnimator;  // 없으면 연출 스킵

    [Header("Phase 00")]
    [SerializeField] private CanvasGroup titleScreenGroup;  // 없으면 스킵

    [Header("Timing")]
    [SerializeField] private float p01NarratorDuration = 15f;
    [SerializeField] private float allocateTimerSeconds = 20f;

    public Phase CurrentPhase { get; private set; } = Phase.P00_Title;

    // ── 공개 콜백 ─────────────────────────────────────────

    public void OnGameStartPressed()
    {
        if (CurrentPhase != Phase.P00_Title) return;
        StartCoroutine(RunP01());
    }

    public void OnAllocateConfirmed()
    {
        if (CurrentPhase != Phase.P02_Allocate) return;
        if (timerUI) timerUI.Stop();
        StartCoroutine(RunP04());
    }

    public void OnPaperClicked()
    {
        if (CurrentPhase != Phase.P04_Confirm) return;
        GameFlowManager.Instance?.StartGame();
    }

    // ── Phase 실행 ────────────────────────────────────────

    private IEnumerator RunP01()
    {
        CurrentPhase = Phase.P01_Intro;

        if (titleScreenGroup)
        {
            yield return FadeCanvasGroup(titleScreenGroup, 1f, 0f, 0.5f);
            titleScreenGroup.gameObject.SetActive(false);
        }

        if (titleAnimator) titleAnimator.ShowNotebook();
        if (narratorUI) yield return narratorUI.PlayIntro(p01NarratorDuration);

        StartCoroutine(RunP02());
    }

    private IEnumerator RunP02()
    {
        CurrentPhase = Phase.P02_Allocate;

        if (titleAnimator) yield return titleAnimator.SummonObjects();

        if (timerUI) timerUI.StartTimer(allocateTimerSeconds, OnTimerExpired);
        if (statAllocator) statAllocator.Activate();
    }

    private void OnTimerExpired()
    {
        if (CurrentPhase != Phase.P02_Allocate) return;
        StartCoroutine(RunP03());
    }

    private IEnumerator RunP03()
    {
        CurrentPhase = Phase.P03_Random;

        if (statAllocator) statAllocator.Deactivate();
        if (titleAnimator) yield return titleAnimator.ScatterMarbles();
        if (statAllocator) statAllocator.RandomizeRemaining();

        yield return new WaitForSeconds(1f);

        StartCoroutine(RunP04());
    }

    private IEnumerator RunP04()
    {
        CurrentPhase = Phase.P04_Confirm;

        if (statAllocator) statAllocator.CommitStats();

        if (titleAnimator)
        {
            yield return titleAnimator.TearNotebookPage();
            titleAnimator.ShowFinalStatPaper(PlayerStats.Instance);
            titleAnimator.EnablePaperClick(OnPaperClicked);
        }

        if (narratorUI) yield return narratorUI.PlayConfirm();
    }

    // ── 헬퍼 ─────────────────────────────────────────────

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        float t = 0f;
        cg.alpha = from;
        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        cg.alpha = to;
    }
}
