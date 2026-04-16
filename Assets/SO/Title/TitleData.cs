using UnityEngine;

/// <summary>
/// TitleScene 나레이터 텍스트 데이터.
/// 각 Phase별 블록 배열 — 배열 원소 하나가 화면에 출력되는 텍스트 단위.
///
/// [Inspector 연결]
///   TitleSceneController.titleData 에 연결.
/// </summary>
[CreateAssetMenu(fileName = "TitleData", menuName = "AllocateStats/TitleData")]
public class TitleData : ScriptableObject
{
    [Header("Phase 01 — 인트로 (블록 단위)")]
    public NarrationBlock[] introBlocks;

    [Header("Phase 02 — 스탯 분배 (블록 단위)")]
    public NarrationBlock[] allocateBlocks;

    [Header("Phase 04 — 확정 (블록 단위)")]
    public NarrationBlock[] confirmBlocks;
}
