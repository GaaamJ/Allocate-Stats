using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Phase 02의 스탯 분배 UI.
/// 
/// [Inspector 연결 목록]
///   - statData          : StatData ScriptableObject
///   - statRowPrefab     : StatRowUI 프리팹 (바 1줄)
///   - rowParent         : 행 부모 Transform (VerticalLayoutGroup 권장)
///   - remainingPointsTMP: 남은 포인트 표시 TextMeshPro
///   - confirmButton     : 배분 완료 버튼
///   - narratorUI        : 마우스 오버 설명 전달용
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
    [SerializeField] private NarratorUI narratorUI;

    // ── 런타임 상태 ─────────────────────────────────────
    private readonly Dictionary<StatType, int> allocation = new();
    private readonly Dictionary<StatType, StatRowUI> rows = new();
    private int remainingPoints;

    private void Awake()
    {
        // 초기화
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
            StatRowUI row = Instantiate(statRowPrefab, rowParent);
            row.Init(
                entry,
                onValueChanged: (newVal) => TrySet(entry.type, newVal),
                onHover: () => narratorUI.ShowStatDescription(entry.description)
            );
            rows[entry.type] = row;
        }
    }

    // ── 포인트 조작 ──────────────────────────────────────

    /// <summary>세그먼트 클릭 시 "이 스탯을 newVal로 바꾸고 싶다"는 요청.</summary>
    private void TrySet(StatType type, int newVal)
    {
        int prev = allocation[type];
        int delta = newVal - prev;       // 양수면 포인트 소모, 음수면 환급
        int newRemain = remainingPoints - delta;

        if (newRemain < 0) return;           // 포인트 부족 → 무시
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

        confirmButton.interactable = true; // 언제든 확정 가능
    }

    // ── 랜덤 배분 (Phase 03) ─────────────────────────────

    public void RandomizeRemaining()
    {
        PlayerStats.Instance.RandomizeRemaining(new Dictionary<StatType, int>(allocation), remainingPoints);
        // UI도 동기화
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
        // TitleSceneController로 위임
        FindFirstObjectByType<TitleSceneController>().OnAllocateConfirmed();
    }
}
