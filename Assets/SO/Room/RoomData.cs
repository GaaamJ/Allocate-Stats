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
    public string roomID;
    public string displayName;

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

        [Header("트리거 오브젝트 — triggerCondition == Interact 일 때 매칭할 objectID")]
        public string triggerObjectID;

        [Header("선행 조건 — 이 Phase 트리거 전 완료돼야 하는 phaseID 목록")]
        public string[] requiredPhaseIDs;

        [Header("성공 선행 조건 — 반드시 성공으로 완료돼야 하는 phaseID 목록")]
        public string[] requiredSuccessPhaseIDs;

        [Header("반복 실행 가능 여부")]
        public bool isRepeatable;

        [Header("선행 조건 미충족 시 출력할 나레이션")]
        public NarrationBlock[] requirementFailNarration;

        [Header("진입 나레이션 (블록 단위)")]
        public NarrationBlock[] onEnter;

        [Header("종료 조건")]
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
        RoomStart,
        Interact,
        PhaseComplete,
    }

    // ─────────────────────────────────────────────────────────
    // ExitCondition
    // ─────────────────────────────────────────────────────────

    public enum ExitCondition
    {
        Auto,
        Check,
    }

    // ─────────────────────────────────────────────────────────
    // CheckData
    // ─────────────────────────────────────────────────────────

    [System.Serializable]
    public class CheckData
    {
        [Header("판정 전 나레이션")]
        public NarrationBlock[] onBeforeCheck;

        [Header("판정 후 나레이션 — 결과 나레이션 이전 출력")]
        public NarrationBlock[] onAfterCheck;

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

        [Header("판정 기록 요약")]
        public string summaryText_success;
        public string summaryText_failure;
    }

    // ─────────────────────────────────────────────────────────
    // OutcomeData
    // ─────────────────────────────────────────────────────────

    [System.Serializable]
    public class OutcomeData
    {
        [Header("결과 나레이션")]
        public NarrationBlock[] narration;

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
        PhaseTo,
        ReturnToWait,
        NextRoom,
        Escape,
        Death,
    }
}
