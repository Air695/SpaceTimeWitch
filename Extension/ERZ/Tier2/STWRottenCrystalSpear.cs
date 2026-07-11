using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using SpaceTimeWitch.Cards;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.CombatSkill;
using SpaceTimeWitch.Extension.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Extension.ERZ.Tier2;

[RegisterCard(typeof(SpaceTimeWitchExCardPool))]
public class STWRottenCrystalSpear : CombatSkillCard
{
    public override CombatSkillActionData GetActionData() => new()
    {
        CoreAction = CombatSkillCoreAction.Strike
    };

    protected override HashSet<CardTag> CanonicalTags =>
    [
        CardTags.CS,
        CardTags.ERZ2
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(9m, ValueProp.Move),
        new PowerVar<STWScarletRot>(9m)
    ];
    public override async Task ApplyPowers(Creature target, PlayerChoiceContext ctx)
    {
        await PowerCmd.Apply<STWScarletRot>(ctx, target,
            DynamicVars["STWScarletRot"].BaseValue, Owner!.Creature!, this);
    }

    public STWRottenCrystalSpear()
        : base(
            baseCost: 1,
            type: CardType.Attack,
            rarity: CardRarity.Rare
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
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars["STWScarletRot"].UpgradeValueBy(2m);
    }
    
    protected override string PortraitPath => $"res://images/Extension/Cards/{GetType().Name}.png";
}