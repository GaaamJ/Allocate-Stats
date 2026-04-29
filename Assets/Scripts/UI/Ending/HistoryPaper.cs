using UnityEngine;
using System.Collections.Generic;

public class HistoryPaper : MonoBehaviour
{
    [SerializeField] private HistoryRecordRow rowPrefab;
    [SerializeField] private Transform rowContainer;

    public void Populate(List<GameFlowManager.CheckRecord> records)
    {
        if (rowContainer == null || rowPrefab == null) return;

        for (int i = rowContainer.childCount - 1; i >= 0; i--)
            Destroy(rowContainer.GetChild(i).gameObject);

        if (records == null || records.Count == 0) return;

        foreach (var r in records)
        {
            var row = Instantiate(rowPrefab, rowContainer);
            row.Setup(r);
        }
    }
}
