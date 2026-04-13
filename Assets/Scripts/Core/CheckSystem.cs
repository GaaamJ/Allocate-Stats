using UnityEngine;

/// <summary>
/// 스탯 판정 유틸리티. 어느 씬에서든 재사용.
///
/// Check Rule (기획서 기준):
///   - Threshold  : statValue >= threshold 면 무조건 성공
///   - Probability: statValue → 0%/24%/49%/74%/99%
///   - LuckFixed  : statValue >= threshold 면 50% 고정, 미달 시 0%
///   - Compound   : stat >= threshold AND stat2 >= threshold2 (확정 판정 베이스)
/// </summary>
public static class CheckSystem
{
    private static readonly float[] ProbTable = { 0f, 0.24f, 0.49f, 0.74f, 0.99f };

    public enum CheckType
    {
        Threshold,   // 수치 >= threshold 면 성공
        Probability, // ProbTable 기반 확률
        LuckFixed,   // LUK: 수치 >= threshold 면 50% 고정
        Compound,    // stat >= threshold && stat2 >= threshold2
    }

    public static bool Roll(StatType stat, CheckType checkType, int threshold = 0)
    {
        int value = PlayerStats.Instance != null ? PlayerStats.Instance.Get(stat) : 0;

        return checkType switch
        {
            CheckType.Threshold => value >= threshold,
            CheckType.Probability => RollProbability(value),
            CheckType.LuckFixed => value >= threshold && Random.value < 0.5f,
            _ => false,
        };
    }

    public static bool RollCompound(
        StatType stat, int threshold,
        StatType stat2, int threshold2)
    {
        int v1 = PlayerStats.Instance != null ? PlayerStats.Instance.Get(stat) : 0;
        int v2 = PlayerStats.Instance != null ? PlayerStats.Instance.Get(stat2) : 0;
        return v1 >= threshold && v2 >= threshold2;
    }

    private static bool RollProbability(int value)
    {
        int clamped = Mathf.Clamp(value, 0, ProbTable.Length - 1);
        return Random.value < ProbTable[clamped];
    }

    /// <summary>결과 로그 포함 판정 (에디터 디버깅용).</summary>
    public static bool RollDebug(StatType stat, CheckType checkType, int threshold, out string log)
    {
        int value = PlayerStats.Instance != null ? PlayerStats.Instance.Get(stat) : 0;
        bool result = Roll(stat, checkType, threshold);
        log = $"[Check] {stat}={value} / {checkType}(threshold={threshold}) → {(result ? "SUCCESS" : "FAIL")}";
        Debug.Log(log);
        return result;
    }

    /// <summary>복합 판정 로그 포함 버전.</summary>
    public static bool RollCompoundDebug(
        StatType stat, int threshold,
        StatType stat2, int threshold2,
        out string log)
    {
        int v1 = PlayerStats.Instance != null ? PlayerStats.Instance.Get(stat) : 0;
        int v2 = PlayerStats.Instance != null ? PlayerStats.Instance.Get(stat2) : 0;
        bool result = v1 >= threshold && v2 >= threshold2;
        log = $"[Check] {stat}={v1}(>={threshold}) && {stat2}={v2}(>={threshold2}) → {(result ? "SUCCESS" : "FAIL")}";
        Debug.Log(log);
        return result;
    }
}