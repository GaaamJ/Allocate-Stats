using UnityEngine;

[CreateAssetMenu(fileName = "TitleData", menuName = "AllocateStats/TitleData")]
public class TitleData : ScriptableObject
{   
    [Header("P00 — 프롤로그")]
    public NarrationBlock[] prologueBlocks;

    [Header("P01 — 가면 등장 후")]
    public NarrationBlock[] introBlocks;

    [Header("P01 — 공책 착지 후")]
    public NarrationBlock[] postNotebookBlocks;

    [Header("P02 — 구슬 등장 전")]
    public NarrationBlock[] p02PreBlocks;

    [Header("P02 — 구슬 등장 후 / 공책 클릭 유도")]
    public NarrationBlock[] p02PostBlocks;

    [Header("P02 - 공책 클릭 후 공책에 띄울 텍스트")]
    public NarrationBlock[] p02NotebookBlocks;

    [Header("P03_01")]
    public NarrationBlock[] p03_01Blocks;

    [Header("P03_02")]
    public NarrationBlock[] p03_02Blocks;
}