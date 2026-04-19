using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

/// <summary>
/// 1인칭 플레이어 컨트롤러. PlayerControllerStub 교체 구현체.
///
/// [모드]
///   Title : Look만 가능. 초기 forward 기준 Yaw/Pitch clamp.
///   FPS   : WASD 이동 + Look + Interact(Raycast) 가능.
///
/// [입력 — PlayerInputActions.Player 액션맵]
///   Move     — Vector2 : WASD 이동
///   Look     — Vector2 : 마우스 델타
///   Interact — Button  : Raycast → InteractableObject.OnInteract()
///   Skip     — Button  : OnSkipPressed 이벤트 발행 → PlayerInputHandler 수신
///
/// [씬 계층 구조]
///   PlayerRoot                     ← 이 컴포넌트, Rigidbody, CapsuleCollider
///   └─ CameraHolder (빈 Transform) ← 수직 회전 pivot
///       └─ CinemachineCamera
///
/// [Inspector 연결]
///   cinemachineCamera : 플레이어 전용 CinemachineCamera
///   cameraHolder      : 수직 회전 pivot Transform
///   initialMode       : 씬 로드 시 적용 모드
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : PlayerControllerStub
{
    public enum ControlMode { Title, FPS }

    [Header("Cinemachine")]
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private Transform cameraHolder;

    [Header("Mode")]
    [SerializeField] private ControlMode initialMode = ControlMode.FPS;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float gravity = -20f;

    [Header("Look Sensitivity")]
    [SerializeField] private float sensitivity = 0.15f;

    [Header("Title Clamp (degrees ±)")]
    [SerializeField] private float titleYawClamp = 60f;
    [SerializeField] private float titlePitchClamp = 30f;

    [Header("FPS Pitch Clamp (degrees)")]
    [SerializeField] private float fpsPitchMin = -80f;
    [SerializeField] private float fpsPitchMax = 80f;

    [Header("Interaction")]
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask interactLayer = ~0;

    /// <summary>Skip 버튼 pressed 시 발행.</summary>
    public event System.Action OnSkipPressed;

    private Rigidbody _rb;
    private ControlMode _currentMode;
    private bool _movementEnabled = true;

    private PlayerInput _actions;
    private PlayerInput.PlayerActions _player;

    private float _yaw;
    private float _pitch;
    private float _titleBaseYaw;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        _yaw = transform.eulerAngles.y;
        _pitch = cameraHolder ? NormalizeAngle(cameraHolder.localEulerAngles.x) : 0f;
        _titleBaseYaw = _yaw;

        _actions = new PlayerInput();
        _player = _actions.Player;

        _player.Interact.performed += _ => HandleInteract();
        _player.Skip.performed += _ => OnSkipPressed?.Invoke();

        SetMode(initialMode);
    }

    private void OnEnable() => _actions.Enable();
    private void OnDisable() => _actions.Disable();

    private void Update()
    {
        Vector2 lookDelta = _player.Look.ReadValue<Vector2>() * sensitivity;
        switch (_currentMode)
        {
            case ControlMode.Title: UpdateTitleLook(lookDelta); break;
            case ControlMode.FPS: UpdateFPSLook(lookDelta); break;
        }
    }

    private void FixedUpdate()
    {
        if (!_movementEnabled || _currentMode != ControlMode.FPS) return;
        UpdateFPSMove();
    }

    public override void EnableMovement()
    {
        _movementEnabled = true;
        SetCursorLocked(_currentMode == ControlMode.FPS);
    }

    public override void DisableMovement()
    {
        _movementEnabled = false;
        if (_rb) _rb.linearVelocity = Vector3.zero;
        SetCursorLocked(false);
    }

    public void SetMode(ControlMode mode)
    {
        _currentMode = mode;
        if (mode == ControlMode.Title)
        {
            _titleBaseYaw = _yaw;
            if (_rb) _rb.linearVelocity = Vector3.zero;
        }
        SetCursorLocked(mode == ControlMode.FPS);
    }

    private void UpdateTitleLook(Vector2 delta)
    {
        _yaw += delta.x;
        _pitch -= delta.y;

        float rel = Mathf.DeltaAngle(_titleBaseYaw, _yaw);
        rel = Mathf.Clamp(rel, -titleYawClamp, titleYawClamp);
        _yaw = _titleBaseYaw + rel;
        _pitch = Mathf.Clamp(_pitch, -titlePitchClamp, titlePitchClamp);

        ApplyRotation();
    }

    private void UpdateFPSLook(Vector2 delta)
    {
        _yaw += delta.x;
        _pitch -= delta.y;
        _pitch = Mathf.Clamp(_pitch, fpsPitchMin, fpsPitchMax);
        ApplyRotation();
    }

    private void UpdateFPSMove()
    {
        Vector2 input = _player.Move.ReadValue<Vector2>();
        Vector3 dir = (transform.right * input.x + transform.forward * input.y).normalized;
        Vector3 vel = dir * moveSpeed;
        vel.y = _rb.linearVelocity.y + gravity * Time.fixedDeltaTime;
        _rb.linearVelocity = vel;
    }

    private void HandleInteract()
    {
        if (!_movementEnabled || _currentMode != ControlMode.FPS) return;
        Transform origin = cinemachineCamera ? cinemachineCamera.transform
                         : cameraHolder ? cameraHolder
                         : transform;
        if (Physics.Raycast(origin.position, origin.forward,
                            out RaycastHit hit, interactRange, interactLayer))
            hit.collider.GetComponentInParent<InteractableObject>()?.OnInteract();
    }

    private void ApplyRotation()
    {
        transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
        if (cameraHolder)
            cameraHolder.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }

    private static float NormalizeAngle(float angle)
        => angle > 180f ? angle - 360f : angle;

    private static void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

#if UNITY_EDITOR
    // 에디터 전용 — 추후 테스트용
#endif
}