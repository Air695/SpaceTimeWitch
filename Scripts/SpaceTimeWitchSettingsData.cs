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
}
