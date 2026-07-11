using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using SpaceTimeWitch.Cards;
using SpaceTimeWitch.Character;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Extension.DCB.Tier2;

[RegisterCard(typeof(SpaceTimeWitchExCardPool))]
public class TheThrone2 : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
        CardTags.DCB2
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<RitualPower>(2)
    ];
    
    public override CardMultiplayerConstraint MultiplayerConstraint => 
        CardMultiplayerConstraint.MultiplayerOnly;


    public TheThrone2()
        : base(
            baseCost:2,
            type: CardType.Power,
            rarity: CardRarity.Uncommon,
            target: TargetType.AllAllies
        )
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        IEnumerable<Creature> enumerable = from c in CombatState.GetTeammatesOf(Owner.Creature)
            where c != null && c.IsAlive && c.IsPlayer
            select c;

        var rP = DynamicVars["RitualPower"].IntValue;
        foreach (Creature item in enumerable)
        {
            await PowerCmd.Apply<RitualPower>(choiceContext,item,rP,Owner.Creature,this);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }

    protected override string PortraitPath => "res://images/Extension/Cards/TheThrone.png";
}