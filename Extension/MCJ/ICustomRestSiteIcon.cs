using MegaCrit.Sts2.Core.Localization;

namespace SpaceTimeWitch.Extension.MCJ;

/// <summary>
/// 实现此接口的 RestSiteOption 可自定义标题和启用状态。
/// 图标由基类自动从 res://images/ui/rest_site/option_{OptionId}.png 加载。
/// </summary>
public interface ICustomRestSiteIcon
{
    LocString CustomTitle { get; }
    bool CustomIsEnabled { get; }
    string CustomIconPath => ""; // 默认空=使用基类图标，覆写可指定卡牌肖像等
}
