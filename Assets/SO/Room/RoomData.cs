using UnityEngine;

/// <summary>
/// 방 하나를 정의하는 ScriptableObject.
/// 방은 여러 Phase로 구성되며, 각 Phase는 독립적인 트리거 / 나레이션 / 판정 / 연출을 가진다.
///
/// [Inspector 연결]
///   GameFlowManager.roomSequence 배열에 순서대로 등록.
/// </summary>
[CreateAssetMenu(fileName = "RoomData", menuName = "AllocateStats/RoomData")]
public class RoomData : ScriptableObject
{
    [Header("기본 정보")]
    public string roomID;       // 고유 ID — RoomBridge, PhaseAnimator 식별에 사용
    public string displayName;  // 에디터 및 UI 표시용

    [Header("Phase 목록")]
    public PhaseData[] phases;

    // ─────────────────────────────────────────────────────────
    // PhaseData
    // ─────────────────────────────────────────────────────────

    [System.Serializable]
    public class PhaseData
    {
        [Header("디버깅용 식별자 — 로그 및 RoomEventBus 매칭 키")]
        public string phaseID;

        [Header("트리거 조건 — 이 Phase는 어떻게 시작되는가")]
        public TriggerCondition triggerCondition;

        [Header("선행 조건")]
        // 비워두면 조건 없음. TriggerCondition == Interact일 때만 유효.
        public string[] requiredPhaseIDs;

        [Header("선행 조건 미충족 시 출력할 나레이션")]
        // 비워두면 스킵. requiredPhaseIDs가 있을 때만 유효.
        [TextArea(2, 6)] public string[] requirementFailNarration;

        [Header("진입 나레이션 (블록 단위)")]
        [TextArea(2, 6)] public string[] onEnter;

        [Header("종료 조건 — 이 Phase는 어떻게 끝나는가")]
        public ExitCondition exitCondition;

        [Header("판정 데이터 — exitCondition == Check 일 때만 사용")]
        public CheckData checkData;

        [Header("완료 결과 — exitCondition == Auto 일 때 사용")]
        public OutcomeData outcome;

        [Header("연출 — 없으면 스킵")]
        public PhaseAnimator animator;
    }

    // ─────────────────────────────────────────────────────────
    // TriggerCondition
    // ─────────────────────────────────────────────────────────

    public enum TriggerCondition
    {
        RoomStart,      // 방 입장 시 자동 실행 — 방의 첫 Phase
        Interact,       // 대기 상태에서 플레이어 상호작용 시 (phaseID 매칭)
        PhaseComplete,  // 이전 Phase 완료 후 자동 연결 (선형 흐름용)
    }

    // ─────────────────────────────────────────────────────────
    // ExitCondition
    // ─────────────────────────────────────────────────────────

    public enum ExitCondition
    {
        Auto,   // 나레이션 종료 후 자동 종료 → outcome으로 분기
        Check,  // 판정 실행 후 결과에 따라 분기 → checkData.onSuccess / onFailure
    }

    // ─────────────────────────────────────────────────────────
    // CheckData
    // ─────────────────────────────────────────────────────────

    [System.Serializable]
    public class CheckData
    {
        [Header("판정 전 나레이션")]
        [TextArea(2, 6)] public string[] onBeforeCheck;

        [Header("판정 후 나레이션 — 결과 나레이션 이전 출력")]
        [TextArea(2, 6)] public string[] onAfterCheck;

        [Header("판정 설정")]
        public StatType stat;
        public CheckSystem.CheckType checkType;
        public int threshold;

        [Header("복합 판정 — checkType == Compound 일 때만 사용")]
        public StatType stat2;
        public int threshold2;

        [Header("결과 분기")]
        public OutcomeData onSuccess;
        public OutcomeData onFailure;

        [Header("판정 기록 요약 — EndingScene 히스토리 패널에 표시")]
        public string summaryText_success;
        public string summaryText_failure;
    }

    // ─────────────────────────────────────────────────────────
    // OutcomeData
    // ─────────────────────────────────────────────────────────

    [System.Serializable]
    public class OutcomeData
    {
        [Header("결과 나레이션 — 분기 전 출력")]
        [TextArea(2, 6)] public string[] narration;

        [Header("결과 타입")]
        public OutcomeType type;

        [Header("PhaseTo 전용 — 이동할 Phase의 phaseID")]
        public string targetPhaseID;

        [Header("Death / Escape 전용 — EndingData roomID와 매칭")]
        public string endingID;
    }

    // ─────────────────────────────────────────────────────────
    // OutcomeType
    // ─────────────────────────────────────────────────────────

    public enum OutcomeType
    {
        PhaseTo,        // 특정 Phase로 이동 — targetPhaseID로 매칭
        ReturnToWait,   // 대기 상태로 전환
        NextRoom,       // 다음 방으로 이동
        Escape,         // 탈출 — 게임 클리어
        Death,          // 사망 — 게임 오버
    }
}