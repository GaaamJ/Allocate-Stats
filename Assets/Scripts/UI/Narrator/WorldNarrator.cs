using UnityEngine;
using TMPro;

/// <summary>
/// World 채널 나레이터.
/// 플레이어 행동 결과를 해당 오브젝트 위에 출력 (당신은 ~했습니다.).
/// WorldSpace Canvas에 붙어서 동작.
///
/// 각 InteractableObject마다 인스턴스를 가지거나,
/// 씬에 하나만 두고 위치를 동적으로 이동시키는 방식 둘 다 가능.
/// 현재는 단일 인스턴스 기준으로 구현 — 위치는 SetTarget()으로 지정.
///
/// [Inspector 연결]
///   narratorTMP : TextMeshPro (WorldSpace Canvas 안에 배치)
/// </summary>
public class WorldNarrator : BaseNarrator
{
    [Header("World 채널 TMP (WorldSpace Canvas)")]
    [SerializeField] private TextMeshProUGUI narratorTMP;

    protected override TextMeshProUGUI GetTMP() => narratorTMP;

    // ── 위치 지정 ─────────────────────────────────────────

    /// <summary>
    /// 나레이션을 출력할 월드 좌표 설정.
    /// 3D 씬 구성 전까지는 호출하지 않아도 무방.
    /// </summary>
    public void SetTarget(Vector3 worldPosition)
    {
        transform.position = worldPosition;
    }
}
