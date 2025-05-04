using System.Net.Mime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;

public class CardManager : MonoBehaviour
{
    private Vector3 originalPlayer1DeckPos;
    private Vector3 originalPlayer2DeckPos;
    public GameObject cardPrefab;
    public Sprite backSprite;
    public Transform player1DeckPosition;
    public Transform player2DeckPosition;
    public TMP_Text Player1Text;
    public TMP_Text Player2Text;
    public GameObject winCanvas;
    public TMP_Text winText;

    public AudioSource cardDrawSound;
    private AudioSource cardDrawAudio;

    private List<Sprite> allCardSprites = new List<Sprite>();
    private List<Card> player1Cards = new List<Card>();
    private List<Card> player2Cards = new List<Card>();
    private List<Card> centralPile = new List<Card>();

    private float playedCardZOffset = 0f;
    private int currentPlayerTurn = 1;

    private bool gameOver = false;
    private int winningPlayer = 0;

    public int Player1CardCount => player1Cards.Count;
    public int Player2CardCount => player2Cards.Count;

    public bool isAI = true; // Add this variable
    public bool isNonAI = false; // Add this variable

    void Start()
    {
        // Store original positions
        originalPlayer1DeckPos = player1DeckPosition.position;
        originalPlayer2DeckPos = player2DeckPosition.position;
        LoadAllCardSprites();
        ShuffleAndDistribute();
        UpdateCardCountText();

        cardDrawAudio = gameObject.AddComponent<AudioSource>();
        cardDrawAudio.clip = Resources.Load<AudioClip>("Audio/cardDraw");

        if (isAI)
        {
            InitializeAI();
        }

        if (isNonAI)
        {
            InitializeNonAI();
        }
    }

    void InitializeAI()
    {
        // You can implement AI behavior here, like automatic moves.
        if (currentPlayerTurn == 2)
        {
            StartCoroutine(PlayAIPlayer2Card());
        }
    }

    void InitializeNonAI()
    {
        // Regular card dealing for player
    }

    public void ResetGame()
    {
        // Reset deck positions
        originalPlayer1DeckPos = player1DeckPosition.position;
        originalPlayer2DeckPos = player2DeckPosition.position;

        // Destroy all child cards in each deck position
        foreach (Transform child in player1DeckPosition)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in player2DeckPosition)
        {
            Destroy(child.gameObject);
        }
        foreach (Card card in centralPile)
        {
            if (card != null)
            {
                Destroy(card.gameObject);
            }
        }
        centralPile.Clear();
        player1Cards.Clear();
        player2Cards.Clear();

        

        playedCardZOffset = 0f;
        currentPlayerTurn = 1;
        gameOver = false;
        winningPlayer = 0;

        if (Player1Text != null)
            Player1Text.text = "00";
        if (Player2Text != null)
            Player2Text.text = "00";
        if (winText != null)
            winText.text = "";

        LoadAllCardSprites();
        ShuffleAndDistribute();
        UpdateCardCountText();

        if (isAI)
        {
            InitializeAI();
        }

