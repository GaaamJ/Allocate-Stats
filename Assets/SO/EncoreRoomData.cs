using UnityEngine;

/// <summary>
/// 영원 방 앙코르 루프용 SO.
/// 태양계 행성 순서(수금지화목토천해인)로 방을 순환.
/// 지구(인덱스 2) = 자비 방.
///
/// [Inspector 연결 목록]
///   - GameFlowManager.encoreRoomData 에 연결
/// </summary>
[CreateAssetMenu(fileName = "EncoreRoomData", menuName = "AllocateStats/EncoreRoomData")]
public class EncoreRoomData : ScriptableObject
{
    [Header("행성 순서 (수금지화목토천해인 — 9개 고정)")]
    public PlanetEntry[] planets;   // 반드시 9개

    [Header("자비 방 (지구) 사망 endingID")]
    public string mercyDeathEndingID = "화형";

    public int PlanetCount => planets != null ? planets.Length : 0;

    /// <summary>지구(자비 방) 인덱스인지 확인.</summary>
    public bool IsMercy(int planetIndex) =>
        planets != null && planetIndex < planets.Length && planets[planetIndex].isMercy;

    /// <summary>현재 행성 엔트리 반환. loopCounter → planetIndex로 변환.</summary>
    public PlanetEntry GetPlanet(int loopCounter) =>
        planets != null ? planets[loopCounter % planets.Length] : null;

    // ── 데이터 구조 ───────────────────────────────────────

    [System.Serializable]
    public class PlanetEntry
    {
        public string planetName;               // "수성", "금성" 등 에디터 식별용
        public bool isMercy;                  // 자비 방 여부 (지구만 true)

        [Header("앙코르 나레이션 (블록 단위)")]
        [TextArea(2, 6)] public string[] narration;

        [Header("자비 방 전용 (isMercy = true일 때만)")]
        [TextArea(2, 6)] public string[] mercyDeathNarration;    // 사망 시
        [TextArea(2, 6)] public string[] mercySurviveNarration;  // 생존 시
    }
}