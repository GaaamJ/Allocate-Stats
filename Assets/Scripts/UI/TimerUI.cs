using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

/// <summary>
/// Phase 02 타이머 UI.
/// 
/// [Inspector 연결 목록]
///   - timerTMP      : 초 표시 TextMeshPro
///   - timerFillImage: 모래시계 옆 원형 / 선형 fill Image (선택)
///   - hourglassAnimator: 모래시계 오브젝트 Animator (선택)
/// </summary>
public class TimerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerTMP;
    [SerializeField] private Image timerFillImage;   // fillAmount 방식
    [SerializeField] private Animator hourglassAnimator;

    private float totalTime;
    private float elapsed;
    private bool running;
    private Action onExpired;
    private Coroutine timerCoroutine;

    // ── 공개 API ─────────────────────────────────────────

    public void StartTimer(float seconds, Action expiredCallback)
    {
        totalTime = seconds;
        elapsed = 0f;
        onExpired = expiredCallback;
        running = true;

        if (hourglassAnimator) hourglassAnimator.SetBool("Running", true);
        gameObject.SetActive(true);

        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        timerCoroutine = StartCoroutine(Tick());
    }

    public void Stop()
    {
        running = false;
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        if (hourglassAnimator) hourglassAnimator.SetBool("Running", false);
        gameObject.SetActive(false);
    }

    // ── 내부 틱 ──────────────────────────────────────────

    private IEnumerator Tick()
    {
        while (elapsed < totalTime && running)
        {
            elapsed += Time.deltaTime;
            float remain = Mathf.Max(0f, totalTime - elapsed);

            if (timerTMP)
                timerTMP.text = Mathf.CeilToInt(remain).ToString();

            if (timerFillImage)
                timerFillImage.fillAmount = 1f - (elapsed / totalTime);

            // 5초 이하: 텍스트 빨갛게
            if (timerTMP)
                timerTMP.color = remain <= 5f ? Color.red : Color.white;

            yield return null;
        }

        if (running) // 자연 만료
        {
            running = false;
            onExpired?.Invoke();
        }
    }
}
