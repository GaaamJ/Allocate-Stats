using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// RoomScene 총괄 디스패처.
/// 현재 방이 일반 방인지 앙코르 루프인지 판단해서 적절한 Runner에 위임.
/// 실제 방 진행 로직은 Runner가 담당 — 이 클래스는 얇게 유지.
///
/// 새 방 타입이 생기면 IRoomRunner 구현체를 추가하고 Start()에 분기만 추가.
///
/// [Inspector 연결]
///   - narratorUI        : NarratorUI
///   - continueButton    : 진행 버튼
///   - bridge            : RoomBridge
///   - playerController  : PlayerControllerStub (3D 구현 전까지)
/// </summary>
public class RoomSceneController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private NarratorUI narratorUI;
    [SerializeField] private Button continueButton;

    [Header("Bridge / Controller")]
    [SerializeField] private RoomBridge bridge;
    [SerializeField] private PlayerControllerStub playerController;

    private void Start()
    {
        // 이전 씬 구독 잔류 방지
        RoomEventBus.Clear();

        var context = new RoomRunContext(
            narratorUI,
            continueButton,
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
