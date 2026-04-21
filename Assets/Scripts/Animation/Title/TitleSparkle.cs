using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// P00 타이틀 화면 픽셀 반짝이 이펙트.
/// Canvas 자식으로 Image를 동적 생성해서 랜덤 위치에 깜빡임.
///
/// [Inspector 연결]
///   parentCanvas   : 스파클을 붙일 Canvas
///   sprites        : 사용할 스프라이트 배열 (2개)
///   count          : 동시에 떠있는 스파클 수
///   minSize        : 최소 크기 (px)
///   maxSize        : 최대 크기 (px)
///   minOnDuration  : 켜져 있는 최소 시간
///   maxOnDuration  : 켜져 있는 최대 시간
///   minOffDuration : 꺼져 있는 최소 시간
///   maxOffDuration : 꺼져 있는 최대 시간
/// </summary>
public class TitleSparkle : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform parentCanvas;

    [Header("Sprites")]
    [SerializeField] private Sprite[] sprites;

    [Header("Settings")]
    [SerializeField] private int   count          = 20;
    [SerializeField] private float minSize        = 4f;
    [SerializeField] private float maxSize        = 12f;
    [SerializeField] private float minOnDuration  = 0.05f;
    [SerializeField] private float maxOnDuration  = 0.2f;
    [SerializeField] private float minOffDuration = 0.3f;
    [SerializeField] private float maxOffDuration = 2.0f;

    private void Start()
    {
        if (sprites == null || sprites.Length == 0) return;

        for (int i = 0; i < count; i++)
            StartCoroutine(SparkleLoop(CreateImage()));
    }

    private Image CreateImage()
    {
        var go = new GameObject("Sparkle", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parentCanvas, false);

        var img = go.GetComponent<Image>();
        img.raycastTarget = false;
        SetAlpha(img, 0f);
        return img;
    }

    private IEnumerator SparkleLoop(Image img)
    {
        // 처음 등장 타이밍 분산
        yield return new WaitForSeconds(Random.Range(0f, maxOffDuration));

        while (true)
        {
            // 랜덤 위치 / 크기 / 스프라이트 설정
            Reposition(img);

            // 켜기
            SetAlpha(img, 1f);
            yield return new WaitForSeconds(Random.Range(minOnDuration, maxOnDuration));

            // 끄기
            SetAlpha(img, 0f);
            yield return new WaitForSeconds(Random.Range(minOffDuration, maxOffDuration));
        }
    }

    private void Reposition(Image img)
    {
        // 스프라이트 랜덤 선택
        img.sprite = sprites[Random.Range(0, sprites.Length)];

        // 크기
        float size = Random.Range(minSize, maxSize);
        img.rectTransform.sizeDelta = new Vector2(size, size);

        // 위치 (Canvas 범위 내 랜덤)
        float hw = parentCanvas.rect.width  * 0.5f;
        float hh = parentCanvas.rect.height * 0.5f;
        img.rectTransform.anchoredPosition = new Vector2(
            Random.Range(-hw, hw),
            Random.Range(-hh, hh));
    }

    private static void SetAlpha(Image img, float a)
    {
        var c = img.color; c.a = a; img.color = c;
    }
}
