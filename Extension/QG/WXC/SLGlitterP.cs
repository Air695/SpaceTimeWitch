using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace SpaceTimeWitch.Extension.QG.WXC;

[RegisterPower]
public class SLGlitterP : ModPowerTemplate
{
    public override PowerType Type => PowerType.None;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://images/Extension/Powers/SLQZ.png",
        BigIconPath: $"res://images/Extension/Powers/SLQZ.png"
    );

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;

        var rng = player.RunState.Rng.CombatCardGeneration;

        foreach (var enemy in CombatState.HittableEnemies)
        {
            if (rng.NextInt(2) == 0)
            {
                await PowerCmd.Apply<SLAllured>(choiceContext, enemy, 1, Owner, null);
            }
        }
    }
}