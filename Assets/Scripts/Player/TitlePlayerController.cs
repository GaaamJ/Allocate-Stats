using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

/// <summary>
/// 타이틀씬 전용 플레이어 컨트롤러.
/// 이동 없음. Look + Raycast Interact만 담당.
///
/// [Look — 우클릭 홀드]
///   우클릭 홀드 중에만 시점 회전. 커서는 항상 표시.
///   P02 진입 시 EnableLook(), 이탈 시 DisableLook().
///
/// [Interact — 좌클릭]
///   P02 진입 시 EnableInteract() → 좌클릭으로 Raycast → InteractableObject.OnInteract().
///   공책 클릭 확인 후 DisableInteract().
///
/// [Look clamp (degrees, Inspector에서 양수로 입력)]
///   pitchUpMax   : 위로 올릴 수 있는 최대 각도
///   pitchDownMax : 아래로 내릴 수 있는 최대 각도
///   yawLeftMax   : 왼쪽으로 돌릴 수 있는 최대 각도
///   yawRightMax  : 오른쪽으로 돌릴 수 있는 최대 각도
///
/// [씬 계층 구조]
///   TitlePlayerRoot          ← 이 컴포넌트 (Rigidbody 불필요)
///   └─ CameraHolder          ← 수직 회전 pivot
///       └─ CinemachineCamera
///
/// [Inspector 연결]
///   cinemachineCamera : 타이틀 전용 CinemachineCamera
///   cameraHolder      : 수직 회전 pivot Transform
/// </summary>
public class TitlePlayerController : MonoBehaviour
{
    [Header("Cinemachine")]
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private Camera renderCamera;

    [Header("Look Sensitivity")]
    [SerializeField] private float sensitivity = 0.15f;

    [Header("Look Clamp (degrees, 양수로 입력)")]
    [SerializeField] private float pitchUpMax = 20f;  // 위
    [SerializeField] private float pitchDownMax = 40f;  // 아래
    [SerializeField] private float yawLeftMax = 50f;  // 왼쪽
    [SerializeField] private float yawRightMax = 50f;  // 오른쪽

    [Header("Interaction")]
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask interactLayer = ~0;


    /// <summary>Skip 버튼 pressed 시 발행.</summary>
    public event System.Action OnSkipPressed;

    private PlayerInput _actions;
    private PlayerInput.PlayerActions _player;

    private float _yaw;
    private float _pitch;
    private float _baseYaw;

    private bool _lookEnabled = false;
    private bool _interactEnabled = false;

    // ── 생명주기 ──────────────────────────────────────────

    private void Awake()
    {
        _yaw = transform.eulerAngles.y;
        _pitch = cameraHolder ? NormalizeAngle(cameraHolder.localEulerAngles.x) : 0f;
        _baseYaw = _yaw;

        _actions = new PlayerInput();
        _player = _actions.Player;

        _player.Skip.performed += _ => OnSkipPressed?.Invoke();
    }

    private void OnEnable() => _actions.Enable();
    private void OnDisable() => _actions.Disable();

    private void Update()
    {
        if (_lookEnabled)
        {
            Vector2 delta = _player.Look.ReadValue<Vector2>() * sensitivity;
            UpdateLook(delta);
        }

        // 좌클릭으로 공책 Interact
        if (_interactEnabled && Mouse.current.leftButton.wasPressedThisFrame)
            HandleInteract();
    }

    // ── Look 제어 ─────────────────────────────────────────

    /// <summary>P02 진입 시 호출. 현재 방향을 기준 Yaw로 고정.</summary>
    public void EnableLook()
    {
        _baseYaw = _yaw;
        _lookEnabled = true;

        // 커서 항상 표시 — 우클릭 홀드 Look, 좌클릭 Interact 동시 가능
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>P02 이외 Phase 또는 StatAllocatorUI 오픈 시 호출.</summary>
    public void DisableLook()
    {
        _lookEnabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // ── Interact 제어 ─────────────────────────────────────

    /// <summary>P02 진입 시 호출. 공책 좌클릭 활성화.</summary>
    public void EnableInteract() => _interactEnabled = true;

    /// <summary>공책 클릭 확인 후 / P02 이탈 시 호출.</summary>
    public void DisableInteract() => _interactEnabled = false;

    // ── 내부 ──────────────────────────────────────────────

    private void UpdateLook(Vector2 delta)
    {
        _yaw += delta.x;
        _pitch -= delta.y;

        // 상하 개별 clamp
        _pitch = Mathf.Clamp(_pitch, -pitchUpMax, pitchDownMax);

        // 좌우 개별 clamp (baseYaw 기준 상대각)
        float rel = Mathf.DeltaAngle(_baseYaw, _yaw);
        rel = Mathf.Clamp(rel, -yawLeftMax, yawRightMax);
        _yaw = _baseYaw + rel;

        ApplyRotation();
    }

    private void HandleInteract()
    {
        Ray ray = renderCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactLayer))
        {
            Debug.Log($"[TitlePlayer] Hit: {hit.collider.gameObject.name}");
            hit.collider.GetComponentInParent<NotebookInteractable>()?.OnInteract();
        }
        else
        {
            Debug.Log("[TitlePlayer] Hit 없음");
        }
    }

    private void ApplyRotation()
    {
        transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
        if (cameraHolder)
            cameraHolder.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }

    private static float NormalizeAngle(float angle)
        => angle > 180f ? angle - 360f : angle;
}