using UnityEngine;

/// <summary>
/// 나레이션 한 블록의 데이터 단위.
/// text는 TMP Rich Text를 지원한다 — 인라인 강조는 태그로 직접 작성.
/// 예) "너를 &lt;color=red&gt;원해&lt;/color&gt; 너를 가지고싶어"
///
/// pauseAfter가 0이면 채널 기본값(BaseNarrator.defaultPauseAfter)을 따른다.
/// shakeIntensity가 0이면 흔들림 없음.
/// </summary>
[System.Serializable]
public class NarrationBlock
{
    [TextArea(2, 6)]
    public string text;

    [Tooltip("출력 채널. 채널마다 위치·연출이 다르다.")]
    public NarratorChannel channel;

    [Tooltip("블록 출력 완료 후 대기 시간(초). 0이면 채널 기본값 사용.")]
    public float pauseAfter;

    [Tooltip("타이핑 완료 후 텍스트 흔들림 강도. 0이면 흔들림 없음.")]
    [Range(0f, 5f)]
    public float shakeIntensity;

    [Tooltip("흔들림 주파수(Hz). shakeIntensity > 0일 때만 유효.")]
    [Range(1f, 30f)]
    public float shakeFrequency = 10f;

    // ── 편의 생성자 ───────────────────────────────────────

    public NarrationBlock() { }

    public NarrationBlock(string text, NarratorChannel channel = NarratorChannel.World)
    {
        this.text = text;
        this.channel = channel;
    }
}
