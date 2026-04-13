using UnityEngine;
using System.Collections;

/// <summary>
/// 방 연출 담당 컴포넌트.
/// 현재는 모두 stub — 에셋/애니메이션 완성 시 내부만 채워 넣으면 됨.
/// RoomSceneController는 null 체크로 처리하므로 연결하지 않으면 연출 전체 스킵.
///
/// 방별 연출이 달라지면 RoomAnimator를 상속한 클래스를 만들어서
/// OnRoomEnter / OnBeforeCheck / OnAfterCheck 를 override.
///
/// [Inspector 연결 목록 (예시 — 방마다 다를 수 있음)]
///   - roomEnvironmentAnimator : 방 배경 Animator
/// </summary>
public class RoomAnimator : MonoBehaviour
{
    [Header("공통 연출 오브젝트 (선택)")]
    [SerializeField] private Animator roomEnvironmentAnimator;

    // ── 진입 연출 ──────────────────────────────────────────

    public virtual IEnumerator OnRoomEnter(string roomID)
    {
        // TODO: roomID 별 연출 구현
        // 예) if (roomID == "mirror") yield return ShowMirrors();
        yield return null;
    }

    // ── 판정 전 연출 ───────────────────────────────────────

    public virtual IEnumerator OnBeforeCheck(string roomID, int stepIndex)
    {
        // TODO: 판정 전 연출 (카메라 이동, 이펙트 등)
        yield return null;
    }

    // ── 판정 후 연출 ───────────────────────────────────────

    public virtual IEnumerator OnAfterCheck(string roomID, int stepIndex, bool success)
    {
        // TODO: 성공/실패 연출 분기
        // 예) if (!success) yield return PlayFailEffect();
        yield return null;
    }

    // ── 방별 연출 stub ────────────────────────────────────

    protected virtual IEnumerator ShowMirrors()  { yield return null; }
    protected virtual IEnumerator BreakMirror()  { yield return null; }
    protected virtual IEnumerator ShowMonster()  { yield return null; }
    protected virtual IEnumerator KillMonster()  { yield return null; }
    protected virtual IEnumerator ShowAltar()    { yield return null; }
    protected virtual IEnumerator OfferIdol()    { yield return null; }
    protected virtual IEnumerator PlayWhisper()  { yield return null; }
}
