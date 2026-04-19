using UnityEngine;

/// <summary>
/// PlayerController 입력 이벤트를 나레이션 시스템으로 중계.
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
        if (playerController == null) return;
        playerController.OnSkipPressed += HandleSkip;
    }

    private void OnDisable()
    {
        if (playerController == null) return;
        playerController.OnSkipPressed -= HandleSkip;
    }

    private void HandleSkip() => narratorRouter?.SkipAll();
}