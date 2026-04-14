using UnityEngine;
using System.Collections;

/// <summary>
/// 방 실행 공통 로직 베이스 클래스.
/// Phase 실행 / 대기 상태 / 판정 / OutcomeType 처리를 담당.
///
/// 서브클래스에서 구현해야 할 것:
///   - GetRoomPhases()  : 실행할 PhaseData 배열 반환
///   - GetRoomLabel()   : 로그 식별용 문자열 반환
///   - OnRoomComplete() : 방 완료 시 처리
/// </summary>
public abstract class BaseRoomRunner : IRoomRunner
{
    protected RoomRunContext ctx;
    protected CheckExecutor checkExecutor;

    // RunCheck() 결과 캐시 — Unity 코루틴은 IEnumerator<T> 미지원
    private RoomData.OutcomeData checkOutcome;

    // 대기 상태
    private string pendingPhaseID;
    private bool waitingForInteract = false;
    private bool isWaiting = false;

    // ── 서브클래스 구현 ───────────────────────────────────

    protected abstract RoomData.PhaseData[] GetRoomPhases();
    protected abstract string GetRoomLabel();
    protected abstract void OnRoomComplete();

    // ── IRoomRunner ───────────────────────────────────────

    public IEnumerator Run(RoomRunContext context)
    {
        ctx = context;
        checkExecutor = new CheckExecutor(ctx.Bridge, GetRoomLabel());

        RoomEventBus.OnPhaseRequested += OnPhaseRequested;

        int startIndex = FindPhaseByTrigger(RoomData.TriggerCondition.RoomStart);
        if (startIndex < 0)
        {
            Debug.LogError($"[{GetType().Name}] TriggerCondition.RoomStart Phase 없음 ({GetRoomLabel()})");
            yield break;
        }

        yield return RunPhase(startIndex);

        RoomEventBus.OnPhaseRequested -= OnPhaseRequested;
    }

    // ── Phase 실행 ────────────────────────────────────────

    private IEnumerator RunPhase(int index)
    {
        var phases = GetRoomPhases();

        if (phases == null || index < 0 || index >= phases.Length)
        {
            Debug.LogError($"[{GetType().Name}] Phase 인덱스 {index} 없음 ({GetRoomLabel()})");
            yield break;
        }

        var phase = phases[index];
        string label = string.IsNullOrEmpty(phase.phaseID) ? $"phase{index}" : phase.phaseID;
        Debug.Log($"[{GetType().Name}] {GetRoomLabel()} / {label} 진입");

        // 진입 연출
        if (phase.animator) yield return phase.animator.OnPhaseEnter();

        // 진입 나레이션
        if (phase.onEnter != null && phase.onEnter.Length > 0)
            yield return ctx.NarratorUI.ShowBlocks(phase.onEnter);

        // ExitCondition 분기
        RoomData.OutcomeData outcome = null;

        switch (phase.exitCondition)
        {
            case RoomData.ExitCondition.Auto:
                // Phase 자체의 outcome으로 분기
                outcome = phase.outcome;
                break;

            case RoomData.ExitCondition.Check:
                yield return RunCheck(phase, index);
                outcome = checkOutcome;
                break;
        }

        // 종료 연출
        if (phase.animator) yield return phase.animator.OnPhaseExit();

        // Phase 나레이션 클리어
        ctx.NarratorUI.Clear();

        if (outcome != null)
            yield return HandleOutcome(outcome);
        else
            Debug.LogWarning($"[{GetType().Name}] outcome이 null ({GetRoomLabel()} / {label})");
    }

    // ── 대기 상태 ─────────────────────────────────────────

