public class Card
{
    public string Title { get; }
    public string Description { get; }
    public CombatAction Action { get; }

    public Card(string title, string description, CombatAction action)
    {
        Title = title;
        Description = description;
        Action = action;
    }
}