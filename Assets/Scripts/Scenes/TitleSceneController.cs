using UnityEngine;
using System.Collections;

/// <summary>
/// TitleScene 총괄 컨트롤러.
///
/// Phase 흐름 (선형, 분기 없음):
///   P00_Title    → 타이틀 화면 (게임 시작 버튼 대기)
///   P01_Intro    → 나레이터 인트로 + 공책 등장
///   P02_Allocate → 스탯 분배 (타이머)
///   P03_Random   → 타이머 만료 시 남은 포인트 랜덤 배분
///   P04_Confirm  → 최종 스탯 확인 (종이 클릭 → 게임 시작)
///
/// [Inspector 연결]
///   titleData           : TitleData SO
///   narrator            : NarratorRouter
///   statAllocator       : StatAllocatorUI
///   timerUI             : TimerUI
///   titleAnimator       : TitleAnimator (없으면 연출 스킵)
///   titleScreenGroup    : Phase 00 타이틀 CanvasGroup (없으면 스킵)
///   p01NarratorDuration : 인트로 나레이터 최대 지속 시간(초)
///   allocateTimerSeconds: 스탯 분배 타이머(초)
/// </summary>
public class TitleSceneController : MonoBehaviour
{
    public enum Phase { P00_Title, P01_Intro, P02_Allocate, P03_Random, P04_Confirm }

    [Header("Data")]
    [SerializeField] private TitleData titleData;

    [Header("Sub-Controllers")]
    [SerializeField] private NarratorRouter narrator;
    [SerializeField] private StatAllocatorUI statAllocator;
    [SerializeField] private TimerUI timerUI;
    [SerializeField] private TitleAnimator titleAnimator;

    [Header("Phase 00")]
    [SerializeField] private CanvasGroup titleScreenGroup;

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
        timerUI?.Stop();
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

        titleAnimator?.ShowNotebook();

        if (titleData?.introBlocks?.Length > 0)
            yield return RunBlocksWithDuration(titleData.introBlocks, p01NarratorDuration);

        StartCoroutine(RunP02());
    }

    private IEnumerator RunP02()
    {
        CurrentPhase = Phase.P02_Allocate;

        if (titleAnimator) yield return titleAnimator.SummonObjects();

        if (titleData?.allocateBlocks?.Length > 0)
            yield return narrator.ShowBlocks(titleData.allocateBlocks);

        timerUI?.StartTimer(allocateTimerSeconds, OnTimerExpired);
        statAllocator?.Activate();
    }

    private void OnTimerExpired()
    {
        if (CurrentPhase != Phase.P02_Allocate) return;
        StartCoroutine(RunP03());
    }

    private IEnumerator RunP03()
    {
        CurrentPhase = Phase.P03_Random;

        statAllocator?.Deactivate();
        if (titleAnimator) yield return titleAnimator.ScatterMarbles();
        statAllocator?.RandomizeRemaining();

        yield return new WaitForSeconds(1f);
        StartCoroutine(RunP04());
    }

    private IEnumerator RunP04()
    {
        CurrentPhase = Phase.P04_Confirm;

        statAllocator?.CommitStats();

        if (titleAnimator)
        {
            yield return titleAnimator.TearNotebookPage();
            titleAnimator.ShowFinalStatPaper(PlayerStats.Instance);
            titleAnimator.EnablePaperClick(OnPaperClicked);
        }

        if (titleData?.confirmBlocks?.Length > 0)
            yield return narrator.ShowBlocks(titleData.confirmBlocks);
    }

    // ── 헬퍼 ─────────────────────────────────────────────

    /// <summary>
    /// 블록 배열을 maxDuration 시간 내에서 순서대로 출력.
    /// maxDuration이 0이면 끝날 때까지 출력.
    /// </summary>
    private IEnumerator RunBlocksWithDuration(NarrationBlock[] blocks, float maxDuration)
    {
        float elapsed = 0f;
        foreach (var block in blocks)
        {
            if (block == null || string.IsNullOrEmpty(block.text)) continue;
            if (maxDuration > 0f && elapsed >= maxDuration) yield break;

            yield return narrator.ShowText(block);

            float pause = block.pauseAfter > 0f ? block.pauseAfter : 0.8f;
            elapsed += pause;
            yield return new WaitForSeconds(pause);
        }
    }

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
