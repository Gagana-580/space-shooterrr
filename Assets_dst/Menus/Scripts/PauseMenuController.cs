using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace SpaceShooterMenus
{
    /// <summary>
    /// Esc (or P) pauses / resumes the game. Pausing sets Time.timeScale to 0 and stops the player script,
    /// so the ship can't move or shoot. Does nothing once an end screen is showing.
    /// </summary>
    public class PauseMenuController : MonoBehaviour
    {
        public GameObject pausePanel;
        public string mainMenuScene = "MainMenu";

        bool paused;

        public bool IsPaused => paused;

        void Start()
        {
            if (pausePanel != null) pausePanel.SetActive(false);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
            {
                if (paused) Resume();
                else if (CanPause()) Pause();
            }
        }

        bool CanPause()
        {
            if (GameFlow.Instance != null && GameFlow.Instance.HasEnded) return false;
            return Time.timeScale > 0f;
        }

        public void Pause()
        {
            paused = true;
            Time.timeScale = 0f;
            SetPlayerEnabled(false);
            if (pausePanel != null) pausePanel.SetActive(true);
        }

        public void Resume()
        {
            Close();
            Time.timeScale = 1f;
        }

        /// <summary>Hides the pause menu without changing Time.timeScale (used when the game ends).</summary>
        public void Close()
        {
            paused = false;
            if (pausePanel != null) pausePanel.SetActive(false);
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
            SetPlayerEnabled(true);
        }

        public void Restart()
        {
            paused = false;
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void MainMenu()
        {
            paused = false;
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuScene);
        }

        static void SetPlayerEnabled(bool on)
        {
            PlayerController player = FindFirstObjectByType<PlayerController>(FindObjectsInactive.Include);
            if (player != null) player.enabled = on;
        }
    }
}
