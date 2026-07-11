using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using SpaceTimeWitch.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Cards;
using SpaceTimeWitch.Extension.ExKeyWords;

namespace SpaceTimeWitch.Powers;

[RegisterPower]
public class STWNextTurnDraw : ModPowerTemplate
{

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://images/SpaceTimeWitch/Powers/STWNextTurnDraw.png",
        BigIconPath: $"res://images/SpaceTimeWitch/Powers/STWNextTurnDraw.png"
    );

    public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner?.Player) return;
        var creature = player.Creature;
        if (creature == null) return;
        
        await CardPileCmd.Draw(choiceContext, Amount, player);
        await PowerCmd.Remove(this);
    }
}