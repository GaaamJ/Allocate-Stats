using UnityEngine;
using TMPro;

public class StatPaperUI : MonoBehaviour
{
    [Header("Ending ID")]
    [SerializeField] private TextMeshProUGUI endingIdTMP;

    [Header("Stat Rows")]
    [SerializeField] private StatRowUI strRow;
    [SerializeField] private StatRowUI dexRow;
    [SerializeField] private StatRowUI perRow;
    [SerializeField] private StatRowUI intRow;
    [SerializeField] private StatRowUI lukRow;
    [SerializeField] private StatRowUI humRow;

    public void Populate(string endingId, int str, int dex, int per, int intStat, int luk, int hum)
    {
        if (endingIdTMP) endingIdTMP.text = endingId;

        strRow?.ShowImmediate(str);
        dexRow?.ShowImmediate(dex);
        perRow?.ShowImmediate(per);
        intRow?.ShowImmediate(intStat);
        lukRow?.ShowImmediate(luk);
        humRow?.ShowImmediate(hum);
    }
}
