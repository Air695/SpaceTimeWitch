namespace SpaceTimeWitch.Nodes;

/// <summary>
/// 标签遗物卡池设置页本地化文本容器。
/// 在 Entry.cs 中通过 ModSettingsText.LocString 填充，传入 NTagRelicPoolSettingsControl。
/// </summary>
public sealed class TagRelicPoolText
{
    /// <summary>卡池权重滑条标签</summary>
    public string GroupWeightLabel { get; init; } = "链接卡池权重";

    /// <summary>组去重开关标签</summary>
    public string GroupDedupLabel { get; init; } = "去重（此组每次只抽一个遗物）";

    /// <summary>重置全部为默认值</summary>
    public string ResetAllLabel { get; init; } = "重置全部为默认值";

    /// <summary>升级分支权重标签</summary>
    public string BranchWeightFormat { get; init; } = "分支权重";

    /// <summary>标签去重 Section 标题</summary>
    public string ClassDedupTitle { get; init; } = "标签去重";

    /// <summary>标签去重描述</summary>
    public string ClassDedupDesc { get; init; } = "不会抽到重复的启用标签";

    /// <summary>重置全部去重</summary>
    public string ResetDedupLabel { get; init; } = "重置全部去重为默认";

    /// <summary>遗物名称解析器，传入 Type 返回本地化名称。未设置则使用 Type.Name。</summary>
    public Func<Type, string>? RelicNameResolver { get; init; }

    /// <summary>切换开关 ON 文本</summary>
    public string ToggleOnLabel { get; init; } = "ON";

    /// <summary>切换开关 OFF 文本</summary>
    public string ToggleOffLabel { get; init; } = "OFF";
}
