using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public Text healthText;
    public Text scoreText;
    public GameObject gameOverScreen;

    private int score = 0;

    public void UpdateHealth(float health)
    {
        healthText.text = $"Health: {health}";
    }

    public void UpdateScore(int points)
    {
        score += points;
        scoreText.text = $"Score: {score}";
    }

    public void ShowGameOverScreen()
    {
        gameOverScreen.SetActive(true);
    }
}