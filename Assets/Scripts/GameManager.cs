using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    const string level1SceneName = "Level1";
    const string titleScreen = "TitleScreen";


    void Awake()
    {
        EventManager.Instance.OnGameOver += ShowEndScreen;
    }

    private void OnDestroy()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnGameOver -= ShowEndScreen;
        }
    }

    private void ShowEndScreen()
    {
        // Find the UI container for the end-game screen and enable it.
        GameObject endScreen = GameObject.Find("EndGameScreen");
        endScreen.SetActive(true);
    }

    public void HandleStartClick()  {
        EventManager.ResetInstance();
        ServiceLocator.ResetInstance();
        SceneManager.LoadScene(level1SceneName);
    }
    

    public void HandleRestartClick() => HandleStartClick();
    

    public void HandleReturnToStartClick() => SceneManager.LoadScene(titleScreen);
    
}
