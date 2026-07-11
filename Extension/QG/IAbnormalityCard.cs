using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace SpaceTimeWitch.Extension.QG;

/// <summary>
/// 异想体卡牌接口。异想体卡牌带有情感区间，被选取后立即生效并施加对应能力。
/// </summary>
public interface IAbnormalityCard
{
    /// <summary>
    /// 该异想体对应的情感区间（-2 ~ 2）。
    /// 用于抽取时按距离排序。
    /// </summary>
    int Interval { get; }

    /// <summary>
    /// 该异想体绑定的标签。
    /// 抽取时用于与遗物的 AbnormalityTags 匹配。
    /// </summary>
    CardTag Tag { get; }

    /// <summary>
    /// 选取后立即生效，施加该异想体对应的能力。
    /// </summary>
    Task ApplyEffect(PlayerChoiceContext ctx, Player player);
}
