using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using SpaceTimeWitch.Extension.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace SpaceTimeWitch.Extension.Powers;

[RegisterPower]
public class STWExsanguinateO : ModPowerTemplate
{

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://images/Extension/Powers/STWExsanguinate.png",
        BigIconPath: $"res://images/Extension/Powers/STWExsanguinate.png"
    );

    private bool _triggeredThisTurn;

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;
        _triggeredThisTurn = false;
    }

    public override async Task AfterCardPlayed(
        PlayerChoiceContext ctx, CardPlay cardPlay)
    {
        if (_triggeredThisTurn) return;
        if (cardPlay.Card?.Type != CardType.Attack) return;
        if (cardPlay.Card.Owner?.Creature != Owner) return;
        if (cardPlay.Target == null) return;
        if (Amount <= 0) return;

        _triggeredThisTurn = true;

        await PowerCmd.Apply<STWBleed>(
            ctx, cardPlay.Target, (int)Amount, Owner, cardPlay.Card);
    }
}