using TMPro;
using UnityEngine;

public class UIScoreEntry : MonoBehaviour
{
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text completionTimeText;
    [SerializeField] private TMP_Text dateText;

    public void SetData( int rank,string playerName, float completionTime, string date )
    {
        rankText.text = rank.ToString();

        playerNameText.text = playerName;

        completionTimeText.text = $"{completionTime:F2} s";

        dateText.text = string.IsNullOrWhiteSpace(date) ? "—" : date;
    }
}
