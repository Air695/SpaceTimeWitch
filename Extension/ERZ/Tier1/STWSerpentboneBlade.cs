using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using SpaceTimeWitch.Cards;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.CombatSkill;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Extension.ERZ.Tier1;

[RegisterCard(typeof(SpaceTimeWitchExCardPool))]
public class STWSerpentboneBlade : CombatSkillCard
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
        new DamageVar(7m, ValueProp.Move),
        new PowerVar<PoisonPower>(7m)
    ];
    
    public override async Task ApplyPowers(Creature target, PlayerChoiceContext ctx)
    {
        await PowerCmd.Apply<PoisonPower>(ctx, target,
            DynamicVars["PoisonPower"].BaseValue, Owner!.Creature!, this);
    }

    public STWSerpentboneBlade()
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
        DynamicVars["PoisonPower"].UpgradeValueBy(2m);
    }
    
    protected override string PortraitPath => $"res://images/Extension/Cards/{GetType().Name}.png";
}