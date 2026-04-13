using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 엔딩 씬 빠른 검증용. 확인 후 삭제.
///
/// 사용법:
///   1. ClearScene 또는 GameOverScene의 빈 GameObject에 추가
///   2. Inspector에서 원하는 시나리오 설정
///   3. Play — Start()에서 GameFlowManager/PlayerStats를 더미 데이터로 채우고 씬 시작
///
/// GameFlowManager / PlayerStats가 DontDestroyOnLoad 싱글턴이라
/// 씬을 직접 열어도 Instance가 없을 수 있음.
/// 이 스크립트가 없으면 null인 경우를 대신 채워줌.
/// </summary>
public class EndingTest : MonoBehaviour
{
    [Header("시나리오")]
    [Tooltip("true = 게임오버, false = 클리어")]
    [SerializeField] private bool isGameOver = false;

    [Tooltip("게임오버 원인 방 ID (isGameOver = true 일 때)")]
    [SerializeField] private string gameOverRoomID = "mirror";

    [Tooltip("클리어 방 ID (isGameOver = false 일 때)")]
    [SerializeField] private string clearRoomID = "altar";

    [Header("더미 스탯")]
    [SerializeField] private int str = 3;
    [SerializeField] private int dex = 2;
    [SerializeField] private int per = 4;
    [SerializeField] private int @int = 3;
    [SerializeField] private int luk = 2;
    [SerializeField] private int hum = 3;

    [Header("더미 판정 기록")]
    [SerializeField] private DummyRecord[] dummyRecords = new DummyRecord[]
    {
        new() { stat = StatType.LUK, statValue = 2, success = false, context = "시작_step0", summaryText = "운이 따르지 않았다." },
        new() { stat = StatType.INT, statValue = 3, success = true,  context = "제단_step0", summaryText = "의식의 방법을 알아냈다." },
        new() { stat = StatType.STR, statValue = 3, success = true,  context = "제단_step1", summaryText = "제물을 들어올렸다." },
        new() { stat = StatType.LUK, statValue = 2, success = false, context = "제단_step2", summaryText = "신이 만족하지 않았다." },
    };

    private void Awake()
    {
        // ── GameFlowManager 없으면 생성 ──────────────────
        if (GameFlowManager.Instance == null)
        {
            var go = new GameObject("[Test] GameFlowManager");
            go.AddComponent<GameFlowManager>();
            // Awake에서 싱글턴 등록되므로 바로 사용 가능
        }

        // ── PlayerStats 없으면 생성 ──────────────────────
        if (PlayerStats.Instance == null)
        {
            var go = new GameObject("[Test] PlayerStats");
            go.AddComponent<PlayerStats>();
        }

        InjectDummyData();
    }

    private void InjectDummyData()
    {
        var flow  = GameFlowManager.Instance;
        var stats = PlayerStats.Instance;

        // 스탯 주입
        stats.SetStat(StatType.STR, str);
        stats.SetStat(StatType.DEX, dex);
        stats.SetStat(StatType.PER, per);
        stats.SetStat(StatType.INT, @int);
        stats.SetStat(StatType.LUK, luk);
        stats.SetStat(StatType.HUM, hum);

        // 판정 기록 주입
        flow.ResetForNewGame();   // 혹시 남아 있는 기록 클리어
        foreach (var r in dummyRecords)
            flow.RecordCheck(r.stat, r.success, r.context, r.summaryText);

        // 상태 주입 — private setter를 우회하기 위해 공개 메서드 경유
        if (isGameOver)
            flow.InjectTestGameOver(gameOverRoomID);
        else
            flow.InjectTestClear(clearRoomID);

        Debug.Log($"[EndingTest] 더미 데이터 주입 완료 — isGameOver={isGameOver}");
    }

    // ── 내부 데이터 구조 ──────────────────────────────────

    [System.Serializable]
    public class DummyRecord
    {
        public StatType stat;
        public int      statValue;
        public bool     success;
        public string   context;
        public string   summaryText;
    }
}
