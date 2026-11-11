using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Health UI")]
    public GameObject[] healthIcons;

    [Header("Score UI")]
    public TextMeshProUGUI scoreText;
    private int currentScore = 0;

    void Start()
    {
        UpdateScoreDisplay();
    }

    public void UpdateHealthDisplay(int currentHealth)
    {
        for (int i = 0; i < healthIcons.Length; i++)
        {
            if (healthIcons[i] != null)
            {
                healthIcons[i].SetActive(i < currentHealth);
            }
        }
    }

    public void AddScore(int points)
    {
        currentScore += points;
        UpdateScoreDisplay();
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            // The space instead of \n keeps it on one line
            scoreText.text = "SCORE: " + currentScore.ToString("D6");
        }
        else
        {
            Debug.LogWarning("UIManager: Score Text is not assigned in the Inspector!");
        }
    }

    public void ShowGameOver()
    {
        if (scoreText != null)
        {
            scoreText.text = "GAME OVER\nSCORE: " + currentScore;
        }
    }
}