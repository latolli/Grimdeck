using System.Text;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class CardPileUI : MonoBehaviour
{
    private GameObject renderedText;

    public void UpdatePileUI(int deckSize)
    {
        RenderPileUI($"{deckSize}");
    }

    public void ResetPileUI()
    {
        if (renderedText != null)
        {
            Destroy(renderedText);
            renderedText = null;
        }
    }

    private void RenderPileUI(string text)
    {
        if (renderedText != null)
        {
            Destroy(renderedText);
            renderedText = null;
        }

        renderedText = new GameObject("CardPileUI");
        renderedText.transform.SetParent(transform, false);

        RectTransform rectTransform = renderedText.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.1f, 0.65f);
        rectTransform.anchorMax = new Vector2(0.9f, 0.85f);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        TextMeshProUGUI displayText = renderedText.AddComponent<TextMeshProUGUI>();
        displayText.alignment = TextAlignmentOptions.Center;
        displayText.textWrappingMode = TextWrappingModes.Normal;
        displayText.fontSize = 36;
        displayText.color = Color.black;
        displayText.text = text ?? string.Empty;
    }
}