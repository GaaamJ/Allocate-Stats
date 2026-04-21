using UnityEngine;

/// <summary>
/// 타이틀씬 공책 전용 상호작용 컴포넌트.
/// TitlePlayerController Raycast 히트 시 OnInteract() 호출.
/// RoomEventBus 등 Room 시스템과 무관.
///
/// [Inspector 연결]
///   공책 오브젝트에 Collider + 이 컴포넌트만 붙이면 됨.
///
/// [TitleP02Controller에서 사용]
///   notebook.OnClicked += () => clicked = true;
/// </summary>
public class NotebookInteractable : MonoBehaviour
{
    /// <summary>Raycast로 클릭됐을 때 발행.</summary>
    public event System.Action OnClicked;

    public void OnInteract()
    {
        OnClicked?.Invoke();
    }
}
