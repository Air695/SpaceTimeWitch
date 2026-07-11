using MegaCrit.Sts2.Core.Entities.Cards;

namespace SpaceTimeWitch.Extension.QG;

/// <summary>
/// QG遗物接口。持有此接口的遗物启用情感系统。
/// </summary>
public interface IQGRelic
{
    /// <summary>
    /// 该遗物绑定的异想体标签列表。
    /// 第一个元素应为 <see cref="Cards.CardTags.WXC"/>，后续为具体异想体标识标签。
    /// </summary>
    IReadOnlyList<CardTag> AbnormalityTags { get; }
}
