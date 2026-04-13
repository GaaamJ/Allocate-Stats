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

    public void OnEscape()
    {
        IsEscaped = true;
        UnityEngine.SceneManagement.SceneManager.LoadScene(clearSceneName);
    }

    public void OnGameOver(string causeRoomID)
    {
        IsGameOver = true;
        LastGameOverCause = causeRoomID;
        UnityEngine.SceneManagement.SceneManager.LoadScene(gameOverSceneName);
    }

    public string LastGameOverCause { get; private set; } = "";

    private void LoadRoomScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(roomSceneName);
    }

    // ── 판정 기록 ─────────────────────────────────────────

    public void RecordCheck(StatType stat, bool success, string context)
    {
        CheckHistory.Add(new CheckRecord
        {
            stat = stat,
            success = success,
            context = context,
            statValue = PlayerStats.Instance != null ? PlayerStats.Instance.Get(stat) : 0,
        });
    }

    public void ResetForNewGame()
    {
        CurrentRoomIndex = 0;
        IsGameOver = false;
        IsEscaped = false;
        LastGameOverCause = "";
        CheckHistory.Clear();
    }

    // ── 내부 데이터 구조 ──────────────────────────────────

    [System.Serializable]
    public class CheckRecord
    {
        public StatType stat;
        public int statValue;
        public bool success;
        public string context;
    }
}
