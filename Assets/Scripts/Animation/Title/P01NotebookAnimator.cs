using UnityEngine;
using MoreMountains.Feedbacks;
using System.Collections;

/// <summary>
/// P01 공책 등장 연출.
///
/// 연출 흐름:
///   1. SetActive(true)
///   2. appearFeedback — 꺼내기 + 이동 + 착지 (MMF_Player 단일)
///   3. (추후) OpenNotebook — 공책 펼치기
///
/// [Inspector 연결]
///   notebookObject  : 공책 GameObject (평소 SetActive false)
///   appearFeedback  : MMF_Player
/// </summary>
public class P01NotebookAnimator : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] private GameObject notebookObject;

    [Header("Feel")]
    [SerializeField] private MMF_Player appearFeedback;

    public IEnumerator FlyToTable()
    {
        notebookObject.SetActive(true);

        if (appearFeedback != null)
        {
            appearFeedback.PlayFeedbacks();
            yield return new WaitForSeconds(appearFeedback.TotalDuration);
        }
        else yield return null;

        // (추후) yield return OpenNotebook();
    }

    public IEnumerator OpenNotebook()
    {
        // TODO: 에셋 완성 후 구현
        yield return null;
    }
}