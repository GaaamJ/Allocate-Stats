using UnityEngine;

[CreateAssetMenu(fileName = "StatData", menuName = "AllocateStats/StatData")]
public class StatData : ScriptableObject
{
    [System.Serializable]
    public class StatEntry
    {
        public StatType type;
        public string displayName;      // "근력"
        public string category;         // "신체 / 두뇌 / 엑스트라"
        [TextArea] public string description;
    }

    public StatEntry[] stats;
}

public enum StatType
{
    STR,
    DEX,
    PER,
    INT,
    LUK,
    HUM
}
