using System.Collections;

/// <summary>
/// 방 실행 전략 인터페이스.
/// 일반 방(NormalRoomRunner)과 앙코르 루프(EncoreRoomRunner)가 구현.
///
/// 새 방 타입이 생기면 이 인터페이스를 구현하고
/// RoomSceneController.SelectRunner()에 분기만 추가.
/// </summary>
public interface IRoomRunner
{
    IEnumerator Run(RoomRunContext context);
}
