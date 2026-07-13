using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using SpaceTimeWitch.Cards;
using SpaceTimeWitch.Character;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Extension.DCB.Tier3;

[RegisterCard(typeof(SpaceTimeWitchExCardPool))]
public class SummonedSwords3 : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
        CardTags.DCB3
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(4m,ValueProp.Move),
        new CardsVar(4)
    ];
    
    protected override IEnumerable<IHoverTip> CardSpecificHoverTips =>
    [
        HoverTipFactory.FromCard<STWMirageBlades>(),
    ];

    public SummonedSwords3()
        : base(
            baseCost:1,
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
        if (owner?.Creature == null) return;
        
        ArgumentNullException.ThrowIfNull(play.Target, "cardPlay.Target");
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(play.Target!)
            .Execute(choiceContext);
        
        for (int i = 0; i < DynamicVars.Cards.IntValue; i++)
        {
            var mb = CombatState.CreateCard<STWMirageBlades>(owner);
            await CardPileCmd.AddGeneratedCardToCombat(mb, PileType.Hand,Owner);
            await Cmd.Wait(0.1f);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }

    protected override string PortraitPath => "res://images/Extension/Cards/SummonedSwords.png";
}