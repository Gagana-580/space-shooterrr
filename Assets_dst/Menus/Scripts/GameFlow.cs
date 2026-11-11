using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace SpaceShooterMenus
{
    /// <summary>
    /// Watches the game and opens the end screens:
    ///  - Game Over when the player (PlayerController) is gone,
    ///  - Victory when every enemy (EnemyController) is destroyed (can be switched off).
    /// After a short delay it freezes the game (Time.timeScale = 0) and hides the HUD while an end screen is showing.
    /// Lives on the "Menus Canvas" in the game scene. Other scripts can also call GameFlow.Instance.EndGame(true/false).
    /// </summary>
    public class GameFlow : MonoBehaviour
    {
        public static GameFlow Instance { get; private set; }

        [Header("Panels")]
        public GameObject winPanel;
        public GameObject losePanel;

        [Tooltip("The game's HUD (score + hearts). Hidden while an end screen is showing. Found automatically from UIManager when left empty.")]
        public GameObject hud;

        [Header("Rules")]
        [Tooltip("Show the Victory screen when no enemies are left in the scene.")]
        public bool winWhenAllEnemiesDestroyed = true;

        [Tooltip("Seconds to wait before the end screen appears, so the last hit can be seen.")]
        public float endScreenDelay = 1f;

        [Header("Scenes")]
        public string mainMenuScene = "MainMenu";

        /// <summary>True as soon as the game is won or lost (also during the short delay).</summary>
        public bool HasEnded { get; private set; }

        /// <summary>Seconds played when the game ended (or so far, while still playing).</summary>
        public float PlayTime => HasEnded ? endTime : Time.timeSinceLevelLoad;

        bool sawPlayer;
        bool sawEnemy;
        float nextCheck;
        float endTime;

        void Awake()
        {
            Instance = this;
            Time.timeScale = 1f;
            if (winPanel != null) winPanel.SetActive(false);
            if (losePanel != null) losePanel.SetActive(false);
        }

        void Start()
        {
            // The HUD canvas is the one holding UIManager's score text.
            if (hud == null)
            {
                UIManager ui = FindFirstObjectByType<UIManager>(FindObjectsInactive.Include);
                if (ui != null && ui.scoreText != null && ui.scoreText.canvas != null)
                {
                    GameObject hudCanvas = ui.scoreText.canvas.rootCanvas.gameObject;
                    if (hudCanvas != gameObject) hud = hudCanvas;
                }
            }
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            if (HasEnded || Time.unscaledTime < nextCheck) return;
            nextCheck = Time.unscaledTime + 0.2f;

            bool playerAlive = FindFirstObjectByType<PlayerController>() != null;
            bool enemiesLeft = FindFirstObjectByType<EnemyController>() != null;
            sawPlayer |= playerAlive;
            sawEnemy |= enemiesLeft;

            if (sawPlayer && !playerAlive) EndGame(false);
            else if (winWhenAllEnemiesDestroyed && sawEnemy && !enemiesLeft) EndGame(true);
        }

        /// <summary>Opens the Victory (true) or Game Over (false) screen.</summary>
        public void EndGame(bool victory)
        {
            if (HasEnded) return;
            HasEnded = true;
            endTime = Time.timeSinceLevelLoad;

            var pause = GetComponent<PauseMenuController>();
            if (pause != null && pause.IsPaused) pause.Close();

            StartCoroutine(ShowEndScreen(victory));
        }

        IEnumerator ShowEndScreen(bool victory)
        {
            if (endScreenDelay > 0f) yield return new WaitForSecondsRealtime(endScreenDelay);

            Time.timeScale = 0f;
            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null) player.enabled = false;
            if (hud != null) hud.SetActive(false);
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
            if (victory && winPanel != null) winPanel.SetActive(true);
            if (!victory && losePanel != null) losePanel.SetActive(true);
        }

        public void PlayAgain()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void MainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuScene);
        }
    }
}
