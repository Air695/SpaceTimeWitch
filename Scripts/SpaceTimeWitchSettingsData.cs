namespace SpaceTimeWitch.Scripts;

public enum LockTarget
{
    Common,
    Uncommon,
    Rare
}

/// <summary>
/// 模组设置数据模型，由 ModDataStore 持久化。
/// </summary>
public sealed class SpaceTimeWitchSettingsData
{
    // 概率控制
    public int CommonWeight { get; set; } = SpaceTimeWitchSettings.DefaultCommonWeight;
    public int UncommonWeight { get; set; } = SpaceTimeWitchSettings.DefaultUncommonWeight;
    public int RareWeight { get; set; } = SpaceTimeWitchSettings.DefaultRareWeight;
    public LockTarget LockedRarity { get; set; } = LockTarget.Rare;

    // 复现配置
    public int DiscoverOfferCount { get; set; } = SpaceTimeWitchSettings.DefaultDiscoverOfferCount;

    // ==================== 链接时空卡池配置 ====================
    /// <summary>卡池组权重，Key=组名（如 EldenRing），Value=权重（0–5）。</summary>
    public Dictionary<string, int> TagRelicGroupWeights { get; set; } = new()
    {
        ["EldenRing"] = 1,
        ["DevilMayCry"] = 1,
        ["MineCraft"] = 1,
        ["LibraryOfRuina"] = 1,
    };

    /// <summary>组去重开关，Key=组名，Value=是否去重（仅抽一个）。</summary>
    public Dictionary<string, bool> TagRelicGroupDedup { get; set; } = new()
    {
        ["EldenRing"] = false,
        ["DevilMayCry"] = false,
        ["MineCraft"] = false,
        ["LibraryOfRuina"] = false,
    };

    /// <summary>遗物出现权重，Key=遗物类型名（如 ERZ1），Value=权重（0–5）。</summary>
    public Dictionary<string, int> TagRelicWeights { get; set; } = [];

    /// <summary>升级分支权重，Key="父类型名:子类型名"，Value=权重（0–5）。</summary>
    public Dictionary<string, int> TagRelicBranchWeights { get; set; } = [];

    /// <summary>标签去重开关，Key=标签名（如 Attack），Value=是否去重。</summary>
    public Dictionary<string, bool> TagRelicClassDedup { get; set; } = new()
    {
        ["Attack"] = true,
        ["Debuff"] = true,
        ["Adaptive"] = true,
        ["Pickaxe"] = true,
    };
}
