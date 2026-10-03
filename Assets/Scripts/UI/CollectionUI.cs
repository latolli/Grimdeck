using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class CollectionUI : MonoBehaviour
{
    [SerializeField] private CardCollection cardCollection;

    private const float MaximumCardWidth = 215f;
    private const float CardAspectRatio = 120f / 85f;
    private const float ScrollbarWidth = 24f;

    private RectTransform content;
    private RectTransform viewport;
    private GridLayoutGroup cardGrid;
    private ScrollRect cardScrollRect;
    private TextMeshProUGUI collectionCount;
    private GameObject generatedUI;
    private bool started;
    private float lastViewportWidth = -1f;
    private float lastViewportHeight = -1f;

    private void OnEnable()
    {
        EnsureUI();
        if (started)
            RefreshCollection();
    }

    private void Start()
    {
        started = true;
        RefreshCollection();
    }

    private void LateUpdate()
    {
        UpdateGridColumns();
    }

    public void RefreshCollection()
    {
        EnsureUI();

        if (cardCollection == null)
            cardCollection = FindFirstObjectByType<CardCollection>();

        if (cardCollection == null)
        {
            Debug.LogError("CollectionUI requires a CardCollection in the scene.", this);
            collectionCount.text = "Collection unavailable";
            ClearCards();
            return;
        }

        ClearCards();
        if (cardCollection.collection == null)
        {
            Debug.LogError("CardCollection has not initialized its card list.", cardCollection);
            collectionCount.text = "Collection unavailable";
            return;
        }

        collectionCount.text = $"Collection ({cardCollection.collection.Count})";

        foreach (Card card in cardCollection.collection)
        {
            if (card == null)
            {
                Debug.LogWarning("CardCollection contains a null card; skipping it.", cardCollection);
                continue;
            }

            CreateCardView(card);
        }

        Canvas.ForceUpdateCanvases();
        UpdateGridColumns();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }

    private void OnRectTransformDimensionsChange()
    {
        UpdateGridColumns();
        if (content != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }

    private void EnsureUI()
    {
        if (generatedUI != null)
            return;

        generatedUI = new GameObject("CollectionPage", typeof(RectTransform), typeof(Image));
        generatedUI.transform.SetParent(transform, false);

        RectTransform pageRect = generatedUI.GetComponent<RectTransform>();
        pageRect.anchorMin = Vector2.zero;
        pageRect.anchorMax = Vector2.one;
        pageRect.offsetMin = Vector2.zero;
        pageRect.offsetMax = Vector2.zero;

        Image pageBackground = generatedUI.GetComponent<Image>();
        pageBackground.color = new Color(0.08f, 0.07f, 0.1f, 0.96f);

        collectionCount = CreateText(
            "CollectionCount",
            generatedUI.transform,
            "Collection",
            20,
            TextAlignmentOptions.MidlineLeft);
        collectionCount.color = Color.white;
        RectTransform countRect = collectionCount.rectTransform;
        countRect.anchorMin = new Vector2(0.04f, 0.86f);
        countRect.anchorMax = new Vector2(0.96f, 0.98f);
        countRect.offsetMin = Vector2.zero;
        countRect.offsetMax = Vector2.zero;

        CreateScrollView(generatedUI.transform);
    }

    private void CreateScrollView(Transform parent)
    {
        GameObject scrollObject = new GameObject("CardScrollView", typeof(RectTransform), typeof(ScrollRect));
        scrollObject.transform.SetParent(parent, false);

        RectTransform scrollRectTransform = scrollObject.GetComponent<RectTransform>();
        scrollRectTransform.anchorMin = new Vector2(0.04f, 0.04f);
        scrollRectTransform.anchorMax = new Vector2(0.96f, 0.84f);
        scrollRectTransform.offsetMin = Vector2.zero;
        scrollRectTransform.offsetMax = Vector2.zero;

        GameObject viewportObject = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        viewportObject.transform.SetParent(scrollObject.transform, false);

        viewport = viewportObject.GetComponent<RectTransform>();
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = new Vector2(0f, 0f);
        viewport.offsetMax = new Vector2(-ScrollbarWidth - 2f, 0f);

        Image viewportImage = viewportObject.GetComponent<Image>();
        viewportImage.color = new Color(0.14f, 0.13f, 0.17f, 1f);
        viewportObject.GetComponent<Mask>().showMaskGraphic = false;

        GameObject contentObject = new GameObject(
            "Content",
            typeof(RectTransform),
            typeof(GridLayoutGroup),
            typeof(ContentSizeFitter));
        contentObject.transform.SetParent(viewportObject.transform, false);

        content = contentObject.GetComponent<RectTransform>();
        content.anchorMin = Vector2.up;
        content.anchorMax = Vector2.up;
        content.pivot = Vector2.up;
        content.anchoredPosition = Vector2.zero;

        cardGrid = contentObject.GetComponent<GridLayoutGroup>();
        cardGrid.spacing = new Vector2(16f, 16f);
        cardGrid.padding = new RectOffset(24, 24, 16, 16);
        cardGrid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        cardGrid.startAxis = GridLayoutGroup.Axis.Horizontal;
        cardGrid.childAlignment = TextAnchor.UpperLeft;
        cardGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        UpdateGridColumns();

        ContentSizeFitter fitter = contentObject.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        GameObject scrollbarObject = new GameObject("VerticalScrollbar", typeof(RectTransform), typeof(Image), typeof(Scrollbar));
        scrollbarObject.transform.SetParent(scrollObject.transform, false);

        RectTransform scrollbarRect = scrollbarObject.GetComponent<RectTransform>();
        scrollbarRect.anchorMin = new Vector2(1f, 0f);
        scrollbarRect.anchorMax = new Vector2(1f, 1f);
        scrollbarRect.offsetMin = new Vector2(-ScrollbarWidth, 0f);
        scrollbarRect.offsetMax = new Vector2(-2f, 0f);

        Image scrollbarTrack = scrollbarObject.GetComponent<Image>();
        scrollbarTrack.color = new Color(0.22f, 0.2f, 0.25f, 1f);

        GameObject handleObject = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handleObject.transform.SetParent(scrollbarObject.transform, false);
        RectTransform handleRect = handleObject.GetComponent<RectTransform>();
        handleRect.anchorMin = Vector2.zero;
        handleRect.anchorMax = Vector2.one;
        handleRect.offsetMin = Vector2.zero;
        handleRect.offsetMax = Vector2.zero;
        Image handleImage = handleObject.GetComponent<Image>();
        handleImage.color = new Color(0.72f, 0.68f, 0.78f, 1f);

        Scrollbar scrollbar = scrollbarObject.GetComponent<Scrollbar>();
        scrollbar.handleRect = handleRect;
        scrollbar.targetGraphic = handleImage;
        scrollbar.direction = Scrollbar.Direction.BottomToTop;

        cardScrollRect = scrollObject.GetComponent<ScrollRect>();
        cardScrollRect.viewport = viewport;
        cardScrollRect.content = content;
        cardScrollRect.horizontal = false;
        cardScrollRect.vertical = true;
        cardScrollRect.movementType = ScrollRect.MovementType.Clamped;
        cardScrollRect.verticalScrollbar = scrollbar;
        cardScrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
        cardScrollRect.verticalScrollbarSpacing = -2f;
    }

    private void UpdateGridColumns()
    {
        if (viewport == null || cardGrid == null)
            return;

        float viewportWidth = viewport.rect.width;
        float viewportHeight = viewport.rect.height;
        if (Mathf.Approximately(viewportWidth, lastViewportWidth) &&
            Mathf.Approximately(viewportHeight, lastViewportHeight))
            return;

        lastViewportWidth = viewportWidth;
        lastViewportHeight = viewportHeight;

        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, viewportWidth);
        Vector2 contentPosition = content.anchoredPosition;
        contentPosition.x = 0f;
        content.anchoredPosition = contentPosition;

        float availableWidth = Mathf.Max(1f, viewport.rect.width - cardGrid.padding.horizontal);
        int columns = Mathf.Max(
            1,
            Mathf.FloorToInt((availableWidth + cardGrid.spacing.x) /
                (MaximumCardWidth + cardGrid.spacing.x)));
        cardGrid.constraintCount = columns;

        float cellWidth = Mathf.Min(
            MaximumCardWidth,
            Mathf.Max(1f, (availableWidth - cardGrid.spacing.x * (columns - 1)) / columns));
        float availableHeight = Mathf.Max(
            1f,
            viewport.rect.height - cardGrid.padding.vertical - cardGrid.spacing.y);
        float cellHeight = Mathf.Min(cellWidth * CardAspectRatio, availableHeight);
        cardGrid.cellSize = new Vector2(cellWidth, cellHeight);

        collectionCount.fontSize = Mathf.Clamp(
            GetComponent<RectTransform>().rect.height * 0.1f,
            12f,
            30f);

        for (int i = 0; i < content.childCount; i++)
        {
            Transform cardTransform = content.GetChild(i);
            TextMeshProUGUI title = cardTransform.Find("Title")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI description = cardTransform.Find("Description")?.GetComponent<TextMeshProUGUI>();
            if (title != null)
                title.fontSize = Mathf.Clamp(cardGrid.cellSize.y * 0.1f, 16f, 26f);
            if (description != null)
                description.fontSize = Mathf.Clamp(cardGrid.cellSize.y * 0.075f, 12f, 20f);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }

    private void CreateCardView(Card card)
    {
        GameObject cardObject = new GameObject(card.Title, typeof(RectTransform), typeof(Image));
        cardObject.transform.SetParent(content, false);

        Image cardImage = cardObject.GetComponent<Image>();
        cardImage.sprite = GetCardTemplate(card);
        cardImage.color = cardImage.sprite == null ? new Color(0.84f, 0.82f, 0.78f) : Color.white;
        cardImage.raycastTarget = false;

        TextMeshProUGUI title = CreateText(
            "Title",
            cardObject.transform,
            card.Title,
            Mathf.Clamp(cardGrid.cellSize.y * 0.1f, 16f, 26f),
            TextAlignmentOptions.Center);
        SetTextArea(title.rectTransform, new Vector2(0.06f, 0.78f), new Vector2(0.94f, 0.98f));

        TextMeshProUGUI description = CreateText(
            "Description",
            cardObject.transform,
            card.Description,
            Mathf.Clamp(cardGrid.cellSize.y * 0.075f, 12f, 20f),
            TextAlignmentOptions.Center);
        SetTextArea(description.rectTransform, new Vector2(0.1f, 0.08f), new Vector2(0.9f, 0.5f));
    }

    private Sprite GetCardTemplate(Card card)
    {
        bool isBlockCard = card.Action != null &&
            card.Action.ActionTypes != null &&
            card.Action.ActionTypes.Length > 0 &&
            card.Action.ActionTypes[0] == CombatActionType.Block;

        string templateName = isBlockCard ? "defend_template" : "attack_template";
        Sprite template = Resources.Load<Sprite>(templateName);
        if (template == null)
            Debug.LogError($"Couldn't load card template '{templateName}' from Resources.", this);

        return template;
    }

    private static TextMeshProUGUI CreateText(
        string objectName,
        Transform parent,
        string text,
        float fontSize,
        TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        TextMeshProUGUI label = textObject.GetComponent<TextMeshProUGUI>();
        label.text = text ?? string.Empty;
        label.fontSize = fontSize;
        label.alignment = alignment;
        label.color = Color.black;
        label.raycastTarget = false;
        label.textWrappingMode = TextWrappingModes.Normal;
        label.overflowMode = TextOverflowModes.Truncate;
        TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Roboto_Customfont SDF");
        if (font == null)
            Debug.LogError("Couldn't load TMP font asset 'Roboto_Customfont SDF' from Resources.", parent);
        else
            label.font = font;

        return label;
    }

    private void ClearCards()
    {
        if (content == null)
            return;

        for (int i = content.childCount - 1; i >= 0; i--)
            Destroy(content.GetChild(i).gameObject);
    }

    private static void SetTextArea(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax)
    {
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
}
