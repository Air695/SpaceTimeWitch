using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using SpaceTimeWitch.Extension.Powers;

namespace SpaceTimeWitch.Extension.QG.WXC;

[RegisterPower]
public class SLGooeyWasteP : ModPowerTemplate
{
    public override PowerType Type => PowerType.None;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://images/Extension/Powers/{GetType().Name}.png",
        BigIconPath: $"res://images/Extension/Powers/{GetType().Name}.png"
    );

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;

        var enemies = CombatState.HittableEnemies.ToList();
        if (enemies.Count == 0) return;

        await PowerCmd.Apply<VulnerablePower>(choiceContext, enemies, 2, Owner, null);
        await PowerCmd.Apply<WeakPower>(choiceContext, enemies, 2, Owner, null);
        await PowerCmd.Apply<FrailPower>(choiceContext, enemies, 2, Owner, null);
        await PowerCmd.Apply<STWBleed>(choiceContext, enemies, 2, Owner, null);
    }
}