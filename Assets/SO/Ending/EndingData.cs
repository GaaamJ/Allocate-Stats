using UnityEngine;

/// <summary>
/// HUM 분기가 있는 탈출 엔딩 텍스트만 담는 SO 테이블.
/// 일반 사망/탈출 나레이션은 RoomData.StepOutcome.narration[] 에서 처리.
/// 에셋 1개만 만들어서 EndingSceneController에 연결.
/// </summary>
[CreateAssetMenu(fileName = "EndingData", menuName = "AllocateStats/EndingData")]
public class EndingData : ScriptableObject
{
    [Header("탈출 엔딩 목록 (HUM 분기 있는 것만)")]
    public EscapeEntry[] escapeEntries;

    // ── 조회 API ──────────────────────────────────────────

    /// <summary>
    /// HUM 분기 탈출 엔딩 나레이션 블록 배열 반환.
    /// isHumEnding: HUM 특수엔딩 발동 여부.
    /// endingID 매칭 없으면 null 반환 — 호출부에서 null 체크 필요.
    /// </summary>
    public string[] GetEscapeNarration(string endingID, int hum, out bool isHumEnding)
    {
        foreach (var e in escapeEntries)
        {
            if (e.endingID != endingID) continue;
            isHumEnding = e.humEndingEligible && hum >= e.humThreshold;
            return hum >= e.humThreshold ? e.narration_high : e.narration_low;
        }
        isHumEnding = false;
        return null;
    }

    // ── 데이터 구조 ───────────────────────────────────────

    [System.Serializable]
    public class EscapeEntry
    {
        public string endingID;
        public string endingName;           // 에디터 식별용 ("성공적인 거래" 등)

        [Header("HUM 분기")]
        public int humThreshold = 3;       // 기획서: 0~2 // 3~4
        public bool humEndingEligible;      // 이 엔딩에서 HUM 특수엔딩 발동 가능?

        [TextArea(2, 6)] public string[] narration_low;   // HUM < threshold
        [TextArea(2, 6)] public string[] narration_high;  // HUM >= threshold
    }
}