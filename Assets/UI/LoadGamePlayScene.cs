using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadGamePlayScene : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;

    private void Start()
    {
        if (startButton is not null) startButton.onClick.AddListener(LoadGameScene);
        if (quitButton is not null) quitButton.onClick.AddListener(CloseGame);
    }

    public void PlayAgen()
    {
        Cursor.lockState = CursorLockMode.Locked;
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }

    public void LoadGameScene()
    {
        SceneManager.LoadScene(1, LoadSceneMode.Single);
    }

    public void CloseGame()
    {
        Application.Quit();
    }
}