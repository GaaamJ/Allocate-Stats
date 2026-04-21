using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

/// <summary>
/// P02 스탯 분배 UI.
/// 스탯 로직만 담당 — 흐름 제어는 TitleP02Controller.
///
/// [Inspector 연결]
///   statData           : StatData SO
///   statRowPrefab      : StatRowUI 프리팹
///   rowParent          : 행 부모 Transform
///   remainingPointsTMP : 남은 포인트 표시 TMP
///   confirmButton      : 배분 완료 버튼
///   screenNarrator     : ScreenNarrator — 호버 설명용
/// </summary>
public class StatAllocatorUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private StatData statData;

    [Header("UI")]
    [SerializeField] private StatRowUI statRowPrefab;
    [SerializeField] private Transform rowParent;
    [SerializeField] private TextMeshProUGUI remainingPointsTMP;
    [SerializeField] private Button confirmButton;

    [Header("Narrator")]
    [SerializeField] private ScreenNarrator screenNarrator;

    private readonly Dictionary<StatType, int> allocation = new();
    private readonly Dictionary<StatType, StatRowUI> rows = new();
    private int remainingPoints;
    private Action onConfirm;

    private void Awake()
    {
        foreach (StatType t in Enum.GetValues(typeof(StatType)))
            allocation[t] = 0;

        remainingPoints = PlayerStats.TOTAL_POINTS;
    }

    // ── 공개 API ──────────────────────────────────────────

    public void Activate(Action onConfirmCallback = null)
    {
        onConfirm = onConfirmCallback;
        gameObject.SetActive(true);
        BuildRows();
        UpdateRemaining();
        confirmButton.onClick.AddListener(OnConfirmClicked);
    }

    public void Deactivate()
    {
        confirmButton.onClick.RemoveListener(OnConfirmClicked);
        gameObject.SetActive(false);
    }

    public void CommitStats()
    {
        PlayerStats.Instance.Apply(allocation);
    }

    // ── 내부 ──────────────────────────────────────────────

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

    private void OnConfirmClicked()
    {
        CommitStats();
        onConfirm?.Invoke();
    }
}