using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SpaceTimeWitch.Cards.KeyWords;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.Commands;
using SpaceTimeWitch.Scripts;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace SpaceTimeWitch.Cards.Rare;

[RegisterCard(typeof(SpaceTimeWitchCardPool))]
public class STWDestined : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
        CardTags.MarkA
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        SecondaryResourceVars.For("ChronoMark", ModChronoResources.Id, 3m)
    ];

    public STWDestined()
        : base(
            baseCost:1,
            type: CardType.Skill,
            rarity: CardRarity.Rare,
            target: TargetType.Self
        )
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var owner = Owner;
        if (owner?.Creature == null) return;

        var cM = DynamicVars["ChronoMark"].IntValue;
        await ChronoMark.Gain(owner.Creature, cM,this);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != Owner?.Creature?.Side) return;

        var owner = Owner;
        if (owner?.Creature == null) return;

        // 检查是否在个人空间
        var psPile = Entry.PersonalSpacePile.GetPile(owner);
        if (!psPile.Cards.Contains(this)) return;

        // 移到抽牌堆后自动打出（AutoPlay 会触发 OnPlay → 再刻2道时痕）
        await CardPileCmd.Add(this, PileType.Draw);
        await CardCmd.AutoPlay(choiceContext, this, null);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(STWKeywords.Binding.GetModKeywordCardKeyword());
    }
}