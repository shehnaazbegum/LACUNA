public static class GameSession
{
    public static bool HasSelectedDifficulty { get; private set; }

    public static MemoryGameManager.Difficulty SelectedDifficulty { get; private set; }

    public static GameResult LastResult { get; set; }

    public static void SetDifficulty(MemoryGameManager.Difficulty difficulty)
    {
        SelectedDifficulty = difficulty;
        HasSelectedDifficulty = true;
    }

    public static void ClearDifficulty()
    {
        HasSelectedDifficulty = false;
    }
}