using UnityEngine;
using TMPro;

/// <summary>
/// Paper 채널 나레이터.
/// 플레이어가 들고 있는 종이 오브젝트 위에 지시사항 출력.
/// WorldSpace Canvas에 붙어서 동작.
///
/// 종이 오브젝트의 Transform을 paperTarget으로 연결하면
/// 종이를 따라 이동한다.
///
/// [Inspector 연결]
///   narratorTMP : TextMeshPro (종이 오브젝트 위 Canvas 안)
///   paperTarget : 종이 오브젝트 Transform (없으면 고정 위치)
/// </summary>
public class PaperNarrator : BaseNarrator
{
    [Header("Paper 채널 TMP (종이 위 WorldSpace Canvas)")]
    [SerializeField] private TextMeshProUGUI narratorTMP;

    [Header("종이 오브젝트 Transform — 이 위치를 따라간다")]
    [SerializeField] private Transform paperTarget;

    protected override TextMeshProUGUI GetTMP() => narratorTMP;

    private void LateUpdate()
    {
        if (paperTarget != null)
            transform.position = paperTarget.position;
    }
}
