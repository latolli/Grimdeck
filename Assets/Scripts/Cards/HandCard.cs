using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

[RequireComponent(typeof(CanvasGroup))]
public class HandCard : MonoBehaviour,
    IPointerClickHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    public UnityEvent<GameObject> onClicked;
    public UnityEvent<GameObject> onCardDropped;

    private Transform originalParent;
    private Vector3 originalPosition;
    private Canvas rootCanvas;
    private bool wasDragged;
    private bool wasPlayed;
    private CardHand owner;

    public CardHand Owner => owner;

    public void SetOwner(CardHand cardHand)
    {
        owner = cardHand;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (wasDragged)
        {
            wasDragged = false;
            return;
        }

        onClicked?.Invoke(gameObject);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        wasDragged = true;
        wasPlayed = false;
        originalParent = transform.parent;
        originalPosition = transform.position;
        rootCanvas = GetComponentInParent<Canvas>()?.rootCanvas;

        if (rootCanvas != null)
        {
            transform.SetParent(rootCanvas.transform, true);
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null)
                canvasGroup.blocksRaycasts = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;

        if (!wasPlayed)
            RestoreToHand();

        onCardDropped?.Invoke(gameObject);
    }

    public void MarkPlayed()
    {
        wasPlayed = true;
    }

    private void RestoreToHand()
    {
        if (originalParent == null)
            return;

        transform.SetParent(originalParent, false);
        transform.position = originalPosition;
    }
}