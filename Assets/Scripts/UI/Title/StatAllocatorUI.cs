using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Phase 02의 스탯 분배 UI.
///
/// [Inspector 연결]
///   statData           : StatData SO
///   statRowPrefab      : StatRowUI 프리팹
///   rowParent          : 행 부모 Transform (VerticalLayoutGroup 권장)
///   remainingPointsTMP : 남은 포인트 표시 TMP
///   confirmButton      : 배분 완료 버튼
///   screenNarrator     : ScreenNarrator — 호버 설명 전달용
/// </summary>
public class StatAllocatorUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private StatData statData;

    [Header("UI References")]
    [SerializeField] private StatRowUI statRowPrefab;
    [SerializeField] private Transform rowParent;
    [SerializeField] private TextMeshProUGUI remainingPointsTMP;
    [SerializeField] private Button confirmButton;

    [Header("Narrator — 호버 설명 전달 (ScreenNarrator)")]
    [SerializeField] private ScreenNarrator screenNarrator;

    private readonly Dictionary<StatType, int> allocation = new();
    private readonly Dictionary<StatType, StatRowUI> rows = new();
    private int remainingPoints;

    private void Awake()
    {
        foreach (StatType t in System.Enum.GetValues(typeof(StatType)))
            allocation[t] = 0;

        remainingPoints = PlayerStats.TOTAL_POINTS;
    }

    // ── 활성화 / 비활성화 ────────────────────────────────

    public void Activate()
    {
        gameObject.SetActive(true);
        BuildRows();
        UpdateRemaining();
        confirmButton.onClick.AddListener(OnConfirmClicked);
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }

    // ── 행 생성 ──────────────────────────────────────────

    private void BuildRows()
    {
        foreach (var entry in statData.stats)
        {
            var row = Instantiate(statRowPrefab, rowParent);
            row.Init(
                entry,
                onValueChanged: newVal => TrySet(entry.type, newVal),
                onHover: () => screenNarrator?.ShowStatDescription(entry.description),
                onExit: () => screenNarrator?.RestoreText()
            );
            rows[entry.type] = row;
        }
    }

    // ── 포인트 조작 ──────────────────────────────────────

    private void TrySet(StatType type, int newVal)
    {
        int prev = allocation[type];
        int delta = newVal - prev;
        int newRemain = remainingPoints - delta;

        if (newRemain < 0) return;
        if (newVal < PlayerStats.MIN_STAT || newVal > PlayerStats.MAX_STAT) return;

        allocation[type] = newVal;
        remainingPoints = newRemain;

        rows[type].SetValue(newVal);
        UpdateRemaining();
    }

    private void UpdateRemaining()
    {
        if (remainingPointsTMP)
            remainingPointsTMP.text = $"남은 포인트: {remainingPoints}";

        confirmButton.interactable = true;
    }

    // ── 랜덤 배분 (Phase 03) ─────────────────────────────

    public void RandomizeRemaining()
    {
        PlayerStats.Instance.RandomizeRemaining(new Dictionary<StatType, int>(allocation), remainingPoints);
        foreach (var kv in allocation)
            if (rows.TryGetValue(kv.Key, out var row))
                row.SetValue(PlayerStats.Instance.Get(kv.Key));
    }

    // ── 확정 (Phase 04) ──────────────────────────────────

    public void CommitStats()
    {
        PlayerStats.Instance.Apply(allocation);
    }

    private void OnConfirmClicked()
    {
        FindFirstObjectByType<TitleSceneController>()?.OnAllocateConfirmed();
    }
}
