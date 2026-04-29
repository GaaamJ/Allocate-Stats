using UnityEngine;
using System.Collections;

/// <summary>
/// 씬의 나레이터 채널 세 개를 관리하는 라우터.
/// NarrationBlock.channel을 읽어서 알맞은 INarrator로 위임한다.
///
/// RoomRunContext, TitleSceneController 등에서
/// 개별 Narrator 대신 이 컴포넌트 하나만 참조하면 된다.
///
/// [Inspector 연결]
///   screenNarrator : ScreenNarrator
///   worldNarrator  : WorldNarrator
///   paperNarrator  : PaperNarrator
/// </summary>
public class NarratorRouter : MonoBehaviour
{
    [SerializeField] private ScreenNarrator screenNarrator;
    [SerializeField] private WorldNarrator worldNarrator;
    [SerializeField] private PaperNarrator paperNarrator;
    [SerializeField] private CheckPhaseAnimator checkPhaseAnimator;

    // ── 공개 채널 접근 ────────────────────────────────────

    public ScreenNarrator Screen => screenNarrator;
    public WorldNarrator World => worldNarrator;
    public PaperNarrator Paper => paperNarrator;

    /// <summary>
    /// Screen / World 채널 나레이션 재생 중 여부.
    /// PaperNarrator가 이 값을 확인해서 Tab 토글을 차단한다.
    /// </summary>
    public bool IsNarrating { get; private set; } = false;

    // ── 라우팅 ────────────────────────────────────────────

    /// <summary>
    /// 블록 배열을 채널별로 올바른 Narrator에 넘긴다.
    /// 블록들이 서로 다른 채널이면 각자의 Narrator가 독립적으로 처리한다.
    /// 단일 채널 블록 배열은 해당 Narrator에 한 번에 넘긴다.
    /// </summary>
    public IEnumerator ShowBlocks(NarrationBlock[] blocks)
    {
        IsNarrating = true;
        if (blocks == null)
        {
            IsNarrating = false;
            yield break;
        }

        // 채널이 하나라면 그 채널에 한 번에 위임 (순서 보장)
        // 채널이 섞여 있으면 블록 단위로 순서대로 처리
        foreach (var block in blocks)
        {
            if (block == null || string.IsNullOrEmpty(block.text)) continue;
            if (!block.CanShow(PlayerStats.Instance)) continue;
            var narrator = Resolve(block.channel);
            if (narrator == null)
            {
                Debug.LogWarning($"[NarratorRouter] {block.channel} 채널 Narrator가 연결되지 않았습니다.");
                continue;
            }
            if (block.channel == NarratorChannel.Paper)
                checkPhaseAnimator?.ResetGraphic();
            yield return narrator.ShowText(block);

            float pause = block.pauseAfter > 0f ? block.pauseAfter : 0f;
            if (pause > 0f) yield return new WaitForSeconds(pause);
        }
        IsNarrating = false;
    }

    /// <summary>단일 블록을 채널에 맞게 출력.</summary>
    public IEnumerator ShowText(NarrationBlock block)
    {
        IsNarrating = true;
        if (block == null) { IsNarrating = false; yield break; }
        if (!block.CanShow(PlayerStats.Instance)) { IsNarrating = false; yield break; }
        if (block.channel == NarratorChannel.Paper)
            checkPhaseAnimator?.ResetGraphic();
        var narrator = Resolve(block.channel);
        if (narrator == null) { IsNarrating = false; yield break; }
        yield return narrator.ShowText(block);
        IsNarrating = false;
    }

    /// <summary>지정 채널 클리어.</summary>
    public void Clear(NarratorChannel channel)
    {
        Resolve(channel)?.Clear();
    }

    /// <summary>Screen / World 채널 클리어. Paper는 유지 (Phase 종료 시 호출).</summary>
    public void ClearAll()
    {
        screenNarrator?.Clear();
        worldNarrator?.Clear();
    }

    /// <summary>Paper 포함 모든 채널 클리어 (방 전환 시 호출).</summary>
    public void ClearAllIncludingPaper()
    {
        screenNarrator?.Clear();
        worldNarrator?.Clear();
        paperNarrator?.Clear();
    }

    /// <summary>지정 채널 Skip.</summary>
    public void Skip(NarratorChannel channel)
    {
        Resolve(channel)?.Skip();
    }

    /// <summary>모든 채널 Skip.</summary>
    public void SkipAll()
    {
        screenNarrator?.Skip();
        worldNarrator?.Skip();
        paperNarrator?.Skip();
    }

    // ── 내부 ─────────────────────────────────────────────

    private INarrator Resolve(NarratorChannel channel) => channel switch
    {
        NarratorChannel.Screen => screenNarrator,
        NarratorChannel.World => worldNarrator,
        NarratorChannel.Paper => paperNarrator,
        _ => null,
    };
}
