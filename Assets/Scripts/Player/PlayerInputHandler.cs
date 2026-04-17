using UnityEngine;

/// <summary>
/// PlayerController.OnSkipPressed 이벤트를 NarratorRouter.SkipAll()로 중계.
///
/// 역할 분리:
///   PlayerController — 입력 수신 및 이동/룩/상호작용 처리
///   PlayerInputHandler — Skip 신호 → 나레이션 시스템 전달
///
/// PlayerController와 같은 GameObject에 부착하거나
/// NarratorRouter가 있는 GameObject에 따로 부착해도 무방.
///
/// [Inspector 연결]
///   playerController : PlayerController
///   narratorRouter   : NarratorRouter
/// </summary>
public class PlayerInputHandler : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private NarratorRouter narratorRouter;

    private void Awake()
    {
        if (playerController == null)
            playerController = GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        if (playerController != null)
            playerController.OnSkipPressed += HandleSkip;
    }

    private void OnDisable()
    {
        if (playerController != null)
            playerController.OnSkipPressed -= HandleSkip;
    }

    private void HandleSkip() => narratorRouter?.SkipAll();
}
