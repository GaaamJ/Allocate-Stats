using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// TitleSceneController Phase 흐름 로그 체크용.
/// 확인 후 삭제하세요.
/// 
/// 사용법:
///   1. 빈 GameObject에 추가
///   2. controller에 TitleSceneController 연결
///   3. 각 버튼 연결 (선택)
///   4. Play → 버튼 클릭으로 Phase 진행 확인
/// </summary>
public class PhaseTest : MonoBehaviour
{
    [SerializeField] private TitleSceneController controller;

    [Header("버튼 (선택)")]
    [SerializeField] private Button btnGameStart;   // Phase 00 → 01
    [SerializeField] private Button btnConfirm;     // Phase 02 → 04
    [SerializeField] private Button btnPaperClick;  // Phase 04 → 씬 전환

    private void Start()
    {
        if (controller == null) { Debug.LogError("[PhaseTest] controller가 연결되지 않았습니다."); return; }

        if (btnGameStart) btnGameStart.onClick.AddListener(() => { Log("GameStart 버튼 클릭"); controller.OnGameStartPressed(); });
        if (btnConfirm) btnConfirm.onClick.AddListener(() => { Log("Confirm 버튼 클릭"); controller.OnAllocateConfirmed(); });
        if (btnPaperClick) btnPaperClick.onClick.AddListener(() => { Log("Paper 클릭"); controller.OnPaperClicked(); });

        // 씬 전환 감지
        SceneManager.sceneLoaded += OnSceneLoaded;

        Log("PhaseTest 시작");
        LogPhase();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Log($"씬 전환 완료 → {scene.name}");
    }

    private void Update()
    {
        LogPhaseIfChanged();
    }

    private TitleSceneController.Phase lastPhase;

    private void LogPhaseIfChanged()
    {
        if (controller == null) return;
        if (controller.CurrentPhase != lastPhase)
        {
            lastPhase = controller.CurrentPhase;
            LogPhase();
        }
    }

    private void LogPhase()
    {
        Debug.Log($"[PhaseTest] 현재 Phase → {controller.CurrentPhase}");
    }

    private void Log(string msg)
    {
        Debug.Log($"[PhaseTest] {msg}");
    }
}