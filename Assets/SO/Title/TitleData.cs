using UnityEngine;

[CreateAssetMenu(fileName = "TitleData", menuName = "AllocateStats/TitleData")]
public class TitleData : ScriptableObject
{
    [Header("P01 — 가면 등장 후")]
    public NarrationBlock[] introBlocks;

    [Header("P01 — 공책 착지 후")]
    public NarrationBlock[] postNotebookBlocks;

    [Header("P02 — 구슬 등장 전")]
    public NarrationBlock[] p02PreBlocks;

    [Header("P02 — 구슬 등장 후 / 공책 클릭 유도")]
    public NarrationBlock[] p02PostBlocks;

    [Header("P03 — 스탯 확정")]
    public NarrationBlock[] confirmBlocks;
}