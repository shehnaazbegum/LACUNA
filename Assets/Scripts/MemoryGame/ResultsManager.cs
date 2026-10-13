using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class ResultsManager : MonoBehaviour
{
    [Header("Result Texts")]
    [SerializeField] private TMP_Text difficultyText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text attemptsText;
    [SerializeField] private TMP_Text accuracyText;
    [SerializeField] private TMP_Text mistakesText;
    [SerializeField] private TMP_Text matchesText;
    [SerializeField] private TMP_Text scoreText;

    private GameResult result;

    private void Start()
    {
        result = GameSession.LastResult;

        if (result == null)
        {
            Debug.LogError("ResultsManager: No game result was found.");
            return;
        }

        DisplayResults();
        SaveResult();
    }

    private void DisplayResults()
    {
        difficultyText.text = "Difficulty: " + GetDifficultyLabel(result.difficulty);

        timerText.text = "Timer: " + FormatTime(result.timeSeconds);

        attemptsText.text = "Attempts: " + result.attempts.ToString("00");

        accuracyText.text = "Accuracy: " + result.accuracyPercent.ToString("0") + "%";

        mistakesText.text = "Mistakes: " + result.mistakes.ToString("00");

        matchesText.text = "Matches: " + result.pairsMatched.ToString("00");

        scoreText.text = "Score: " + CalculateScore().ToString("00");
    }

    private void SaveResult()
    {
        string history = PlayerPrefs.GetString("GameHistory", "");

        List<string> games = new List<string>();

        if (!string.IsNullOrEmpty(history))
        {
            games.AddRange(history.Split('|'));
        }

        string newEntry =
            result.completedAt + "~" +
            result.difficulty + "~" +
            result.timeSeconds + "~" +
            result.accuracyPercent + "~" +
            CalculateScore();

        games.Insert(0, newEntry);

        // Keep only the latest 3 games
        if (games.Count > 3)
        {
            games.RemoveRange(3, games.Count - 3);
        }

        PlayerPrefs.SetString("GameHistory", string.Join("|", games));

        // Total games
        int totalGames = PlayerPrefs.GetInt("TotalGames", 0);
        PlayerPrefs.SetInt("TotalGames", totalGames + 1);

        // Best score
        int currentScore = CalculateScore();
        int bestScore = PlayerPrefs.GetInt("BestScore", 0);

        if (currentScore > bestScore)
        {
            PlayerPrefs.SetInt("BestScore", currentScore);
        }

        // Best time
        float bestTime = PlayerPrefs.GetFloat("BestTime", -1f);

        if (bestTime < 0f || result.timeSeconds < bestTime)
        {
            PlayerPrefs.SetFloat("BestTime", result.timeSeconds);
        }

        // Best accuracy
        float bestAccuracy = PlayerPrefs.GetFloat("BestAccuracy", 0f);

        if (result.accuracyPercent > bestAccuracy)
        {
            PlayerPrefs.SetFloat("BestAccuracy", result.accuracyPercent);
        }

        PlayerPrefs.Save();
    }

    private int CalculateScore()
    {
        // Base score
        int score = 1000;

        // Reward matching pairs
        score += result.pairsMatched * 100;

        // Penalize mistakes
        score -= result.mistakes * 25;

        // Penalize time
        score -= Mathf.RoundToInt(result.timeSeconds);

        // Never allow negative score
        return Mathf.Max(0, score);
    }

    private string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60f);
        int remainingSeconds = Mathf.FloorToInt(seconds % 60f);

        return minutes.ToString("00") + ":" +
               remainingSeconds.ToString("00");
    }

    private string GetDifficultyLabel(MemoryGameManager.Difficulty difficulty)
    {
        switch (difficulty)
        {
            case MemoryGameManager.Difficulty.Easy:
                return "EASY";

            case MemoryGameManager.Difficulty.Medium:
                return "MEDIUM";

            case MemoryGameManager.Difficulty.Hard:
                return "HARD";

            case MemoryGameManager.Difficulty.ExtraHard:
                return "EXTRA HARD";

            default:
                return difficulty.ToString().ToUpper();
        }
    }

        public void PlayAgain()
    {
        switch (result.difficulty)
        {
            case MemoryGameManager.Difficulty.Easy:
                SceneManager.LoadScene("MemoryEasy");
                break;

            case MemoryGameManager.Difficulty.Medium:
                SceneManager.LoadScene("MemoryMedium");
                break;

            case MemoryGameManager.Difficulty.Hard:
                SceneManager.LoadScene("MemoryHard");
                break;

            case MemoryGameManager.Difficulty.ExtraHard:
                SceneManager.LoadScene("MemoryGame");
                break;

            default:
                Debug.LogError("ResultsManager: Unknown difficulty.");
                break;
        }
    }
}
