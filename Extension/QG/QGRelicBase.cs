using System.Linq;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using SpaceTimeWitch.Cards;
using SpaceTimeWitch.Relics;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interactions.RightClick;
using STS2RitsuLib.Scaffolding.Content;
using SpaceTimeWitch.Scripts;

namespace SpaceTimeWitch.Extension.QG;

/// <summary>
/// QG情感遗物基类。持有时启用情感系统，右键触发升级。
/// 升级逻辑集中在 EmotionSystem.TryUpgrade。
/// </summary>
public abstract class QGRelicBase : SpaceTimeWitchRelics, IQGRelic,
    IModRightClickableRelic, ISecondaryResourceHookListener
{
    protected QGRelicBase(RelicRarity rarity = RelicRarity.Event) : base(rarity) { }

    /// <summary>该遗物允许的最大情感等级</summary>
    public virtual int MaxRelicLevel => 3;
    public abstract IReadOnlyList<CardTag> AbnormalityTags { get; }

    public virtual CardTag Tier1Tag => (CardTag)((int)AbnormalityTags[0] + 1);
    public virtual CardTag Tier2Tag => (CardTag)((int)AbnormalityTags[0] + 2);
    public virtual CardTag Tier3Tag => (CardTag)((int)AbnormalityTags[0] + 3);
    public virtual CardTag EGOTag => (CardTag)((int)AbnormalityTags[0] + 4);

    public decimal ModifyMaxSecondaryResource(SecondaryResourceMaxContext context, decimal amount)
    {
        if (Owner == null) return amount;
        if (context.Definition.Id != QGResources.PositiveId &&
            context.Definition.Id != QGResources.NegativeId)
            return amount;
        return EmotionSystem.GetCap(EmotionSystem.GetLevel(Owner));
    }

    public override async Task BeforeCombatStartLate()
    {
        if (Owner != null)
        {
            await EmotionSystem.ResetForCombat(Owner);
            await QGEmotionAcquisition.EnsureApplied(Owner);
        }
    }

    /// <summary>右键 → 委托 EmotionSystem 执行升级</summary>
    public virtual async Task OnRightClick(ModRightClickExecutionContext context)
    {
        if (Owner == null) return;
        await EmotionSystem.TryUpgrade(Owner, context.PlayerChoiceContext!);
        Flash();
    }
}
