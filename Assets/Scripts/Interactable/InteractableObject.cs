using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 상호작용 오브젝트 컴포넌트.
/// 상호작용 시 RoomEventBus.TriggerObject(objectID) 발행.
/// 어떤 Phase로 연결할지는 BaseRoomRunner가 결정.
///
/// 현재는 버튼으로 임시 구현.
/// 추후 3D 구현 시 OnInteract()만 트리거하면 됨 — 내부 로직 변경 불필요.
///
/// [Inspector 연결]
///   objectID : 이 오브젝트의 식별자 — RoomData.PhaseData.triggerObjectID와 매칭
///   label    : 버튼 표시 텍스트 (임시)
/// </summary>
public class InteractableObject : MonoBehaviour
{
    [Header("오브젝트 식별자 — RoomData.PhaseData.triggerObjectID와 매칭")]
    [SerializeField] private string objectID;

    [Header("임시 버튼 UI — 3D 구현 시 제거")]
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI labelTMP;
    [SerializeField] private string label;

    private void Start()
    {
        // 임시 버튼 연결 — 3D 구현 시 이 블록 제거
        if (button)
        {
            if (labelTMP) labelTMP.text = label;
            button.onClick.AddListener(OnInteract);
        }
    }

    /// <summary>
    /// objectID 외부 주입 — RoomSceneController.SpawnInteractables()에서 호출.
    /// Inspector에서 직접 입력한 경우 호출 불필요.
    /// </summary>
    public void SetObjectID(string id)
    {
        objectID = id;
        if (labelTMP) labelTMP.text = id;  // 임시 — 3D 구현 시 제거
    }

    /// <summary>
    /// 상호작용 진입점.
    /// 현재는 버튼 클릭으로 호출.
    /// 추후 3D 충돌 / 레이캐스트 등으로 교체.
    /// </summary>
    public void OnInteract()
    {
        RoomEventBus.TriggerObject(objectID);
    }
}
