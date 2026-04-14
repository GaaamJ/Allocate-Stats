using System;

/// <summary>
/// 방 내 상호작용 이벤트 창구.
/// 플레이어가 오브젝트와 상호작용하면 phaseID를 발행.
/// NormalRoomRunner / EncoreRoomRunner가 구독해서 해당 Phase로 분기.
///
/// 현재는 버튼 프리팹(DoorButtonUI)에서 Trigger() 호출.
/// 추후 InteractableObject(3D)로 교체 시 Trigger() 호출부만 변경.
/// </summary>
public static class RoomEventBus
{
    /// <summary>
    /// phaseID를 키로 Phase 분기 요청.
    /// 구독자(NormalRoomRunner)가 수신해서 해당 Phase 실행.
    /// </summary>
    public static event Action<string> OnPhaseRequested;

    /// <summary>phaseID에 해당하는 Phase 실행 요청.</summary>
    public static void Trigger(string phaseID)
    {
        OnPhaseRequested?.Invoke(phaseID);
    }

    /// <summary>씬 전환 시 이벤트 구독 초기화 — RoomSceneController에서 호출.</summary>
    public static void Clear()
    {
        OnPhaseRequested = null;
    }
}
