using UnityEngine;
using System.Collections;

/// <summary>
/// 영원 방 앙코르 루프 컨트롤러.
/// RoomScene을 재사용 — RoomSceneController.Start()에서 IsEncoreLoop 감지 시 Run() 호출.
/// 태양계 행성 순서(수금지화목토천해인)로 방을 순환.
/// 지구(isMercy=true) = 자비 방.
///
/// [Inspector 연결 목록]
///   - RoomSceneController.encoreController 에 연결
/// </summary>
public class EncoreRoomController : MonoBehaviour
{
    private NarratorUI narratorUI;
    private UnityEngine.UI.Button continueButton;
    private bool waitingForContinue = false;

    /// <summary>RoomSceneController에서 호출.</summary>
    public void Run(NarratorUI narrator, UnityEngine.UI.Button btn)
    {
        narratorUI = narrator;
        continueButton = btn;

        continueButton.onClick.AddListener(OnContinueClicked);
        SetContinueVisible(false);

        StartCoroutine(RunEncore());
    }

    private IEnumerator RunEncore()
    {
        var flow = GameFlowManager.Instance;
        var data = flow?.EncoreRoomData;

        if (flow == null || data == null)
        {
            Debug.LogError("[EncoreScene] GameFlowManager 또는 EncoreRoomData 없음");
            yield break;
        }

        var planet = data.GetPlanet(flow.EncoreCounter);
        if (planet == null)
        {
            Debug.LogError("[EncoreScene] PlanetEntry 없음");
            yield break;
        }

        if (planet.isMercy)
            yield return RunMercy(flow, data, planet);
        else
            yield return RunAnchor(flow, planet);
    }

    // ── 앙코르 방 ────────────────────────────────────────

    private IEnumerator RunAnchor(GameFlowManager flow, EncoreRoomData.PlanetEntry planet)
    {
        if (planet.narration != null && planet.narration.Length > 0)
        {
            yield return narratorUI.ShowBlocks(planet.narration);
            yield return WaitForContinue();
        }

        flow.OnEncoreClear();
    }

    // ── 자비 방 ──────────────────────────────────────────

    private IEnumerator RunMercy(GameFlowManager flow, EncoreRoomData data, EncoreRoomData.PlanetEntry planet)
    {
        // 1. 진입 나레이션
        if (planet.narration != null && planet.narration.Length > 0)
        {
            yield return narratorUI.ShowBlocks(planet.narration);
            yield return WaitForContinue();
        }


        // 3. 50% 사망 판정 (스탯 무관)
        bool isDeath = UnityEngine.Random.value < 0.5f;

        if (isDeath)
        {
            // 4a. 사망 나레이션 → 엔딩 씬
            if (planet.mercyDeathNarration != null && planet.mercyDeathNarration.Length > 0)
            {
                yield return narratorUI.ShowBlocks(planet.mercyDeathNarration);
                yield return WaitForContinue();
            }
            flow.OnDeath(data.mercyDeathEndingID);
        }
        else
        {
            // 4b. 다음 앙코르
            if (planet.mercySurviveNarration != null && planet.mercySurviveNarration.Length > 0)
            {
                yield return narratorUI.ShowBlocks(planet.mercySurviveNarration);
                yield return WaitForContinue();
            }
            flow.OnEncoreClear();
        }
    }

    // ── UI 헬퍼 ──────────────────────────────────────────

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
}