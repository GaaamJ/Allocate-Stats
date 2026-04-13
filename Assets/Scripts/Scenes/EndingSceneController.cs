using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Text;

/// <summary>
/// 클리어/게임오버 엔딩 씬 공통 컨트롤러.
/// ClearScene / GameOverScene 둘 다 이 컴포넌트 사용 가능.
///
/// [Inspector 연결 목록]
///   - narratorUI        : NarratorUI
///   - statsDisplayTMP   : 최종 스탯 텍스트
///   - historyDisplayTMP : 판정 기록 텍스트
///   - restartButton     : 재시작 버튼
///   - titleSceneName    : 타이틀 씬 이름
///   - clearNarration    : 탈출 성공 텍스트
///   - defaultGameOver   : 기본 게임 오버 텍스트
///   - endingTextEntries : 방별 커스텀 게임오버 텍스트 목록
/// </summary>
public class EndingSceneController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private NarratorUI narratorUI;
    [SerializeField] private TextMeshProUGUI statsDisplayTMP;
    [SerializeField] private TextMeshProUGUI historyDisplayTMP;
    [SerializeField] private Button restartButton;

    [Header("Scene")]
    [SerializeField] private string titleSceneName = "TitleScene";

    [Header("Ending Texts")]
    [TextArea, SerializeField] private string clearNarration = "탈출했다.";
    [TextArea, SerializeField] private string defaultGameOver = "끝났다.";
    [SerializeField] private EndingTextEntry[] endingTextEntries;

    private void Start()
    {
        StartCoroutine(RunEnding());
    }

    private IEnumerator RunEnding()
    {
        var flow = GameFlowManager.Instance;
        var stats = PlayerStats.Instance;

        string narration = flow != null && flow.IsGameOver
            ? GetGameOverText(flow.LastGameOverCause)
            : clearNarration;

        yield return StartCoroutine(narratorUI.ShowText(narration));

        if (statsDisplayTMP != null && stats != null)
            statsDisplayTMP.text = BuildStatsText(stats);

        if (historyDisplayTMP != null && flow != null)
            historyDisplayTMP.text = BuildHistoryText(flow);

        if (restartButton)
        {
            restartButton.gameObject.SetActive(true);
            restartButton.onClick.AddListener(OnRestart);
        }
    }

    private void OnRestart()
    {
        GameFlowManager.Instance?.ResetForNewGame();
        UnityEngine.SceneManagement.SceneManager.LoadScene(titleSceneName);
    }

    // ── 텍스트 빌더 ──────────────────────────────────────

    private string GetGameOverText(string causeRoomID)
    {
        if (endingTextEntries != null)
            foreach (var e in endingTextEntries)
                if (e.roomID == causeRoomID) return e.narration;
        return defaultGameOver;
    }

    private string BuildStatsText(PlayerStats s) =>
        $"STR  {s.STR}\nDEX  {s.DEX}\nPER  {s.PER}\nINT  {s.INT}\nLUK  {s.LUK}\nHUM  {s.HUM}";

    private string BuildHistoryText(GameFlowManager flow)
    {
        var sb = new StringBuilder();
        foreach (var r in flow.CheckHistory)
            sb.AppendLine($"{r.stat}({r.statValue}) [{r.context}] → {(r.success ? "✓" : "✗")}");
        return sb.ToString();
    }

    // ── 내부 데이터 구조 ──────────────────────────────────

    [System.Serializable]
    public class EndingTextEntry
    {
        public string roomID;
        [TextArea] public string narration;
    }
}
