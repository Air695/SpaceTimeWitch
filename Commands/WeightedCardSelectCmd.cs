using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;

namespace SpaceTimeWitch.Commands;

public static class WeightedCardSelectCmd
{
    public const double DefaultCommonWeight = 0.60;
    public const double DefaultUncommonWeight = 0.30;
    public const double DefaultRareWeight = 0.10;

    /// <summary>
    /// 从 SpaceTimeWitchSettings 读取玩家配置的稀有度权重（0-1 范围）。
    /// </summary>
    public static (double common, double uncommon, double rare) GetConfiguredWeights()
    {
        return Scripts.SpaceTimeWitchSettings.GetConfiguredWeights();
    }

    /// <summary>
    /// 从指定卡池中按稀有度权重生成卡牌，让玩家至多选择一张。
    /// </summary>
    public static async Task<CardModel?> PickFromPools(
        PlayerChoiceContext context,
        Player player,
        IEnumerable<CardPoolModel> pools,
        int offerCount = 5,
        LocString? prompt = null,
        Rng? rng = null,
        double commonWeight = DefaultCommonWeight,
        double uncommonWeight = DefaultUncommonWeight,
        double rareWeight = DefaultRareWeight,
        bool upgradeOffered = false)
    {
        IEnumerable<CardModel> allCards = pools
            .SelectMany(p => p.GetUnlockedCards(
                player.UnlockState,
                player.RunState.CardMultiplayerConstraint))
            .Distinct();

        return await PickFromCards(context, player, allCards,
            offerCount, prompt, rng,
            commonWeight, uncommonWeight, rareWeight,
            upgradeOffered);
    }

    /// <summary>
    /// 从给定的卡牌集合中按稀有度权重生成卡牌，让玩家至多选择一张。
    /// </summary>
    public static async Task<CardModel?> PickFromCards(
        PlayerChoiceContext context,
        Player player,
        IEnumerable<CardModel> cardPool,
        int offerCount = 5,
        LocString? prompt = null,
        Rng? rng = null,
        double commonWeight = DefaultCommonWeight,
        double uncommonWeight = DefaultUncommonWeight,
        double rareWeight = DefaultRareWeight,
        bool upgradeOffered = false)
    {
        Rng resolvedRng = rng ?? player.RunState.Rng.CombatCardGeneration;

        List<CardModel> offered = GenerateWeighted(
            player, cardPool, offerCount,
            commonWeight, uncommonWeight, rareWeight,
            resolvedRng);

        if (offered.Count == 0)
            return null;

        if (upgradeOffered)
        {
            foreach (var c in offered)
                CardCmd.Upgrade(c);
        }

        var prefs = new CardSelectorPrefs(
            prompt ?? new LocString("card_selection", "TO_SELECT"),
            0, 1);

        IEnumerable<CardModel> selected = await CardSelectCmd.FromSimpleGrid(
            context, offered, player, prefs);

        return selected.FirstOrDefault();
    }

    /// <summary>
    /// 按稀有度权重生成指定数量的战斗卡牌实例。
    /// </summary>
    public static List<CardModel> GenerateWeighted(
        Player player,
        IEnumerable<CardModel> cardPool,
        int count,
        double commonWeight,
        double uncommonWeight,
        double rareWeight,
        Rng rng)
    {
        List<CardModel> cards = cardPool
            .Where(c => player.RunState.Players.Count > 1
                ? c.MultiplayerConstraint != CardMultiplayerConstraint.SingleplayerOnly
                : c.MultiplayerConstraint != CardMultiplayerConstraint.MultiplayerOnly)
            .Where(c => c.CanBeGeneratedInCombat
                && c.Rarity != CardRarity.Basic
                && c.Rarity != CardRarity.Ancient
                && c.Rarity != CardRarity.Event)
            .Distinct()
            .ToList();

        if (cards.Count == 0)
            return new List<CardModel>();

        var byRarity = new Dictionary<CardRarity, List<CardModel>>();
        foreach (CardModel c in cards)
        {
            if (!byRarity.ContainsKey(c.Rarity))
                byRarity[c.Rarity] = new List<CardModel>();
            byRarity[c.Rarity].Add(c);
        }

        double totalWeight = 0.0;
        if (byRarity.ContainsKey(CardRarity.Common)) totalWeight += commonWeight;
        if (byRarity.ContainsKey(CardRarity.Uncommon)) totalWeight += uncommonWeight;
        if (byRarity.ContainsKey(CardRarity.Rare)) totalWeight += rareWeight;

        if (totalWeight == 0.0)
            return new List<CardModel>();

        double normCommon = byRarity.ContainsKey(CardRarity.Common)
            ? commonWeight / totalWeight : 0.0;
        double normUncommon = byRarity.ContainsKey(CardRarity.Uncommon)
            ? uncommonWeight / totalWeight : 0.0;

        var results = new List<CardModel>();
        var used = new HashSet<CardModel>();

        for (int i = 0; i < count && i < cards.Count; i++)
        {
            CardRarity rolled = RollRarity(rng,
                normCommon, normCommon + normUncommon, 1.0);

            CardModel? picked = null;
            CardRarity current = rolled;
            while (current != CardRarity.None)
            {
                if (byRarity.TryGetValue(current, out var list))
                {
                    var available = list
                        .Where(c => !used.Contains(c))
                        .ToList();
                    if (available.Count > 0)
                    {
                        picked = rng.NextItem(available);
                        break;
                    }
                }
                current = current switch
                {
                    CardRarity.Common => CardRarity.Uncommon,
                    CardRarity.Uncommon => CardRarity.Rare,
                    _ => CardRarity.None
                };
            }

            if (picked == null)
            {
                var remaining = cards
                    .Where(c => !used.Contains(c))
                    .ToList();
                if (remaining.Count == 0)
                    break;
                picked = rng.NextItem(remaining);
            }

            used.Add(picked);
            results.Add(player.Creature.CombatState.CreateCard(picked, player));
        }

        return results;
    }

    private static CardRarity RollRarity(Rng rng,
        double commonThreshold,
        double uncommonThreshold,
        double rareThreshold)
    {
        double roll = rng.NextFloat();
        if (roll < commonThreshold)
            return CardRarity.Common;
        if (roll < uncommonThreshold)
            return CardRarity.Uncommon;
        return CardRarity.Rare;
    }
}
