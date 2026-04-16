using System;

/// <summary>
/// 방 내 상호작용 이벤트 창구.
///
/// 두 가지 채널:
///   TriggerObject  : InteractableObject가 objectID를 발행 → BaseRoomRunner가 Phase 결정
///   TriggerPhase   : phaseID를 직접 발행 (레거시 / 특수 케이스용)
///
/// 추후 InteractableObject(3D)로 교체 시 TriggerObject() 호출부만 변경.
/// </summary>
public static class RoomEventBus
{
    /// <summary>
    /// objectID 기반 상호작용 이벤트.
    /// BaseRoomRunner가 objectID를 받아서 어떤 Phase로 연결할지 결정.
    /// </summary>
    public static event Action<string> OnObjectInteracted;

    /// <summary>
    /// phaseID 직접 발행 — RoomStart / PhaseComplete 등 내부 흐름용.
    /// </summary>
    public static event Action<string> OnPhaseRequested;

    /// <summary>오브젝트 상호작용 발행 — InteractableObject에서 호출.</summary>
    public static void TriggerObject(string objectID)
    {
        OnObjectInteracted?.Invoke(objectID);
    }

    /// <summary>phaseID 직접 발행 — 내부 흐름용.</summary>
    public static void TriggerPhase(string phaseID)
    {
        OnPhaseRequested?.Invoke(phaseID);
    }

    /// <summary>씬 전환 시 이벤트 구독 초기화 — RoomSceneController에서 호출.</summary>
    public static void Clear()
    {
        OnObjectInteracted = null;
        OnPhaseRequested = null;
    }
}