        if (isNonAI)
        {
            InitializeNonAI();
        }
    }

    void LoadAllCardSprites()
    {
        Sprite[] loaded = Resources.LoadAll<Sprite>("Cards");
        allCardSprites = new List<Sprite>(loaded);
    }

    void ShuffleAndDistribute()
    {
        if (allCardSprites.Count < 52)
        {
            return;
        }

        List<Sprite> deck = new List<Sprite>(allCardSprites);
        Shuffle(deck);

        for (int i = 0; i < 52; i++)
        {
            Vector3 spawnPos;
            Quaternion rotation;

            if (i < 26)
            {
                spawnPos = new Vector3(player1DeckPosition.position.x, player1DeckPosition.position.y, 0f);
                rotation = Quaternion.Euler(0, 0, 0);
            }
            else
            {
                spawnPos = new Vector3(player2DeckPosition.position.x, player2DeckPosition.position.y, 0f);
                rotation = Quaternion.Euler(0, 0, 0);
            }

            GameObject cardObj = Instantiate(cardPrefab, spawnPos, rotation);
            cardObj.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);

            // Flip horizontally for Player 2
            if (i >= 26)
            {
                cardObj.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
            }

            Transform frontCardTransform = cardObj.transform.Find("FrontCard");
            SpriteRenderer frontRenderer = frontCardTransform?.GetComponent<SpriteRenderer>();
            if (frontRenderer != null)
            {
                frontRenderer.sprite = deck[i];
                frontCardTransform.gameObject.SetActive(false);
            }

            if (cardObj.GetComponent<BoxCollider2D>() == null)
                cardObj.AddComponent<BoxCollider2D>();

            Card card = cardObj.GetComponent<Card>();
            card.SetCard(deck[i], backSprite);
            card.playerNumber = i < 26 ? 1 : 2;
            card.cardManager = this;

            if (i < 26) player1Cards.Add(card);
            else player2Cards.Add(card);
        }
    }

    void Shuffle(List<Sprite> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            Sprite temp = list[i];
            list[i] = list[rand];
            list[rand] = temp;
        }
    }

    public void PlayNextCardForPlayer(int playerNumber)
    {
        if (gameOver)
        {
            return;
        }

        if (playerNumber != currentPlayerTurn) return;

        List<Card> targetDeck = playerNumber == 1 ? player1Cards : player2Cards;
        if (targetDeck.Count == 0) return;

        int topCardIndex = targetDeck.Count - 1;
        Card card = targetDeck[topCardIndex];
        targetDeck.RemoveAt(topCardIndex);

        GameObject cardObj = card.gameObject;
        SpriteRenderer backRenderer = cardObj.GetComponent<SpriteRenderer>();
        Transform front = cardObj.transform.Find("FrontCard");

        if (backRenderer != null) backRenderer.enabled = false;
        if (front != null) front.gameObject.SetActive(true);

        cardObj.transform.position = new Vector3(0f, 0f, playedCardZOffset);
        float baseRotation = 0f;
        cardObj.transform.rotation = Quaternion.Euler(0f, 0f, baseRotation + Random.Range(-20f, 20f));
        if (cardDrawSound != null)
            cardDrawSound.Play();
        if (cardDrawAudio != null && cardDrawAudio.clip != null)
            cardDrawAudio.Play();

        playedCardZOffset -= 0.01f;

        centralPile.Add(card);

        UpdateCardCountText();
        CheckWinConditions();

        if (centralPile.Count >= 2)
        {
            Card last = centralPile[centralPile.Count - 1];
            Card secondLast = centralPile[centralPile.Count - 2];

            string lastRank = ExtractRank(last.FrontSpriteName);
            string secondLastRank = ExtractRank(secondLast.FrontSpriteName);

            if (lastRank == secondLastRank)
            {
                StartCoroutine(CollectPileWithDelay(playerNumber, lastRank));
            }
        }

        currentPlayerTurn = playerNumber == 1 ? 2 : 1;

        if (isAI && !gameOver && currentPlayerTurn == 2)
        {
            StartCoroutine(PlayAIPlayer2Card());
        }
    }

    private IEnumerator PlayAIPlayer2Card()
    {
        yield return new WaitForSeconds(0.5f); // Delay to simulate thinking time
        PlayNextCardForPlayer(2);
    }

    private IEnumerator CollectPileWithDelay(int playerNumber, string rank)
    {
        yield return new WaitForSeconds(0.3f);


        List<Card> winnerDeck = playerNumber == 1 ? player1Cards : player2Cards;
        Transform winnerPos = playerNumber == 1 ? player1DeckPosition : player2DeckPosition;

        List<Card> cardsToAdd = new List<Card>(centralPile);

        for (int i = cardsToAdd.Count - 1; i >= 0; i--)
        {
            Card pileCard = cardsToAdd[i];
            GameObject pileCardObj = pileCard.gameObject;

            pileCard.playerNumber = playerNumber;

            SpriteRenderer backRenderer = pileCardObj.GetComponent<SpriteRenderer>();
            Transform frontCard = pileCardObj.transform.Find("FrontCard");
            if (backRenderer != null) backRenderer.enabled = true;
            if (frontCard != null) frontCard.gameObject.SetActive(false);

            winnerDeck.Insert(0, pileCard);
        }

        RearrangeDeck(playerNumber);
        centralPile.Clear();
        UpdateCardCountText();
        CheckWinConditions();
    }

    private void RearrangeDeck(int playerNumber)
    {
        List<Card> deck = player1Cards;
        Transform deckPos = player1DeckPosition;
        if (playerNumber == 2)
        {
            deck = player2Cards;
            deckPos = player2DeckPosition;
        }

        Vector3 basePosition = deckPos.position;
        Quaternion rotation = Quaternion.Euler(0, 0, 0);

        for (int i = 0; i < deck.Count; i++)
        {
            Card card = deck[i];
            GameObject cardObj = card.gameObject;

            Vector3 cardPosition = new Vector3(
                basePosition.x,
                basePosition.y,
                basePosition.z - (i * 0.001f)
            );

            cardObj.transform.position = cardPosition;
            cardObj.transform.rotation = rotation;
        }
    }

    string ExtractRank(string spriteName)
    {
        return spriteName.Replace("cardClubs", "")
                         .Replace("cardDiamonds", "")
                         .Replace("cardHearts", "")
                         .Replace("cardSpades", "")
                         .Replace("card", "")
                         .ToUpper();
    }

    public void OnCardPileClicked(int playerNumber)
    {
        if (!isAI || playerNumber == 1)
        {
            PlayNextCardForPlayer(playerNumber);
        }
    }

    public void PlayPlayer1Card()
    {
        PlayNextCardForPlayer(1);
    }

    public void PlayPlayer2Card()
    {
        if (!isAI)
        {
            PlayNextCardForPlayer(2);
        }
    }

    void Update()
    {
        if (!gameOver)
        {
            if (Player1CardCount == 0 || Player2CardCount == 0)
            {
                UpdateCardCountText();
                CheckWinConditions();
            }

            if (Player1CardCount >= 52 || Player2CardCount >= 52)
            {
                CheckWinConditions();
            }
        }
    }

    void UpdateCardCountText()
    {
        if (Player1Text != null)
            Player1Text.text = Player1CardCount <= 9 ? $"0{Player1CardCount}" : $"{Player1CardCount}";

        if (Player2Text != null)
            Player2Text.text = Player2CardCount <= 9 ? $"0{Player2CardCount}" : $"{Player2CardCount}";

    }

    void CheckWinConditions()
    {
        // Player 1 wins
        if (Player1CardCount >= 52)
        {
            ShowWinCanvas(1);
            return;
        }
        // Player 2 or AI wins
        if (Player2CardCount >= 52)
        {
            if (isAI)
                ShowWinCanvas(3); // AI wins
            else
                ShowWinCanvas(2); // Player 2 wins
            return;
        }
        // Player 1 loses all cards
        if (Player1CardCount == 0)
        {
            if (isAI)
                ShowWinCanvas(3); // AI wins
            else
                ShowWinCanvas(2); // Player 2 wins
            return;
        }
        // Player 2 loses all cards
        if (Player2CardCount == 0)
        {
            ShowWinCanvas(1); // Player 1 wins
            return;
        }
    }

    void ShowWinCanvas(int winner)
    {
        // Display the win canvas
        if (winCanvas != null)
        {
            winCanvas.SetActive(true);

            // Show different messages based on the winner
            if (winner == 1)
            {
                winText.text = "Player 1 Wins";
            }
            else if (winner == 2)
            {
                winText.text = "Player 2 Wins";
            }
            else if (winner == 3)
            {
                winText.text = "AI Wins";
            }
        }
    }

