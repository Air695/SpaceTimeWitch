using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using SpaceTimeWitch.Field;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Cards.Fields;

[RegisterPower]
public class MirrorRealmPower : FieldPowerBase
{
    public override FieldBackgroundType BackgroundType => FieldBackgroundType.Replace;
    public override string BackgroundPath => "res://images/SpaceTimeWitch/Field/YourBg.png";

    private HashSet<CardModel> _createdCopies;

    /// <summary>不会被复制的卡牌类型黑名单。</summary>
    private static readonly HashSet<Type> CopyBlacklist = new HashSet<Type>();

    private HashSet<CardModel> CreatedCopies
    {
        get
        {
            AssertMutable();
            return _createdCopies ??= new HashSet<CardModel>();
        }
    }

    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
    {
        var pile = card.Pile;
        if (pile == null || pile.Type != PileType.Hand) return;
        if (card.Owner != Owner.Player) return;

        if (CreatedCopies.Contains(card)) return;
        if (CopyBlacklist.Contains(card.GetType())) return;

        var clone = CombatState.CloneCard(card);
        clone.AddKeyword(CardKeyword.Ethereal);
        clone.AddKeyword(CardKeyword.Exhaust);

        CreatedCopies.Add(clone);

        await CardPileCmd.Add(clone, PileType.Hand, clonedBy: this);
    }
}