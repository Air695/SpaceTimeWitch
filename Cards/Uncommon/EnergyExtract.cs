using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.Commands;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Cards.Uncommon;

[RegisterCard(typeof(SpaceTimeWitchCardPool))]
public class EnergyExtract : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(2)
    ];

    public EnergyExtract()
        : base(
            baseCost: 0,
            type: CardType.Skill,
            rarity: CardRarity.Uncommon,
            target: TargetType.Self
        )
    {
        SetChronoMarkCost(1);
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var owner = Owner;
        if (owner?.Creature == null) return;

        var cards = PersonalSpaceCmd.GetCards(owner);
        if (cards.Count == 0) return;

        var chosen = (await CardSelectCmd.FromSimpleGrid(
                choiceContext,
                cards.ToList(),
                owner,
                new CardSelectorPrefs(SharedChooseCardPrompt, minCount: 1, maxCount: 1)))
            .FirstOrDefault();

        if (chosen == null) return;

        // �Ӹ��˿ռ�������ƶѣ�ʹ���ƽ�����ұ�׼ Piles
        await CardPileCmd.Add(chosen, PileType.Draw);

        await CardCmd.Exhaust(choiceContext, chosen);
        await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Energy.UpgradeValueBy(1);
    }
}