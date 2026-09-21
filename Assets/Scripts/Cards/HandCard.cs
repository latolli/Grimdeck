using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class HandCard : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    private Canvas rootCanvas;
    private RectTransform canvasRect;
    private Image arrowShaft;
    private Image arrowHeadLeft;
    private Image arrowHeadRight;
    private Sprite arrowSprite;

    [SerializeField] private float arrowWidth = 6f;
    [SerializeField] private float arrowHeadLength = 24f;
    [SerializeField] private float arrowHeadAngle = 30f;

    private CardManager owner;
    private Card card;
    private GameObject renderedCard;

    public Card Card => card;
    public CardManager Owner => owner;

    public void SetOwner(CardManager cardManager)
    {
        owner = cardManager;
    }

    public void SetCard(Card cardData)
    {
        card = cardData;
        RenderCard(cardData);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        rootCanvas = GetComponentInParent<Canvas>()?.rootCanvas;

        if (rootCanvas != null)
        {
            transform.SetParent(rootCanvas.transform, true);
            transform.SetAsLastSibling();
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null)
                canvasGroup.blocksRaycasts = false;

            CreateDragArrow();
            SetArrowAsLastSibling();
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
        PlayerCombatHandler PlayerCombatHandler = FindFirstObjectByType<PlayerCombatHandler>();
        PlayerCombatHandler.CheckCardTarget(this.card, eventData.position);
    }

    private void RenderCard(Card card)
    {
        if (renderedCard != null)
        {
            Destroy(renderedCard);
            renderedCard = null;
        }

        if (card == null)
            return;

        // Check if card is attack or defend type
        string templateName = "attack_template";
        if (card.Action != null &&
            card.Action.ActionTypes != null &&
            card.Action.ActionTypes.Length > 0 &&
            card.Action.ActionTypes[0] == CombatActionType.Block)
        {
            templateName = "defend_template";
        }

        // Load the image as Sprite
        Sprite templateSprite = Resources.Load<Sprite>(templateName);
        if (templateSprite == null)
        {
            Debug.LogError($"Couldn't load card template '{templateName}' from Resources.", this);
            return;
        }

        // Create new game object
        renderedCard = new GameObject(card.Title, typeof(RectTransform));
        renderedCard.transform.SetParent(transform, false);
        renderedCard.transform.SetAsFirstSibling();

        RectTransform cardRect = renderedCard.GetComponent<RectTransform>();
        cardRect.anchorMin = Vector2.zero;
        cardRect.anchorMax = Vector2.one;
        cardRect.offsetMin = Vector2.zero;
        cardRect.offsetMax = Vector2.zero;

        // Add the image to the gameobject
        Image cardTemplate = renderedCard.AddComponent<Image>();
        cardTemplate.sprite = templateSprite;
        cardTemplate.type = Image.Type.Simple;
        cardTemplate.preserveAspect = false;
        cardTemplate.raycastTarget = false;

        CreateCardText(
            "Title",
            card.Title,
            cardRect,
            12,
            new Vector2(0f, 0.8f),
            new Vector2(1f, 1f));

        CreateCardText(
            "Description",
            card.Description,
            cardRect,
            11,
            new Vector2(0.1f, 0.1f),
            new Vector2(0.9f, 0.5f));
    }

    private static void CreateCardText(
    string objectName,
    string text,
    RectTransform parent,
    int fontSize,
    Vector2 anchorMin,
    Vector2 anchorMax)
    {
        GameObject textObject = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(TextMeshProUGUI));

        textObject.transform.SetParent(parent, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = anchorMin;
        textRect.anchorMax = anchorMax;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI cardText = textObject.GetComponent<TextMeshProUGUI>();
        cardText.text = text ?? string.Empty;
        cardText.fontSize = fontSize;
        cardText.alignment = TextAlignmentOptions.Center;
        cardText.color = Color.black;
        cardText.raycastTarget = false;
        cardText.textWrappingMode = TextWrappingModes.Normal;
        cardText.overflowMode = TextOverflowModes.Truncate;

        TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Roboto_Customfont SDF");
        if (font == null)
        {
            Debug.LogError(
                "Couldn't load TMP font asset 'Roboto_Customfont SDF' from Resources.",
                parent);
            return;
        }

        cardText.font = font;
        textObject.transform.SetAsLastSibling();
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
        part.transform.SetAsLastSibling();

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

    private void SetArrowAsLastSibling()
    {
        if (arrowShaft != null)
            arrowShaft.transform.SetAsLastSibling();
        if (arrowHeadLeft != null)
            arrowHeadLeft.transform.SetAsLastSibling();
        if (arrowHeadRight != null)
            arrowHeadRight.transform.SetAsLastSibling();
    }
}