using UnityEngine;

/// <summary>
/// 모든 엔딩 텍스트를 담는 SO 테이블.
/// 에셋 1개만 만들어서 EndingSceneController에 연결.
/// </summary>
[CreateAssetMenu(fileName = "EndingData", menuName = "AllocateStats/EndingData")]
public class EndingData : ScriptableObject
{
    [Header("클리어 엔딩 목록")]
    public ClearEntry[] clearEntries;

    [Header("게임오버 엔딩 목록")]
    public GameOverEntry[] gameOverEntries;

    // ── 조회 API ──────────────────────────────────────────

    /// <summary>
    /// 클리어 엔딩 텍스트 반환.
    /// HUM 수치로 low/high 분기, HUM 특수엔딩 가능 여부도 out으로 전달.
    /// </summary>
    public string GetClearNarration(string roomID, int hum, out bool isHumEnding)
    {
        foreach (var e in clearEntries)
        {
            if (e.roomID != roomID) continue;

            isHumEnding = e.humEndingEligible && hum >= e.humThreshold;
            return hum >= e.humThreshold ? e.narration_high : e.narration_low;
        }

        // roomID 매칭 없음 — 폴백
        isHumEnding = false;
        return "탈출했다.";
    }

    /// <summary>게임오버 엔딩 텍스트 반환.</summary>
    public string GetGameOverNarration(string roomID)
    {
        foreach (var e in gameOverEntries)
            if (e.roomID == roomID) return e.narration;
        return "끝났다.";
    }

    // ── 데이터 구조 ───────────────────────────────────────

    [System.Serializable]
    public class ClearEntry
    {
        public string roomID;
        public string endingName;           // 에디터 식별용 ("성공적인 거래" 등)

        [Header("HUM 분기")]
        public int humThreshold = 3;       // 기획서: 0~2 // 3~4
        public bool humEndingEligible;      // 이 방에서 HUM 특수엔딩 발동 가능?

        [TextArea] public string narration_low;   // HUM < threshold
        [TextArea] public string narration_high;  // HUM >= threshold (특수엔딩 포함)
    }

    [System.Serializable]
    public class GameOverEntry
    {
        public string roomID;
        public string endingName;           // "뒤집힌 것" 등
        [TextArea] public string narration;
    }
}