using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 상호작용 오브젝트 컴포넌트.
/// 상호작용 시 RoomEventBus.Trigger(phaseID) 발행.
///
/// phaseID는 Inspector에서 직접 입력하거나,
/// RoomSceneController가 RoomLayoutData 기반으로 동적 생성 시 SetPhaseID()로 주입.
///
/// 현재는 버튼으로 임시 구현.
/// 추후 3D 구현 시 OnInteract()만 트리거하면 됨 — 내부 로직 변경 불필요.
/// </summary>
public class InteractableObject : MonoBehaviour
{
    [Header("Phase 연결 — RoomData.PhaseData.phaseID와 매칭")]
    [SerializeField] private string phaseID;

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
    /// phaseID 외부 주입 — RoomSceneController.SpawnInteractables()에서 호출.
    /// Inspector에서 직접 입력한 경우 호출 불필요.
    /// </summary>
    public void SetPhaseID(string id)
    {
        phaseID = id;
        if (labelTMP) labelTMP.text = id;  // 임시 — 3D 구현 시 제거
    }

    /// <summary>
    /// 상호작용 진입점.
    /// 현재는 버튼 클릭으로 호출.
    /// 추후 3D 충돌 / 레이캐스트 등으로 교체.
    /// </summary>
    public void OnInteract()
    {
        RoomEventBus.Trigger(phaseID);
    }
}
