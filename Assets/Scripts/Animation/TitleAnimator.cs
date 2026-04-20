using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

/// <summary>
/// TitleScene 오브젝트 연출 담당.
///
/// Phase 흐름에 따른 연출:
///   P01 : 가면 등장(ApproachMask) → 나레이션 종료 후 공책 비행(FlyNotebook)
///   P02 : 구슬 12개 시간차 낙하(DropMarbles) → 공책 클릭 → 확대 + UI 오픈(OpenNotebook)
///         스탯 배분 시마다 구슬 1개 소멸(ConsumeMarble)
///   P04 : 종이 찢기(TearNotebookPage) → 공책 닫히고 가면쪽으로 퇴장(CloseAndExitNotebook)
///         → 가면도 뒤로 물러남(RetractMask) → 종이 플레이어 쪽으로(PresentTornPage)
///
/// [Inspector 연결 목록]
///   maskObject          : 가면 GameObject (Sphere + 구멍 Cube 3개로 구성)
///   notebookObject      : 공책 GameObject
///   notebookCanvas      : 공책 위 World Space Canvas (StatAllocatorUI 부모)
///   notebookClickTarget : 공책 클릭용 Button (3D World Space Canvas 위)
///   marbleObjects       : 구슬 배열 (총 12개, 시작 시 비활성)
///   tornPageObject      : 찢어진 종이 GameObject
///   tornPageStatsTMP    : 찢어진 종이 위 스탯 TMP
///   paperClickTarget    : 찢어진 종이 클릭용 Button
///
///   maskStartPos        : 가면 시작 위치 (화면 먼 어둠 속)
///   maskStopPos         : 가면 멈출 위치 (테이블 건너편)
///   notebookStartPos    : 공책 비행 시작 위치 (가면 앞)
///   notebookRestPos     : 공책이 착지할 테이블 위 위치
///   notebookOpenPos     : 공책 확대 시 카메라 앞 위치
///   tornPagePresentPos  : 종이가 플레이어에게 건네질 위치
///
///   maskApproachDuration   : 가면 이동 시간
///   notebookFlyDuration    : 공책 비행 시간
///   marbleDropInterval     : 구슬 낙하 시간차 (개당)
///   marbleDropDuration     : 구슬 낙하 높이 이동 시간
///   notebookOpenDuration   : 공책 확대 시간
///   tearDuration           : 찢기 연출 시간
///   notebookExitDuration   : 공책 퇴장 시간
///   maskRetractDuration    : 가면 물러나는 시간
///   pagePresentDuration    : 종이 건네는 시간
/// </summary>
public class TitleAnimator : MonoBehaviour
{
    // ── Scene Objects ─────────────────────────────────────

    [Header("Mask (외계 가면)")]
    [SerializeField] private GameObject maskObject;

    [Header("Notebook (공책)")]
    [SerializeField] private GameObject notebookObject;
    [SerializeField] private Canvas notebookCanvas;       // World Space Canvas
    [SerializeField] private Button notebookClickTarget;  // 공책 클릭 버튼

    [Header("Marbles (구슬 12개, 시작 시 비활성)")]
    [SerializeField] private GameObject[] marbleObjects;

    [Header("Torn Page (찢어진 종이)")]
    [SerializeField] private GameObject tornPageObject;
    [SerializeField] private TextMeshProUGUI tornPageStatsTMP;
    [SerializeField] private Button paperClickTarget;

    // ── Positions ─────────────────────────────────────────

    [Header("Positions")]
    [SerializeField] private Transform maskStartPos;
    [SerializeField] private Transform maskStopPos;
    [SerializeField] private Transform notebookStartPos;   // 가면 앞 허공
    [SerializeField] private Transform notebookRestPos;    // 테이블 위
    [SerializeField] private Transform notebookOpenPos;    // 카메라 앞 확대 위치
    [SerializeField] private Transform tornPagePresentPos; // 종이 건네는 위치

    // ── Durations ─────────────────────────────────────────

    [Header("Durations")]
    [SerializeField] private float maskApproachDuration = 3.0f;
    [SerializeField] private float notebookFlyDuration = 1.0f;
    [SerializeField] private float marbleDropInterval = 0.08f; // 개당 시간차
    [SerializeField] private float marbleDropDuration = 0.5f;  // 낙하 이동 시간
    [SerializeField] private float notebookOpenDuration = 0.4f;
    [SerializeField] private float tearDuration = 0.8f;
    [SerializeField] private float notebookExitDuration = 0.8f;
    [SerializeField] private float maskRetractDuration = 1.2f;
    [SerializeField] private float pagePresentDuration = 0.6f;

