using UnityEngine;
using System.Collections;

/// <summary>
/// TitleScene 총괄 컨트롤러.
///
/// Phase 흐름:
///   P00_Title    → 타이틀 화면 (화면 클릭 대기)
///   P01_Intro    → 가면 등장 → 나레이션 → 공책 비행
///   P02_Allocate → 구슬 낙하 → 나레이션 → [공책 클릭] → 스탯 분배
///   P03_Random   → 타이머 만료 시 랜덤 배분 (미구현, 타이머 미정)
///   P04_Confirm  → 종이 찢기 → 공책·가면 퇴장 → 종이 건네기 → 클릭 → 게임 시작
///
/// [Inspector 연결]
///   titleData           : TitleData SO
///   narrator            : NarratorRouter
///   statAllocator       : StatAllocatorUI
///   timerUI             : TimerUI (미정 — 현재 미사용)
///   titleAnimator       : TitleAnimator
///   titleScreenGroup    : Phase 00 타이틀 CanvasGroup
///   p01NarratorDuration : 인트로 나레이터 최대 지속 시간(초)
///   allocateTimerSeconds: 스탯 분배 타이머(초, 미정)
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

#pragma warning disable CS0414 // 타이머 미정 — 확정 후 OnNotebookOpened()에서 활성화
    [SerializeField] private float allocateTimerSeconds = 30f;
#pragma warning restore CS0414

    public Phase CurrentPhase { get; private set; } = Phase.P00_Title;

    // ── 공개 콜백 ─────────────────────────────────────────

    /// <summary>P00 화면 클릭 → P01 시작.</summary>
    public void OnGameStartPressed()
    {
        if (CurrentPhase != Phase.P00_Title) return;
        StartCoroutine(RunP01());
    }

    /// <summary>스탯 분배 완료 버튼 → P04.</summary>
    public void OnAllocateConfirmed()
    {
        if (CurrentPhase != Phase.P02_Allocate) return;
        timerUI?.Stop();
        StartCoroutine(RunP04());
    }

    /// <summary>P04 종이 클릭 → 게임 시작.</summary>
    public void OnPaperClicked()
    {
        if (CurrentPhase != Phase.P04_Confirm) return;
        GameFlowManager.Instance?.StartGame();
    }

    // ── Phase 실행 ────────────────────────────────────────

    // ── P01: 가면 등장 → 나레이션 → 공책 비행 ────────────

    private IEnumerator RunP01()
    {
        CurrentPhase = Phase.P01_Intro;

        // 타이틀 UI 페이드아웃
        if (titleScreenGroup)
        {
            yield return FadeCanvasGroup(titleScreenGroup, 1f, 0f, 0.5f);
            titleScreenGroup.gameObject.SetActive(false);
        }

        // 가면 등장 — 테이블 건너편까지 이동
        if (titleAnimator)
            yield return titleAnimator.ApproachMask();

        // 나레이션 (외계 + 태양 블록 혼합)
        if (titleData?.introBlocks?.Length > 0)
            yield return RunBlocksWithDuration(titleData.introBlocks, p01NarratorDuration);

        // 나레이션 종료 후 공책 비행
        if (titleAnimator)
            yield return titleAnimator.FlyNotebook();

        StartCoroutine(RunP02());
    }

    // ── P02: 구슬 낙하 → 나레이션 → 공책 클릭 대기 → 스탯 분배 ──

    private IEnumerator RunP02()
    {
        CurrentPhase = Phase.P02_Allocate;

        // 구슬 12개 시간차 낙하
        if (titleAnimator)
            yield return titleAnimator.DropMarbles();

        // 나레이션: 공책을 클릭하라는 안내
        if (titleData?.allocateBlocks?.Length > 0)
            yield return narrator.ShowBlocks(titleData.allocateBlocks);

        // 공책 클릭 대기 → 클릭하면 공책 확대 + 스탯 UI 활성화
        if (titleAnimator)
        {
            titleAnimator.EnableNotebookClick(OnNotebookOpened);
        }
        else
        {
            // TitleAnimator 없으면 바로 열기
            OnNotebookOpened();
        }
    }

    /// <summary>공책이 열린 뒤 호출 — 스탯 분배 UI 활성화.</summary>
    private void OnNotebookOpened()
    {
        statAllocator?.Activate();

        // 타이머 미정 — 준비되면 아래 주석 해제
        // timerUI?.StartTimer(allocateTimerSeconds, OnTimerExpired);
    }

    // ── P03: 타이머 만료 (미구현) ────────────────────────

    private void OnTimerExpired()
    {
        if (CurrentPhase != Phase.P02_Allocate) return;
        StartCoroutine(RunP03());
    }

    private IEnumerator RunP03()
    {
        CurrentPhase = Phase.P03_Random;

        statAllocator?.Deactivate();
        statAllocator?.RandomizeRemaining();

        yield return new WaitForSeconds(1f);
        StartCoroutine(RunP04());
    }

    // ── P04: 종이 찢기 + 퇴장 + 종이 건네기 ─────────────

    private IEnumerator RunP04()
    {
        CurrentPhase = Phase.P04_Confirm;

        statAllocator?.CommitStats();
        statAllocator?.Deactivate();

        if (titleAnimator)
        {
            // 찢기 + 공책·가면 퇴장 + 종이 건네기
            yield return titleAnimator.TearNotebookPage();

            // 종이에 최종 스탯 출력
            titleAnimator.ShowFinalStatPaper(PlayerStats.Instance);

            // 종이 클릭 → 게임 시작
            titleAnimator.EnablePaperClick(OnPaperClicked);
        }

        // 나레이션: "이걸로 정해졌다"
        if (titleData?.confirmBlocks?.Length > 0)
            yield return narrator.ShowBlocks(titleData.confirmBlocks);
    }

    // ── 헬퍼 ─────────────────────────────────────────────

    /// <summary>블록 배열을 maxDuration 내에서 순서대로 출력.</summary>
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