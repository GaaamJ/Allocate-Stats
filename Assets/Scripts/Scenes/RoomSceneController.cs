using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine.InputSystem;
#endif

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

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    [Header("Debug")]
    [SerializeField] private bool enableDebugRoomSkip = true;
    [SerializeField] private Key debugRoomSkipKey = Key.F10;
#endif

    private readonly List<GameObject> spawnedLayoutObjects = new();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private bool debugRoomSkipRequested;
#endif

    private void OnEnable()
    {
        if (narratorRouter != null)
            narratorRouter.NarrationActiveChanged += HandleNarrationActiveChanged;
    }

    private void OnDisable()
    {
        if (narratorRouter != null)
            narratorRouter.NarrationActiveChanged -= HandleNarrationActiveChanged;
    }

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

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void Update()
    {
        if (!enableDebugRoomSkip || debugRoomSkipRequested) return;
        if (Keyboard.current == null) return;

        var keyControl = Keyboard.current[debugRoomSkipKey];
        if (keyControl == null || !keyControl.wasPressedThisFrame) return;

        debugRoomSkipRequested = true;
        narratorRouter?.ClearAllIncludingPaper();
        bridge?.DebugSkipCurrentRoom();
    }
#endif

    private void OnDestroy()
    {
        ClearLayoutParent(false);
    }

    private void HandleNarrationActiveChanged(bool isActive)
    {
        if (playerController == null) return;

        if (isActive)
            playerController.DisableMovement();
        else
            playerController.EnableMovement();
    }

    // ── 오브젝트 동적 생성 ────────────────────────────────

    private void SpawnInteractables()
    {
        ClearLayoutParent(true);

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

            Transform parent = GetLayoutParent();
            var obj = Object.Instantiate(entry.prefab, parent);
            spawnedLayoutObjects.Add(obj);

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

    private void ClearLayoutParent(bool createParentIfMissing)
    {
        for (int i = spawnedLayoutObjects.Count - 1; i >= 0; i--)
        {
            var obj = spawnedLayoutObjects[i];
            if (obj == null) continue;
            obj.SetActive(false);
            Destroy(obj);
        }
        spawnedLayoutObjects.Clear();

        Transform parent = createParentIfMissing ? GetLayoutParent() : layoutParent;
        if (parent == null) return;

        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            var child = parent.GetChild(i).gameObject;
            child.SetActive(false);
            Destroy(child);
        }
    }

    private Transform GetLayoutParent()
    {
        if (layoutParent != null) return layoutParent;

        var root = new GameObject("RuntimeRoomLayout");
        root.transform.SetParent(transform);
        layoutParent = root.transform;
        return layoutParent;
    }

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
