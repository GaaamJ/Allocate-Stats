using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 씬 전환 후에도 유지되는 플레이어 스탯 컨테이너.
/// TitleScene에서 분배 후, 이후 모든 씬에서 참조.
///
/// [런타임 수정]
///   플레이모드 중 Inspector에서 직접 수정 가능.
///   SetStat()으로 코드에서도 수정 가능.
/// </summary>
public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    // ── 스탯 값 (SerializeField → 런타임 인스펙터에서 수정 가능) ──────
    [Header("Stats — 런타임 중 직접 수정 가능")]
    [SerializeField] private int str;
    [SerializeField] private int dex;
    [SerializeField] private int per;
    [SerializeField] private int @int;
    [SerializeField] private int luk;
    [SerializeField] private int hum;

    public int STR => str;
    public int DEX => dex;
    public int PER => per;
    public int INT => @int;
    public int LUK => luk;
    public int HUM => hum;

    public const int MAX_STAT = 4;
    public const int MIN_STAT = 0;
    public const int TOTAL_POINTS = 12;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── 외부에서 한 번에 적용 ────────────────────────────
    public void Apply(Dictionary<StatType, int> allocation)
    {
        str = allocation.GetValueOrDefault(StatType.STR, 0);
        dex = allocation.GetValueOrDefault(StatType.DEX, 0);
        per = allocation.GetValueOrDefault(StatType.PER, 0);
        @int = allocation.GetValueOrDefault(StatType.INT, 0);
        luk = allocation.GetValueOrDefault(StatType.LUK, 0);
        hum = allocation.GetValueOrDefault(StatType.HUM, 0);
    }

    /// <summary>런타임 중 특정 스탯만 수정.</summary>
    public void SetStat(StatType type, int value)
    {
        int clamped = Mathf.Clamp(value, MIN_STAT, MAX_STAT);
        switch (type)
        {
            case StatType.STR: str = clamped; break;
            case StatType.DEX: dex = clamped; break;
            case StatType.PER: per = clamped; break;
            case StatType.INT: @int = clamped; break;
            case StatType.LUK: luk = clamped; break;
            case StatType.HUM: hum = clamped; break;
        }
    }

    /// <summary>남은 포인트를 완전 랜덤으로 배분 (HUM 제외).</summary>
    public void RandomizeRemaining(Dictionary<StatType, int> current, int remaining)
    {
        StatType[] randomPool = { StatType.STR, StatType.DEX, StatType.PER, StatType.INT, StatType.LUK };

        while (remaining > 0)
        {
            StatType pick = randomPool[Random.Range(0, randomPool.Length)];
            if (current[pick] < MAX_STAT)
            {
                current[pick]++;
                remaining--;
            }
        }
        Apply(current);
    }

    public int Get(StatType t) => t switch
    {
        StatType.STR => str,
        StatType.DEX => dex,
        StatType.PER => per,
        StatType.INT => @int,
        StatType.LUK => luk,
        StatType.HUM => hum,
        _ => 0
    };

    public override string ToString() =>
        $"STR:{str} DEX:{dex} PER:{per} INT:{@int} LUK:{luk} HUM:{hum}";
}
