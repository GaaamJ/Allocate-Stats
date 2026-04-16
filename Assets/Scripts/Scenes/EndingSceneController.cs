using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Text;

/// <summary>
/// 클리어/게임오버 통합 엔딩 씬 컨트롤러.
///
/// 역할:
///   - 게임오버 / 클리어 표시
///   - 최종 스탯 표시
///   - 판정 기록 표시
///   - HUM 분기 나레이션 (탈출 시, EndingData 매칭 시에만)
///
/// [Inspector 연결 목록]
///   - endingData        : EndingData SO (HUM 분기 엔딩만 등록)
///   - narrator          : NarratorRouter
///   - humEndingBadge    : HUM 특수엔딩 전용 UI (없으면 스킵)
///   - resultLabel       : "CLEAR" / "GAME OVER" 텍스트
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
    [SerializeField] private NarratorRouter narrator;
    [SerializeField] private GameObject humEndingBadge;
    [SerializeField] private TextMeshProUGUI resultLabel;
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

        // ── 결과 레이블 ───────────────────────────────────
        if (resultLabel)
            resultLabel.text = (flow != null && flow.IsGameOver) ? "GAME OVER" : "CLEAR";

        // ── HUM 분기 나레이션 (탈출 + EndingData 매칭 시만) ──
        bool isHumEnding = false;

        if (flow != null && !flow.IsGameOver && endingData != null)
        {
            int hum = stats != null ? stats.HUM : 0;
            var blocks = endingData.GetEscapeNarration(flow.LastEndingID, hum, out isHumEnding);

            if (blocks != null)
                yield return narrator.ShowBlocks(blocks);
        }

        if (humEndingBadge) humEndingBadge.SetActive(isHumEnding);

        // ── 스탯 / 판정 기록 표시 ─────────────────────────
        if (statsDisplayTMP != null && stats != null)
            statsDisplayTMP.text = BuildStatsText(stats);

        if (historyDisplayTMP != null && flow != null)
            historyDisplayTMP.text = BuildHistoryText(flow);

        // ── 재시작 버튼 ───────────────────────────────────
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
            sb.AppendLine(display);
        }
        return sb.ToString();
    }
}