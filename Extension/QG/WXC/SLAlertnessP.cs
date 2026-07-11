using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace SpaceTimeWitch.Extension.QG.WXC;

[RegisterPower]
public class SLAlertnessP : ModPowerTemplate
{
    public override PowerType Type => PowerType.None;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://images/Extension/Powers/{GetType().Name}.png",
        BigIconPath: $"res://images/Extension/Powers/{GetType().Name}.png"
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

        _triggeredThisTurn = true;

        await PowerCmd.Apply<VulnerablePower>(ctx, cardPlay.Target, 1, Owner, cardPlay.Card);
        await PowerCmd.Apply<StrengthPower>(ctx, cardPlay.Target, -1, Owner, cardPlay.Card);
    }
}