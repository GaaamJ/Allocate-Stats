using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Text;

/// <summary>
/// 클리어/게임오버 엔딩 씬 공통 컨트롤러.
///
/// [Inspector 연결 목록]
///   - endingData        : EndingData SO
///   - narratorUI        : NarratorUI
///   - humEndingBadge    : HUM 특수엔딩 전용 UI (없으면 스킵)
///   - statsDisplayTMP   : 최종 스탯 텍스트
///   - historyDisplayTMP : 판정 기록 텍스트
///   - restartButton     : 재시작 버튼
///   - titleSceneName    : 타이틀 씬 이름
/// </summary>
public class EndingSceneController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private EndingData endingData;

    [Header("UI")]
    [SerializeField] private NarratorUI narratorUI;
    [SerializeField] private GameObject humEndingBadge;   // HUM 엔딩 전용 표시 (선택)
    [SerializeField] private TextMeshProUGUI statsDisplayTMP;
    [SerializeField] private TextMeshProUGUI historyDisplayTMP;
    [SerializeField] private Button restartButton;

    [Header("Scene")]
    [SerializeField] private string titleSceneName = "TitleScene";

    private void Start() => StartCoroutine(RunEnding());

    private IEnumerator RunEnding()
    {
        var flow = GameFlowManager.Instance;
        var stats = PlayerStats.Instance;

        // ── 나레이션 결정 ─────────────────────────────────
        string narration;
        bool isHumEnding = false;

        if (endingData == null)
        {
            narration = flow != null && flow.IsGameOver ? "끝났다." : "탈출했다.";
        }
        else if (flow != null && flow.IsGameOver)
        {
            narration = endingData.GetGameOverNarration(flow.LastGameOverCause);
        }
        else
        {
            int hum = stats != null ? stats.HUM : 0;
            // LastGameOverCause 재활용 — 클리어 원인 방 ID는 별도로 GameFlowManager에 추가 권장
            // 지금은 LastClearRoomID 가 없으므로 아래 주석 참조
            narration = endingData.GetClearNarration(
                flow?.LastClearRoomID ?? "",   // ← GameFlowManager에 추가 필요 (아래 참고)
                hum,
                out isHumEnding
            );
        }

        // ── 연출 ─────────────────────────────────────────
        if (humEndingBadge) humEndingBadge.SetActive(isHumEnding);

        yield return narratorUI.ShowText(narration);

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

    private string BuildStatsText(PlayerStats s) =>
        $"STR  {s.STR}\nDEX  {s.DEX}\nPER  {s.PER}\nINT  {s.INT}\nLUK  {s.LUK}\nHUM  {s.HUM}";

    private string BuildHistoryText(GameFlowManager flow)
    {
        var sb = new StringBuilder();
        foreach (var r in flow.CheckHistory)
        {
            string display = !string.IsNullOrEmpty(r.summaryText) ? r.summaryText : r.context;
            sb.AppendLine($"{display}");
        }
        return sb.ToString();
    }
}