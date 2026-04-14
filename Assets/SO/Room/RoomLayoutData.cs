using UnityEngine;

/// <summary>
/// 방 오브젝트 배치 데이터 ScriptableObject.
/// RoomSceneController가 씬 로드 시 이 데이터를 읽어서
/// InteractableObject 프리팹을 동적으로 생성하고 phaseID를 주입.
///
/// [Inspector 연결]
///   RoomSceneController.layoutData 에 연결.
/// </summary>
[CreateAssetMenu(fileName = "RoomLayoutData", menuName = "AllocateStats/RoomLayoutData")]
public class RoomLayoutData : ScriptableObject
{
    [Header("배치할 오브젝트 목록")]
    public InteractableEntry[] interactables;

    // ─────────────────────────────────────────────────────────
    // InteractableEntry
    // ─────────────────────────────────────────────────────────

    [System.Serializable]
    public class InteractableEntry
    {
        [Header("프리팹 — InteractableObject 컴포넌트 포함")]
        public GameObject prefab;

        [Header("연결할 phaseID — RoomData.PhaseData.phaseID와 매칭")]
        public string phaseID;

        [Header("배치 위치 / 회전")]
        public Vector3 position;
        public Vector3 rotation;
    }
}
