using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    private string pendingObjectID;
    private bool waitingForInteract = false;
    private bool isWaiting = false;

    // 완료된 Phase 기록 — requiredPhaseIDs 체크 및 재실행 차단용
    private readonly HashSet<string> completedPhaseIDs = new HashSet<string>();

    // ── 서브클래스 구현 ───────────────────────────────────

    protected abstract RoomData.PhaseData[] GetRoomPhases();
    protected abstract string GetRoomLabel();
    protected abstract void OnRoomComplete();

    // ── IRoomRunner ───────────────────────────────────────

    public IEnumerator Run(RoomRunContext context)
    {
        ctx = context;
        checkExecutor = new CheckExecutor(ctx.Bridge, GetRoomLabel());

        // objectID 기반 상호작용 구독
        RoomEventBus.OnObjectInteracted += OnObjectInteracted;

        int startIndex = FindPhaseByTrigger(RoomData.TriggerCondition.RoomStart);
        if (startIndex < 0)
        {
            Debug.LogError($"[{GetType().Name}] TriggerCondition.RoomStart Phase 없음 ({GetRoomLabel()})");
            yield break;
        }

        yield return RunPhase(startIndex);

        RoomEventBus.OnObjectInteracted -= OnObjectInteracted;
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
                outcome = phase.outcome;
                break;

            case RoomData.ExitCondition.Check:
                yield return RunCheck(phase, index);
                outcome = checkOutcome;
                break;
        }

        // 종료 연출
        if (phase.animator) yield return phase.animator.OnPhaseExit();

        // 나레이션 클리어
        ctx.NarratorUI.Clear();

        // Phase 완료 기록 — isRepeatable이면 기록하지 않아 재실행 허용
        if (!string.IsNullOrEmpty(phase.phaseID) && !phase.isRepeatable)
            completedPhaseIDs.Add(phase.phaseID);

        if (outcome != null)
            yield return HandleOutcome(outcome);
        else
            Debug.LogWarning($"[{GetType().Name}] outcome이 null ({GetRoomLabel()} / {label})");
    }

    // ── 대기 상태 ─────────────────────────────────────────

    /// <summary>
    /// 플레이어 자유 이동 허용 + RoomEventBus 구독 대기.
    /// objectID 수신 시 해당 오브젝트와 매칭되는 Phase 후보 탐색.
    /// requiredPhaseIDs 충족 + 미완료 Phase 중 첫 번째 실행.
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

            // objectID로 실행 가능한 Phase 탐색
            int targetIndex = FindEligiblePhase(pendingObjectID);

            if (targetIndex < 0)
            {
                Debug.Log($"[{GetType().Name}] objectID '{pendingObjectID}' — 실행 가능한 Phase 없음.");
                waitingForInteract = false;
                continue;
            }

            var phase = GetRoomPhases()[targetIndex];

            // 선행 조건 미충족 시 나레이션 출력 후 대기 유지
            if (!IsRequirementMet(phase))
            {
                Debug.Log($"[{GetType().Name}] '{phase.phaseID}' 선행 조건 미충족 — 대기 유지.");
                if (phase.requirementFailNarration != null && phase.requirementFailNarration.Length > 0)
                    yield return ctx.NarratorUI.ShowBlocks(phase.requirementFailNarration);
                waitingForInteract = false;
                continue;
            }

            yield return RunPhase(targetIndex);
        }
    }

    /// <summary>
    /// objectID와 매칭되는 Phase 후보 중
    /// 미완료 + 선행 조건 충족인 첫 번째 Phase 인덱스 반환.
    /// 없으면 -1.
    /// </summary>
    private int FindEligiblePhase(string objectID)
    {
        var phases = GetRoomPhases();
        if (phases == null) return -1;

        for (int i = 0; i < phases.Length; i++)
        {
            var phase = phases[i];

            // triggerCondition이 Interact가 아니면 스킵
            if (phase.triggerCondition != RoomData.TriggerCondition.Interact) continue;

            // objectID 매칭 체크 — 비어있으면 모든 오브젝트에 반응
            if (!string.IsNullOrEmpty(phase.triggerObjectID) && phase.triggerObjectID != objectID) continue;

            // 이미 완료된 Phase 스킵 — isRepeatable이면 스킵하지 않음
            if (!phase.isRepeatable && !string.IsNullOrEmpty(phase.phaseID) && completedPhaseIDs.Contains(phase.phaseID)) continue;

            // 선행 조건 충족 여부 체크
            if (!IsRequirementMet(phase)) continue;

            return i;
        }

        return -1;
    }

    /// <summary>requiredPhaseIDs 전부 완료됐는지 확인.</summary>
    private bool IsRequirementMet(RoomData.PhaseData phase)
    {
        if (phase.requiredPhaseIDs == null || phase.requiredPhaseIDs.Length == 0)
            return true;

        foreach (var id in phase.requiredPhaseIDs)
            if (!completedPhaseIDs.Contains(id)) return false;

        return true;
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

    private void OnObjectInteracted(string objectID)
    {
        if (!waitingForInteract) return;
        pendingObjectID = objectID;
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
