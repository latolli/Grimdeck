using System.Text;
using UnityEngine;
using TMPro;

public class CombatStats
{
    public int maxHP;
    public int currentHP;
    public int currentBlock;
    public EffectStatus statusEffects;
    public bool isAlive;
}

[RequireComponent(typeof(CanvasGroup))]
public class CombatStatsPanel : MonoBehaviour
{
    private GameObject renderedStats;

    public void UpdateStats(CombatStats newStats)
    {
        RenderStats(newStats);
    }

    public void ResetStats()
    {
        if (renderedStats != null)
        {
            Destroy(renderedStats);
            renderedStats = null;
        }
    }

    private void RenderStats(CombatStats newStats)
    {
        if (renderedStats != null)
        {
            Destroy(renderedStats);
            renderedStats = null;
        }

        if (newStats == null)
        {
            return;
        }

        renderedStats = new GameObject("CombatStatsText");
        renderedStats.transform.SetParent(transform, false);

        RectTransform rectTransform = renderedStats.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        TextMeshProUGUI text = renderedStats.AddComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.fontSize = 22;
        text.color = Color.black;
        text.text = BuildStatsText(newStats);
    }

    private static string BuildStatsText(CombatStats combatStats)
    {
        StringBuilder text = new StringBuilder()
            .Append("HP: ")
            .Append(combatStats.currentHP)
            .Append(" / ")
            .Append(combatStats.maxHP)
            .Append(" | Block: ")
            .Append(combatStats.currentBlock);

        EffectStatus effects = combatStats.statusEffects;
        if (effects == null)
        {
            return text.ToString();
        }

        if (effects.Poisoned > 0 || effects.OnFire > 0 || effects.Weakened > 0)
        {
            text.Append('\n');
        }

        AppendStatus(text, " P", effects.Poisoned);
        AppendStatus(text, " F", effects.OnFire);
        AppendStatus(text, " W", effects.Weakened);
        return text.ToString();
    }

    private static void AppendStatus(StringBuilder text, string name, int turns)
    {
        if (turns > 0)
        {
            text.Append(name).Append(": ").Append(turns).Append(" ");
        }
    }
}