using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CardHand : MonoBehaviour
{
    [SerializeField] private HandCard[] slots = new HandCard[10];
    [SerializeField, Range(0, 10)] private int startingCardCount;
    [SerializeField] private List<int> cardAddOrder = new List<int>
    {
        5, 6, 4, 7, 3, 8, 2, 9, 1, 10
    };
    [SerializeField] private UnityEvent<GameObject> onCardPlayed;

    public int CardCount { get; private set; }
    public int MaxCards => slots.Length;

    private void Awake()
    {
        if (slots == null || slots.Length == 0)
        {
            Debug.LogError("CardHand needs at least one HandCard slot.", this);
            return;
        }

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
            {
                Debug.LogError($"CardHand slot {i} is not assigned.", this);
                continue;
            }

            int slotIndex = i;
            slots[i].SetOwner(this);
            slots[i].onClicked.AddListener(_ => PlayCard(slotIndex));
        }

        SetCardCount(startingCardCount);
    }

    public void SetCardCount(int count)
    {
        CardCount = Mathf.Clamp(count, 0, slots.Length);

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
                slots[i].gameObject.SetActive(false);
        }

        int cardsToActivate = CardCount;
        foreach (int slotNumber in cardAddOrder)
        {
            int slotIndex = slotNumber - 1;
            if (slotIndex < 0 || slotIndex >= slots.Length || slots[slotIndex] == null)
                continue;

            slots[slotIndex].gameObject.SetActive(true);
            cardsToActivate--;
            if (cardsToActivate == 0)
                break;
        }
    }

    public void AddCard()
    {
        if (CardCount >= slots.Length)
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

            slots[slotIndex].gameObject.SetActive(true);
            CardCount++;
            return;
        }

        Debug.LogWarning("Cannot add a card: cardAddOrder has no available valid slot.", this);
    }

    public void PlayCard(HandCard card)
    {
        if (card == null)
            return;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != card)
                continue;

            PlayCard(i);
            return;
        }

        Debug.LogWarning($"Card {card.name} does not belong to this hand.", this);
    }

    public void PlayCard(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length)
            return;

        HandCard card = slots[slotIndex];
        if (card == null || !card.gameObject.activeSelf)
            return;

        card.MarkPlayed();
        card.gameObject.SetActive(false);
        CardCount--;
        onCardPlayed?.Invoke(card.gameObject);
    }
}
