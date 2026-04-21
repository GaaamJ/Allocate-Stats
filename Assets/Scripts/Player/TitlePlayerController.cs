using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

/// <summary>
/// 타이틀씬 전용 플레이어 컨트롤러.
/// 이동 없음. Look만 담당.
///
/// [Look — 마우스 이동]
///   P02 진입 시 EnableLook(), 이탈 시 DisableLook().
///   커서는 항상 표시.
///
/// [Look clamp (degrees, Inspector에서 양수로 입력)]
///   pitchUpMax   : 위로 올릴 수 있는 최대 각도
///   pitchDownMax : 아래로 내릴 수 있는 최대 각도
///   yawLeftMax   : 왼쪽으로 돌릴 수 있는 최대 각도
///   yawRightMax  : 오른쪽으로 돌릴 수 있는 최대 각도
///
/// [씬 계층 구조]
///   TitlePlayerRoot          ← 이 컴포넌트
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

    [Header("Look Sensitivity")]
    [SerializeField] private float sensitivity = 0.15f;

    [Header("Look Clamp (degrees, 양수로 입력)")]
    [SerializeField] private float pitchUpMax = 20f;
    [SerializeField] private float pitchDownMax = 40f;
    [SerializeField] private float yawLeftMax = 50f;
    [SerializeField] private float yawRightMax = 50f;

    /// <summary>Skip 버튼 pressed 시 발행.</summary>
    public event System.Action OnSkipPressed;

    private PlayerInput _actions;
    private PlayerInput.PlayerActions _player;

    private float _yaw;
    private float _pitch;
    private float _baseYaw;

    private bool _lookEnabled = false;

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
        if (!_lookEnabled) return;
        Vector2 delta = _player.Look.ReadValue<Vector2>() * sensitivity;
        UpdateLook(delta);
    }

    // ── Look 제어 ─────────────────────────────────────────

    /// <summary>P02 진입 시 호출.</summary>
    public void EnableLook()
    {
        _baseYaw = _yaw;
        _lookEnabled = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>P02 이탈 시 호출.</summary>
    public void DisableLook()
    {
        _lookEnabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // ── 내부 ──────────────────────────────────────────────

    private void UpdateLook(Vector2 delta)
    {
        _yaw += delta.x;
        _pitch -= delta.y;

        _pitch = Mathf.Clamp(_pitch, -pitchUpMax, pitchDownMax);

        float rel = Mathf.DeltaAngle(_baseYaw, _yaw);
        rel = Mathf.Clamp(rel, -yawLeftMax, yawRightMax);
        _yaw = _baseYaw + rel;

        ApplyRotation();
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