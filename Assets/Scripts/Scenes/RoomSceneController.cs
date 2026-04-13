using UnityEngine;
using System.Collections;

/// <summary>
/// RoomScene 총괄 컨트롤러.
/// - GameFlowManager.CurrentRoomData 로 현재 방 데이터를 받음 (Inspector 연결 불필요)
/// - RoomData의 CheckStep 배열을 순서대로 실행
/// - 에셋 없이 텍스트만으로 전체 흐름 테스트 가능
///
/// [Inspector 연결 목록]
///   - narratorUI    : NarratorUI
///   - continueButton: 다음 단계 진행 버튼
///   - inputBlocker  : 판정 중 클릭 방지 CanvasGroup (선택)
///   - roomAnimator  : 연출 담당 (없으면 자동 스킵)
/// </summary>
public class RoomSceneController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private NarratorUI narratorUI;
    [SerializeField] private UnityEngine.UI.Button continueButton;
    [SerializeField] private CanvasGroup inputBlocker;

    [Header("Animator (연출용 — 없으면 스킵)")]
    [SerializeField] private RoomAnimator roomAnimator;

    // ── 런타임 상태 ───────────────────────────────────────
    private RoomData currentRoom;
    private bool waitingForContinue = false;

    private void Start()
    {
        currentRoom = GameFlowManager.Instance?.CurrentRoomData;

        if (currentRoom == null)
        {
            Debug.LogError("[RoomScene] CurrentRoomData가 null. GameFlowManager의 roomSequence 확인 필요.");
            return;
        }

        continueButton.onClick.AddListener(OnContinueClicked);
        SetContinueVisible(false);

        StartCoroutine(RunRoom());
    }

    // ══════════════════════════════════════════════════════
    // 방 진행 메인 루프
    // ══════════════════════════════════════════════════════

    private IEnumerator RunRoom()
    {
        // 진입 나레이션
        if (!string.IsNullOrEmpty(currentRoom.entryNarration))
        {
            yield return ShowNarration(currentRoom.entryNarration);
            yield return WaitForContinue();
        }

        // 진입 연출 훅
        if (roomAnimator) yield return roomAnimator.OnRoomEnter(currentRoom.roomID);

        yield return ExecuteStep(0);
    }

    private IEnumerator ExecuteStep(int index)
    {
        if (index < 0 || index >= currentRoom.steps.Length)
        {
            Debug.LogError($"[RoomScene] step 인덱스 {index} 없음 (roomID: {currentRoom.roomID})");
            yield break;
        }

        RoomData.CheckStep step = currentRoom.steps[index];

        // 단계 나레이션
        if (!string.IsNullOrEmpty(step.narration))
        {
            yield return ShowNarration(step.narration);
            yield return WaitForContinue();
        }

        // 판정 전 연출 훅
        if (roomAnimator) yield return roomAnimator.OnBeforeCheck(currentRoom.roomID, index);

        // 판정
        bool success = CheckSystem.RollDebug(step.stat, step.checkType, step.threshold, out string log);
        string summary = success ? step.endingSummary_success : step.endingSummary_failure;
        GameFlowManager.Instance?.RecordCheck(step.stat, success, $"{currentRoom.roomID}_step{index}", summary);

        // 결과 나레이션
        RoomData.StepOutcome outcome = success ? step.onSuccess : step.onFailure;
        if (!string.IsNullOrEmpty(outcome.narration))
        {
            yield return ShowNarration(outcome.narration);
            yield return WaitForContinue();
        }

        // 판정 후 연출 훅
        if (roomAnimator) yield return roomAnimator.OnAfterCheck(currentRoom.roomID, index, success);

        // 결과 분기
        yield return HandleOutcome(outcome);
    }

    private IEnumerator HandleOutcome(RoomData.StepOutcome outcome)
    {
        switch (outcome.type)
        {
            case RoomData.OutcomeType.NextRoom:
                GameFlowManager.Instance?.OnRoomClear_NextRoom();
                break;

            case RoomData.OutcomeType.GameOver:
            case RoomData.OutcomeType.Death:
                GameFlowManager.Instance?.OnGameOver(currentRoom.roomID);
                break;

            case RoomData.OutcomeType.GoToStep:
                yield return ExecuteStep(outcome.nextStepIndex);
                break;

            case RoomData.OutcomeType.Escape:
                GameFlowManager.Instance?.OnEscape(currentRoom.roomID);  // roomID 전달
                break;
        }
    }

    // ══════════════════════════════════════════════════════
    // UI 헬퍼
    // ══════════════════════════════════════════════════════

    private IEnumerator ShowNarration(string text)
    {
        SetInputBlock(true);
        yield return narratorUI.ShowText(text);
        SetInputBlock(false);
    }

    private IEnumerator WaitForContinue()
    {
        waitingForContinue = true;
        SetContinueVisible(true);
        yield return new WaitUntil(() => !waitingForContinue);
        SetContinueVisible(false);
    }

    private void OnContinueClicked()
    {
        if (waitingForContinue) waitingForContinue = false;
    }

    private void SetContinueVisible(bool v)
    {
        if (continueButton) continueButton.gameObject.SetActive(v);
    }

    private void SetInputBlock(bool block)
    {
        if (inputBlocker)
        {
            inputBlocker.interactable = !block;
            inputBlocker.blocksRaycasts = !block;
        }
    }
}
