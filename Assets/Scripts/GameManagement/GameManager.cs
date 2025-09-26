using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using TMPro;
using Unity.Behavior;

public class GameManager : MonoBehaviour
{
    #nullable disable
    [SerializeField] private GameObject GameOverPanel;
    #nullable enable
    const string level1SceneName = "Level1";
    const string titleScreen = "TitleScreen";


    void Awake()
    {
        Debug.LogWarning("Awake gamemanager");
        ServiceLocator.RegisterScoringManager(new ScoringManager());
        //new UtilityManager();
        var mainPlayer = new Player("Main Player");
        EventManager.Instance.TriggerScoreChanged(mainPlayer, 0);
        EventManager.Instance.OnGameOver += HandleGameOver;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            HandleTKeyPressed();
        }
    }

    private void OnDestroy()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnGameOver -= HandleGameOver;
        }
    }

    public void HandleStartClick()  {
        EventManager.ResetInstance();
        ServiceLocator.ResetInstance();
        SceneManager.LoadScene(level1SceneName);
    }
    
    private void HandleGameOver(Player winningPlayer)
    {
        var text = "Game over";
        var textComponent = GameOverPanel.transform.Find("Text1").GetComponent<TMP_Text>();
        textComponent.text = text;
        GameOverPanel.gameObject.SetActive(true);
    }

    private void HandleTKeyPressed()
    {
        var priest = GameObject.Find("Priest");
        var agent = priest.GetComponent<BehaviorGraphAgent>();
        agent.SetVariableValue("PriestStatus", Status.Poison);
    }

    public void HandleRestartClick() => HandleStartClick();
    

    public void HandleReturnToStartClick() => SceneManager.LoadScene(titleScreen);
    
}
