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
public class MirageCommand2 : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
        CardTags.DCB2
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(1)
    ];
    
    
    protected override IEnumerable<IHoverTip> CardSpecificHoverTips =>
    [
        HoverTipFactory.FromCard<STWMirageBlades>(),
    ];

    public MirageCommand2()
        : base(
            baseCost:0,
            type: CardType.Skill,
            rarity: CardRarity.Common,
            target: TargetType.RandomEnemy
        )
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var owner = Owner;
        if (owner?.Creature == null) return;
        
        for (int i = 0; i < DynamicVars.Cards.IntValue; i++)
        {
            var mb = CombatState.CreateCard<STWMirageBlades>(owner);
            await CardPileCmd.AddGeneratedCardToCombat(mb, PileType.Hand,Owner);
            await Cmd.Wait(0.1f);
        }

        var handBlades = PileType.Hand.GetPile(owner).Cards.OfType<STWMirageBlades>().ToList();
        var drawBlades = PileType.Draw.GetPile(owner).Cards.OfType<STWMirageBlades>().ToList();
        var discardBlades = PileType.Discard.GetPile(owner).Cards.OfType<STWMirageBlades>().ToList();
        var exhaustBlades = PileType.Exhaust.GetPile(owner).Cards.OfType<STWMirageBlades>().ToList();
        var allBlades = handBlades.Concat(drawBlades).Concat(discardBlades).Concat(exhaustBlades).ToList();

        if (allBlades.Count == 0) return;

        foreach (var blade in allBlades)
            CardCmd.Upgrade(blade);

        var aliveEnemies = CombatState.HittableEnemies
            .Where(e => !e.IsDead)
            .ToList();

        if (aliveEnemies.Count == 0) return;

        foreach (var blade in allBlades)
        {
            var randomTarget = aliveEnemies.Count == 1
                ? aliveEnemies[0]
                : owner.RunState.Rng.CombatCardGeneration.NextItem(aliveEnemies);

            if (randomTarget != null && !randomTarget.IsDead)
                await CardCmd.AutoPlay(choiceContext, blade, randomTarget);

            await PersonalSpaceCmd.Store(owner, blade);
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }

    protected override string PortraitPath => "res://images/Extension/Cards/MirageCommand.png";
}
