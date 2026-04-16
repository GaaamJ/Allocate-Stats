/// <summary>
/// 나레이션 출력 채널.
/// 각 채널은 고유한 연출 방식과 위치를 가진다.
///   Screen — 화면을 크게 채우는 천체의 대사
///   World  — 플레이어 행동 결과 (당신은 ~했습니다.)
///   Paper  — 플레이어 종이 오브젝트 위의 지시 사항
/// </summary>
public enum NarratorChannel
{
    Screen,
    World,
    Paper,
}
