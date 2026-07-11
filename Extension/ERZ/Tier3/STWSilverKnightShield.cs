using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;
using SpaceTimeWitch.Cards;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.CombatSkill;
using SpaceTimeWitch.Extension.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Extension.ERZ.Tier3;

[RegisterCard(typeof(SpaceTimeWitchExCardPool))]
public class STWSilverKnightShield : CombatSkillCard
{
    public override CombatSkillActionData GetActionData() => new()
    {
        CoreAction = CombatSkillCoreAction.Defend,
        ExtraEffect = async (card, ctx, play) =>
        {
            var target = play.Target;
            if (target?.Monster?.NextMove?.Intents == null) return;
            var attackIntent = target.Monster.NextMove.Intents
                .OfType<AttackIntent>().FirstOrDefault();
            if (attackIntent == null) return;

            var intentDmg = attackIntent.GetSingleDamage(
                targets: new[] { card.Owner!.Creature }, owner: target);
            if (intentDmg <= 0) return;

            var block = card.Owner!.Creature.Block;
            if (Math.Abs(intentDmg - block) <= 3)
                await PowerCmd.Apply<STWStaggered>(ctx, target, 1, card.Owner.Creature, card);
        }
    };

    protected override HashSet<CardTag> CanonicalTags =>
    [
        CardTags.CS,
        CardTags.ERZ3
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(14m, ValueProp.Move),
        new PowerVar<STWStaggered>(0)
    ];

    public STWSilverKnightShield()
        : base(baseCost: 1, type: CardType.Skill, rarity: CardRarity.Common)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
    }

    protected override string PortraitPath => $"res://images/Extension/Cards/{GetType().Name}.png";
}
