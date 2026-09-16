using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

public class CardManager : MonoBehaviour
{
    [SerializeField] private HandCard[] slots = new HandCard[10];
    [SerializeField, Range(0, 10)] private int startingCardCount;
    [SerializeField] private List<int> cardAddOrder = new List<int>
    {
        5, 6, 4, 7, 3, 8, 2, 9, 1, 10
    };
    [SerializeField] private UnityEvent<GameObject> onCardPlayed;

    // Init lists for keeping track of cards
    private Card[] drawPile;
    private Card[] discardPile;
    private Card[] handCards = new Card[6];     // Indexing of handCards should follow UI handcard slots
    private Card[] destroyedPile;
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

            // Take and remove the last card from the draw pile.
            if (drawPile == null || drawPile.Length == 0)
            {
                slots[slotIndex].gameObject.SetActive(false);
                Debug.LogWarning("Cannot draw a card: the draw pile is empty.", this);
                return;
            }

            int lastCardIndex = drawPile.Length - 1;
            Card lastCard = drawPile[lastCardIndex];
            Array.Resize(ref drawPile, lastCardIndex);
            AssignCardToSlot(slotIndex, lastCard);
            return;
        }

        Debug.LogWarning("Cannot add a card: cardAddOrder has no available valid slot.", this);
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
                return;
            }
        }
        Debug.LogError($"Couldn't find {playedCard.Title} from player's hand.");
    }

    public void StartTurnActions()
    {
        // Draw cards
        for (int i = 0; i < 4; i++)
        {
            DrawCard();
        }

        // Any effects??
        // Decrease statuses etc.
    }

    public void PrepareCardsForCombat()
    {
        // Set all card slots as not active
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
                slots[i].gameObject.SetActive(false);
        }

        // Init and shuffle draw pile
        InitializeDrawPile();
        Shuffle(drawPile);
    }

    // TODO: For now, initialize drawPile with the starter deck.
    // In later stage, this combat deck should be chosen by player from their current card collection
    private void InitializeDrawPile()
    {
        drawPile = new[]
        {
            CreateAttackCard(),
            CreateAttackCard(),
            CreateAttackCard(),
            CreateBlockCard(),
            CreateBlockCard(),
            CreateBlockCard(),
            new Card(
                "Quick Discard",
                "Draw 2 cards, then discard 1 card.",
                CreateAction(
                    new[] { CombatActionType.Draw, CombatActionType.Discard },
                    new[] { CombatEffectType.None, CombatEffectType.None },
                    new[] { 2, 1 })),
            new Card(
                "Quick Destroy",
                "Draw 2 cards, then destroy 1 card.",
                CreateAction(
                    new[] { CombatActionType.Draw, CombatActionType.Destroy },
                    new[] { CombatEffectType.None, CombatEffectType.None },
                    new[] { 2, 1 })),
            new Card(
                "Renew",
                "Draw 1 card and heal for 2.",
                CreateAction(
                    new[] { CombatActionType.Draw, CombatActionType.Heal },
                    new[] { CombatEffectType.None, CombatEffectType.None },
                    new[] { 1, 2 })),
            new Card(
                "Follow-up",
                "Attack for 2, then draw 1 card.",
                CreateAction(
                    new[] { CombatActionType.Attack, CombatActionType.Draw },
                    new[] { CombatEffectType.None, CombatEffectType.None },
                    new[] { 2, 1 }))
        };
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

    private void Shuffle(Card[] cards)
    {
        for (int i = cards.Length - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);

            Card temp = cards[i];
            cards[i] = cards[j];
            cards[j] = temp;
        }
    }
}
