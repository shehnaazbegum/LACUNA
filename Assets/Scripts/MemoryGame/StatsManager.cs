using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class StatsManager : MonoBehaviour
{
    [Header("Statistics Text")]
    [SerializeField] private TMP_Text totalGamesText;
    [SerializeField] private TMP_Text bestScoreText;
    [SerializeField] private TMP_Text bestTimeText;
    [SerializeField] private TMP_Text bestAccuracyText;

    [Header("History - Date")]
    [SerializeField] private TMP_Text dateRow1;
    [SerializeField] private TMP_Text dateRow2;
    [SerializeField] private TMP_Text dateRow3;

    [Header("History - Difficulty")]
    [SerializeField] private TMP_Text difficultyRow1;
    [SerializeField] private TMP_Text difficultyRow2;
    [SerializeField] private TMP_Text difficultyRow3;

    [Header("History - Time")]
    [SerializeField] private TMP_Text timeRow1;
    [SerializeField] private TMP_Text timeRow2;
    [SerializeField] private TMP_Text timeRow3;

    [Header("History - Score")]
    [SerializeField] private TMP_Text scoreRow1;
    [SerializeField] private TMP_Text scoreRow2;
    [SerializeField] private TMP_Text scoreRow3;

    private void Start()
    {
        LoadStatistics();
        LoadHistory();
    }

    private void LoadStatistics()
    {
        int totalGames = PlayerPrefs.GetInt("TotalGames", 0);
        int bestScore = PlayerPrefs.GetInt("BestScore", 0);
        float bestTime = PlayerPrefs.GetFloat("BestTime", -1f);
        float bestAccuracy = PlayerPrefs.GetFloat("BestAccuracy", 0f);

        totalGamesText.text = "TOTAL GAMES: " + totalGames.ToString("000");

        bestScoreText.text = "BEST SCORE: " + bestScore.ToString("00");

        if (bestTime < 0f)
        {
            bestTimeText.text = "BEST TIME: 00:00";
        }
        else
        {
            bestTimeText.text = "BEST TIME: " + FormatTime(bestTime);
        }

        bestAccuracyText.text =
            "BEST ACCURACY: " + bestAccuracy.ToString("0") + "%";
    }

    private void LoadHistory()
    {
        string history = PlayerPrefs.GetString("GameHistory", "");

        // Clear all rows first
        ClearRow(dateRow1, difficultyRow1, timeRow1, scoreRow1);
        ClearRow(dateRow2, difficultyRow2, timeRow2, scoreRow2);
        ClearRow(dateRow3, difficultyRow3, timeRow3, scoreRow3);

        if (string.IsNullOrEmpty(history))
        {
            return;
        }

        string[] games = history.Split('|');

        for (int i = 0; i < games.Length && i < 3; i++)
        {
            string[] data = games[i].Split('~');

            if (data.Length < 5)
            {
                continue;
            }

            string date = data[0];
            string difficulty = data[1];
            float time = float.Parse(data[2]);
            int score = int.Parse(data[4]);

            if (i == 0)
            {
                SetRow(dateRow1, difficultyRow1, timeRow1, scoreRow1,
                    date, difficulty, time, score);
            }
            else if (i == 1)
            {
                SetRow(dateRow2, difficultyRow2, timeRow2, scoreRow2,
                    date, difficulty, time, score);
            }
            else if (i == 2)
            {
                SetRow(dateRow3, difficultyRow3, timeRow3, scoreRow3,
                    date, difficulty, time, score);
            }
        }
    }

    private void SetRow(
        TMP_Text dateText,
        TMP_Text difficultyText,
        TMP_Text timeText,
        TMP_Text scoreText,
        string date,
        string difficulty,
        float time,
        int score)
    {
        dateText.text = date;
        difficultyText.text = difficulty.ToUpper();
        timeText.text = FormatTime(time);
        scoreText.text = score.ToString("00");
    }

    private void ClearRow(
        TMP_Text dateText,
        TMP_Text difficultyText,
        TMP_Text timeText,
        TMP_Text scoreText)
    {
        dateText.text = "-";
        difficultyText.text = "-";
        timeText.text = "--:--";
        scoreText.text = "-";
    }

    private string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60f);
        int remainingSeconds = Mathf.FloorToInt(seconds % 60f);

        return minutes.ToString("00") + ":" +
               remainingSeconds.ToString("00");
    }
}