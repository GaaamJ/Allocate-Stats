using UnityEngine;

/// <summary>
/// Room 씬과 GameFlowManager(레거시) 사이의 연결 지점.
/// Runner는 GameFlowManager를 직접 참조하지 않고 이 클래스만 사용.
///
/// GameFlowManager 리팩토링 시 이 파일 내부만 교체하면 Runner는 건드리지 않아도 됨.
/// </summary>
public class RoomBridge : MonoBehaviour
{
    // ── 흐름 제어 ─────────────────────────────────────────

    /// <summary>현재 방 클리어 → 다음 방 또는 앙코르 루프 진입.</summary>
    public void OnRoomComplete()
    {
        GameFlowManager.Instance?.OnRoomClear_NextRoom();
    }

    /// <summary>앙코르 방 클리어 → 다음 앙코르.</summary>
    public void OnEncoreComplete()
    {
        GameFlowManager.Instance?.OnEncoreClear();
    }

    /// <summary>사망 — EndingScene으로 전환.</summary>
    public void OnDeath(string endingID)
    {
        GameFlowManager.Instance?.OnDeath(endingID);
    }

    /// <summary>탈출 — EndingScene으로 전환.</summary>
    public void OnEscape(string endingID)
    {
        GameFlowManager.Instance?.OnEscape(endingID);
    }

    // ── 판정 기록 ─────────────────────────────────────────

    /// <summary>판정 결과를 GameFlowManager.CheckHistory에 기록.</summary>
    public void RecordCheck(StatType stat, bool success, string context, string summaryText = "")
    {
        GameFlowManager.Instance?.RecordCheck(stat, success, context, summaryText);
    }

    // ── 상태 조회 ─────────────────────────────────────────

    /// <summary>현재 앙코르 루프 여부.</summary>
    public bool IsEncoreLoop => GameFlowManager.Instance?.IsEncoreLoop ?? false;

    /// <summary>현재 앙코르 카운터.</summary>
    public int EncoreCounter => GameFlowManager.Instance?.EncoreCounter ?? 0;

    /// <summary>현재 방 RoomData.</summary>
    public RoomData CurrentRoomData => GameFlowManager.Instance?.CurrentRoomData;

    /// <summary>현재 방 RoomLayoutData. null이면 오브젝트 생성 스킵.</summary>
    public RoomLayoutData CurrentLayoutData => GameFlowManager.Instance?.CurrentLayoutData;

    /// <summary>앙코르 루프 데이터.</summary>
    public EncoreRoomData EncoreRoomData => GameFlowManager.Instance?.EncoreRoomData;
}