    // ── 내부 상태 ─────────────────────────────────────────

    private int _marblesConsumed = 0;

    // ═════════════════════════════════════════════════════
    // P01 — 가면 등장
    // ═════════════════════════════════════════════════════

    /// <summary>
    /// 가면을 어둠 너머에서 테이블 건너편까지 이동.
    /// TitleSceneController.RunP01() 시작 시 호출.
    /// 이동 완료까지 대기.
    /// </summary>
    public IEnumerator ApproachMask()
    {
        if (!maskObject) yield break;

        if (maskStartPos) maskObject.transform.position = maskStartPos.position;
        maskObject.SetActive(true);

        Vector3 target = maskStopPos ? maskStopPos.position : maskObject.transform.position;
        yield return MoveObject(maskObject.transform, target, maskApproachDuration, EaseOutCubic);
    }

    // ═════════════════════════════════════════════════════
    // P01 끝 — 공책 비행 (나레이션 종료 후)
    // ═════════════════════════════════════════════════════

    /// <summary>
    /// 나레이션 종료 후 공책을 가면 앞에서 테이블 위로 날려 보냄.
    /// 착지 완료까지 대기.
    /// </summary>
    public IEnumerator FlyNotebook()
    {
        if (!notebookObject) yield break;

        if (notebookStartPos) notebookObject.transform.position = notebookStartPos.position;
        notebookObject.SetActive(true);

        if (notebookCanvas) notebookCanvas.gameObject.SetActive(false); // 스탯 UI는 아직 숨김

        Vector3 target = notebookRestPos ? notebookRestPos.position : notebookObject.transform.position;
        yield return MoveObject(notebookObject.transform, target, notebookFlyDuration, EaseOutBack);
    }

    // ═════════════════════════════════════════════════════
    // P02 — 구슬 낙하
    // ═════════════════════════════════════════════════════

    /// <summary>
    /// 구슬 12개를 짧은 시간차로 하나씩 위에서 아래로 낙하.
    /// 모든 구슬 착지 완료까지 대기.
    /// </summary>
    public IEnumerator DropMarbles()
    {
        if (marbleObjects == null || marbleObjects.Length == 0) yield break;

        // 각 구슬의 착지 위치는 Inspector에서 미리 배치해 둔 위치를 기준으로 사용.
        // 낙하 시작 위치 = 착지 위치 + Vector3.up * dropHeight
        const float dropHeight = 8f;

        Coroutine lastDrop = null;
        foreach (var marble in marbleObjects)
        {
            if (!marble) continue;

            Vector3 landPos = marble.transform.position;
            marble.transform.position = landPos + Vector3.up * dropHeight;
            marble.SetActive(true);

            lastDrop = StartCoroutine(MoveObject(marble.transform, landPos, marbleDropDuration, EaseInCubic));
            yield return new WaitForSeconds(marbleDropInterval);
        }

        // 마지막 구슬 착지까지 대기
        if (lastDrop != null) yield return lastDrop;
    }

    // ═════════════════════════════════════════════════════
    // P02 — 공책 확대 (클릭 시)
    // ═════════════════════════════════════════════════════

    /// <summary>
    /// 공책 클릭 콜백을 등록. 클릭 시 공책 확대 + 스탯 UI 활성화.
    /// onOpened: 확대 완료 후 TitleSceneController에게 알림.
    /// </summary>
    public void EnableNotebookClick(Action onOpened)
    {
        if (!notebookClickTarget) return;
        notebookClickTarget.interactable = true;
        notebookClickTarget.onClick.RemoveAllListeners();
        notebookClickTarget.onClick.AddListener(() =>
        {
            notebookClickTarget.interactable = false;
            StartCoroutine(OpenNotebook(onOpened));
        });
    }

    private IEnumerator OpenNotebook(Action onOpened)
    {
        if (!notebookObject) yield break;

        Vector3 target = notebookOpenPos ? notebookOpenPos.position : notebookObject.transform.position;
        yield return MoveObject(notebookObject.transform, target, notebookOpenDuration, EaseOutCubic);

        // 스탯 UI 활성화
        if (notebookCanvas) notebookCanvas.gameObject.SetActive(true);

        onOpened?.Invoke();
    }

    // ═════════════════════════════════════════════════════
    // P02 — 스탯 배분 시 구슬 소멸
    // ═════════════════════════════════════════════════════