public void RestartGame()
{
    // Destroy all cards from player decks
    foreach (Card card in player1Cards)
    {
        if (card != null)
        {
            Destroy(card.gameObject);
        }
    }

    foreach (Card card in player2Cards)
    {
        if (card != null)
        {
            Destroy(card.gameObject);
        }
    }

    // Destroy all cards from central pile
    foreach (Card card in centralPile)
    {
        if (card != null)
        {
            Destroy(card.gameObject);
        }
    }

    player1Cards.Clear();
    player2Cards.Clear();
    centralPile.Clear();

    ResetGame();

    if (winCanvas != null)
    winCanvas.SetActive(false);

    // Add Y offset adjustment based on side
}

public void MainMenu(){
    SceneManager.LoadSceneAsync(0);
}
}
// public void RearrangeCards()
// {
//     // Destroy existing card GameObjects
//     foreach (Card card in player1Cards)
//     {
//         if (card != null) Destroy(card.gameObject);
//     }

//     foreach (Card card in player2Cards)
//     {
//         if (card != null) Destroy(card.gameObject);
//     }

//     // Clear the card lists
//     player1Cards.Clear();
//     player2Cards.Clear();

//     // Shuffle and redistribute cards
//     ShuffleAndDistribute();
// }

// void ClearCentralPile()
//     {
//         foreach (Card card in centralPile)
//         {
//             Destroy(card.gameObject);
//         }
//         centralPile.Clear();
//     }
// }
