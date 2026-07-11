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

namespace SpaceTimeWitch.Extension.ERZ.Tier3;

[RegisterCard(typeof(SpaceTimeWitchExCardPool))]
public class STWCommanderStandard : CombatSkillCard
{
    public override CombatSkillActionData GetActionData() => new()
    {
        CoreAction = CombatSkillCoreAction.DefendAll
    };

    protected override HashSet<CardTag> CanonicalTags =>
    [
        CardTags.CS,
        CardTags.ERZ3
    ];
    
    public override CardMultiplayerConstraint MultiplayerConstraint => 
        CardMultiplayerConstraint.MultiplayerOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(4m, ValueProp.Move),
        new PowerVar<StrengthPower>(2m),
        new PowerVar<DexterityPower>(1m)
    ];
    public override async Task ApplyPowers(Creature target, PlayerChoiceContext ctx)
    {
        await PowerCmd.Apply<StrengthPower>(ctx, target,
            DynamicVars["StrengthPower"].BaseValue, Owner!.Creature!, this);
        await PowerCmd.Apply<DexterityPower>(ctx, target,
            DynamicVars["DexterityPower"].BaseValue, Owner!.Creature!, this);
    }

    public STWCommanderStandard()
        : base(
            baseCost: 2,
            type: CardType.Power,
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
        DynamicVars["StrengthPower"].UpgradeValueBy(1m);
        DynamicVars["DexterityPower"].UpgradeValueBy(1m);
    }
    
    protected override string PortraitPath => $"res://images/Extension/Cards/{GetType().Name}.png";
}