using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// RoomScene 총괄 디스패처.
/// 현재 방이 일반 방인지 앙코르 루프인지 판단해서 적절한 Runner에 위임.
/// RoomLayoutData SO를 읽어서 InteractableObject 프리팹을 동적으로 생성.
/// 실제 방 진행 로직은 Runner가 담당 — 이 클래스는 얇게 유지.
///
/// 새 방 타입이 생기면 IRoomRunner 구현체를 추가하고 SelectRunner()에 분기만 추가.
///
/// [Inspector 연결]
///   - narratorUI       : NarratorUI
///   - bridge           : RoomBridge
///   - playerController : PlayerControllerStub (3D 구현 전까지)
///   - layoutParent     : 생성된 오브젝트의 부모 Transform (없으면 씬 루트)
/// </summary>
public class RoomSceneController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private NarratorUI narratorUI;

    [Header("Bridge / Controller")]
    [SerializeField] private RoomBridge bridge;
    [SerializeField] private PlayerControllerStub playerController;

    [Header("Layout — 오브젝트 동적 생성 부모 (없으면 씬 루트)")]
    [SerializeField] private Transform layoutParent;

    private void Start()
    {
        // 이전 씬 구독 잔류 방지
        RoomEventBus.Clear();

        // RoomLayoutData 기반 오브젝트 생성
        SpawnInteractables();

        var context = new RoomRunContext(
            narratorUI,
            bridge,
            playerController
        );

        IRoomRunner runner = SelectRunner();

        if (runner == null)
        {
            Debug.LogError("[RoomSceneController] Runner 선택 실패.");
            return;
        }

        StartCoroutine(runner.Run(context));
    }

    // ── 오브젝트 동적 생성 ────────────────────────────────

    /// <summary>
    /// RoomLayoutData SO를 읽어서 InteractableObject 프리팹 생성 후 phaseID 주입.
    /// 앙코르 루프는 layoutData 없음 — 스킵.
    /// </summary>
    private void SpawnInteractables()
    {
        if (bridge.IsEncoreLoop) return;

        var layoutData = bridge.CurrentLayoutData;
        if (layoutData == null)
        {
            Debug.Log("[RoomSceneController] RoomLayoutData 없음 — 오브젝트 생성 스킵.");
            return;
        }

        if (layoutData.interactables == null) return;

        foreach (var entry in layoutData.interactables)
        {
            if (entry.prefab == null)
            {
                Debug.LogWarning($"[RoomSceneController] objectID '{entry.objectID}' 프리팹이 null.");
                continue;
            }

            var obj = Object.Instantiate(
                entry.prefab,
                layoutParent
            );

            // 임시 UI 배치 — 3D 교체 시 아래 두 줄 제거 후 position/rotation 사용
            var rect = obj.GetComponent<RectTransform>();
            if (rect != null)
                rect.anchoredPosition = Vector2.zero;
            else
            {
                // 3D 오브젝트일 때 — 현재는 미사용
                obj.transform.position = entry.position;
                obj.transform.rotation = Quaternion.Euler(entry.rotation);
            }

            // InteractableObject에 objectID 주입
            var interactable = obj.GetComponent<InteractableObject>();
            if (interactable != null)
                interactable.SetObjectID(entry.objectID);
            else
                Debug.LogWarning($"[RoomSceneController] '{entry.prefab.name}'에 InteractableObject 컴포넌트 없음.");
        }
    }

    // ── Runner 선택 ───────────────────────────────────────

    /// <summary>
    /// 현재 상태에 맞는 Runner 반환.
    /// 새 방 타입 추가 시 여기에 분기 추가.
    /// </summary>
    private IRoomRunner SelectRunner()
    {
        if (bridge.IsEncoreLoop)
            return new EncoreRoomRunner();

        if (bridge.CurrentRoomData != null)
            return new NormalRoomRunner();

        Debug.LogError("[RoomSceneController] IsEncoreLoop=false인데 CurrentRoomData도 null.");
        return null;
    }
}