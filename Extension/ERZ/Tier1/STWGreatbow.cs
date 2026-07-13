using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using SpaceTimeWitch.Cards;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.CombatSkill;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Extension.ERZ.Tier1;

[RegisterCard(typeof(SpaceTimeWitchExCardPool))]
public class STWGreatbow : CombatSkillCard
{
    public override CombatSkillActionData GetActionData() => new()
    {
        CoreAction = CombatSkillCoreAction.Strike
    };

    protected override HashSet<CardTag> CanonicalTags =>
    [
        CardTags.CS,
        CardTags.ERZ1
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(20m, ValueProp.Move)
    ];

    public STWGreatbow()
        : base(
            baseCost: 2,
            type: CardType.Attack,
            rarity: CardRarity.Common
        )
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await base.OnPlay(ctx, play);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
    
    protected override string PortraitPath => $"res://images/Extension/Cards/{GetType().Name}.png";
}