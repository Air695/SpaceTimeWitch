using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.Commands;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Cards.Rare;

[RegisterCard(typeof(SpaceTimeWitchCardPool))]
public class STWBombardment : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(10m,ValueProp.Move)
    ];

    public STWBombardment()
        : base(
            baseCost:3,
            type: CardType.Attack,
            rarity: CardRarity.Rare,
            target: TargetType.AllEnemies
        )
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var owner = Owner;
        if (owner?.Creature == null) return;

        var psCards = PersonalSpaceCmd.GetCards(owner).ToList();
        if (psCards.Count == 0) return;

        var damagePerCard = DynamicVars.Damage.BaseValue;

        foreach (var card in psCards)
        {
            await CardCmd.Exhaust(choiceContext, card);
        }

        for (int i = 0; i < psCards.Count; i++)
        {
            await DamageCmd.Attack(damagePerCard)
                .FromCard(this)
                .TargetingAllOpponents(CombatState)
                .Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}