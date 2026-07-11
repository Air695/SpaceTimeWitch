using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using SpaceTimeWitch.Extension.MCJ;
using SpaceTimeWitch.Relics;

namespace SpaceTimeWitch.Commands;

public static class DiscoverCmd
{
    /// <summary>
    /// 从匹配当前标签遗物所代表标签的卡牌中发现。
    /// </summary>
    /// <param name="cardType">null 表示不限类型</param>
    /// <returns>玩家选中的卡牌列表</returns>
    /// <summary>默认稀有度权重（编译期常量，用于方法签名默认值）。运行时实际读取 SpaceTimeWitchSettings。</summary>
    private static readonly IReadOnlyDictionary<CardRarity, double> DefaultRarityWeights =
        new Dictionary<CardRarity, double>
        {
            { CardRarity.Common,   0.60 },
            { CardRarity.Uncommon, 0.30 },
            { CardRarity.Rare,     0.10 },
        };

    /// <summary>从 SpaceTimeWitchSettings 读取当前配置的稀有度权重字典。</summary>
    private static IReadOnlyDictionary<CardRarity, double> GetConfiguredRarityWeights()
    {
        var (cw, uw, rw) = Scripts.SpaceTimeWitchSettings.GetConfiguredWeights();
        return new Dictionary<CardRarity, double>
        {
            { CardRarity.Common,   cw },
            { CardRarity.Uncommon, uw },
            { CardRarity.Rare,     rw },
        };
    }

    public static async Task<IEnumerable<CardModel>> Discover(
    PlayerChoiceContext ctx,
    Player player,
    CardType? cardType,
    int offerCount,
    int minCount,
    int maxCount,
    LocString prompt,
    Func<CardModel, bool>? extraFilter = null,
    IReadOnlyDictionary<CardRarity, double>? rarityWeights = null,
    bool sourceIsUpgraded = false)
{
    // 读取当前活动标签
    var activeTags = player.Relics
        .OfType<ITagRelic>()
        .Select(r => r.AssociatedTag)
        .ToHashSet();

    if (activeTags.Count == 0) return [];

    // 同时收集镐类遗物的卡牌绑定类型名（跳过已永久禁用的槽位）
    var pickaxeTypeNames = player.Relics
        .OfType<IPickaxeRelic>()
        .SelectMany(p =>
        {
            var bindings = PickaxeCardBindings.Deserialize(p.GetCardBindingsData());
            bool shieldDisabled = ShieldCraftSubMenu.IsSlotDisabled(p);
            return bindings.AllSlots
                .Where((t, i) => t != null && !(i == 5 && shieldDisabled))
                .Select(t => t!.Name);
        })
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    // 筛选匹配的卡（标签 + 镐类绑定类名）
    var rng = player.RunState.Rng.Niche;
    var pool = ModelDb.AllCardPools
        .SelectMany(p => p.GetUnlockedCards(
            player.UnlockState,
            player.RunState.CardMultiplayerConstraint))
        .Where(c => c.CanBeGeneratedInCombat
                    && (c.Tags.Any(t => activeTags.Contains(t))
                        || pickaxeTypeNames.Contains(c.GetType().Name))
                    && (cardType == null || c.Type == cardType)
                    && (extraFilter == null || extraFilter(c)))
        .DistinctBy(c => c.Id)
        .ToList();

    if (pool.Count == 0) return [];

    // 按稀有度加权随机选 N 张，不足时从剩余池补足
    var weights = rarityWeights ?? GetConfiguredRarityWeights();
    var shuffled = pool.OrderBy(_ => rng.NextInt()).ToList();
    var byRarity = shuffled.GroupBy(c => c.Rarity).ToList();

    var weighted = new List<CardModel>();
    foreach (var g in byRarity)
    {
        var w = weights.TryGetValue(g.Key, out var v) ? v : 0;
        var take = (int)Math.Round(offerCount * w);
        weighted.AddRange(g.Take(take));
    }

    // 各稀有度配额可能凑不满 offerCount，从剩余卡中补齐
    var remaining = shuffled.Except(weighted).OrderBy(_ => rng.NextInt());
    weighted.AddRange(remaining.Take(offerCount - weighted.Count));

    var choices = weighted
        .Take(offerCount)
        .OrderBy(_ => rng.NextInt())
        .Select(c => player.Creature.CombatState.CreateCard(c, player))
        .ToList();

    if (sourceIsUpgraded)
    {
        foreach (var card in choices)
            CardCmd.Upgrade(card);
    }

    var chosen = await CardSelectCmd.FromSimpleGrid(ctx, choices, player,
        new CardSelectorPrefs(prompt, minCount: minCount, maxCount: maxCount));

    return chosen;
}
}
