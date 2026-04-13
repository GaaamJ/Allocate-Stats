using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// 복도 문 버튼 프리팹에 붙이는 컴포넌트.
///
/// 프리팹 계층 예시:
///   DoorButton  (DoorButtonUI, Button)
///     ├─ DoorImage      (Image — 문 스프라이트)
///     └─ LabelTMP       (TextMeshPro — 방 이름 또는 "???" 등)
/// </summary>
public class DoorButtonUI : MonoBehaviour
{
    [SerializeField] private Button          button;
    [SerializeField] private TextMeshProUGUI labelTMP;
    [SerializeField] private Image           doorImage;

    // ── 초기화 ───────────────────────────────────────────

    public void Init(string label, Action onClick)
    {
        if (labelTMP)  labelTMP.text = label;
        button.onClick.AddListener(() => onClick?.Invoke());
    }

    /// <summary>문 스프라이트 교체 (에셋 완성 시 호출).</summary>
    public void SetDoorSprite(Sprite sprite)
    {
        if (doorImage) doorImage.sprite = sprite;
    }

    /// <summary>문 잠금 처리.</summary>
    public void SetLocked(bool locked)
    {
        button.interactable = !locked;
        if (labelTMP) labelTMP.text = locked ? "???" : labelTMP.text;
    }
}
