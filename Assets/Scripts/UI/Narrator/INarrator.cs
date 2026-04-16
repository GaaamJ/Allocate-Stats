using System.Collections;

/// <summary>
/// 모든 나레이터 채널이 구현해야 하는 인터페이스.
/// 타이핑 연출은 BaseNarrator에서 공통 처리한다.
///
/// 호버(스탯 설명 교체/복원)는 ScreenNarrator 전용 기능이므로
/// 인터페이스 밖에 둔다.
/// </summary>
public interface INarrator
{
    /// <summary>
    /// 블록 배열을 순서대로 출력한다.
    /// 블록마다 화면을 지우고 타이핑 시작.
    /// 완료까지 대기.
    /// </summary>
    IEnumerator ShowBlocks(NarrationBlock[] blocks);

    /// <summary>
    /// 단일 블록을 출력한다.
    /// 완료까지 대기.
    /// </summary>
    IEnumerator ShowText(NarrationBlock block);

    /// <summary>현재 텍스트를 즉시 클리어.</summary>
    void Clear();

    /// <summary>
    /// 현재 타이핑 중인 블록을 즉시 완성.
    /// 이미 완성 상태라면 다음 블록으로 넘어간다 (ShowBlocks 내부 신호).
    /// </summary>
    void Skip();
}
