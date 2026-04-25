using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// O | X 판정 기호와 판정선을 OnPopulateMesh로 직접 그리는 UI 컴포넌트.
/// RaycastTarget은 Inspector에서 OFF로 설정할 것.
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
public class CheckMarkGraphic : Graphic
{
    [Header("Stroke")]
    [SerializeField] private float strokeWidth = 5f;
    [SerializeField] private Color inkColor = Color.black;

    [Header("Layout")]
    [SerializeField] private float symbolScale = 0.35f;   // rect 높이 대비 심볼 크기 비율
    [SerializeField] private float symbolSpacing = 24f;   // | 중심~심볼 끝 사이 여백
    [SerializeField] private float judgeLineWidth = 3.5f;
    [SerializeField] private float judgeDotRadius = 9f;

    [Header("Result Colors")]
    [SerializeField] private Color oSuccessColor = Color.blue;
    [SerializeField] private Color xFailureColor = Color.red;

    private float _oProgress;
    private float _divProgress;
    private float _xProgress;
    private readonly List<Vector2> _judgePoints = new();

    private Vector2 _oShake;
    private Vector2 _divShake;
    private Vector2 _xShake;
    private Vector2 _judgeLineShake;

    private Color _oColor;
    private Color _xColor;
    private Color _lineColor;

    public Color InkColor       => inkColor;
    public Color OSuccessColor  => oSuccessColor;
    public Color XFailureColor  => xFailureColor;

    // ── Public API ─────────────────────────────────────────

    public void SetOProgress(float t)       { _oProgress   = Mathf.Clamp01(t); SetVerticesDirty(); }
    public void SetDividerProgress(float t) { _divProgress = Mathf.Clamp01(t); SetVerticesDirty(); }
    public void SetXProgress(float t)       { _xProgress   = Mathf.Clamp01(t); SetVerticesDirty(); }

    public void AddJudgePoint(Vector2 p)
    {
        _judgePoints.Add(p);
        SetVerticesDirty();
    }

    /// <summary>마지막 점의 위치를 갱신 (헤드 부드러운 이동용).</summary>
    public void UpdateLastJudgePoint(Vector2 p)
    {
        if (_judgePoints.Count > 0)
            _judgePoints[_judgePoints.Count - 1] = p;
        SetVerticesDirty();
    }

    public void SetSymbolShakes(Vector2 o, Vector2 div, Vector2 x)
    {
        _oShake = o; _divShake = div; _xShake = x;
        SetVerticesDirty();
    }

    public void SetOColor(Color c)    { _oColor    = c; SetVerticesDirty(); }
    public void SetXColor(Color c)    { _xColor    = c; SetVerticesDirty(); }
    public void SetLineColor(Color c) { _lineColor = c; SetVerticesDirty(); }

    public void SetJudgeLineShake(Vector2 shake)
    {
        _judgeLineShake = shake;
        SetVerticesDirty();
    }

    protected override void Awake()
    {
        base.Awake();
        _oColor = _xColor = _lineColor = inkColor;
    }

    public void ResetAll()
    {
        _oProgress = _divProgress = _xProgress = 0f;
        _oShake = _divShake = _xShake = _judgeLineShake = Vector2.zero;
        _oColor = _xColor = _lineColor = inkColor;
        _judgePoints.Clear();
        SetVerticesDirty();
    }

    public void ResetJudgeOnly()
    {
        _judgeLineShake = Vector2.zero;
        _judgePoints.Clear();
        SetVerticesDirty();
    }

    // ── Centers (local space) ──────────────────────────────

    private float SymbolHalfSize => rectTransform.rect.height * symbolScale * 0.5f;

    /// <summary>X 기호의 중심 (로컬 좌표).</summary>
    public Vector2 XCenter      => rectTransform.rect.center + new Vector2( SymbolHalfSize * 2f + symbolSpacing, 0f);
    /// <summary>| 기호의 중심 (로컬 좌표). 판정선 출발점.</summary>
    public Vector2 DividerCenter => rectTransform.rect.center;
    /// <summary>O 기호의 중심 (로컬 좌표).</summary>
    public Vector2 OCenter      => rectTransform.rect.center + new Vector2(-(SymbolHalfSize * 2f + symbolSpacing), 0f);

