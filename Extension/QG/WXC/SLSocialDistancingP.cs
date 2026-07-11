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
using MegaCrit.Sts2.Core.Models.Powers;

namespace SpaceTimeWitch.Extension.QG.WXC;

[RegisterPower]
public class SLSocialDistancingP : ModPowerTemplate
{

    public override PowerType Type => PowerType.None;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://images/Extension/Powers/SLQZ.png",
        BigIconPath: $"res://images/Extension/Powers/SLQZ.png"
    );

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        PowerCmd.Apply<DexterityPower>(choiceContext, Owner, 1, Owner, null);
        return Task.CompletedTask;
    }
}