using System.Reflection;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace SpaceShooterMenus
{
    /// <summary>
    /// Victory / Game Over screen. GameFlow turns this panel on at the end of the game;
    /// it then fills in the stats: score (read from the team's UIManager), time played and hearts left.
    /// </summary>
    public class EndGamePanel : MonoBehaviour
    {
        public TMP_Text scoreValue;
        public TMP_Text timeValue;
        [Tooltip("Optional. Shows hearts left, e.g. 3/4.")]
        public TMP_Text heartsValue;

        void OnEnable()
        {
            if (!Application.isPlaying) return;

            UIManager ui = FindFirstObjectByType<UIManager>(FindObjectsInactive.Include);

            if (scoreValue != null)
                scoreValue.text = ReadScore(ui).ToString("D6");

            if (timeValue != null)
            {
                float seconds = GameFlow.Instance != null ? GameFlow.Instance.PlayTime : Time.timeSinceLevelLoad;
                timeValue.text = FormatTime(seconds);
            }

            if (heartsValue != null)
            {
                int total = 0, left = 0;
                if (ui != null && ui.healthIcons != null)
                {
                    foreach (GameObject heart in ui.healthIcons)
                    {
                        if (heart == null) continue;
                        total++;
                        if (heart.activeSelf) left++;
                    }
                }
                heartsValue.text = left + "/" + total;
            }
        }

        /// <summary>
        /// Reads the score from the team's UIManager without changing that script:
        /// first its private currentScore field, otherwise the last number in the score text.
        /// </summary>
        static int ReadScore(UIManager ui)
        {
            if (ui == null) return 0;

            FieldInfo field = typeof(UIManager).GetField("currentScore", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (field != null && field.FieldType == typeof(int)) return (int)field.GetValue(ui);

            if (ui.scoreText != null)
            {
                Match number = Regex.Match(ui.scoreText.text, @"(\d+)\D*$");
                if (number.Success && int.TryParse(number.Groups[1].Value, out int score)) return score;
            }
            return 0;
        }

        public static string FormatTime(float seconds)
        {
            int s = Mathf.Max(0, Mathf.FloorToInt(seconds));
            return (s / 60).ToString("00") + ":" + (s % 60).ToString("00");
        }

        public void PlayAgain()
        {
            if (GameFlow.Instance != null) GameFlow.Instance.PlayAgain();
        }

        public void MainMenu()
        {
            if (GameFlow.Instance != null) GameFlow.Instance.MainMenu();
        }
    }
}
