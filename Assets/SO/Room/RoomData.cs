using UnityEngine;

[CreateAssetMenu(fileName = "RoomData", menuName = "AllocateStats/RoomData")]
public class RoomData : ScriptableObject
{
    [Header("기본 정보")]
    public string roomID;
    public string displayName;

    [TextArea]
    public string entryNarration;

    [Header("판정 단계 목록")]
    public CheckStep[] steps;

    [Header("탈출 가능 여부")]
    public bool canEscape;

    [System.Serializable]
    public class CheckStep
    {
        [Header("이 단계의 설명 (나레이터 출력)")]
        [TextArea] public string narration;

        [Header("판정 설정")]
        public StatType stat;
        public CheckSystem.CheckType checkType;
        public int threshold;

        [Header("복합 판정 (CheckType.Compound 일 때만 사용)")]
        public StatType stat2;
        public int threshold2;

        [Header("결과 분기")]
        public StepOutcome onSuccess;
        public StepOutcome onFailure;

        [Header("엔딩 요약 텍스트 (판정 기록 패널용)")]
        public string endingSummary_success;
        public string endingSummary_failure;
    }

    [System.Serializable]
    public class StepOutcome
    {
        public OutcomeType type;
        public int nextStepIndex;

        [TextArea] public string narration;
    }

    public enum OutcomeType
    {
        NextRoom,
        GameOver,
        Escape,
        GoToStep,
        Death,
    }
}