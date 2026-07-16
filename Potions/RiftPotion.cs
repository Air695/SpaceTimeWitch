using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using SpaceTimeWitch.Character;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using SpaceTimeWitch.Cards.Token;
using MegaCrit.Sts2.Core.Commands;

namespace SpaceTimeWitch.Potions;

[RegisterPotion(typeof(SpaceTimeWitchPotionPool))]
public class RiftPotion : ModPotionTemplate
{
    public override PotionRarity Rarity => PotionRarity.Common;

    public override PotionUsage Usage => PotionUsage.CombatOnly;

    public override TargetType TargetType => TargetType.Self;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(3)];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromCard<InstantRift>()];

    public override PotionAssetProfile AssetProfile => new(
        ImagePath: "res://images/SpaceTimeWitch/Potions/RiftPotion.png",
        OutlinePath: "res://images/SpaceTimeWitch/Potions/RiftPotion.png"
    );

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        var owner = Owner;
        if (owner?.Creature == null) return;
        
        var count = DynamicVars.Cards.IntValue;
        for (var i = 0; i < count; i++)
        {
            var rift = (InstantRift)owner.Creature.CombatState.CreateCard<InstantRift>(owner);
            await CardPileCmd.AddGeneratedCardToCombat(rift, PileType.Hand, creator: rift.Owner);
        }
    }
}