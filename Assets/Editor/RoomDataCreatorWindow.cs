using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// RoomData SO 템플릿 생성기.
/// 방 ID 입력 시 기본 Phase 구조가 채워진 SO를 자동 생성.
///
/// [열기]
///   Unity 메뉴 → AllocateStats → Room Data Creator
/// </summary>
public class RoomDataCreatorWindow : EditorWindow
{
    private string roomID = "";
    private string displayName = "";
    private string savePath = "Assets/SO/Room";
    private RoomTemplate selectedTemplate = RoomTemplate.Basic;

    private Vector2 scrollPos;

    public enum RoomTemplate
    {
        Basic,      // enter → 대기 → 오브젝트 상호작용
        Linear,     // enter → check → check → 결과
        Monster,    // enter → kill → run → 결과
        Mirror,     // enter → discover → destroy → luck
        Custom,     // 빈 SO만 생성
    }

    [MenuItem("AllocateStats/Room Data Creator")]
    public static void Open()
    {
        var window = GetWindow<RoomDataCreatorWindow>("Room Data Creator");
        window.minSize = new Vector2(400, 300);
    }

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Room Data 생성기", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        // 기본 정보
        EditorGUILayout.LabelField("기본 정보", EditorStyles.boldLabel);
        roomID = EditorGUILayout.TextField("Room ID", roomID);
        displayName = EditorGUILayout.TextField("Display Name", displayName);
        savePath = EditorGUILayout.TextField("저장 경로", savePath);

        EditorGUILayout.Space(10);

        // 템플릿 선택
        EditorGUILayout.LabelField("템플릿", EditorStyles.boldLabel);
        selectedTemplate = (RoomTemplate)EditorGUILayout.EnumPopup("템플릿 선택", selectedTemplate);

        // 템플릿 설명
        EditorGUILayout.HelpBox(GetTemplateDescription(selectedTemplate), MessageType.Info);

        EditorGUILayout.Space(10);

        // 생성 버튼
        GUI.enabled = !string.IsNullOrEmpty(roomID);
        if (GUILayout.Button("SO 생성", GUILayout.Height(30)))
            CreateRoomData();
        GUI.enabled = true;

        if (string.IsNullOrEmpty(roomID))
            EditorGUILayout.HelpBox("Room ID를 입력하세요.", MessageType.Warning);

