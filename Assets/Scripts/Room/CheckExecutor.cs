using UnityEngine;

/// <summary>
/// 판정 실행 + 기록 담당.
/// NormalRoomRunner에서 생성해서 사용.
/// 판정 방식(CheckSystem)이나 기록 방식(RoomBridge)이 바뀌어도 이 클래스만 수정.
/// </summary>
public class CheckExecutor
{
    // ── 테스트 오버라이드 ─────────────────────────────────
    // 에디터에서 Inspector를 통해 또는 코드로 직접 설정.

    public enum ForceResult { None, Success, Failure, Skip }

    /// <summary>
    /// 테스트용 판정 오버라이드.
    /// None이면 실제 판정 실행.
    /// </summary>
    public ForceResult TestForceResult { get; set; } = ForceResult.None;

    // ── 의존성 ────────────────────────────────────────────

    private readonly RoomBridge bridge;
    private readonly string roomID;

    public CheckExecutor(RoomBridge bridge, string roomID)
    {
        this.bridge = bridge;
        this.roomID = roomID;
    }

    // ── 판정 실행 ─────────────────────────────────────────

    /// <summary>
    /// CheckData를 받아 판정 실행 후 결과 반환.
    /// 동시에 GameFlowManager.CheckHistory에 기록.
    /// </summary>
    /// <param name="checkData">판정 설정 데이터</param>
    /// <param name="phaseIndex">현재 Phase 인덱스 — 기록 컨텍스트에 사용</param>
    /// <returns>판정 성공 여부</returns>
    public bool Execute(RoomData.CheckData checkData, int phaseIndex)
    {
        // 스킵 — 다음 방으로 바로 이동 (테스트용)
        if (TestForceResult == ForceResult.Skip)
        {
            bridge.OnRoomComplete();
            return false;
        }

        bool success = ResolveResult(checkData);

        string summary = success
            ? checkData.summaryText_success
            : checkData.summaryText_failure;

        bridge.RecordCheck(
            checkData.stat,
            success,
            $"{roomID}_phase{phaseIndex}",
            summary
        );

        return success;
    }

    // ── 내부 ──────────────────────────────────────────────

    private bool ResolveResult(RoomData.CheckData checkData)
    {
        // 오버라이드
        if (TestForceResult == ForceResult.Success) return true;
        if (TestForceResult == ForceResult.Failure) return false;

        // 실제 판정
        string log;
        if (checkData.checkType == CheckSystem.CheckType.Compound)
            return CheckSystem.RollCompoundDebug(
                checkData.stat, checkData.threshold,
                checkData.stat2, checkData.threshold2,
                out log);

        return CheckSystem.RollDebug(checkData.stat, checkData.checkType, checkData.threshold, out log);
    }
}
