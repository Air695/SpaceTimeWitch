using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SpaceTimeWitch.Cards;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.Commands;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Extension.DCB.Tier2;

[RegisterCard(typeof(SpaceTimeWitchExCardPool))]
public class SpiritSolidify2 : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
        CardTags.DCB2
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(3),
        new DynamicVar("a",1)
    ];


    public SpiritSolidify2()
        : base(
            baseCost:0,
            type: CardType.Skill,
            rarity: CardRarity.Common,
            target: TargetType.Self
        )
    {
    }
    protected override IEnumerable<IHoverTip> CardSpecificHoverTips =>
    [
        HoverTipFactory.FromCard<STWMirageBlades>(),
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var owner = Owner;
        if (owner?.Creature == null) return;
        var a = DynamicVars["a"].IntValue;
        
        for (int i = 0; i < a; i++)
        {
            var mb = CombatState.CreateCard<STWMirageBlades>(owner);
            await CardPileCmd.AddGeneratedCardToCombat(mb, PileType.Hand,Owner);
            await Cmd.Wait(0.1f);
        }

        var handBlades = PileType.Hand.GetPile(owner).Cards.OfType<STWMirageBlades>();
        var drawBlades = PileType.Draw.GetPile(owner).Cards.OfType<STWMirageBlades>();
        var discardBlades = PileType.Discard.GetPile(owner).Cards.OfType<STWMirageBlades>();
        var exhaustBlades = PileType.Exhaust.GetPile(owner).Cards.OfType<STWMirageBlades>();
        var psBlades = PersonalSpaceCmd.GetCards(owner).OfType<STWMirageBlades>();
        var allBlades = handBlades.Concat(drawBlades).Concat(discardBlades)
            .Concat(exhaustBlades).Concat(psBlades).ToList();

        if (allBlades.Count == 0) return;

        var amount = DynamicVars.Cards.IntValue;
        foreach (var blade in allBlades)
            blade.DynamicVars.Damage.BaseValue += amount;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);
    }

    protected override string PortraitPath => "res://images/Extension/Cards/SpiritSolidify.png";
}