    // ── Mesh ──────────────────────────────────────────────

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (_oProgress   > 0f) DrawO(vh);
        if (_divProgress > 0f) DrawDivider(vh);
        if (_xProgress   > 0f) DrawX(vh);
        if (_judgePoints.Count >= 2) DrawPolylineOffset(vh, _judgePoints, _judgeLineShake, judgeLineWidth, LineInk());
        if (_judgePoints.Count >= 1) DrawDot(vh, _judgePoints[_judgePoints.Count - 1] + _judgeLineShake, judgeDotRadius, LineInk());
    }

    private Color Ink()     => inkColor  * color;
    private Color OInk()    => _oColor   * color;
    private Color XInk()    => _xColor   * color;
    private Color LineInk() => _lineColor * color;

    // ── O ─────────────────────────────────────────────────

    private void DrawO(VertexHelper vh)
    {
        Vector2 c  = OCenter + _oShake;
        float   s  = SymbolHalfSize;
        float   rx = s * 0.78f;
        float   ry = s * 0.93f;

        const int totalSegs = 36;
        int drawSegs = Mathf.Clamp(Mathf.CeilToInt(_oProgress * totalSegs), 1, totalSegs);

        var pts = new List<Vector2>(drawSegs + 1);
        for (int i = 0; i <= drawSegs; i++)
        {
            // i < drawSegs: 정규 격자점 / i == drawSegs: 정확한 progress 위치
            float t      = i < drawSegs ? (float)i / totalSegs : _oProgress;
            float angle  = Mathf.PI * 0.5f - t * Mathf.PI * 2f; // 12시 → 시계방향
            float wobble = Mathf.Sin(angle * 2.3f + 1.1f) * 0.055f;
            pts.Add(new Vector2(
                c.x + rx * (1f + wobble) * Mathf.Cos(angle),
                c.y + ry * (1f + wobble * 0.6f) * Mathf.Sin(angle)));
        }

        DrawPolyline(vh, pts, strokeWidth, OInk());
    }

    // ── Divider ───────────────────────────────────────────

    private void DrawDivider(VertexHelper vh)
    {
        Vector2 c   = DividerCenter + _divShake;
        float   s   = SymbolHalfSize;
        Vector2 top = new Vector2(c.x, c.y + s);
        Vector2 bot = new Vector2(c.x, c.y - s);
        DrawLine(vh, top, Vector2.Lerp(top, bot, _divProgress), strokeWidth, LineInk());
    }

    // ── X ─────────────────────────────────────────────────

    private void DrawX(VertexHelper vh)
    {
        Vector2 c   = XCenter + _xShake;
        float   s   = SymbolHalfSize * 0.88f;
        Color   col = XInk();

        Vector2 tl = c + new Vector2(-s,  s);
        Vector2 br = c + new Vector2( s, -s);
        Vector2 tr = c + new Vector2( s,  s);
        Vector2 bl = c + new Vector2(-s, -s);

        // 첫 획: 왼쪽 위 → 오른쪽 아래 (progress 0 ~ 0.55)
        if (_xProgress > 0f)
        {
            float t1 = Mathf.Clamp01(_xProgress / 0.55f);
            DrawLine(vh, tl, Vector2.Lerp(tl, br, t1), strokeWidth, col);
        }
        // 두 번째 획: 오른쪽 위 → 왼쪽 아래 (progress 0.55 ~ 1)
        if (_xProgress > 0.55f)
        {
            float t2 = Mathf.Clamp01((_xProgress - 0.55f) / 0.45f);
            DrawLine(vh, tr, Vector2.Lerp(tr, bl, t2), strokeWidth, col);
        }
    }

    // ── Primitives ────────────────────────────────────────

    private void DrawLine(VertexHelper vh, Vector2 a, Vector2 b, float w, Color col)
    {
        if ((b - a).sqrMagnitude < 0.0001f) return;
        Vector2 perp = new Vector2(-(b - a).y, (b - a).x).normalized * (w * 0.5f);
        int idx = vh.currentVertCount;
        AddVert(vh, a + perp, col);
        AddVert(vh, a - perp, col);
        AddVert(vh, b - perp, col);
        AddVert(vh, b + perp, col);
        vh.AddTriangle(idx,     idx + 1, idx + 2);
        vh.AddTriangle(idx,     idx + 2, idx + 3);
    }

    private void DrawPolyline(VertexHelper vh, IList<Vector2> pts, float w, Color col)
    {
        for (int i = 0; i < pts.Count - 1; i++)
            DrawLine(vh, pts[i], pts[i + 1], w, col);
    }

    private void DrawPolylineOffset(VertexHelper vh, IList<Vector2> pts, Vector2 offset, float w, Color col)
    {
        for (int i = 0; i < pts.Count - 1; i++)
            DrawLine(vh, pts[i] + offset, pts[i + 1] + offset, w, col);
    }

    private void DrawDot(VertexHelper vh, Vector2 pos, float r, Color col)
    {
        const int segs = 8;
        int ci = vh.currentVertCount;
        AddVert(vh, pos, col);
        for (int i = 0; i < segs; i++)
        {
            float a = i * Mathf.PI * 2f / segs;
            AddVert(vh, pos + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r, col);
        }
        for (int i = 0; i < segs; i++)
            vh.AddTriangle(ci, ci + 1 + i, ci + 1 + (i + 1) % segs);
    }

    private void AddVert(VertexHelper vh, Vector2 pos, Color col)
    {
        var v = UIVertex.simpleVert;
        v.position = pos;
        v.color    = col;
        vh.AddVert(v);
    }
}
