using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using SpaceTimeWitch.Nodes;
using SpaceTimeWitch.Scripts;

namespace SpaceTimeWitch.Commands;

public static class PersonalSpaceCmd
{
    private const int MaxHandSize = 10;
    private const int MaxRetry = 5;

    public static async Task Store(Player player, CardModel card)
    {
        if (player.PlayerCombatState == null) return;

        // 如果卡牌当前不在弃牌堆，先移入弃牌堆清除手牌/抽牌堆的视觉残留
        if (card.Pile?.Type != PileType.Discard)
        {
            await CardPileCmd.Add(card, PileType.Discard);
        }

        // 从弃牌堆（或原本就在弃牌堆）移入个人空间
        await CardPileCmd.Add(card, Entry.PersonalSpacePile, skipVisuals: true);
        Entry.PersonalSpacePile.GetPile(player).InvokeCardAddFinished();

        PersonalSpaceManager.BumpVersion();
    }

    public static async Task Retrieve(Player player, CardModel card)
    {
        if (player.PlayerCombatState == null) return;

        var hand = player.PlayerCombatState.Hand;
        if (hand.Cards.Count < MaxHandSize)
            await CardPileCmd.Add(card, PileType.Hand);
        else
            await CardPileCmd.Add(card, PileType.Discard);

        PersonalSpaceManager.BumpVersion();
    }

    public static IReadOnlyList<CardModel> GetCards(Player player)
    {
        return Entry.PersonalSpacePile.GetPile(player).Cards;
    }

    public static int GetCount(Player player)
    {
        return Entry.PersonalSpacePile.GetPile(player).Cards.Count;
    }

    public static async Task<IEnumerable<CardModel>> SelectFromPersonalSpace(
        PlayerChoiceContext ctx, Player player,
        LocString prompt, int minCount, int maxCount)
    {
        for (int retry = 0; retry < MaxRetry; retry++)
        {
            var versionBefore = PersonalSpaceManager.Version;
            var cards = GetCards(player).ToList();
            if (cards.Count == 0) return Enumerable.Empty<CardModel>();

            var selected = await CardSelectCmd.FromSimpleGrid(
                ctx, cards, player,
                new CardSelectorPrefs(prompt, minCount, maxCount));

            if (versionBefore == PersonalSpaceManager.Version)
                return selected;
        }

        return Enumerable.Empty<CardModel>();
    }
}
