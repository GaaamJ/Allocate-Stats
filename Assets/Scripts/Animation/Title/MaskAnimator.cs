using UnityEngine;
using MoreMountains.Feedbacks;
using System;
using System.Collections;

/// <summary>
/// P01 가면 등장 연출.
///
/// 연출 흐름:
///   1. SetActive(true)
///   2. (추후) 페이드인
///   3. appearFeedback(MMF_Player) 재생 + 완료 대기
///   4. onComplete 콜백
///
/// [Inspector 연결]
///   appearFeedback : MMF_Player — Scale, PositionShake, Pause, CinemachineImpulse 등
/// </summary>
public class MaskAnimator : MonoBehaviour
{
    [Header("Feel")]
    [SerializeField] private MMF_Player appearFeedback;

    // ── 공개 API ──────────────────────────────────────────

    public IEnumerator Appear(Action onComplete = null)
    {
        // 1. (추후) 페이드인

        // 2. FEEL 애니메이션 재생
        if (appearFeedback != null)
        {
            appearFeedback.PlayFeedbacks();
            yield return new WaitForSeconds(appearFeedback.TotalDuration);
        }
        else
        {
            yield return null;
        }

        onComplete?.Invoke();
    }
}