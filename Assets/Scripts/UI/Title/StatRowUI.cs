using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;
using System.Collections;

/// <summary>
/// 스탯 1행 프리팹에 붙이는 컴포넌트.
/// 
/// 세그먼트 클릭 방식:
///   - 세그먼트를 클릭하면 "그 칸 번호"가 새 값이 됨.
///   - 이미 켜진 칸을 클릭하면 그 칸으로 줄어듦 (토글 다운).
///   - 켜진 칸들은 펜 획 스프라이트(ScratchSprite)로 표시.
/// 
/// 프리팹 계층 예시:
///   StatRow  (StatRowUI, PointerEnter 감지)
///     ├─ LabelTMP           (TextMeshPro)
///     └─ SegmentContainer   (HorizontalLayoutGroup)
///        ├─ Segment_1       (Button + Image)
///        ├─ Segment_2
///        ├─ Segment_3
///        └─ Segment_4
/// 
/// Segment_N 구조 (단일 오브젝트):
///   Button
///     ├─ BG Image          (기본 빈 칸 스프라이트)
///     └─ Scratch Image     (펜 획 스프라이트 — SetActive로 토글)
/// </summary>
public class StatRowUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI labelTMP;
    [Header("Segments")]
    [SerializeField] private Button[] segmentButtons;   // Inspector에서 연결
    [SerializeField] private GameObject[] scratchObjects;   // 각 세그먼트의 펜 획 오브젝트

    [Header("Colors")]
    [SerializeField] private Color colorOn = Color.white;
    [SerializeField] private Color colorOff = new(0.25f, 0.25f, 0.25f);

    private int currentValue;
    private Action<int> onValueChanged; // StatAllocatorUI에 새 값 전달
    private Action onHover;

    // ── 초기화 ───────────────────────────────────────────

    /// <param name="onValueChanged">새로 선택된 값(1~5, 또는 0으로 끄기)을 전달</param>
    public void Init(
        StatData.StatEntry entry,
        Action<int> onValueChanged,
        Action onHover)
    {
        this.onValueChanged = onValueChanged;
        this.onHover = onHover;

        if (labelTMP)
            labelTMP.text = $"{entry.displayName}";

        // 각 세그먼트 버튼에 클릭 이벤트 등록
        for (int i = 0; i < segmentButtons.Length; i++)
        {
            int segIndex = i + 1; // 1-based
            segmentButtons[i].onClick.AddListener(() => OnSegmentClicked(segIndex));
        }

        SetValue(0);
    }

    // ── 세그먼트 클릭 ────────────────────────────────────

    private void OnSegmentClicked(int clickedIndex)
    {
        // 이미 그 값이면 0으로 (완전히 끄기) — 원하지 않으면 제거 가능
        int newVal = (clickedIndex == currentValue) ? 0 : clickedIndex;
        onValueChanged?.Invoke(newVal);
    }

    // ── 값 갱신 (외부/내부에서 호출) ────────────────────

    public void SetValue(int val)
    {
        currentValue = Mathf.Clamp(val, 0, PlayerStats.MAX_STAT);

        // 세그먼트 상태 갱신
        for (int i = 0; i < segmentButtons.Length; i++)
        {
            bool on = (i + 1) <= currentValue;

            // BG 색상
            var img = segmentButtons[i].GetComponent<Image>();
            if (img) img.color = on ? colorOn : colorOff;

            // 펜 획 스프라이트
            if (scratchObjects != null && i < scratchObjects.Length && scratchObjects[i])
                scratchObjects[i].SetActive(on);
        }
    }

    // ── 호버 → 나레이터 설명 ─────────────────────────────

    public void OnPointerEnter(PointerEventData _) => onHover?.Invoke();
    public void OnPointerExit(PointerEventData _) { }
}
