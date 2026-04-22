using UnityEngine;
using System;
using System.Collections;
using Unity.VisualScripting;
using MoreMountains.Feedbacks;

public class TitleP01Controller : MonoBehaviour
{
    [Header("Animators")]
    [SerializeField] private MaskAnimator maskAnimator;
    [SerializeField] private P01NotebookAnimator notebookAnimator;
    [Header("Narration")]
    [SerializeField] private NarratorRouter narrator;
    [SerializeField] private TitleData titleData;
    [Header("FEEL")]
    [SerializeField] private MMF_Player appearBlackScreen;
    [SerializeField] private MMF_Player blackScreenFadeout;

    public IEnumerator Run(Action onComplete)
    {
        if (appearBlackScreen != null)
            appearBlackScreen.PlayFeedbacks();

        if (narrator != null && titleData?.prologueBlocks?.Length > 0)
            yield return narrator.ShowBlocks(titleData.prologueBlocks);


        if (blackScreenFadeout != null)
            blackScreenFadeout.PlayFeedbacks();

        // 1. 가면 등장
        if (maskAnimator)
            yield return maskAnimator.Appear();

        // 2. 나레이터 (가면 후)
        if (narrator != null && titleData?.introBlocks?.Length > 0)
            yield return narrator.ShowBlocks(titleData.introBlocks);

        // 3. 공책 날아오기
        if (notebookAnimator)
            yield return notebookAnimator.FlyToTable();

        // 4. 나레이터 (공책 후)
        if (narrator != null && titleData?.postNotebookBlocks?.Length > 0)
            yield return narrator.ShowBlocks(titleData.postNotebookBlocks);

        narrator.ClearAllIncludingPaper();
        onComplete?.Invoke();
    }
}