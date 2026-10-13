using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// Runs one memory game: difficulty, pairing, shuffling, clicks, matching,
// mistakes, timer, pause, and completion.
public class MemoryGameManager : MonoBehaviour
{
    public enum Difficulty
    {
        Easy,
        Medium,
        Hard,
        ExtraHard
    }

    [Header("GAME SETTINGS")]
    [Tooltip("Used when you press Play directly in this scene. When the player picks a level in Game Select, that choice is used instead.")]
    [SerializeField] private Difficulty difficulty = Difficulty.ExtraHard;

    [Tooltip("All card buttons in the grid, in order. Extra Hard needs 30.")]
    [SerializeField] private MemoryCard[] cards;

    [Tooltip("Unique card images. Easy needs 3, Medium 6, Hard 9, Extra Hard 15.")]
    [SerializeField] private Sprite[] cardImages;

    [Header("GAMEPLAY")]
    [Tooltip("Seconds two non-matching cards stay visible before flipping back.")]
    [SerializeField] private float mismatchDelay = 1f;
    [SerializeField] private float preparationDelay = 1f;
    [SerializeField] private float studyDelay = 5f;

    [Tooltip("Seconds to wait after the last match before opening the Results scene.")]
    [SerializeField] private float completionDelay = 1.5f;

    [Header("UI")]
    [SerializeField] private TMP_Text difficultyText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text mistakesText;
    [SerializeField] private GameObject pausePanel;

    [Header("SCENES (must match the scene names in Build Profiles > Scene List)")]
    [SerializeField] private string resultsSceneName = "Results";
    [SerializeField] private string quitSceneName = "GameSelect";

    private MemoryCard firstCard;
    private MemoryCard secondCard;

    private bool isGameRunning = false;
    private bool isPaused = false;
    private bool isResolvingPair = false;

    private int mistakes = 0;
    private int matchedPairs = 0;
    private int attempts = 0;
    private float timer = 0f;

    private int cardsRequired;
    private int pairsRequired;

    // --------------------------------------------------
    // START
    // --------------------------------------------------

    private void Start()
    {
        if (GameSession.HasSelectedDifficulty)
            difficulty = GameSession.SelectedDifficulty;

        SetDifficultyValues();

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (!ValidateSetup())
            return;

        if (!SetupCards())
            return;

        timer = 0f;
        mistakes = 0;
        matchedPairs = 0;
        attempts = 0;
        firstCard = null;
        secondCard = null;
        isPaused = false;
        isResolvingPair = false;
        isGameRunning = false;

        UpdateDifficultyUI();
        UpdateTimerUI();
        UpdateMistakesUI();

        StartCoroutine(StartGameSequence());
    }

    private IEnumerator StartGameSequence()
    {
        // 1. Preparation — cards stay face-down
        yield return new WaitForSeconds(preparationDelay);

        // 2. Study — show all cards
        for (int i = 0; i < cardsRequired; i++)
        {
            cards[i].FlipUp();
        }

        yield return new WaitForSeconds(studyDelay);

        // 3. Gameplay — hide all cards again
        for (int i = 0; i < cardsRequired; i++)
        {
            cards[i].FlipDown();
        }

        // Now the actual game begins
        timer = 0f;
        isGameRunning = true;

        UpdateTimerUI();
    }

    private void Update()
    {
        if (!isGameRunning || isPaused)
            return;

        timer += Time.deltaTime;
        UpdateTimerUI();
    }

    // --------------------------------------------------
    // DIFFICULTY
    // --------------------------------------------------

    private void SetDifficultyValues()
    {
        switch (difficulty)
        {
            case Difficulty.Easy:
                pairsRequired = 3;
                break;
            case Difficulty.Medium:
                pairsRequired = 6;
                break;
            case Difficulty.Hard:
                pairsRequired = 10;
                break;
            default:
                pairsRequired = 15;
                break;
        }

        cardsRequired = pairsRequired * 2;
    }

    public static string GetDifficultyLabel(Difficulty value)
    {
        switch (value)
        {
            case Difficulty.Easy: return "EASY";
            case Difficulty.Medium: return "MEDIUM";
            case Difficulty.Hard: return "HARD";
            default: return "EXTRA HARD";
        }
    }

    // --------------------------------------------------
    // SETUP
    // --------------------------------------------------

    private bool ValidateSetup()
    {
        bool ok = true;

        if (cards == null || cards.Length == 0)
        {
            Debug.LogError("MemoryGameManager: No cards assigned.", this);
            return false;
        }

        if (cards.Length < cardsRequired)
        {
            Debug.LogError(
                "MemoryGameManager: " + GetDifficultyLabel(difficulty) + " requires " + cardsRequired +
                " cards, but only " + cards.Length + " are in the Cards list.", this);
            ok = false;
        }

        if (cardImages == null || cardImages.Length < pairsRequired)
        {
            Debug.LogError(
                "MemoryGameManager: " + GetDifficultyLabel(difficulty) + " requires " + pairsRequired +
                " card images, but only " + (cardImages == null ? 0 : cardImages.Length) + " are assigned.", this);
            ok = false;
        }

        if (!ok)
            return false;

        for (int i = 0; i < cardsRequired; i++)
        {
            if (cards[i] == null)
            {
                Debug.LogError("MemoryGameManager: Cards Element " + i + " is empty.", this);
                ok = false;
            }
        }

        for (int i = 0; i < pairsRequired; i++)
        {
            if (cardImages[i] == null)
            {
                Debug.LogError("MemoryGameManager: Card Images Element " + i + " is empty.", this);
                ok = false;
            }
        }

        for (int i = 0; i < pairsRequired; i++)
        {
            for (int j = i + 1; j < pairsRequired; j++)
            {
                if (cardImages[i] != null && cardImages[i] == cardImages[j])
                {
                    Debug.LogWarning(
                        "MemoryGameManager: Card Images Element " + i + " and Element " + j +
                        " are the same image. Players will see two identical-looking pairs.", this);
                }
            }
        }

        return ok;
    }

