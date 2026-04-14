using UnityEngine;

/// <summary>
/// IRoomRunner 구현체들이 공통으로 필요한 의존성 묶음.
/// RoomSceneController에서 생성해서 Runner에 주입.
///
/// 새 의존성이 생기면 여기에 추가 — Runner 시그니처는 바꾸지 않아도 됨.
/// </summary>
public class RoomRunContext
{
    /// <summary>나레이션 출력 담당.</summary>
    public NarratorUI NarratorUI { get; }

    /// <summary>
    /// 씬 전환 / 판정 기록 등 GameFlowManager 래핑.
    /// Runner는 GameFlowManager를 직접 참조하지 않고 RoomBridge만 사용.
    /// </summary>
    public RoomBridge Bridge { get; }

    /// <summary>
    /// 플레이어 이동 제어 — 3D 구현 전까지 stub.
    /// 추후 PlayerController 컴포넌트로 교체.
    /// </summary>
    public PlayerControllerStub PlayerController { get; }

    public RoomRunContext(
        NarratorUI narratorUI,
        RoomBridge bridge,
        PlayerControllerStub playerController)
    {
        NarratorUI = narratorUI;
        Bridge = bridge;
        PlayerController = playerController;
    }
}
