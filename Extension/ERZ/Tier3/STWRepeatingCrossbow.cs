using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using SpaceTimeWitch.Cards;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.CombatSkill;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Extension.ERZ.Tier3;

[RegisterCard(typeof(SpaceTimeWitchExCardPool))]
public class STWRepeatingCrossbow : CombatSkillCard
{
    public override CombatSkillActionData GetActionData() => new()
    {
        CoreAction = CombatSkillCoreAction.StrikeRandom,
        HitCount = 4
    };

    protected override HashSet<CardTag> CanonicalTags =>
    [
        CardTags.CS,
        CardTags.ERZ3
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6m, ValueProp.Move)
    ];

    public STWRepeatingCrossbow()
        : base(baseCost: 1, type: CardType.Attack, rarity: CardRarity.Uncommon)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1m);
    }

    protected override string PortraitPath => $"res://images/Extension/Cards/{GetType().Name}.png";
}