    private bool SetupCards()
    {
        // Build the deck as pair IDs: 0,0,1,1,2,2 ...
        List<int> deck = new List<int>();

        for (int pair = 0; pair < pairsRequired; pair++)
        {
            deck.Add(pair);
            deck.Add(pair);
        }

        ShuffleList(deck);

        // Give each used card one shuffled pair ID and that pair's image.
        for (int i = 0; i < cardsRequired; i++)
        {
            cards[i].gameObject.SetActive(true);

            int pairId = deck[i];

            if (!cards[i].Initialise(this, pairId, cardImages[pairId]))
            {
                Debug.LogError("MemoryGameManager: Could not set up " + cards[i].gameObject.name + ". See the error above.", this);
                return false;
            }
        }

        // Hide any cards this difficulty does not use.
        for (int i = cardsRequired; i < cards.Length; i++)
        {
            if (cards[i] != null)
                cards[i].gameObject.SetActive(false);
        }

        return true;
    }

    // --------------------------------------------------
    // CARD CLICKS (called by MemoryCard)
    // --------------------------------------------------

    public void CardClicked(MemoryCard clickedCard)
    {
        if (!isGameRunning || isPaused || isResolvingPair)
            return;

        if (clickedCard == null || clickedCard.IsMatched() || clickedCard.IsFlipped())
            return;

        clickedCard.FlipUp();

        if (firstCard == null)
        {
            firstCard = clickedCard;
            return;
        }

        secondCard = clickedCard;
        attempts++;

        if (firstCard.PairId == secondCard.PairId)
            HandleMatch();
        else
            StartCoroutine(HandleMismatch());
    }

    private void HandleMatch()
    {
        firstCard.SetMatched();
        secondCard.SetMatched();

        firstCard = null;
        secondCard = null;

        matchedPairs++;

        if (matchedPairs >= pairsRequired)
            StartCoroutine(CompleteGame());
    }

    private IEnumerator HandleMismatch()
    {
        isResolvingPair = true;

        mistakes++;
        UpdateMistakesUI();

        // Wait, but only count time while the game is not paused.
        float waited = 0f;

        while (waited < mismatchDelay)
        {
            if (!isPaused)
                waited += Time.deltaTime;

            yield return null;
        }

        if (firstCard != null)
            firstCard.FlipDown();

        if (secondCard != null)
            secondCard.FlipDown();

        firstCard = null;
        secondCard = null;
        isResolvingPair = false;
    }

    // --------------------------------------------------
    // GAME COMPLETION
    // --------------------------------------------------

    private IEnumerator CompleteGame()
    {
        isGameRunning = false;   // stops the timer and blocks all clicks
        UpdateTimerUI();

        GameResult result = new GameResult();
        result.difficulty = difficulty;
        result.timeSeconds = timer;
        result.mistakes = mistakes;
        result.pairsMatched = matchedPairs;
        result.totalPairs = pairsRequired;
        result.totalCards = cardsRequired;
        result.attempts = attempts;
        result.accuracyPercent = attempts > 0 ? (matchedPairs * 100f) / attempts : 100f;
        result.completedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

        GameSession.LastResult = result;

        Debug.Log(
            "Memory Game Complete! " + GetDifficultyLabel(difficulty) +
            " | Time: " + FormatTime(timer) +
            " | Mistakes: " + mistakes +
            " | Accuracy: " + result.accuracyPercent.ToString("0") + "%", this);

        yield return new WaitForSeconds(completionDelay);

        LoadScene(resultsSceneName);
    }

    // --------------------------------------------------
    // PAUSE (connect these to buttons)
    // --------------------------------------------------

    public void PauseGame()
    {
        if (!isGameRunning)
            return;

        isPaused = true;

        if (pausePanel != null)
            pausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        if (!isGameRunning)
            return;

        isPaused = false;

        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void QuitGame()
    {
        isGameRunning = false;
        LoadScene(quitSceneName);
    }

    // --------------------------------------------------
    // SCENE LOADING
    // --------------------------------------------------

    private void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("MemoryGameManager: A scene name is empty in the Inspector.", this);
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError(
                "MemoryGameManager: Scene '" + sceneName + "' cannot be loaded. " +
                "Check that it exists and is added in File > Build Profiles > Scene List, " +
                "and that the name matches exactly.", this);
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    // --------------------------------------------------
    // UI
    // --------------------------------------------------

    private void UpdateDifficultyUI()
    {
        if (difficultyText != null)
            difficultyText.text = "DIFFICULTY: " + GetDifficultyLabel(difficulty);
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
            timerText.text = "TIMER: " + FormatTime(timer);
    }

    private void UpdateMistakesUI()
    {
        if (mistakesText != null)
            mistakesText.text = "MISTAKES: " + mistakes;
    }

    public static string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

        return minutes.ToString("00") + ":" + seconds.ToString("00");
    }

    // --------------------------------------------------
    // SHUFFLE
    // --------------------------------------------------

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);

            T temporary = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temporary;
        }
    }
}
