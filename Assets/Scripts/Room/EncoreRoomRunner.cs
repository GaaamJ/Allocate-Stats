/// <summary>
/// 앙코르 루프 Runner.
/// BaseRoomRunner의 Phase 실행 로직을 그대로 사용.
/// EncoreRoomData.GetPlanet()을 데이터 소스로 사용.
/// 완료 시 OnRoomComplete() 대신 OnEncoreComplete() 호출.
/// </summary>
public class EncoreRoomRunner : BaseRoomRunner
{
    private EncoreRoomData.PlanetEntry planet;

    protected override RoomData.PhaseData[] GetRoomPhases()
    {
        if (planet == null)
            planet = ctx.Bridge.EncoreRoomData?.GetPlanet(ctx.Bridge.EncoreCounter);
        return planet?.phases;
    }

    protected override string GetRoomLabel() =>
        planet?.planetName ?? "encore_unknown";

    protected override void OnRoomComplete() =>
        ctx.Bridge.OnEncoreComplete();
}