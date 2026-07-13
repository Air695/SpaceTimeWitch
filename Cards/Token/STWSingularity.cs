using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Saves.Runs;
using SpaceTimeWitch.Character;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Cards.Token;

[RegisterCard(typeof(SpaceTimeWitchCardPool))]
public class STWSingularity : SpaceTimeWitchCards
{
    private int _remainingUses;

    [SavedProperty]
    public int RemainingUses
    {
        get => _remainingUses;
        set { AssertMutable(); _remainingUses = value; }
    }

    protected override HashSet<CardTag> CanonicalTags => [CardTags.Reproduce];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1),new StarsVar(1)];

    public STWSingularity()
        : base(
            baseCost: -2,
            type: CardType.Skill,
            rarity: CardRarity.Token,
            target: TargetType.None
        )
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Retain,
        CardKeyword.Unplayable
    ];

    // 悬浮提示：显示剩余次数（替代 PowerVar<STWSingularity>，避免始终显示 0 及不必要的能力提示）
    protected override IEnumerable<IHoverTip> CardSpecificHoverTips
    {
        get
        {
            var usesVar = new IntVar("Uses", RemainingUses);
            var desc = new LocString("cards", "STW_SINGULARITY_USES");
            desc.Add(usesVar);
            yield return new HoverTip(
                new LocString("cards", "STW_SINGULARITY_USES_TITLE"),
                desc);
        }
    }

    // 无限升级：每次升级次数 +1
    public override int MaxUpgradeLevel => int.MaxValue;

    protected override void OnUpgrade()
    {
        // 次数 +1；能力层数由 STWSingularityPower 在回合开始时从卡牌同步
        RemainingUses++;
    }
}
