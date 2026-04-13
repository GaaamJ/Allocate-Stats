using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// StatRowUI 연결 체크용 테스트 스크립트.
/// 확인 후 삭제하세요.
/// 
/// 사용법:
///   1. 빈 GameObject에 이 컴포넌트 추가
///   2. statRow에 StatRow 오브젝트 연결
///   3. Play → +1 / -1 / Reset 버튼으로 확인
/// </summary>
public class StatRowTest : MonoBehaviour
{
    [SerializeField] private StatRowUI statRow;

    [Header("Test UI (선택 — 없으면 키보드로)")]
    [SerializeField] private Button btnUp;
    [SerializeField] private Button btnDown;
    [SerializeField] private Button btnReset;
    [SerializeField] private TextMeshProUGUI logTMP;

    private int fakeAllocation = 0;
    private int fakeRemaining = 4; // MAX_STAT 기준

    private void Start()
    {
        if (statRow == null) { Debug.LogError("[StatRowTest] statRow가 연결되지 않았습니다."); return; }

        // 더미 StatData.StatEntry 생성
        var entry = new StatData.StatEntry
        {
            type = StatType.STR,
            displayName = "STR",
            description = "This is Strong."
        };

        statRow.Init(
            entry,
            onValueChanged: OnValueChanged,
            onHover: () => Log("호버: 근력 설명 표시")
        );

        if (btnUp) btnUp.onClick.AddListener(() => OnValueChanged(fakeAllocation + 1));
        if (btnDown) btnDown.onClick.AddListener(() => OnValueChanged(fakeAllocation - 1));
        if (btnReset) btnReset.onClick.AddListener(() => OnValueChanged(0));

        Log("StatRowTest 시작. 키보드: ↑↓ 또는 버튼 사용");
    }



    private void OnValueChanged(int newVal)
    {
        if (newVal < 0 || newVal > PlayerStats.MAX_STAT)
        {
            Log($"범위 초과 → 무시 (요청값: {newVal})");
            return;
        }

        int delta = newVal - fakeAllocation;
        fakeRemaining -= delta;

        if (fakeRemaining < 0)
        {
            fakeRemaining += delta;
            Log($"포인트 부족 → 무시 (남은 포인트: {fakeRemaining})");
            return;
        }

        fakeAllocation = newVal;
        statRow.SetValue(fakeAllocation);
        Log($"값 변경 → {fakeAllocation}  |  남은 포인트: {fakeRemaining}");
    }

    private void Log(string msg)
    {
        Debug.Log($"[StatRowTest] {msg}");
        if (logTMP) logTMP.text = msg;
    }
}