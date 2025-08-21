using UnityEngine;
using TMPro;

public class ScoreUI : MonoBehaviour
{
    #nullable disable
    private TMP_Text textComponent;
    #nullable enable

    private void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
        EventManager.Instance.OnScoreChanged += HandleScoreChanged;
    }

    private void OnDestroy()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnScoreChanged -= HandleScoreChanged;
        }
    }

    private void HandleScoreChanged(Player player, int newScore)
    {
        throw new System.NotImplementedException();      
    }
}
