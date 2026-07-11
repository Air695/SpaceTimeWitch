using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.Cards;
using SpaceTimeWitch.Extension.Powers;

namespace SpaceTimeWitch.Extension.LRL.Tier2;

[RegisterCard(typeof(SpaceTimeWitchExCardPool))]
public class SLCleanUp1 : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
        CardTags.LRL2
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(4m,ValueProp.Move),
        new PowerVar<STWBleed>(2m),
        new CardsVar(6),
        new EnergyVar(1)
    ];


    public SLCleanUp1()
        : base(
            baseCost:0,
            type: CardType.Attack,
            rarity: CardRarity.Common,
            target: TargetType.AnyEnemy
        )
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var owner = Owner;
        var target = play.Target;
        if (owner?.Creature == null || target == null) return;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(target)
            .Execute(choiceContext);

        await PowerCmd.Apply<STWBleed>(
            choiceContext, target,
            DynamicVars["STWBleed"].IntValue,
            owner.Creature, this);

        var bleedCount = target.Powers.OfType<STWBleed>()
            .FirstOrDefault()?.Amount ?? 0;
        if (bleedCount >= DynamicVars.Cards.IntValue)
        {
            await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, owner);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars["STWBleed"].UpgradeValueBy(1m);
    }

    protected override string PortraitPath => $"res://images/Extension/Cards/SLCleanUp.png";
}