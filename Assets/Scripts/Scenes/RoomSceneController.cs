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
///   - narratorRouter     : NarratorRouter
///   - bridge             : RoomBridge
///   - playerController   : PlayerController
///   - layoutParent       : 생성된 오브젝트의 부모 Transform (없으면 씬 루트)
/// </summary>
public class RoomSceneController : MonoBehaviour
{
    [Header("Narrator")]
    [SerializeField] private NarratorRouter narratorRouter;

    [Header("Bridge / Controller")]
    [SerializeField] private RoomBridge bridge;
    [SerializeField] private PlayerController playerController;

    [Header("Layout — 오브젝트 동적 생성 부모 (없으면 씬 루트)")]
    [SerializeField] private Transform layoutParent;

    [Header("방 이름 UI")]
    [SerializeField] private RoomNameUI roomNameUI;

    [Header("판정 연출")]
    [SerializeField] private CheckPhaseAnimator checkPhaseAnimator;

    private void Start()
    {
        RoomEventBus.Clear();
        SpawnInteractables();
        InitRoomNameUI();

        var context = new RoomRunContext(
            narratorRouter,
            bridge,
            playerController
        );

        IRoomRunner runner = SelectRunner();

        if (runner == null)
        {
            Debug.LogError("[RoomSceneController] Runner 선택 실패.");
            return;
        }

        if (runner is BaseRoomRunner baseRunner)
            baseRunner.CheckAnimator = checkPhaseAnimator;

        StartCoroutine(runner.Run(context));
    }

    // ── 오브젝트 동적 생성 ────────────────────────────────

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

            var obj = Object.Instantiate(entry.prefab, layoutParent);

            var rect = obj.GetComponent<RectTransform>();
            if (rect != null)
                rect.anchoredPosition = Vector2.zero;
            else
            {
                obj.transform.position = entry.position;
                obj.transform.rotation = Quaternion.Euler(entry.rotation);
            }

            var interactable = obj.GetComponent<InteractableObject>();
            if (interactable != null)
                interactable.SetObjectID(entry.objectID);
            else
                Debug.LogWarning($"[RoomSceneController] '{entry.prefab.name}'에 InteractableObject 컴포넌트 없음.");
        }
    }

    // ── Runner 선택 ───────────────────────────────────────

    private void InitRoomNameUI()
    {
        if (roomNameUI == null) return;
        string name = bridge.IsEncoreLoop
            ? "영원의 방"
            : (bridge.CurrentRoomData?.displayName ?? "");
        roomNameUI.Init(name);
    }

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
