using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 앙코르 루프 테스트용.
/// 확인 후 삭제하세요.
///
/// 단축키:
///   F3 — 앙코르 루프 진입 (카운터 0부터)
///   F4 — 특정 카운터로 점프 (testEncoreCounter 값 사용)
/// </summary>
public class EncoreTest : MonoBehaviour
{
    [Header("테스트 설정")]
    [SerializeField] private int testEncoreCounter = 0; // 0=수, 1=금, 2=지(자비), 3=화 ...

    [Header("버튼 (선택)")]
    [SerializeField] private Button btnEnterEncore;
    [SerializeField] private Button btnJumpCounter;

    private void Start()
    {
        if (btnEnterEncore) btnEnterEncore.onClick.AddListener(EnterEncore);
        if (btnJumpCounter) btnJumpCounter.onClick.AddListener(JumpToCounter);
    }

    private void Update()
    {
        if (UnityEngine.InputSystem.Keyboard.current != null)
        {
            if (UnityEngine.InputSystem.Keyboard.current.f3Key.wasPressedThisFrame) EnterEncore();
            if (UnityEngine.InputSystem.Keyboard.current.f4Key.wasPressedThisFrame) JumpToCounter();
        }
    }

    [ContextMenu("Enter Encore")]
    public void EnterEncore()
    {
        Debug.Log("[EncoreTest] 앙코르 루프 진입 (카운터 0)");
        GameFlowManager.Instance?.EnterEncoreLoop();
    }

    [ContextMenu("Jump To Counter")]
    public void JumpToCounter()
    {
        var flow = GameFlowManager.Instance;
        if (flow == null) return;

        Debug.Log($"[EncoreTest] 앙코르 카운터 {testEncoreCounter}으로 점프 ({GetPlanetName(testEncoreCounter)})");
        flow.JumpToEncore(testEncoreCounter);
    }

    private string GetPlanetName(int counter)
    {
        string[] names = { "수", "금", "지(자비)", "화", "목", "토", "천", "해", "인" };
        return names[counter % names.Length];
    }
}