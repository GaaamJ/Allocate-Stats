/// <summary>
/// 일반 방 Runner.
/// BaseRoomRunner의 Phase 실행 로직을 그대로 사용.
/// RoomData.CurrentRoomData를 데이터 소스로 사용.
/// </summary>
public class NormalRoomRunner : BaseRoomRunner
{
    protected override RoomData.PhaseData[] GetRoomPhases() =>
        ctx.Bridge.CurrentRoomData?.phases;

    protected override string GetRoomLabel() =>
        ctx.Bridge.CurrentRoomData?.roomID ?? "unknown";

    protected override void OnRoomComplete() =>
        ctx.Bridge.OnRoomComplete();
}