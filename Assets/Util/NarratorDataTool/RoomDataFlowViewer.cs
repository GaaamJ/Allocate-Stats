using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// RoomData Phase 흐름 시각화 / 검증기.
/// SO 선택 시 Phase 연결 관계를 그래프로 표시하고 오류를 감지.
///
/// [열기]
///   Unity 메뉴 → AllocateStats → Room Data Flow Viewer
/// </summary>
public class RoomDataFlowViewer : EditorWindow
{
    private RoomData targetRoomData;
    private Vector2 scrollPos;
    private List<string> errors = new List<string>();
    private List<string> warnings = new List<string>();

    // 노드 그리기용
    private const float NodeWidth = 200f;
    private const float NodeHeight = 70f;
    private const float NodeSpacingX = 240f;
    private const float NodeSpacingY = 100f;

    [MenuItem("AllocateStats/Room Data Flow Viewer")]
    public static void Open()
    {
        var window = GetWindow<RoomDataFlowViewer>("Room Flow Viewer");
        window.minSize = new Vector2(600, 400);
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Room Data Flow Viewer", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        // SO 선택
        var newTarget = (RoomData)EditorGUILayout.ObjectField(
            "Room Data", targetRoomData, typeof(RoomData), false);

        if (newTarget != targetRoomData)
        {
            targetRoomData = newTarget;
            Validate();
        }

        if (targetRoomData == null)
        {
            EditorGUILayout.HelpBox("RoomData SO를 선택하세요.", MessageType.Info);
            return;
        }

        EditorGUILayout.Space(5);

        // 오류 / 경고 표시
        DrawValidationResults();

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Phase 흐름", EditorStyles.boldLabel);

        // 스크롤 영역에 Phase 노드 그리기
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos,
            GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

        DrawPhaseNodes();

        EditorGUILayout.EndScrollView();
    }

    // ── 검증 ─────────────────────────────────────────────

    private void Validate()
    {
        errors.Clear();
        warnings.Clear();

        if (targetRoomData?.phases == null) return;

        var phases = targetRoomData.phases;
        var phaseIDs = new HashSet<string>();

        // phaseID 수집
        foreach (var p in phases)
            if (!string.IsNullOrEmpty(p.phaseID))
                phaseIDs.Add(p.phaseID);

        // RoomStart Phase 확인
        bool hasRoomStart = false;
        foreach (var p in phases)
            if (p.triggerCondition == RoomData.TriggerCondition.RoomStart)
            { hasRoomStart = true; break; }

        if (!hasRoomStart)
            errors.Add("TriggerCondition.RoomStart Phase가 없습니다.");

        foreach (var p in phases)
        {
            string label = string.IsNullOrEmpty(p.phaseID) ? "(phaseID 없음)" : p.phaseID;

            // phaseID 중복 확인
            if (string.IsNullOrEmpty(p.phaseID))
                warnings.Add($"'{label}' — phaseID가 비어있습니다.");

            // Check Phase 검증
            if (p.exitCondition == RoomData.ExitCondition.Check)
            {
                if (p.checkData == null)
                {
                    errors.Add($"'{label}' — exitCondition이 Check인데 checkData가 null입니다.");
                    continue;
                }

                ValidateOutcome(p.checkData.onSuccess, $"'{label}'.onSuccess", phaseIDs);
                ValidateOutcome(p.checkData.onFailure, $"'{label}'.onFailure", phaseIDs);
            }

            // Auto Phase 검증
            if (p.exitCondition == RoomData.ExitCondition.Auto)
            {
                if (p.outcome == null)
                    errors.Add($"'{label}' — exitCondition이 Auto인데 outcome이 null입니다.");
                else
                    ValidateOutcome(p.outcome, $"'{label}'.outcome", phaseIDs);
            }

            // requiredPhaseIDs 검증
            if (p.requiredPhaseIDs != null)
                foreach (var req in p.requiredPhaseIDs)
                    if (!string.IsNullOrEmpty(req) && !phaseIDs.Contains(req))
                        errors.Add($"'{label}' — requiredPhaseID '{req}'가 존재하지 않습니다.");
        }
    }

