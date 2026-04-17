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

    // ── 공개 채널 접근 ────────────────────────────────────

    public ScreenNarrator Screen => screenNarrator;
    public WorldNarrator World => worldNarrator;
    public PaperNarrator Paper => paperNarrator;

    // ── 라우팅 ────────────────────────────────────────────

    /// <summary>
    /// 블록 배열을 채널별로 올바른 Narrator에 넘긴다.
    /// 블록들이 서로 다른 채널이면 각자의 Narrator가 독립적으로 처리한다.
    /// 단일 채널 블록 배열은 해당 Narrator에 한 번에 넘긴다.
    /// </summary>
    public IEnumerator ShowBlocks(NarrationBlock[] blocks)
    {
        if (blocks == null) yield break;

        foreach (var block in blocks)
        {
            if (block == null || string.IsNullOrEmpty(block.text)) continue;
            var narrator = Resolve(block.channel);
            if (narrator == null) continue;

            // 이 블록 채널 제외 나머지 클리어
            ClearExcept(block.channel);

            yield return narrator.ShowBlocks(new[] { block });
        }
    }
    private void ClearExcept(NarratorChannel channel)
    {
        if (channel != NarratorChannel.Screen) screenNarrator?.Clear();
        if (channel != NarratorChannel.World) worldNarrator?.Clear();
        if (channel != NarratorChannel.Paper) paperNarrator?.Clear();
    }

    /// <summary>단일 블록을 채널에 맞게 출력.</summary>
    public IEnumerator ShowText(NarrationBlock block)
    {
        if (block == null) yield break;
        var narrator = Resolve(block.channel);
        if (narrator == null) yield break;
        yield return narrator.ShowText(block);
    }

    /// <summary>지정 채널 클리어.</summary>
    public void Clear(NarratorChannel channel)
    {
        Resolve(channel)?.Clear();
    }

    /// <summary>모든 채널 클리어.</summary>
    public void ClearAll()
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
