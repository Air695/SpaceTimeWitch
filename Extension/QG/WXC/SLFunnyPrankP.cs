using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Enchantments;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace SpaceTimeWitch.Extension.QG.WXC;

[RegisterPower]
public class SLFunnyPrankP : ModPowerTemplate
{
    public override PowerType Type => PowerType.None;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://images/Extension/Powers/SLQZ.png",
        BigIconPath: $"res://images/Extension/Powers/SLQZ.png"
    );

    private bool _triggeredThisTurn;

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;
        _triggeredThisTurn = false;
    }

    public override async Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (_triggeredThisTurn) return;
        if (cardPlay.Card?.Type != CardType.Attack) return;
        if (cardPlay.Card.Owner?.Creature != Owner) return;

        _triggeredThisTurn = true;
        CardCmd.Enchant<Corrupted>(cardPlay.Card, 1);
    }
}