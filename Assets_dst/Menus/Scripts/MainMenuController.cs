using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceShooterMenus
{
    /// <summary>Main menu buttons: Play, How to Play, Quit.</summary>
    public class MainMenuController : MonoBehaviour
    {
        public string gameScene = "SampleScene";
        public GameObject mainPanel;
        public GameObject howToPlayPanel;

        void Start()
        {
            Time.timeScale = 1f;
            if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
            if (mainPanel != null) mainPanel.SetActive(true);
        }

        void Update()
        {
            if (howToPlayPanel != null && howToPlayPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
                CloseHowToPlay();
        }

        public void Play()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(gameScene);
        }

        public void OpenHowToPlay()
        {
            if (mainPanel != null) mainPanel.SetActive(false);
            if (howToPlayPanel != null) howToPlayPanel.SetActive(true);
        }

        public void CloseHowToPlay()
        {
            if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
            if (mainPanel != null) mainPanel.SetActive(true);
        }

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
