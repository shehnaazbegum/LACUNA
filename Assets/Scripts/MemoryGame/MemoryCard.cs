using UnityEngine;
using UnityEngine.UI;

public class MemoryCard : MonoBehaviour
{
    private Button button;
    private GameObject cardFront;
    private GameObject cardBack;

    private MemoryGameManager gameManager;

    private bool isFlipped = false;
    private bool isMatched = false;

    public int PairId { get; private set; }

    private void Awake()
    {
        button = GetComponent<Button>();

        Transform front = transform.Find("CardFront");
        Transform back = transform.Find("CardBack");

        if (front != null)
            cardFront = front.gameObject;

        if (back != null)
            cardBack = back.gameObject;

        if (button == null)
        {
            Debug.LogError("MemoryCard: Button missing on " + gameObject.name);
            return;
        }

        if (cardFront == null)
        {
            Debug.LogError("MemoryCard: Child 'CardFront' not found on " + gameObject.name);
            return;
        }

        if (cardBack == null)
        {
            Debug.LogError("MemoryCard: Child 'CardBack' not found on " + gameObject.name);
            return;
        }

        button.onClick.RemoveListener(OnCardClicked);
        button.onClick.AddListener(OnCardClicked);

        ShowBack();
    }

    public bool Initialise(
        MemoryGameManager manager,
        int pairId,
        Sprite image)
    {
        if (manager == null)
        {
            Debug.LogError(
                "MemoryCard: Cannot initialise because MemoryGameManager is missing.",
                this);
            return false;
        }

        if (image == null)
        {
            Debug.LogError(
                "MemoryCard: Cannot initialise because the card image is missing.",
                this);
            return false;
        }

        gameManager = manager;
        PairId = pairId;

        Image frontImage = cardFront.GetComponent<Image>();

        if (frontImage == null)
        {
            Debug.LogError(
                "MemoryCard: CardFront needs an Image component on " +
                gameObject.name,
                this);
            return false;
        }

        frontImage.sprite = image;

        isFlipped = false;
        isMatched = false;

        if (button != null)
            button.interactable = true;

        ShowBack();

        return true;
    }

    public void OnCardClicked()
    {
        if (isMatched || isFlipped)
            return;

        if (gameManager == null)
        {
            Debug.LogError(
                "MemoryCard: GameManager reference is missing on " +
                gameObject.name,
                this);
            return;
        }

        gameManager.CardClicked(this);
    }

    public void FlipUp()
    {
        if (cardFront == null || cardBack == null)
            return;

        isFlipped = true;

        cardFront.SetActive(true);
        cardBack.SetActive(false);
    }

    public void FlipDown()
    {
        if (isMatched)
            return;

        if (cardFront == null || cardBack == null)
            return;

        isFlipped = false;

        cardFront.SetActive(false);
        cardBack.SetActive(true);
    }

    public void SetMatched()
    {
        isMatched = true;
        isFlipped = true;

        if (button != null)
            button.interactable = false;
    }

    public bool IsFlipped()
    {
        return isFlipped;
    }

    public bool IsMatched()
    {
        return isMatched;
    }

    private void ShowBack()
    {
        if (cardFront != null)
            cardFront.SetActive(false);

        if (cardBack != null)
            cardBack.SetActive(true);
    }
}