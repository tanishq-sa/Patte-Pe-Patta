using UnityEngine;

public class Card : MonoBehaviour
{
    private Sprite frontSprite;
    private Sprite backSprite;
    private bool isFaceUp = false;
    private SpriteRenderer spriteRenderer;

    public int playerNumber;
    public CardManager cardManager;

    // Get the name of the front sprite (used for rank matching)
    public string FrontSpriteName => frontSprite != null ? frontSprite.name : "";

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetCard(Sprite front, Sprite back)
    {
        frontSprite = front;
        backSprite = back;
        spriteRenderer.sprite = backSprite;
        isFaceUp = false;
    }

    public void FlipCard()
    {
        isFaceUp = !isFaceUp;
        spriteRenderer.sprite = isFaceUp ? frontSprite : backSprite;
    }

    void OnMouseDown()
    {
        if (cardManager != null)
        {
            cardManager.OnCardPileClicked(playerNumber);
        }
    }
}