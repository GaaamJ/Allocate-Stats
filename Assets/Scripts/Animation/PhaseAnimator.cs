using UnityEngine;
using System.Collections;

/// <summary>
/// Phase별 연출 담당 컴포넌트.
/// RoomData.PhaseData.animator 필드에 연결. 없으면 NormalRoomRunner가 스킵.
///
/// 방별로 커스텀 연출이 필요하면 이 클래스를 상속해서 override.
/// 예) MirrorPhaseAnimator : PhaseAnimator
///
/// [Inspector 연결]
///   RoomData.PhaseData.animator 에 씬 오브젝트를 드래그.
/// </summary>
public class PhaseAnimator : MonoBehaviour
{
    /// <summary>Phase 진입 시 호출 — onEnter 나레이션 이전.</summary>
    public virtual IEnumerator OnPhaseEnter()
    {
        yield return null;
    }

    /// <summary>판정 직전 호출 — onBeforeCheck 나레이션 이후, 판정 실행 이전.</summary>
    public virtual IEnumerator OnBeforeCheck()
    {
        yield return null;
    }

    /// <summary>판정 직후 호출 — 판정 실행 이후, 결과 분기 이전.</summary>
    public virtual IEnumerator OnAfterCheck(bool success)
    {
        yield return null;
    }

    /// <summary>Phase 종료 시 호출 — OutcomeData 처리 이전.</summary>
    public virtual IEnumerator OnPhaseExit()
    {
        yield return null;
    }
}
