using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public enum ZoneType { DrawPile, DiscardPile, Hand, PlayArea }

public class CardZone : MonoBehaviour, IPointerClickHandler, IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public ZoneType zoneType;

    public UnityEvent<GameObject> onClicked;
    public UnityEvent<GameObject> onCardDropped;

    public void OnPointerClick(PointerEventData eventData)
    {
        GameObject clickedCard = eventData.pointerClick;
        Debug.Log("Clicked: " + clickedCard);
        onClicked?.Invoke(gameObject);
    }

    // TODO: Implement drage effects for HAND cards only!!
    // Not sure if separate script would be better
    // Drag = some aiming method like arrow, player can choose the targety
    // Release = play card effect to the target
    public void OnBeginDrag(PointerEventData eventData) { }
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position; // follow cursor
    }
    public void OnEndDrag(PointerEventData eventData) { }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedCard = eventData.pointerDrag;
        Debug.Log("Drag and dropped: " + droppedCard);
        onCardDropped?.Invoke(droppedCard);
    }
}