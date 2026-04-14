using System.Collections;

/// <summary>
/// 방 실행 전략 인터페이스.
/// 새 방 타입이 생기면 이 인터페이스를 구현하는 클래스를 추가.
/// RoomSceneController는 IRoomRunner만 알고, 구체 구현은 모름.
///
/// 구현체 목록:
///   - NormalRoomRunner  : 일반 방
///   - EncoreRoomRunner  : 앙코르 루프
/// </summary>
public interface IRoomRunner
{
    /// <summary>
    /// 방 실행 진입점.
    /// RoomSceneController.Start()에서 호출.
    /// </summary>
    IEnumerator Run(RoomRunContext context);
}
