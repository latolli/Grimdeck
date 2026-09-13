using UnityEngine;
using UnityEngine.EventSystems;

public class CardPlayArea : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;

        HandCard card = eventData.pointerDrag.GetComponent<HandCard>();
        if (card == null)
            return;

        CardHand hand = card.Owner;
        if (hand == null)
        {
            Debug.LogWarning($"Card {card.name} is not inside a CardHand.", card);
            return;
        }

        hand.PlayCard(card);
    }
}
