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
    [SerializeField] private Button[] segmentButtons;
    [SerializeField] private GameObject[] scratchObjects;

    [Header("Colors")]
    [SerializeField] private Color colorOn = Color.white;
    [SerializeField] private Color colorOff = new(0.25f, 0.25f, 0.25f);

    private int currentValue;
    private Action<int> onValueChanged;
    private Action onHover;
    private Action onExit;  // 호버 해제 시 나레이터 복원용

    // ── 초기화 ───────────────────────────────────────────

    /// <param name="onValueChanged">새로 선택된 값(1~5, 또는 0으로 끄기)을 전달</param>
    /// <param name="onHover">커서 진입 시 스탯 설명 출력</param>
    /// <param name="onExit">커서 이탈 시 나레이터 텍스트 복원</param>
    public void Init(
        StatData.StatEntry entry,
        Action<int> onValueChanged,
        Action onHover,
        Action onExit)
    {
        this.onValueChanged = onValueChanged;
        this.onHover = onHover;
        this.onExit = onExit;

        if (labelTMP)
            labelTMP.text = $"{entry.displayName}";

        for (int i = 0; i < segmentButtons.Length; i++)
        {
            int segIndex = i + 1;
            segmentButtons[i].onClick.AddListener(() => OnSegmentClicked(segIndex));
        }

        SetValue(0);
    }

    // ── 세그먼트 클릭 ────────────────────────────────────

    private void OnSegmentClicked(int clickedIndex)
    {
        int newVal = (clickedIndex == currentValue) ? 0 : clickedIndex;
        onValueChanged?.Invoke(newVal);
    }

    // ── 값 갱신 ──────────────────────────────────────────

    public void SetValue(int val)
    {
        currentValue = Mathf.Clamp(val, 0, PlayerStats.MAX_STAT);

        for (int i = 0; i < segmentButtons.Length; i++)
        {
            bool on = (i + 1) <= currentValue;

            var img = segmentButtons[i].GetComponent<Image>();
            if (img) img.color = on ? colorOn : colorOff;

            if (scratchObjects != null && i < scratchObjects.Length && scratchObjects[i])
                scratchObjects[i].SetActive(on);
        }
    }

    // ── 호버 ─────────────────────────────────────────────

    public void OnPointerEnter(PointerEventData _) => onHover?.Invoke();
    public void OnPointerExit(PointerEventData _) => onExit?.Invoke();
}