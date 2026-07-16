using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace SpaceTimeWitch.Extension.DCB.Tier1;

[RegisterPower]
public class SpiritForgeP1 : ModPowerTemplate
{

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://images/Extension/Powers/SpiritForgeP.png",
        BigIconPath: "res://images/Extension/Powers/SpiritForgeP.png"
    );

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        // 仅对能力持有者生效
        if (cardPlay.Card.Owner.Creature != Owner) return;

        // 排除幻影剑自身，仅非幻影剑的攻击牌触发
        if (cardPlay.Card is STWMirageBlades) return;
        if (cardPlay.Card.Type != CardType.Attack) return;

        var blade = (STWMirageBlades)Owner.CombatState.CreateCard<STWMirageBlades>(Owner.Player);
        await CardPileCmd.AddGeneratedCardToCombat(blade, PileType.Hand, creator: blade.Owner);
    }

}
