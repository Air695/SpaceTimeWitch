using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Random;
using SpaceTimeWitch.Cards;
using SpaceTimeWitch.Extension.DCB;
using SpaceTimeWitch.Extension.ERZ;
using SpaceTimeWitch.Extension.LRL;
using SpaceTimeWitch.Extension.MCJ;
using SpaceTimeWitch.Scripts;

namespace SpaceTimeWitch.Relics;

/// <summary>
/// 标签遗物元数据，集中定义所有 TagRelic 的 CardTag、角色组、等级、子分类、下一级、权重。
/// 新增标签遗物时在此添加一行即可，无需在每个遗物类中写构造函数。
/// </summary>
public record TagRelicData(
    CardTag Tag,
    string Group,
    int Tier,
    string Class = "",
    IReadOnlyList<Type> NextTierTypes = null,
    double Weight = 1.0,
    IReadOnlyDictionary<Type, double> NextTierWeights = null
);

/// <summary>
/// 标签遗物注册表 —— 所有标签遗物元数据的唯一入口。
/// 格式: CardTags.xxx group:"xxx" tier:x class:"xxx" nextTierTypes:[xxx, yyy] weight:1.0
/// </summary>
public static class TagRelicRegistry
{
    public static readonly IReadOnlyDictionary<Type, TagRelicData> Entries =
        new Dictionary<Type, TagRelicData>
        {
            [typeof(ERZ1)] = new(CardTags.ERZ1, "Elden", 1, "J",
                NextTierWeights: new Dictionary<Type, double> {
                    [typeof(ERZ2)] = 1.0,
                }),
            [typeof(ERZ2)] = new(CardTags.ERZ2, "Elden", 2, "J", [typeof(ERZ3)]),
            [typeof(ERZ3)] = new(CardTags.ERZ3, "Elden", 3, "J"),
            [typeof(DCB1)] = new(CardTags.DCB1, "Devil", 1, "A", [typeof(DCB2)]),
            [typeof(DCB2)] = new(CardTags.DCB2, "Devil", 2, "A", [typeof(DCB3)]),
            [typeof(DCB3)] = new(CardTags.DCB3, "Devil", 3, "A"),
            [typeof(STWWoodenPickaxe)] = new(CardTags.MCJ, "MC", 1, "Pickaxe"),
            [typeof(STWStonePickaxe)] = new(CardTags.MCJ, "MC", 2, "Pickaxe"),
            [typeof(STWIronPickaxe)] = new(CardTags.MCJ, "MC", 3, "Pickaxe"),
            [typeof(STWDiamondPickaxe)] = new(CardTags.MCJ, "MC", 4, "Pickaxe"),
            [typeof(STWNetheritePickaxe)] = new(CardTags.MCJ, "MC", 5, "Pickaxe"),
            [typeof(STWBookofPierre)] = new(CardTags.LRL1, "LOR", 1,"J",[typeof(STWBookofSayo)]),
            [typeof(STWBookofSayo)] = new(CardTags.LRL2, "LOR", 2,"J",[typeof(STWBookofDonghwan)]),
            [typeof(STWBookofDonghwan)] = new(CardTags.LRL3, "LOR", 3,"J"),
        };

    /// <summary>角色池权重（Group → 权重），默认 1。</summary>
    public static readonly IReadOnlyDictionary<string, double> GroupWeights =
        new Dictionary<string, double>
        {
            ["Elden"] = 1.0,
            ["Devil"] = 1.0,
            ["MC"] = 1.0,
            ["LOR"] = 10.0,
        };

    /// <summary>限制 class：在此集合中的 class 至多获取 1 个遗物。</summary>
    public static readonly HashSet<string> LimitedClasses = new HashSet<string>
    {
        "A"
    };

    /// <summary>加权随机选取，总权重为 0 时返回 default。</summary>
    public static T? WeightedPick<T>(IEnumerable<T> items, Func<T, double> weightSelector, Rng rng)
    {
        var list = items.ToList();
        if (list.Count == 0) return default;
        var total = list.Sum(w => Math.Max(0, weightSelector(w)));
        if (total <= 0) return list[rng.NextInt(list.Count)];
        var roll = rng.NextDouble() * total;
        var cumulative = 0.0;
        foreach (var item in list)
        {
            cumulative += Math.Max(0, weightSelector(item));
            if (roll <= cumulative) return item;
        }
        return list[^1];
    }
}

public interface ITagRelic
{
    CardTag AssociatedTag { get; }
    string CharacterGroup { get; }
    string Class { get; }
    int Tier { get; }
    IReadOnlyList<Type> NextTierRelicTypes { get; }
    double Weight { get; }
    IReadOnlyDictionary<Type, double> NextTierWeights { get; }
}

/// <summary>
/// 标签遗物抽象基类。子类无需构造函数，元数据在 <see cref="TagRelicRegistry"/> 中集中定义。
/// 示例: public class STWRF1 : TagRelic { /* 遗物效果 */ }
/// </summary>
public abstract class TagRelic() : SpaceTimeWitchRelics(RelicRarity.Event), ITagRelic
{
    private TagRelicData? _data;
    private TagRelicData Data => _data ??= TagRelicRegistry.Entries[GetType()];

    public CardTag AssociatedTag => Data.Tag;
    public string CharacterGroup => Data.Group;
    public string Class => Data.Class;
    public int Tier => Data.Tier;
    public IReadOnlyList<Type> NextTierRelicTypes => Data.NextTierTypes ?? [];
    public double Weight => Data.Weight;
    public IReadOnlyDictionary<Type, double> NextTierWeights => Data.NextTierWeights ?? new Dictionary<Type, double>();
}
