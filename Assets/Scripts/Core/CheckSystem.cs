using UnityEngine;

/// <summary>
/// 스탯 판정 유틸리티. 어느 씬에서든 재사용.
///
/// Check Rule (기획서 기준):
///   - Threshold  : statValue >= threshold 면 무조건 성공
///   - Probability: statValue → 0%/24%/49%/74%/99%
///   - LuckFixed  : statValue >= threshold 면 50% 고정, 미달 시 0%
/// </summary>
public static class CheckSystem
{
    // 확률 테이블 (인덱스 = 스탯 수치 0~4)
    private static readonly float[] ProbTable = { 0f, 0.24f, 0.49f, 0.74f, 0.99f };

    public enum CheckType
    {
        Threshold,   // 수치 >= threshold 면 성공
        Probability, // ProbTable 기반 확률
        LuckFixed,   // LUK: 수치 >= threshold 면 50% 고정
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
}
