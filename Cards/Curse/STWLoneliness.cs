using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using SpaceTimeWitch.Scripts;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Cards.Curse;

[RegisterCard(typeof(CurseCardPool))]
public class STWLoneliness : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
    ];

    public STWLoneliness()
        : base(
            baseCost:-1,
            type: CardType.Curse,
            rarity: CardRarity.None,
            target: TargetType.Self
        )
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable,CardKeyword.Innate,CardKeyword.Eternal];

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != Owner?.Creature?.Side) return;

        var owner = Owner;
        if (owner?.Creature == null) return;

        var psPile = Entry.PersonalSpacePile.GetPile(owner);

        if (PileType.Hand.GetPile(owner).Cards.Contains(this))
        {
            // 在手牌：回到抽牌堆顶端（必须在 BeforeTurnEnd 处理，AfterTurnEnd 时手牌已清空）
            await CardPileCmd.Add(this, PileType.Draw, CardPilePosition.Top);
        }
        else if (psPile.Cards.Contains(this))
        {
            // 在个人空间：回到抽牌堆
            await CardPileCmd.Add(this, PileType.Draw);
        }
        else if (PileType.Exhaust.GetPile(owner).Cards.Contains(this))
        {
            // 在消耗堆：进入弃牌堆
            await CardPileCmd.Add(this, PileType.Discard);
        }
    }

    protected override void OnUpgrade()
    {
    }
}