    private void ValidateOutcome(RoomData.OutcomeData outcome, string context, HashSet<string> phaseIDs)
    {
        if (outcome == null) return;

        if (outcome.type == RoomData.OutcomeType.PhaseTo)
        {
            if (string.IsNullOrEmpty(outcome.targetPhaseID))
                errors.Add($"{context} — PhaseTo인데 targetPhaseID가 비어있습니다.");
            else if (!phaseIDs.Contains(outcome.targetPhaseID))
                errors.Add($"{context} — targetPhaseID '{outcome.targetPhaseID}'가 존재하지 않습니다.");
        }

        if ((outcome.type == RoomData.OutcomeType.Escape ||
             outcome.type == RoomData.OutcomeType.Death) &&
            string.IsNullOrEmpty(outcome.endingID))
            warnings.Add($"{context} — Escape/Death인데 endingID가 비어있습니다.");
    }

    // ── 검증 결과 표시 ────────────────────────────────────

    private void DrawValidationResults()
    {
        if (errors.Count == 0 && warnings.Count == 0)
        {
            EditorGUILayout.HelpBox("오류 없음.", MessageType.Info);
            return;
        }

        foreach (var e in errors)
            EditorGUILayout.HelpBox(e, MessageType.Error);

        foreach (var w in warnings)
            EditorGUILayout.HelpBox(w, MessageType.Warning);
    }

    // ── Phase 노드 그리기 ─────────────────────────────────

    private void DrawPhaseNodes()
    {
        if (targetRoomData?.phases == null) return;

        var phases = targetRoomData.phases;

        // 최소 캔버스 크기 확보
        float canvasWidth = phases.Length * NodeSpacingX + NodeWidth;
        float canvasHeight = 300f;
        GUILayoutUtility.GetRect(canvasWidth, canvasHeight);

        for (int i = 0; i < phases.Length; i++)
        {
            var phase = phases[i];
            float x = i * NodeSpacingX + 10f;
            float y = 10f;

            // 노드 색상 — exitCondition 기준
            Color nodeColor = phase.exitCondition == RoomData.ExitCondition.Check
                ? new Color(0.3f, 0.5f, 0.8f)   // Check — 파란색
                : new Color(0.3f, 0.7f, 0.4f);   // Auto — 초록색

            // RoomStart는 주황색
            if (phase.triggerCondition == RoomData.TriggerCondition.RoomStart)
                nodeColor = new Color(0.9f, 0.6f, 0.2f);

            DrawNode(x, y, phase, nodeColor);

            // 화살표 — Auto outcome / Check onSuccess / onFailure
            if (phase.exitCondition == RoomData.ExitCondition.Auto && phase.outcome != null)
                DrawArrow(x + NodeWidth, y + NodeHeight / 2f, phase.outcome, phases, Color.green);

            if (phase.exitCondition == RoomData.ExitCondition.Check && phase.checkData != null)
            {
                DrawArrow(x + NodeWidth, y + NodeHeight / 2f - 10f,
                    phase.checkData.onSuccess, phases, Color.cyan);
                DrawArrow(x + NodeWidth, y + NodeHeight / 2f + 10f,
                    phase.checkData.onFailure, phases, Color.red);
            }
        }
    }

    private void DrawNode(float x, float y, RoomData.PhaseData phase, Color color)
    {
        var rect = new Rect(x, y, NodeWidth, NodeHeight);

        // 배경
        EditorGUI.DrawRect(rect, color * 0.5f);
        GUI.Box(rect, GUIContent.none);

        // 텍스트
        var style = new GUIStyle(EditorStyles.label)
        {
            wordWrap = true,
            alignment = TextAnchor.MiddleCenter,
            fontSize = 11,
        };

        string phaseLabel = string.IsNullOrEmpty(phase.phaseID) ? "(no ID)" : phase.phaseID;
        string triggerLabel = phase.triggerCondition.ToString();
        string exitLabel = phase.exitCondition.ToString();

        GUI.Label(rect,
            $"{phaseLabel}\n{triggerLabel} / {exitLabel}",
            style);
    }

    private void DrawArrow(float fromX, float fromY,
        RoomData.OutcomeData outcome, RoomData.PhaseData[] phases, Color color)
    {
        if (outcome == null) return;

        string label = outcome.type switch
        {
            RoomData.OutcomeType.PhaseTo      => $"→ {outcome.targetPhaseID}",
            RoomData.OutcomeType.ReturnToWait => "→ Wait",
            RoomData.OutcomeType.NextRoom     => "→ NextRoom",
            RoomData.OutcomeType.Escape       => "→ Escape",
            RoomData.OutcomeType.Death        => "→ Death",
            _ => "→ ?"
        };

        var style = new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = color } };
        GUI.Label(new Rect(fromX + 5f, fromY - 8f, 150f, 16f), label, style);
    }
}
