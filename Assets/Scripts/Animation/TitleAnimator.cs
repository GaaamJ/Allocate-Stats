using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

/// <summary>
/// TitleScene의 오브젝트 연출 담당.
/// 실제 애니메이션은 에셋(Animator / 파티클)이 완성되면 채워 넣으세요.
/// 지금은 각 메서드가 연결 포인트(stub)로 동작합니다.
/// 
/// [Inspector 연결 목록]
///   - notebookObject    : 공책 GameObject
///   - marbleObjects     : 구슬 배열
///   - hourglassObject   : 모래시계 GameObject
///   - tornPageObject    : 찢어진 종이 GameObject
///   - tornPageStatsTMP  : 찢어진 종이 위 스탯 텍스트
///   - paperClickTarget  : 찢어진 종이 Button (클릭 콜백 연결용)
///   - summonAnimDuration: 소환 연출 길이 (초)
///   - scatterAnimDuration: 흩어지는 연출 길이 (초)
///   - tearAnimDuration  : 찢기 연출 길이 (초)
/// </summary>
public class TitleAnimator : MonoBehaviour
{
    [Header("Scene Objects")]
    [SerializeField] private GameObject notebookObject;
    [SerializeField] private GameObject[] marbleObjects;
    [SerializeField] private GameObject hourglassObject;
    [SerializeField] private GameObject tornPageObject;
    [SerializeField] private TextMeshProUGUI tornPageStatsTMP;
    [SerializeField] private Button paperClickTarget;

    [Header("Animators (optional)")]
    [SerializeField] private Animator notebookAnimator;
    [SerializeField] private Animator[] marbleAnimators;
    [SerializeField] private Animator hourglassAnimator;
    [SerializeField] private Animator tornPageAnimator;

    [Header("Durations")]
    [SerializeField] private float summonAnimDuration = 1.5f;
    [SerializeField] private float scatterAnimDuration = 1.2f;
    [SerializeField] private float tearAnimDuration = 1.0f;

    // ── Phase 01 ─────────────────────────────────────────

    /// <summary>공책 등장.</summary>
    public void ShowNotebook()
    {
        if (notebookObject) notebookObject.SetActive(true);
        if (notebookAnimator) notebookAnimator.SetTrigger("Show");
    }

    // ── Phase 02 ─────────────────────────────────────────

    /// <summary>구슬 + 모래시계 소환 연출. 끝날 때까지 대기.</summary>
    public IEnumerator SummonObjects()
    {
        if (hourglassObject) hourglassObject.SetActive(true);
        if (hourglassAnimator) hourglassAnimator.SetTrigger("Summon");

        foreach (var m in marbleObjects)
            if (m) m.SetActive(true);

        // TODO: 구슬 하나씩 등장하는 stagger 연출
        // marbleAnimators[i].SetTrigger("Roll") 등으로 교체 예정

        yield return new WaitForSeconds(summonAnimDuration);
    }

    // ── Phase 03 ─────────────────────────────────────────

    /// <summary>구슬 흩어지기 연출.</summary>
    public IEnumerator ScatterMarbles()
    {
        foreach (var anim in marbleAnimators)
            if (anim) anim.SetTrigger("Scatter");

        yield return new WaitForSeconds(scatterAnimDuration);

        // 구슬 오브젝트 비활성화
        foreach (var m in marbleObjects)
            if (m) m.SetActive(false);
    }

    // ── Phase 04 ─────────────────────────────────────────

    /// <summary>공책 찢기 + 나머지 오브젝트 소멸.</summary>
    public IEnumerator TearNotebookPage()
    {
        // 모래시계 등 퇴장
        if (hourglassObject) hourglassObject.SetActive(false);

        // 공책 찢기 애니메이션
        if (notebookAnimator) notebookAnimator.SetTrigger("Tear");

        yield return new WaitForSeconds(tearAnimDuration);

        if (notebookObject) notebookObject.SetActive(false);

        // 찢어진 종이 등장
        if (tornPageObject) tornPageObject.SetActive(true);
        if (tornPageAnimator) tornPageAnimator.SetTrigger("Show");
    }

    /// <summary>찢어진 종이에 최종 스탯 출력.</summary>
    public void ShowFinalStatPaper(PlayerStats stats)
    {
        if (tornPageStatsTMP == null) return;
        tornPageStatsTMP.text =
            $"STR  {stats.STR}\n" +
            $"DEX  {stats.DEX}\n" +
            $"PER  {stats.PER}\n" +
            $"INT  {stats.INT}\n" +
            $"LUK  {stats.LUK}\n" +
            $"HUM  {stats.HUM}";
    }

    /// <summary>찢어진 종이 클릭 이벤트 등록.</summary>
    public void EnablePaperClick(Action callback)
    {
        if (paperClickTarget == null) return;
        paperClickTarget.interactable = true;
        paperClickTarget.onClick.RemoveAllListeners();
        paperClickTarget.onClick.AddListener(() => callback?.Invoke());
    }
}
