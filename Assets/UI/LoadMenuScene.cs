using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Wargon.UI
{
    public class LoadMenuScene : MonoBehaviour
    {
        [SerializeField] private Button goToMainMenuButton;
        [SerializeField] private Button quitButton;

        private void Start()
        {
            goToMainMenuButton.onClick.AddListener(LoadMainMenu);
            quitButton.onClick.AddListener(CloseGame);
        }
        
        public void LoadMainMenu()
        {
            SceneManager.LoadScene(0, LoadSceneMode.Single);
        }

        public void CloseGame()
        {
            Application.Quit();
        }
    }
}