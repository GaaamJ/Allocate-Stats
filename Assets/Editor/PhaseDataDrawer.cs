using UnityEngine;
using UnityEditor;

/// <summary>
/// RoomData.PhaseData Inspector 커스텀 드로어.
///
/// 1. 배열 요소 이름을 phaseID로 표시
/// 2. exitCondition에 따라 필드 조건부 표시
///    - Auto  : checkData 숨김, outcome 표시
///    - Check : outcome 숨김, checkData 표시
///
/// [위치]
///   Assets/Editor/PhaseDataDrawer.cs
/// </summary>
[CustomPropertyDrawer(typeof(RoomData.PhaseData))]
public class PhaseDataDrawer : PropertyDrawer
{
    private const float HelpBoxHeight = 30f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded)
            return EditorGUIUtility.singleLineHeight;

        float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        // 항상 표시
        height += GetPropertyHeightOf(property, "phaseID");
        height += GetPropertyHeightOf(property, "triggerCondition");
        height += GetPropertyHeightOf(property, "triggerObjectID");
        height += GetPropertyHeightOf(property, "requiredPhaseIDs");
        height += GetPropertyHeightOf(property, "requirementFailNarration");
        height += GetPropertyHeightOf(property, "isRepeatable");
        height += GetPropertyHeightOf(property, "onEnter");
        height += GetPropertyHeightOf(property, "exitCondition");
        height += GetPropertyHeightOf(property, "animator");

        // HelpBox
        height += HelpBoxHeight + EditorGUIUtility.standardVerticalSpacing;

        // 조건부
        var exitCondition = property.FindPropertyRelative("exitCondition");
        bool isCheck = exitCondition != null &&
            exitCondition.enumValueIndex == (int)RoomData.ExitCondition.Check;

        if (isCheck)
            height += GetPropertyHeightOf(property, "checkData");
        else
            height += GetPropertyHeightOf(property, "outcome");

        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var phaseIDProp = property.FindPropertyRelative("phaseID");
        string displayName = !string.IsNullOrEmpty(phaseIDProp?.stringValue)
            ? phaseIDProp.stringValue
            : label.text;

        property.isExpanded = EditorGUI.Foldout(
            new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
            property.isExpanded,
            displayName,
            true
        );

        if (!property.isExpanded) return;

        EditorGUI.indentLevel++;
        float y = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        // 항상 표시
        y = DrawProperty(position, y, property, "phaseID");
        y = DrawProperty(position, y, property, "triggerCondition");
        y = DrawProperty(position, y, property, "triggerObjectID");
        y = DrawProperty(position, y, property, "requiredPhaseIDs");
        y = DrawProperty(position, y, property, "requirementFailNarration");
        y = DrawProperty(position, y, property, "isRepeatable");
        y = DrawProperty(position, y, property, "onEnter");
        y = DrawProperty(position, y, property, "exitCondition");
        y = DrawProperty(position, y, property, "animator");

        var exitCondition = property.FindPropertyRelative("exitCondition");
        bool isCheck = exitCondition != null &&
            exitCondition.enumValueIndex == (int)RoomData.ExitCondition.Check;

        // HelpBox
        EditorGUI.HelpBox(
            new Rect(position.x, y, position.width, HelpBoxHeight),
            isCheck ? "Check — 판정 데이터를 채워주세요." : "Auto — 완료 결과를 채워주세요.",
            MessageType.None
        );
        y += HelpBoxHeight + EditorGUIUtility.standardVerticalSpacing;

        // 조건부 필드
        if (isCheck)
            y = DrawProperty(position, y, property, "checkData");
        else
            y = DrawProperty(position, y, property, "outcome");

        EditorGUI.indentLevel--;
    }

    // ── 헬퍼 ─────────────────────────────────────────────

    private float DrawProperty(Rect position, float y, SerializedProperty parent, string name)
    {
        var prop = parent.FindPropertyRelative(name);
        if (prop == null) return y;

        float height = EditorGUI.GetPropertyHeight(prop, true);
        EditorGUI.PropertyField(
            new Rect(position.x, y, position.width, height),
            prop,
            true
        );
        return y + height + EditorGUIUtility.standardVerticalSpacing;
    }

    private float GetPropertyHeightOf(SerializedProperty parent, string name)
    {
        var prop = parent.FindPropertyRelative(name);
        if (prop == null) return 0f;
        return EditorGUI.GetPropertyHeight(prop, true) + EditorGUIUtility.standardVerticalSpacing;
    }
}
