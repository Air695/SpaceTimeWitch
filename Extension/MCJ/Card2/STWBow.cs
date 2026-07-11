using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using SpaceTimeWitch.Cards;
using SpaceTimeWitch.Cards.KeyWords;
using SpaceTimeWitch.Character;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace SpaceTimeWitch.Extension.MCJ.Card2;

[RegisterCard(typeof(SpaceTimeWitchExCardPool))]
public class STWBow : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags => [];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(25m, ValueProp.Move)
    ];

    public STWBow()
        : base(
            baseCost: 1,
            type: CardType.Attack,
            rarity: CardRarity.Common,
            target: TargetType.AnyEnemy
        )
    {
    }
    public override IEnumerable<CardKeyword> CanonicalKeywords => [STWKeywords.Soulbinding.GetModKeywordCardKeyword()];

    /// <summary>
    /// 无镐类遗物或材料不足时无法打出。
    /// </summary>
    protected override bool IsPlayable
    {
        get
        {
            if (!base.IsPlayable)
                return false;

            var pickaxe = Owner?.Relics.OfType<IPickaxeRelic>().FirstOrDefault();
            return pickaxe != null && pickaxe.CanConsumeMaterial(1);
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var owner = Owner;
        if (owner?.Creature == null) return;
        // 消费 1 份材料
        var pickaxe = Owner!.Relics.OfType<IPickaxeRelic>().First();
        pickaxe.ConsumeMaterial(1);

        // 造成伤害
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(play.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5m);
    }

    protected override string PortraitPath => $"res://images/Extension/Cards/{GetType().Name}.png";
}
