using MegaCrit.Sts2.Core.Entities.Cards;

namespace SpaceTimeWitch.Extension.QG;

/// <summary>
/// EGO卡牌标记接口。EGO卡牌被选取后置入玩家的手牌/牌组。
/// </summary>
public interface IEGOCard
{
    /// <summary>
    /// 该EGO绑定的标签。
    /// 抽取时用于与遗物的 AbnormalityTags 匹配。
    /// </summary>
    CardTag Tag { get; }
}
