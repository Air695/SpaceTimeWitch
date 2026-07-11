using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using SpaceTimeWitch.Commands;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace SpaceTimeWitch.Powers;

[RegisterPower]
public class GravenEternityDawnPower : ModPowerTemplate, IAfterMonsterAction
{

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://images/SpaceTimeWitch/Powers/{GetType().Name}.png",
        BigIconPath: $"res://images/SpaceTimeWitch/Powers/{GetType().Name}.png"
    );

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;
        await ChronoMark.Gain(Owner, Amount);
    }

    // ② 玩家回合结束时
    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player) return;
        await ChronoMark.Gain(Owner, Amount);
    }

    // ③ 每个怪物行动后（由 Harmony Patch 调用）
    void IAfterMonsterAction.OnMonsterActed()
    {
        ChronoMark.Gain(Owner, Amount).ContinueWith(_ => { });
    }
    
}