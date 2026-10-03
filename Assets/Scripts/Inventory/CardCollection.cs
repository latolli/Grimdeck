using System.Collections.Generic;
using System;
using UnityEngine;

public class CardCollection : MonoBehaviour
{
    public List<Card> collection = new List<Card>();

    private void Awake()
    {
        InitializeCollection();
    }

    public void AddCardToCollection(Card card)
    {
        collection.Add(card);
    }

    public void RemoveCardFromCollection(Card card)
    {
        for(int i = 0; i < collection.Count; i++)
        {
            if (collection[i] == card)
            {
                collection.RemoveAt(i);
                return;
            }
        }
    }

    // TEMP CODE TO CREATE DUMMY COLLECTION ->
    private void InitializeCollection()
    {
        // Init piles to empty lists
        collection = new List<Card>();

        // Make dummy collection for now
        for (int i = 0; i < 3; i++)
        {
            collection.Add(CreateAttackCard());
        }
        for (int i = 0; i < 3; i++)
        {
            collection.Add(CreateBlockCard());
        }
        collection.Add(new Card(
                "Quick Discard",
                "Draw 2, discard 2.",
                CreateAction(
                    new[] { CombatActionType.Draw, CombatActionType.Discard },
                    new[] { CombatEffectType.None, CombatEffectType.None },
                    new[] { 2, 2 })));
        collection.Add(new Card(
                "Quick Destroy",
                "Draw 2 cards, then destroy 1 card.",
                CreateAction(
                    new[] { CombatActionType.Draw, CombatActionType.Destroy },
                    new[] { CombatEffectType.None, CombatEffectType.None },
                    new[] { 2, 1 })));
        collection.Add(new Card(
                "Renew",
                "Draw 1 card and heal for 2.",
                CreateAction(
                    new[] { CombatActionType.Draw, CombatActionType.Heal },
                    new[] { CombatEffectType.None, CombatEffectType.None },
                    new[] { 1, 2 })));
        collection.Add(new Card(
                "Follow-up",
                "Attack for 2, then draw 1 card.",
                CreateAction(
                    new[] { CombatActionType.Attack, CombatActionType.Draw },
                    new[] { CombatEffectType.None, CombatEffectType.None },
                    new[] { 2, 1 })));
        collection.Add(new Card(
                "Poison Stab",
                "Attack for 2, apply 2 poison.",
                CreateAction(
                    new[] { CombatActionType.Attack, CombatActionType.ApplyDebuff },
                    new[] { CombatEffectType.None, CombatEffectType.Poison },
                    new[] { 2, 2 })));
        collection.Add(new Card(
                "Fire Bash",
                "Attack for 2, apply 3 fire.",
                CreateAction(
                    new[] { CombatActionType.Attack, CombatActionType.ApplyDebuff },
                    new[] { CombatEffectType.None, CombatEffectType.Fire },
                    new[] { 2, 3 })));
        collection.Add(new Card(
                "Prepared",
                "Draw 2, destroy 2",
                CreateAction(
                    new[] { CombatActionType.Draw, CombatActionType.Destroy },
                    new[] { CombatEffectType.None, CombatEffectType.None },
                    new[] { 2, 2 })));
    }

    private static Card CreateAttackCard()
    {
        return new Card(
            "Strike",
            "Attack for 4 damage.",
            CreateAction(
                new[] { CombatActionType.Attack },
                new[] { CombatEffectType.None },
                new[] { 4 }));
    }

    private static Card CreateBlockCard()
    {
        return new Card(
            "Guard",
            "Gain 3 block.",
            CreateAction(
                new[] { CombatActionType.Block },
                new[] { CombatEffectType.None },
                new[] { 3 }));
    }

    private static CombatAction CreateAction(
        CombatActionType[] actionTypes,
        CombatEffectType[] combatEffectTypes,
        int[] actionValues)
    {
        CombatAction action = new CombatAction();
        action.ActionTypes = actionTypes;
        action.CombatEffectTypes = combatEffectTypes;
        action.ActionValues = actionValues;
        return action;
    }
}