namespace SpaceTimeWitch.Scripts;

public static class SpaceTimeWitchSettings
{
    // ==================== 概率控制 ====================
    // UI 由 NProbabilitySettingsControl（AddCustom）负责。
    // 以下静态属性供 WeightedCardSelectCmd 等运行时代码读取。

    public const int DefaultCommonWeight = 60;
    public const int DefaultUncommonWeight = 30;
    public const int DefaultRareWeight = 10;

    // ==================== 复现配置 ====================
    public const int DefaultDiscoverOfferCount = 5;

    private static int _discoverOfferCount = DefaultDiscoverOfferCount;

    public static int DiscoverOfferCount => _discoverOfferCount;

    public static void SyncDiscoverOfferCount(int count)
    {
        _discoverOfferCount = count;
    }

    private static int _commonWeight = DefaultCommonWeight;
    private static int _uncommonWeight = DefaultUncommonWeight;
    private static int _rareWeight = DefaultRareWeight;
    private static LockTarget _lockedRarity = LockTarget.Rare;

    public static int CommonWeight => _commonWeight;
    public static int UncommonWeight => _uncommonWeight;
    public static int RareWeight => _rareWeight;
    public static LockTarget LockedRarity => _lockedRarity;

    /// <summary>由 NProbabilitySettingsControl 调用，将 UI 变动同步到静态属性。</summary>
    public static void SyncFrom(int common, int uncommon, int rare, LockTarget locked)
    {
        _commonWeight = common;
        _uncommonWeight = uncommon;
        _rareWeight = rare;
        _lockedRarity = locked;
    }

    /// <summary>将百分比整数转为 WeightedCardSelectCmd 所用的 0–1 范围的 double 权重。</summary>
    public static (double common, double uncommon, double rare) GetConfiguredWeights()
    {
        return (
            CommonWeight / 100.0,
            UncommonWeight / 100.0,
            RareWeight / 100.0
        );
    }

    // ==================== 链接时空卡池配置 ====================
    // 以下字典由 NTagRelicPoolSettingsControl 在保存时同步，供 TagRelicRegistry 运行时读取。

    public static Dictionary<string, int> TagRelicGroupWeights { get; private set; } = new()
    {
        ["EldenRing"] = 1, ["DevilMayCry"] = 1, ["MineCraft"] = 1, ["LibraryOfRuina"] = 1,
    };

    public static Dictionary<string, bool> TagRelicGroupDedup { get; private set; } = new()
    {
        ["EldenRing"] = false, ["DevilMayCry"] = false, ["MineCraft"] = false, ["LibraryOfRuina"] = false,
    };

    public static Dictionary<string, int> TagRelicWeights { get; private set; } = [];

    public static Dictionary<string, int> TagRelicBranchWeights { get; private set; } = [];

    public static Dictionary<string, bool> TagRelicClassDedup { get; private set; } = new()
    {
        ["Attack"] = true, ["Debuff"] = true, ["Adaptive"] = true, ["Pickaxe"] = true,
    };

    /// <summary>由 NTagRelicPoolSettingsControl 调用，将 UI 变动同步到静态属性。</summary>
    public static void SyncTagRelicFrom(
        Dictionary<string, int> groupWeights,
        Dictionary<string, bool> groupDedup,
        Dictionary<string, int> weights,
        Dictionary<string, int> branchWeights,
        Dictionary<string, bool> classDedup)
    {
        TagRelicGroupWeights = new Dictionary<string, int>(groupWeights);
        TagRelicGroupDedup = new Dictionary<string, bool>(groupDedup);
        TagRelicWeights = new Dictionary<string, int>(weights);
        TagRelicBranchWeights = new Dictionary<string, int>(branchWeights);
        TagRelicClassDedup = new Dictionary<string, bool>(classDedup);
    }
}
