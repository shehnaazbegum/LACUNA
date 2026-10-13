using System;

[Serializable]
public class GameResult
{
    public MemoryGameManager.Difficulty difficulty;

    public float timeSeconds;
    public int mistakes;
    public int pairsMatched;
    public int totalPairs;
    public int totalCards;
    public int attempts;

    public float accuracyPercent;

    public string completedAt;
}