    /// <summary>
    /// 스탯 1포인트 배분 시마다 호출. 구슬 1개를 순서대로 비활성화.
    /// StatAllocatorUI에서 onValueChanged 콜백으로 호출.
    /// </summary>
    public void ConsumeMarble()
    {
        if (marbleObjects == null) return;
        if (_marblesConsumed >= marbleObjects.Length) return;

        var marble = marbleObjects[_marblesConsumed];
        if (marble) marble.SetActive(false);
        _marblesConsumed++;
    }

    /// <summary>구슬 소비 카운터 리셋 (스탯 감소로 인한 반환 처리 시 사용).</summary>
    public void SetMarblesConsumed(int count)
    {
        _marblesConsumed = Mathf.Clamp(count, 0, marbleObjects?.Length ?? 0);
        if (marbleObjects == null) return;
        for (int i = 0; i < marbleObjects.Length; i++)
        {
            if (marbleObjects[i])
                marbleObjects[i].SetActive(i >= _marblesConsumed);
        }
    }

    // ═════════════════════════════════════════════════════
    // P04 — 종이 찢기 + 공책 퇴장 + 가면 물러남 + 종이 건네기
    // ═════════════════════════════════════════════════════

    /// <summary>
    /// P04 전체 연출 시퀀스.
    /// ① 스탯 UI 닫기
    /// ② 공책 찢기 연출
    /// ③ 공책 닫히고 가면 방향으로 날아가 사라짐
    /// ④ 가면도 뒤로 물러나 어둠 속으로
    /// ⑤ 찢어진 종이가 플레이어 쪽으로
    /// </summary>
    public IEnumerator TearNotebookPage()
    {
        // ① 스탯 UI 닫기
        if (notebookCanvas) notebookCanvas.gameObject.SetActive(false);

        // ② 찢기 연출 (Animator 없으면 대기만)
        yield return new WaitForSeconds(tearDuration);

        // ③ 공책 퇴장 (maskStopPos 방향으로)
        if (notebookObject)
        {
            Vector3 exitTarget = maskStopPos ? maskStopPos.position + Vector3.back * 3f
                                            : notebookObject.transform.position + Vector3.back * 5f;
            yield return MoveObject(notebookObject.transform, exitTarget, notebookExitDuration, EaseInCubic);
            notebookObject.SetActive(false);
        }

        // ④ 가면 물러남
        if (maskObject)
        {
            Vector3 retractTarget = maskStartPos ? maskStartPos.position : maskObject.transform.position + Vector3.back * 10f;
            // 공책 퇴장과 가면 물러남을 동시에 진행하려면 ③에서 yield하지 않고
            // 여기서 함께 시작하는 방식도 가능. 지금은 순차로.
            StartCoroutine(MoveObject(maskObject.transform, retractTarget, maskRetractDuration, EaseInCubic));
        }

        // ⑤ 찢어진 종이 등장
        if (tornPageObject)
        {
            if (notebookRestPos) tornPageObject.transform.position = notebookRestPos.position;
            tornPageObject.SetActive(true);

            Vector3 presentTarget = tornPagePresentPos ? tornPagePresentPos.position
                                                      : tornPageObject.transform.position + Vector3.forward * 2f;
            yield return MoveObject(tornPageObject.transform, presentTarget, pagePresentDuration, EaseOutBack);
        }
    }

    /// <summary>찢어진 종이에 최종 스탯 출력.</summary>
    public void ShowFinalStatPaper(PlayerStats stats)
    {
        if (!tornPageStatsTMP || stats == null) return;
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
        if (!paperClickTarget) return;
        paperClickTarget.interactable = true;
        paperClickTarget.onClick.RemoveAllListeners();
        paperClickTarget.onClick.AddListener(() => callback?.Invoke());
    }

    // ═════════════════════════════════════════════════════
    // 공용 이동 헬퍼
    // ═════════════════════════════════════════════════════

    private IEnumerator MoveObject(Transform t, Vector3 target, float duration, Func<float, float> ease)
    {
        Vector3 start = t.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float p = ease(Mathf.Clamp01(elapsed / duration));
            t.position = Vector3.LerpUnclamped(start, target, p);
            yield return null;
        }
        t.position = target;
    }

    // ── Easing 함수 ──────────────────────────────────────

    private static float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);
    private static float EaseInCubic(float t) => t * t * t;
    private static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f, c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
    private static float EaseInCubic2(float t) => t * t * t; // alias
}
