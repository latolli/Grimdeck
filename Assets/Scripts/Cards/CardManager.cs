using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

public class CardManager : MonoBehaviour
{
    [SerializeField] private HandCard[] slots = new HandCard[6];
    [SerializeField, Range(0, 6)] private int startingCardCount;
    [SerializeField] private List<int> cardAddOrder = new List<int>
    {
        5, 6, 4, 7, 3, 8, 2, 9, 1, 10
    };
    [SerializeField] private UnityEvent<GameObject> onCardPlayed;

    // Init lists for keeping track of cards
    //private Card[] drawPile;
    private List<Card> drawPile;
    private List<Card> discardPile;
    private Card[] handCards = new Card[6];     // Indexing of handCards should follow UI handcard slots
    private List<Card> destroyedPile;
    private int cardsInHand;

    public int CardCount { get; private set; }
    public int MaxCards => slots.Length;

    private void Awake()
    {
        if (slots == null || slots.Length == 0)
        {
            Debug.LogError("CardManager needs at least one HandCard slot.", this);
            return;
        }

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
            {
                Debug.LogError($"CardManager slot {i} is not assigned.", this);
                continue;
            }
            // Set every slot as not active and assign owner
            slots[i].gameObject.SetActive(false);
            slots[i].SetOwner(this);
        }
    }

    public void DrawCard()
    {
        if (cardsInHand >= slots.Length)
        {
            Debug.LogWarning("Cannot add a card: the hand is full.", this);
            return;
        }

        foreach (int slotNumber in cardAddOrder)
        {
            int slotIndex = slotNumber - 1;
            if (slotIndex < 0 || slotIndex >= slots.Length ||
                slots[slotIndex] == null || slots[slotIndex].gameObject.activeSelf)
                continue;

            // Check if resuffle is needed
            if (drawPile.Count == 0)
            {
                drawPile = discardPile;
                discardPile = new List<Card>();
                Shuffle(drawPile);
            }

            if (drawPile.Count > 0)
            {
                int lastCardIndex = drawPile.Count - 1;
                Card lastCard = drawPile[lastCardIndex];
                drawPile.RemoveAt(lastCardIndex);
                AssignCardToSlot(slotIndex, lastCard);
                return;
            }
        }

        Debug.LogWarning("No more cards or suitable slots.", this);
    }

    // Add card to handCard list and UI slot
    private void AssignCardToSlot(int slotIndex, Card card)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length)
        {
            Debug.LogError($"Invalid card slot index: {slotIndex}", this);
            return;
        }

        if (slots[slotIndex] == null)
        {
            Debug.LogError($"Card slot {slotIndex} is not assigned.", this);
            return;
        }

        // Add to hand
        slots[slotIndex].gameObject.SetActive(true);
        slots[slotIndex].SetCard(card);
        handCards[slotIndex] = card;
        cardsInHand++;
    }

    public void PlayCard(Card playedCard)
    {
        // Check which card was played and free its slot
        for (int i = 0; i < slots.Length; i++)
        {
            if (playedCard == handCards[i])
            {
                // Remove card from slot
                slots[i].gameObject.SetActive(false);
                handCards[i] = null;
                cardsInHand--;
                discardPile.Add(playedCard);
                break;
            }
        }

        // Play deck related card effects
        CombatAction action = playedCard.Action;
        for (int i = 0; i < action.ActionTypes.Length; i++)
        {
            CombatActionType actionType = action.ActionTypes[i];
            if (actionType == CombatActionType.Draw)
            {
                for (int draw = 0; draw < action.ActionValues[i]; draw++)
                {
                    DrawCard();  
                }
            }
            else if (actionType == CombatActionType.Discard)
            {
                // TODO:
                Debug.Log("Discarding card...");
            }
            else if (actionType == CombatActionType.Destroy)
            {
                // TODO:
                Debug.Log("Destroying card...");
            }
        }
    }

    public void StartTurnActions()
    {
        // TODO: Something very weird happening and handslots decrease after first combat????
        // Draw cards
        for (int i = 0; i < 4; i++)
        {
            DrawCard();
        }

        // Any effects??
        // Decrease statuses etc.
    }

    public void PrepareCardsForCombat(bool start)
    {
        // Set all card slots as not active
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
            {
                slots[i].gameObject.SetActive(false);
                handCards[i] = null;
            }
        }

        if (start == true)
        {
            // Init and shuffle draw pile
            InitializeCardPiles();
            Shuffle(drawPile);
        }
    }

    public void DiscardCards(int discardAmount)
    {
        // Check if this is discard all situation
        if (discardAmount >= cardsInHand)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (handCards[i] != null)
                {
                    // Remove card from slot
                    discardPile.Add(handCards[i]);
                    slots[i].gameObject.SetActive(false);
                    handCards[i] = null;
                    cardsInHand--;
                }
            }
        }
        // If not, let player choose
        // TODO:
    }

    private void InitializeCardPiles()
    {
        // Init piles to empty lists
        drawPile = new List<Card>();
        discardPile = new List<Card>();
        destroyedPile = new List<Card>();

        // Make dummy deck for now
        for (int i = 0; i < 3; i++)
        {
            drawPile.Add(CreateAttackCard());
        }
        for (int i = 0; i < 3; i++)
        {
            drawPile.Add(CreateBlockCard());
        }
        drawPile.Add(new Card(
                "Quick Discard",
                "Draw 2 cards, then discard 1 card.",
                CreateAction(
                    new[] { CombatActionType.Draw, CombatActionType.Discard },
                    new[] { CombatEffectType.None, CombatEffectType.None },
                    new[] { 2, 1 })));
        drawPile.Add(new Card(
                "Quick Destroy",
                "Draw 2 cards, then destroy 1 card.",
                CreateAction(
                    new[] { CombatActionType.Draw, CombatActionType.Destroy },
                    new[] { CombatEffectType.None, CombatEffectType.None },
                    new[] { 2, 1 })));
        drawPile.Add(new Card(
                "Renew",
                "Draw 1 card and heal for 2.",
                CreateAction(
                    new[] { CombatActionType.Draw, CombatActionType.Heal },
                    new[] { CombatEffectType.None, CombatEffectType.None },
                    new[] { 1, 2 })));
        drawPile.Add(new Card(
                "Follow-up",
                "Attack for 2, then draw 1 card.",
                CreateAction(
                    new[] { CombatActionType.Attack, CombatActionType.Draw },
                    new[] { CombatEffectType.None, CombatEffectType.None },
                    new[] { 2, 1 })));
        drawPile.Add(new Card(
                "Poison Stab",
                "Attack for 2, apply 2 poison.",
                CreateAction(
                    new[] { CombatActionType.Attack, CombatActionType.ApplyDebuff },
                    new[] { CombatEffectType.None, CombatEffectType.Poison },
                    new[] { 2, 2 })));
        drawPile.Add(new Card(
                "Fire Bash",
                "Attack for 2, apply 3 fire.",
                CreateAction(
                    new[] { CombatActionType.Attack, CombatActionType.ApplyDebuff },
                    new[] { CombatEffectType.None, CombatEffectType.Fire },
                    new[] { 2, 3 })));
    }

    private static Card CreateAttackCard()
    {
        return new Card(
            "Strike",
            "Attack for 4 damage.",
            CreateAction(
                new[] { CombatActionType.Attack },
                new[] { CombatEffectType.None },
                new[] { 4 }));
    }

    private static Card CreateBlockCard()
    {
        return new Card(
            "Guard",
            "Gain 3 block.",
            CreateAction(
                new[] { CombatActionType.Block },
                new[] { CombatEffectType.None },
                new[] { 3 }));
    }

    private static CombatAction CreateAction(
        CombatActionType[] actionTypes,
        CombatEffectType[] combatEffectTypes,
        int[] actionValues)
    {
        CombatAction action = new CombatAction();
        action.ActionTypes = actionTypes;
        action.CombatEffectTypes = combatEffectTypes;
        action.ActionValues = actionValues;
        return action;
    }

    private void Shuffle(List<Card> cards)
    {
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);

            Card temp = cards[i];
            cards[i] = cards[j];
            cards[j] = temp;
        }
    }
}
