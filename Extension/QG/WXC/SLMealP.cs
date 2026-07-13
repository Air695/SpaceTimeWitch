using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Combat.AttackHits;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using SpaceTimeWitch.Extension.Powers;

namespace SpaceTimeWitch.Extension.QG.WXC;

[RegisterPower]
public class SLMealP : ModPowerTemplate, IAttackHitHookListener
{
    public override PowerType Type => PowerType.None;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://images/Extension/Powers/{GetType().Name}.png",
        BigIconPath: $"res://images/Extension/Powers/{GetType().Name}.png"
    );

    private int _healCount;

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;
        _healCount = 0;
    }

    public async Task BeforeAttackHit(AttackHitContext context)
    {
        if (context.Dealer != Owner) return;
        if (_healCount >= 3) return;

        // 检查目标中是否有带 STWBleed 的
        bool hasBleed = context.Targets.Any(t =>
            t.Powers.OfType<STWBleed>().Any(p => p.Amount > 0));

        if (!hasBleed) return;

        _healCount++;
        await CreatureCmd.Heal(Owner, 1);
    }

    public Task AfterAttackHit(AttackHitContext context) => Task.CompletedTask;
}