        EditorGUILayout.EndScrollView();
    }

    // ── 템플릿 설명 ───────────────────────────────────────

    private string GetTemplateDescription(RoomTemplate template) => template switch
    {
        RoomTemplate.Basic   => "enter(RoomStart/Auto) → 대기 → Interact Phase 1개",
        RoomTemplate.Linear  => "enter → check1 → check2 → 결과 (선형 흐름)",
        RoomTemplate.Monster => "enter → kill(Check/STR) → run(Check/DEX) → 결과",
        RoomTemplate.Mirror  => "enter → discover(Check/PER) → destroy(Check/STR) → luck(Check/LUK)",
        RoomTemplate.Custom  => "빈 SO만 생성 — Phase 직접 입력",
        _ => ""
    };

    // ── SO 생성 ───────────────────────────────────────────

    private void CreateRoomData()
    {
        // 경로 확인 및 생성
        if (!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);

        string assetPath = $"{savePath}/{roomID}.asset";

        // 중복 확인
        if (File.Exists(assetPath))
        {
            if (!EditorUtility.DisplayDialog(
                "덮어쓰기 확인",
                $"'{assetPath}' 가 이미 존재합니다. 덮어쓸까요?",
                "덮어쓰기", "취소"))
                return;
        }

        var roomData = CreateInstance<RoomData>();
        roomData.roomID = roomID;
        roomData.displayName = string.IsNullOrEmpty(displayName) ? roomID : displayName;
        roomData.phases = BuildPhases(selectedTemplate);

        AssetDatabase.CreateAsset(roomData, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // 생성된 에셋 선택
        Selection.activeObject = roomData;
        EditorGUIUtility.PingObject(roomData);

        Debug.Log($"[RoomDataCreator] '{assetPath}' 생성 완료.");
    }

    // ── 템플릿별 Phase 빌드 ───────────────────────────────

    private RoomData.PhaseData[] BuildPhases(RoomTemplate template) => template switch
    {
        RoomTemplate.Basic   => BuildBasic(),
        RoomTemplate.Linear  => BuildLinear(),
        RoomTemplate.Monster => BuildMonster(),
        RoomTemplate.Mirror  => BuildMirror(),
        RoomTemplate.Custom  => new RoomData.PhaseData[0],
        _ => new RoomData.PhaseData[0]
    };

    private RoomData.PhaseData[] BuildBasic() => new[]
    {
        MakePhase("enter", RoomData.TriggerCondition.RoomStart, RoomData.ExitCondition.Auto,
            outcome: MakeOutcome(RoomData.OutcomeType.ReturnToWait)),

        MakePhase("interact", RoomData.TriggerCondition.Interact, RoomData.ExitCondition.Check,
            triggerObjectID: "object_01"),
    };

    private RoomData.PhaseData[] BuildLinear() => new[]
    {
        MakePhase("enter", RoomData.TriggerCondition.RoomStart, RoomData.ExitCondition.Auto,
            outcome: MakeOutcome(RoomData.OutcomeType.ReturnToWait)),

        MakePhase("check_01", RoomData.TriggerCondition.Interact, RoomData.ExitCondition.Check,
            triggerObjectID: "object_01"),

        MakePhase("check_02", RoomData.TriggerCondition.Interact, RoomData.ExitCondition.Check,
            triggerObjectID: "object_02"),
    };

    private RoomData.PhaseData[] BuildMonster() => new[]
    {
        MakePhase("enter", RoomData.TriggerCondition.RoomStart, RoomData.ExitCondition.Auto,
            outcome: MakeOutcome(RoomData.OutcomeType.ReturnToWait)),

        MakePhase("kill", RoomData.TriggerCondition.Interact, RoomData.ExitCondition.Check,
            triggerObjectID: "monster",
            checkData: MakeCheckData(StatType.STR, CheckSystem.CheckType.Threshold, 3,
                success: MakeOutcome(RoomData.OutcomeType.ReturnToWait),
                failure: MakeOutcome(RoomData.OutcomeType.PhaseTo, "run"))),

        MakePhase("run", RoomData.TriggerCondition.PhaseComplete, RoomData.ExitCondition.Check,
            checkData: MakeCheckData(StatType.DEX, CheckSystem.CheckType.Threshold, 1,
                success: MakeOutcome(RoomData.OutcomeType.ReturnToWait),
                failure: MakeOutcome(RoomData.OutcomeType.Death))),

        MakePhase("kill_after_run", RoomData.TriggerCondition.Interact, RoomData.ExitCondition.Auto,
            triggerObjectID: "monster",
            requiredPhaseIDs: new[] { "run" },
            outcome: MakeOutcome(RoomData.OutcomeType.Death)),
    };

    private RoomData.PhaseData[] BuildMirror() => new[]
    {
        MakePhase("enter", RoomData.TriggerCondition.RoomStart, RoomData.ExitCondition.Auto,
            outcome: MakeOutcome(RoomData.OutcomeType.ReturnToWait)),

        MakePhase("discover", RoomData.TriggerCondition.Interact, RoomData.ExitCondition.Check,
            triggerObjectID: "mirror",
            checkData: MakeCheckData(StatType.PER, CheckSystem.CheckType.Threshold, 3,
                success: MakeOutcome(RoomData.OutcomeType.ReturnToWait),
                failure: MakeOutcome(RoomData.OutcomeType.Death))),

        MakePhase("destroy", RoomData.TriggerCondition.Interact, RoomData.ExitCondition.Check,
            triggerObjectID: "mirror",
            requiredPhaseIDs: new[] { "discover" },
            checkData: MakeCheckData(StatType.STR, CheckSystem.CheckType.Threshold, 2,
                success: MakeOutcome(RoomData.OutcomeType.ReturnToWait),
                failure: MakeOutcome(RoomData.OutcomeType.Death))),

        MakePhase("luck", RoomData.TriggerCondition.Interact, RoomData.ExitCondition.Check,
            triggerObjectID: "mirror",
            requiredPhaseIDs: new[] { "destroy" },
            checkData: MakeCheckData(StatType.LUK, CheckSystem.CheckType.LuckFixed, 3,
                success: MakeOutcome(RoomData.OutcomeType.Escape),
                failure: MakeOutcome(RoomData.OutcomeType.NextRoom))),
    };

    // ── 팩토리 헬퍼 ──────────────────────────────────────

    private RoomData.PhaseData MakePhase(
        string phaseID,
        RoomData.TriggerCondition trigger,
        RoomData.ExitCondition exit,
        string triggerObjectID = "",
        string[] requiredPhaseIDs = null,
        RoomData.OutcomeData outcome = null,
        RoomData.CheckData checkData = null)
    {
        return new RoomData.PhaseData
        {
            phaseID = phaseID,
            triggerCondition = trigger,
            triggerObjectID = triggerObjectID,
            requiredPhaseIDs = requiredPhaseIDs ?? new string[0],
            exitCondition = exit,
            outcome = outcome,
            checkData = checkData,
        };
    }

    private RoomData.OutcomeData MakeOutcome(
        RoomData.OutcomeType type,
        string targetPhaseID = "",
        string endingID = "")
    {
        return new RoomData.OutcomeData
        {
            type = type,
            targetPhaseID = targetPhaseID,
            endingID = endingID,
        };
    }

    private RoomData.CheckData MakeCheckData(
        StatType stat,
        CheckSystem.CheckType checkType,
        int threshold,
        RoomData.OutcomeData success = null,
        RoomData.OutcomeData failure = null)
    {
        return new RoomData.CheckData
        {
            stat = stat,
            checkType = checkType,
            threshold = threshold,
            onSuccess = success ?? MakeOutcome(RoomData.OutcomeType.ReturnToWait),
            onFailure = failure ?? MakeOutcome(RoomData.OutcomeType.Death),
        };
    }
}
