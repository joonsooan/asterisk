using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private string titleSceneName = "TitleScene";
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadGameScene()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void LoadTitleScene()
    {
        SceneManager.LoadScene(titleSceneName);
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
}