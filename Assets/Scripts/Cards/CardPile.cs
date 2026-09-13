using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public enum PileType { DrawPile, DiscardPile }

public class CardPile : MonoBehaviour, IPointerClickHandler
{
    public PileType pileType;
    public UnityEvent<GameObject> onClicked;

    public void OnPointerClick(PointerEventData eventData)
    {
        GameObject clickedCard = eventData.pointerClick;
        Debug.Log("Clicked: " + clickedCard);
        onClicked?.Invoke(gameObject);
    }
}