using UnityEngine;
using System.Collections.Generic;

public class HistoryPaper : MonoBehaviour
{
    [SerializeField] HistoryRecordRow rowPrefab;
    [SerializeField] Transform rowContainer;

    public void Populate(List<GameFlowManager.CheckRecord> records)
    {
        foreach (var r in records)
        {
            var row = Instantiate(rowPrefab, rowContainer);
            row.Setup(r);
        }
    }
}
