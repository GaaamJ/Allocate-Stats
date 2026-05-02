using UnityEngine;
using TMPro;

public class HistoryRecordRow : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI statLabelTMP;
    [SerializeField] private TextMeshProUGUI resultMarkTMP;
    [SerializeField] private TextMeshProUGUI summaryTMP;

    public void Setup(GameFlowManager.CheckRecord record)
    {
        if (record == null) return;

        if (statLabelTMP != null)
            statLabelTMP.text = record.stat.ToString();
        if (resultMarkTMP != null)
            resultMarkTMP.text = record.success ? "O" : "X";
        if (summaryTMP != null)
            summaryTMP.text = !string.IsNullOrEmpty(record.summaryText) ? record.summaryText : record.context;
    }
}
