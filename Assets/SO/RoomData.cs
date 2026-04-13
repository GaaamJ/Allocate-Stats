using UnityEngine;

/// <summary>
/// 방 하나를 정의하는 ScriptableObject.
/// [CreateAssetMenu] 로 에디터에서 방마다 에셋 생성.
///
/// 판정 흐름은 RoomSceneController가 읽어서 실행.
/// </summary>
[CreateAssetMenu(fileName = "RoomData", menuName = "AllocateStats/RoomData")]
public class RoomData : ScriptableObject
{
    [Header("기본 정보")]
    public string roomID;           // "mirror", "monster" 등 고유 ID
    public string displayName;      // "거울 방"

    [TextArea]
    public string entryNarration;   // 방 진입 시 나레이터 텍스트

    [Header("판정 단계 목록")]
    public CheckStep[] steps;

    [Header("탈출 가능 여부")]
    public bool canEscape;          // 이 방에서 탈출 결말이 존재하는가

    // ── 내부 데이터 구조 ──────────────────────────────────

    [System.Serializable]
    public class CheckStep
    {
        [Header("이 단계의 설명 (나레이터 출력)")]
        [TextArea] public string narration;

        [Header("판정 설정")]
        public StatType stat;
        public CheckSystem.CheckType checkType;
        public int threshold;              // Threshold / LuckFixed 방식에서만 사용

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
        public int nextStepIndex;          // GoToStep 일 때 사용 (-1 = 없음)

        [TextArea] public string narration; // 결과 나레이터 텍스트
    }

    public enum OutcomeType
    {
        NextRoom,   // 다음 방으로
        GameOver,   // 게임 오버
        Escape,     // 탈출 (클리어)
        GoToStep,   // 같은 방 내 다른 단계로
        Death,      // 사망 (GameOver와 구분이 필요하면 별도 엔딩용)
    }
}
