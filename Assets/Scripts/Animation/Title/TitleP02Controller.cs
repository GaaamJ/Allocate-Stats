using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// P02 스탯 분배 페이즈 — 흐름 제어만 담당.
///
/// 흐름:
///   1. 나레이터 (구슬 등장 전)
///   2. 구슬/별똥별 등장 (TODO)
///   3. 나레이터 (공책 클릭 유도)
///   4. Look + 공책 클릭 대기
///   5. 공책 확대 애니메이션 (TODO)
///   6. StatAllocatorUI 활성화 → 확정 대기
///
/// [Inspector 연결]
///   narrator        : NarratorRouter
///   titleData       : TitleData
///   titlePlayer     : TitlePlayerController
///   notebook        : 공책 오브젝트의 NotebookInteractable
///   statAllocatorUI : StatAllocatorUI
/// </summary>
public class TitleP02Controller : MonoBehaviour
{
    [Header("Narration")]
    [SerializeField] private NarratorRouter narrator;
    [SerializeField] private TitleData titleData;

    [Header("Interact")]
    [SerializeField] private TitlePlayerController titlePlayer;
    [SerializeField] private NotebookInteractable notebook;
    [SerializeField] private MarbleSpawner marbleSpawner;


    [Header("UI")]
    [SerializeField] private StatAllocatorUI statAllocatorUI;

    public IEnumerator Run(Action onComplete)
    {
        // 1. 나레이터 (구슬 등장 전)
        if (narrator != null && titleData?.p02PreBlocks?.Length > 0)
            yield return narrator.ShowBlocks(titleData.p02PreBlocks);

        // 2. 구슬/별똥별 등장 (TODO)
        if (marbleSpawner != null)
            yield return StartCoroutine(marbleSpawner.SpawnAll());

        // 3. 나레이터 (공책 클릭 유도)
        if (narrator != null && titleData?.p02PostBlocks?.Length > 0)
            yield return narrator.ShowBlocks(titleData.p02PostBlocks);

        // 4. Look + 공책 클릭 대기
        if (notebook != null && titlePlayer != null)
        {
            bool clicked = false;
            Action onClicked = () => clicked = true;
            notebook.OnClicked += onClicked;

            titlePlayer.EnableLook();
            titlePlayer.EnableInteract();

            Debug.Log("[TitleP02] 공책 클릭 대기 중...");
            yield return new WaitUntil(() => clicked);

            titlePlayer.DisableInteract();
            titlePlayer.DisableLook();
            notebook.OnClicked -= onClicked;
            Debug.Log("[TitleP02] 공책 클릭 확인, 다음 단계로");
        }
        else
        {
            Debug.LogWarning("[TitleP02] notebook 또는 titlePlayer 미연결 — 클릭 단계 스킵");
        }

        // 5. 공책 확대 애니메이션 (TODO)
        // yield return notebookAnimator.Open();

        // 6. 스탯 분배 UI
        // if (statAllocatorUI != null)
        // {
        //     bool confirmed = false;
        //     statAllocatorUI.Activate(onConfirmCallback: () => confirmed = true);
        //     yield return new WaitUntil(() => confirmed);
        //     statAllocatorUI.Deactivate();
        // }

        onComplete?.Invoke();
    }
}