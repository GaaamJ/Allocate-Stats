using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 씬 전환 후에도 유지되는 게임 흐름 관리자.
/// 방을 roomSequence 순서대로 진행. 복도 씬 없음.
///
/// [Inspector 연결 목록]
///   - roomSequence    : 방 진행 순서 (RoomData 배열)
///   - roomSceneName   : "RoomScene"
///   - clearSceneName  : "ClearScene"
///   - gameOverSceneName : "GameOverScene"
/// </summary>
public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    // ── 방 순서 ───────────────────────────────────────────
    [Header("방 순서 (인덱스 순서대로 진행)")]
    [SerializeField] private RoomData[] roomSequence;

    // ── 공개 상태 ─────────────────────────────────────────
    public int CurrentRoomIndex { get; private set; } = 0;
    public bool IsGameOver { get; private set; } = false;
    public bool IsEscaped { get; private set; } = false;

    /// <summary>현재 방 데이터. RoomSceneController에서 참조.</summary>
    public RoomData CurrentRoomData =>
        (CurrentRoomIndex >= 0 && CurrentRoomIndex < roomSequence.Length)
        ? roomSequence[CurrentRoomIndex]
        : null;

    // ── 판정 기록 ─────────────────────────────────────────
    public readonly List<CheckRecord> CheckHistory = new();

    // ── 씬 이름 ───────────────────────────────────────────
    [Header("Scene Names")]
    [SerializeField] private string roomSceneName = "RoomScene";
    [SerializeField] private string clearSceneName = "ClearScene";
    [SerializeField] private string gameOverSceneName = "GameOverScene";

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── 흐름 제어 ─────────────────────────────────────────

    /// <summary>TitleScene 완료 후 첫 번째 방으로.</summary>
    public void StartGame()
    {
        CurrentRoomIndex = 0;
        LoadRoomScene();
    }

    /// <summary>
    /// 방 클리어 → 다음 방으로.
    /// 마지막 방 이후면 OnEscape() 호출.
    /// </summary>
    public void OnRoomClear_NextRoom()
    {
        CurrentRoomIndex++;

        if (CurrentRoomIndex >= roomSequence.Length)
            OnEscape();
        else
            LoadRoomScene();
    }

    public void OnGameOver(string causeRoomID)
    {
        IsGameOver = true;
        LastGameOverCause = causeRoomID;

        SnapshotFinalStats();

        UnityEngine.SceneManagement.SceneManager.LoadScene(gameOverSceneName);
    }

    public string LastGameOverCause { get; private set; } = "";

    // 기존 LastGameOverCause 바로 아래에 추가
    public string LastClearRoomID { get; private set; } = "";

    public void OnEscape(string clearRoomID = "")
    {
        IsEscaped = true;
        LastClearRoomID = clearRoomID;

        // 엔딩 씬 진입 직전 스탯 스냅샷 저장
        SnapshotFinalStats();

        UnityEngine.SceneManagement.SceneManager.LoadScene(clearSceneName);
    }

    private void LoadRoomScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(roomSceneName);
    }

    // ── 판정 기록 ─────────────────────────────────────────

    public void RecordCheck(StatType stat, bool success, string context, string summaryText = "")
    {
        CheckHistory.Add(new CheckRecord
        {
            stat = stat,
            success = success,
            context = context,
            statValue = PlayerStats.Instance != null ? PlayerStats.Instance.Get(stat) : 0,
            summaryText = summaryText,
        });
    }

    public void ResetForNewGame()
    {
        CurrentRoomIndex = 0;
        IsGameOver = false;
        IsEscaped = false;
        LastGameOverCause = "";
        LastClearRoomID = "";
        FinalStats = null;
        CheckHistory.Clear();
    }

    // ── 스탯 스냅샷 ───────────────────────────────────────
    // PlayerStats는 DontDestroyOnLoad라 엔딩 씬에서도 살아있지만,
    // "게임 종료 시점의 스탯"을 명시적으로 굳혀두면 나중에 재시작해도 오염 없음.

    public FinalStatsSnapshot FinalStats { get; private set; }

    private void SnapshotFinalStats()
    {
        if (PlayerStats.Instance == null) return;
        var s = PlayerStats.Instance;
        FinalStats = new FinalStatsSnapshot
        {
            STR = s.STR,
            DEX = s.DEX,
            PER = s.PER,
            INT = s.INT,
            LUK = s.LUK,
            HUM = s.HUM,
        };
    }

    // ── 내부 데이터 구조 ──────────────────────────────────

    [System.Serializable]
    public class CheckRecord
    {
        public StatType stat;
        public int statValue;
        public bool success;
        public string context;
        public string summaryText;
    }

    // ── 내부 데이터 구조 (기존 CheckRecord 아래에 추가) ───

    [System.Serializable]
    public class FinalStatsSnapshot
    {
        public int STR, DEX, PER, INT, LUK, HUM;

        public override string ToString() =>
            $"STR  {STR}\nDEX  {DEX}\nPER  {PER}\nINT  {INT}\nLUK  {LUK}\nHUM  {HUM}";
    }

    // ── 테스트용 데이터 주입 API (EndingTest에서 사용) ───────────────
#if UNITY_EDITOR
    /// <summary>EndingTest 전용 — 에디터에서만 컴파일됨.</summary>
    public void InjectTestGameOver(string roomID)
    {
        IsGameOver = true;
        IsEscaped = false;
        LastGameOverCause = roomID;
        SnapshotFinalStats();
    }

    /// <summary>EndingTest 전용 — 에디터에서만 컴파일됨.</summary>
    public void InjectTestClear(string roomID)
    {
        IsGameOver = false;
        IsEscaped = true;
        LastClearRoomID = roomID;
        SnapshotFinalStats();
    }
#endif
}