    /// <summary>
    /// 플레이어 자유 이동 허용 + RoomEventBus 구독 대기.
    /// 상호작용 발생 시 해당 Phase 실행.
    /// Phase 완료 후 outcome이 ReturnToWait이면 다시 대기 유지.
    /// </summary>
    private IEnumerator EnterWaitState()
    {
        ctx.PlayerController?.EnableMovement();
        isWaiting = true;

        while (isWaiting)
        {
            waitingForInteract = true;
            yield return new UnityEngine.WaitUntil(() => !waitingForInteract);

            if (!isWaiting) yield break;

            int targetIndex = FindPhaseByID(pendingPhaseID);
            if (targetIndex < 0)
            {
                Debug.LogError($"[{GetType().Name}] phaseID '{pendingPhaseID}' 를 찾을 수 없음.");
                waitingForInteract = false;
                continue;
            }

            yield return RunPhase(targetIndex);
        }
    }

    /// <summary>대기 상태 종료 — NextRoom / Escape / Death 시 호출.</summary>
    private void ExitWaitState()
    {
        isWaiting = false;
        waitingForInteract = false;
    }

    // ── 판정 실행 ─────────────────────────────────────────

    private IEnumerator RunCheck(RoomData.PhaseData phase, int phaseIndex)
    {
        var check = phase.checkData;

        if (check == null)
        {
            Debug.LogError($"[{GetType().Name}] checkData가 null ({GetRoomLabel()}, phase: {phaseIndex})");
            yield break;
        }

        if (check.onBeforeCheck != null && check.onBeforeCheck.Length > 0)
            yield return ctx.NarratorUI.ShowBlocks(check.onBeforeCheck);

        if (phase.animator) yield return phase.animator.OnBeforeCheck();

        bool success = checkExecutor.Execute(check, phaseIndex);

        if (phase.animator) yield return phase.animator.OnAfterCheck(success);

        if (check.onAfterCheck != null && check.onAfterCheck.Length > 0)
            yield return ctx.NarratorUI.ShowBlocks(check.onAfterCheck);

        checkOutcome = success ? check.onSuccess : check.onFailure;

        if (checkOutcome.narration != null && checkOutcome.narration.Length > 0)
            yield return ctx.NarratorUI.ShowBlocks(checkOutcome.narration);
    }

    // ── OutcomeType 처리 ──────────────────────────────────

    private IEnumerator HandleOutcome(RoomData.OutcomeData outcome)
    {
        switch (outcome.type)
        {
            case RoomData.OutcomeType.PhaseTo:
                int targetIndex = FindPhaseByID(outcome.targetPhaseID);
                if (targetIndex < 0)
                {
                    Debug.LogError($"[{GetType().Name}] targetPhaseID '{outcome.targetPhaseID}' 를 찾을 수 없음.");
                    yield break;
                }
                yield return RunPhase(targetIndex);
                break;

            case RoomData.OutcomeType.ReturnToWait:
                yield return EnterWaitState();
                break;

            case RoomData.OutcomeType.NextRoom:
                ExitWaitState();
                OnRoomComplete();
                break;

            case RoomData.OutcomeType.Escape:
                ExitWaitState();
                ctx.Bridge.OnEscape(outcome.endingID);
                break;

            case RoomData.OutcomeType.Death:
                ExitWaitState();
                ctx.Bridge.OnDeath(outcome.endingID);
                break;
        }
    }

    // ── RoomEventBus 수신 ─────────────────────────────────

    private void OnPhaseRequested(string phaseID)
    {
        if (!waitingForInteract) return;
        pendingPhaseID = phaseID;
        waitingForInteract = false;
    }

    // ── 탐색 헬퍼 ────────────────────────────────────────

    private int FindPhaseByID(string phaseID)
    {
        var phases = GetRoomPhases();
        for (int i = 0; i < phases.Length; i++)
            if (phases[i].phaseID == phaseID) return i;
        return -1;
    }

    private int FindPhaseByTrigger(RoomData.TriggerCondition trigger)
    {
        var phases = GetRoomPhases();
        for (int i = 0; i < phases.Length; i++)
            if (phases[i].triggerCondition == trigger) return i;
        return -1;
    }
}