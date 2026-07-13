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

    // 筛选匹配的卡（标签 + 镐类绑定类名），按 ID 确定性排序防止多人不同步
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
        .OrderBy(c => c.Id)
        .ToList();

    if (pool.Count == 0) return [];

    // 使用战斗共享 RNG 洗牌一次（synced across players）
    var rng = player.RunState.Rng.CombatCardGeneration;
    var shuffled = pool.OrderBy(_ => rng.NextInt()).ToList();

    // 按稀有度加权取卡
    var weights = rarityWeights ?? GetConfiguredRarityWeights();
    var byRarity = new Dictionary<CardRarity, List<CardModel>>();
    foreach (var c in shuffled)
    {
        if (!byRarity.ContainsKey(c.Rarity))
            byRarity[c.Rarity] = new List<CardModel>();
        byRarity[c.Rarity].Add(c);
    }

    var weighted = new List<CardModel>();
    foreach (var kv in byRarity)
    {
        var w = weights.TryGetValue(kv.Key, out var v) ? v : 0;
        var take = (int)Math.Round(offerCount * w);
        weighted.AddRange(kv.Value.Take(take));
    }

    // 不足时从剩余卡中补齐
    if (weighted.Count < offerCount)
    {
        var remaining = shuffled.Except(weighted);
        weighted.AddRange(remaining.Take(offerCount - weighted.Count));
    }

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
