using UnityEngine;

/// <summary>
/// 영원 방 앙코르 루프용 SO.
/// 태양계 행성 순서(수금지화목토천해인)로 방을 순환.
/// 지구(isMercy=true) = 자비 방.
///
/// 각 행성은 RoomData.PhaseData[] 양식을 따르며,
/// NormalRoomRunner와 동일한 Phase 흐름으로 실행됨.
///
/// [Inspector 연결]
///   GameFlowManager.encoreRoomData 에 연결.
/// </summary>
[CreateAssetMenu(fileName = "EncoreRoomData", menuName = "AllocateStats/EncoreRoomData")]
public class EncoreRoomData : ScriptableObject
{
    [Header("행성 순서 (수금지화목토천해인 — 9개 고정)")]
    public PlanetEntry[] planets;

    public int PlanetCount => planets != null ? planets.Length : 0;

    /// <summary>loopCounter → planetIndex 변환 후 반환.</summary>
    public PlanetEntry GetPlanet(int loopCounter) =>
        planets != null ? planets[loopCounter % planets.Length] : null;

    // ─────────────────────────────────────────────────────────
    // PlanetEntry
    // ─────────────────────────────────────────────────────────

    [System.Serializable]
    public class PlanetEntry
    {
        [Header("기본 정보")]
        public string planetName;   // "수성", "금성" 등 에디터 식별용
        public bool isMercy;        // 자비 방 여부 — 지구만 true

        [Header("Phase 목록 — RoomData.PhaseData 양식과 동일")]
        public RoomData.PhaseData[] phases;

        [Header("자비 방 사망 endingID — isMercy=true 일 때만 사용")]
        public string mercyDeathEndingID = "화형";
    }
}