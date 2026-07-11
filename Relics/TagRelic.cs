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
/// 标签遗物配置 —— 从 SpaceTimeWitchSettings 读取运行时配置，未配置则回退到注册表默认值。
/// 配置页（NTagRelicPoolSettingsControl）在保存时调用 SpaceTimeWitchSettings.SyncTagRelicFrom() 同步到此。
/// </summary>
public static class TagRelicConfig
{
    /// <summary>获取卡池组的有效权重（0–5）。</summary>
    public static double GetEffectiveGroupWeight(string group) =>
        SpaceTimeWitchSettings.TagRelicGroupWeights.TryGetValue(group, out var w)
            ? w
            : TagRelicRegistry.GroupWeights.GetValueOrDefault(group, 1.0);

    /// <summary>获取遗物出现权重（0–5）。Key 为遗物类型名。</summary>
    public static double GetEffectiveRelicWeight(Type relicType) =>
        SpaceTimeWitchSettings.TagRelicWeights.TryGetValue(relicType.Name, out var w)
            ? w
            : (TagRelicRegistry.Entries.TryGetValue(relicType, out var data) ? data.Weight : 1.0);

    /// <summary>获取升级分支权重（0–5）。Key 格式 "父类型名:子类型名"。</summary>
    public static double GetEffectiveBranchWeight(Type parent, Type child)
    {
        var key = $"{parent.Name}:{child.Name}";
        if (SpaceTimeWitchSettings.TagRelicBranchWeights.TryGetValue(key, out var w))
            return w;
        if (TagRelicRegistry.Entries.TryGetValue(parent, out var data)
            && data.NextTierWeights != null
            && data.NextTierWeights.TryGetValue(child, out var dw))
            return dw;
        return 1.0;
    }

    /// <summary>获取指定 class 的去重开关。</summary>
    public static bool IsClassLimited(string className) =>
        SpaceTimeWitchSettings.TagRelicClassDedup.TryGetValue(className, out var enabled)
            ? enabled
            : TagRelicRegistry.LimitedClasses.Contains(className);

    /// <summary>获取组去重开关。</summary>
    public static bool IsGroupDedupEnabled(string group) =>
        SpaceTimeWitchSettings.TagRelicGroupDedup.TryGetValue(group, out var enabled) && enabled;

    /// <summary>获取所有启用了去重的 class 集合，供运行时快速查找。</summary>
    public static HashSet<string> GetActiveLimitedClasses()
    {
        var set = new HashSet<string>();
        // 从设置中收集启用的
        foreach (var (className, enabled) in SpaceTimeWitchSettings.TagRelicClassDedup)
            if (enabled) set.Add(className);
        // 回退到注册表默认（设置中不存在的 key）
        foreach (var className in TagRelicRegistry.LimitedClasses)
            if (!SpaceTimeWitchSettings.TagRelicClassDedup.ContainsKey(className))
                set.Add(className);
        return set;
    }

    /// <summary>获取指定组的有效 NextTierWeights（融合设置覆盖与注册表默认）。</summary>
    public static Dictionary<Type, double> GetEffectiveNextTierWeights(Type relicType)
    {
        if (!TagRelicRegistry.Entries.TryGetValue(relicType, out var data))
            return [];
        var result = new Dictionary<Type, double>();
        if (data.NextTierWeights != null)
        {
            foreach (var (childType, defaultWeight) in data.NextTierWeights)
            {
                var key = $"{relicType.Name}:{childType.Name}";
                result[childType] = SpaceTimeWitchSettings.TagRelicBranchWeights.TryGetValue(key, out var w)
                    ? w
                    : defaultWeight;
            }
        }
        return result;
    }
}

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
            [typeof(ERZ1)] = new(CardTags.ERZ1, "EldenRing", 1, "Adaptive",
                NextTierTypes: [typeof(ERZ2)]),
            [typeof(ERZ2)] = new(CardTags.ERZ2, "EldenRing", 2, "Adaptive", [typeof(ERZ3)]),
            [typeof(ERZ3)] = new(CardTags.ERZ3, "EldenRing", 3, "Adaptive"),
            [typeof(DCB1)] = new(CardTags.DCB1, "DevilMayCry", 1, "Attack", [typeof(DCB2)]),
            [typeof(DCB2)] = new(CardTags.DCB2, "DevilMayCry", 2, "Attack", [typeof(DCB3)]),
            [typeof(DCB3)] = new(CardTags.DCB3, "DevilMayCry", 3, "Attack"),
            [typeof(STWWoodenPickaxe)] = new(CardTags.MCJ, "MineCraft", 1, "Pickaxe"),
            [typeof(STWStonePickaxe)] = new(CardTags.MCJ, "MineCraft", 2, "Pickaxe"),
            [typeof(STWIronPickaxe)] = new(CardTags.MCJ, "MineCraft", 3, "Pickaxe"),
            [typeof(STWDiamondPickaxe)] = new(CardTags.MCJ, "MineCraft", 4, "Pickaxe"),
            [typeof(STWNetheritePickaxe)] = new(CardTags.MCJ, "MineCraft", 5, "Pickaxe"),
            [typeof(STWBookofPierre)] = new(CardTags.LRL1, "LibraryOfRuina", 1,"Debuff",[typeof(STWBookofSayo)]),
            [typeof(STWBookofSayo)] = new(CardTags.LRL2, "LibraryOfRuina", 2,"Debuff",[typeof(STWBookofDonghwan)]),
            [typeof(STWBookofDonghwan)] = new(CardTags.LRL3, "LibraryOfRuina", 3,"Debuff"),
        };

    /// <summary>角色池权重（Group → 权重），默认 1。</summary>
    public static readonly IReadOnlyDictionary<string, double> GroupWeights =
        new Dictionary<string, double>
        {
            ["EldenRing"] = 1.0,
            ["DevilMayCry"] = 1.0,
            ["MineCraft"] = 1.0,
            ["LibraryOfRuina"] = 1.0,
        };

    /// <summary>限制 class：在此集合中的 class 至多获取 1 个遗物。</summary>
    public static readonly HashSet<string> LimitedClasses = new HashSet<string>
    {
        "Attack",
        "Debuff"
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
/// 权重和分支权重会先查配置（TagRelicConfig），未配置则回退到注册表默认值。
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

    /// <summary>出现权重：先查配置，未配置则回退到注册表默认。</summary>
    public double Weight => TagRelicConfig.GetEffectiveRelicWeight(GetType());

    /// <summary>升级分支权重：先查配置，未配置则回退到注册表默认。</summary>
    public IReadOnlyDictionary<Type, double> NextTierWeights =>
        TagRelicConfig.GetEffectiveNextTierWeights(GetType());
}
