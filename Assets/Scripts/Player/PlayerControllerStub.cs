using UnityEngine;

/// <summary>
/// 플레이어 이동 제어 stub.
/// 3D PlayerController 구현 전까지 RoomRunContext에서 사용.
/// 실제 구현으로 교체할 때 이 파일을 대체하거나 인터페이스로 추상화.
/// </summary>
public class PlayerControllerStub : MonoBehaviour
{
    /// <summary>플레이어 이동 허용.</summary>
    public virtual void EnableMovement()
    {
        // TODO: 실제 PlayerController 연결 시 구현
        Debug.Log("[PlayerController] Movement enabled (stub)");
    }

    /// <summary>플레이어 이동 차단 — 나레이션 / 판정 중 호출.</summary>
    public virtual void DisableMovement()
    {
        // TODO: 실제 PlayerController 연결 시 구현
        Debug.Log("[PlayerController] Movement disabled (stub)");
    }
}
