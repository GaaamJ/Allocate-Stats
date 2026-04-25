using UnityEngine;
using TMPro;

public class HistoryRecordRow : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI statLabelTMP;
    [SerializeField] TextMeshProUGUI resultMarkTMP;
    [SerializeField] TextMeshProUGUI summaryTMP;

    public void Setup(GameFlowManager.CheckRecord record)
    {
        statLabelTMP.text = record.stat.ToString();
        resultMarkTMP.text = record.success ? "○" : "✗";
        summaryTMP.text = !string.IsNullOrEmpty(record.summaryText) ? record.summaryText : record.context;
    }
}
