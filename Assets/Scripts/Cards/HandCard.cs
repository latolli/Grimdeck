using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class HandCard : MonoBehaviour,
    IPointerClickHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    public UnityEvent<GameObject> onClicked;
    public UnityEvent<GameObject> onCardDropped;
    private Canvas rootCanvas;
    private bool wasDragged;
    private bool wasPlayed;
    private HandManager owner;
    private RectTransform canvasRect;
    private Image arrowShaft;
    private Image arrowHeadLeft;
    private Image arrowHeadRight;
    private Sprite arrowSprite;

    [SerializeField] private float arrowWidth = 6f;
    [SerializeField] private float arrowHeadLength = 24f;
    [SerializeField] private float arrowHeadAngle = 30f;

    public HandManager Owner => owner;

    public void SetOwner(HandManager HandManager)
    {
        owner = HandManager;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //if (wasDragged)
        //{
        //    wasDragged = false;
        //    return;
        //}

        //onClicked?.Invoke(gameObject);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        wasDragged = true;
        wasPlayed = false;
        rootCanvas = GetComponentInParent<Canvas>()?.rootCanvas;

        if (rootCanvas != null)
        {
            transform.SetParent(rootCanvas.transform, true);
            transform.SetAsLastSibling();
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null)
                canvasGroup.blocksRaycasts = false;

            CreateDragArrow();
            UpdateDragArrow(eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateDragArrow(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;

        SetArrowVisible(false);
        CombatActions combatActions = FindFirstObjectByType<CombatActions>();
        combatActions.CheckCardTarget(eventData.position);
        //onCardDropped?.Invoke(gameObject);
    }

    public void MarkPlayed()
    {
        wasPlayed = true;
    }

    private void CreateDragArrow()
    {
        if (arrowShaft != null)
            return;

        canvasRect = rootCanvas.transform as RectTransform;
        arrowSprite = Sprite.Create(
            Texture2D.whiteTexture,
            new Rect(0, 0, 1, 1),
            new Vector2(0.5f, 0.5f));

        arrowShaft = CreateArrowPart("DragArrowShaft");
        arrowHeadLeft = CreateArrowPart("DragArrowHeadLeft");
        arrowHeadRight = CreateArrowPart("DragArrowHeadRight");
        SetArrowVisible(false);
    }

    private Image CreateArrowPart(string partName)
    {
        GameObject part = new GameObject(partName, typeof(RectTransform), typeof(Image));
        part.transform.SetParent(canvasRect, false);
        part.transform.SetAsFirstSibling();

        Image image = part.GetComponent<Image>();
        image.sprite = arrowSprite;
        image.color = Color.white;
        image.raycastTarget = false;
        return image;
    }

    private void UpdateDragArrow(PointerEventData eventData)
    {
        if (rootCanvas == null)
            return;

        CreateDragArrow();

        Camera eventCamera = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : eventData.pressEventCamera;

        Vector2 mousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            eventData.position,
            eventCamera,
            out mousePosition);

        Vector2 cardScreenPosition = RectTransformUtility.WorldToScreenPoint(
            eventCamera,
            transform.position);
        Vector2 cardPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            cardScreenPosition,
            eventCamera,
            out cardPosition);

        Vector2 direction = mousePosition - cardPosition;
        if (direction.sqrMagnitude < 0.01f)
        {
            SetArrowVisible(false);
            return;
        }

        SetArrowVisible(true);
        SetArrowPart(arrowShaft, cardPosition, mousePosition, arrowWidth);

        Vector2 backwards = -direction.normalized;
        Vector2 left = mousePosition + Rotate(backwards, arrowHeadAngle) * arrowHeadLength;
        Vector2 right = mousePosition + Rotate(backwards, -arrowHeadAngle) * arrowHeadLength;
        SetArrowPart(arrowHeadLeft, mousePosition, left, arrowWidth);
        SetArrowPart(arrowHeadRight, mousePosition, right, arrowWidth);
    }

    private static Vector2 Rotate(Vector2 vector, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);
        return new Vector2(
            vector.x * cos - vector.y * sin,
            vector.x * sin + vector.y * cos);
    }

    private static void SetArrowPart(Image image, Vector2 start, Vector2 end, float width)
    {
        RectTransform rect = image.rectTransform;
        Vector2 delta = end - start;
        rect.anchoredPosition = (start + end) * 0.5f;
        rect.sizeDelta = new Vector2(delta.magnitude, width);
        rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
    }

    private void SetArrowVisible(bool visible)
    {
        if (arrowShaft != null)
            arrowShaft.gameObject.SetActive(visible);
        if (arrowHeadLeft != null)
            arrowHeadLeft.gameObject.SetActive(visible);
        if (arrowHeadRight != null)
            arrowHeadRight.gameObject.SetActive(visible);
    }
}