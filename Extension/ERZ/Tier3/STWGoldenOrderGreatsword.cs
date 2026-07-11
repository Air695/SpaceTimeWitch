using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using SpaceTimeWitch.Cards;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.CombatSkill;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Extension.ERZ.Tier3;

[RegisterCard(typeof(SpaceTimeWitchExCardPool))]
public class STWGoldenOrderGreatsword : CombatSkillCard
{
    public override CombatSkillActionData GetActionData() => new()
    {
        CoreAction = CombatSkillCoreAction.Strike,
        ExtraEffect = async (card, ctx, play) =>
        {
            var baseValue = CombatSkillExecutor.GetBaseValue(card);
            var applier = card.Owner!.Creature!;
            var isNative = card.Type == CardType.Attack;

            foreach (var enemy in card.CombatState.HittableEnemies)
            {
                if (isNative)
                {
                    await DamageCmd.Attack(baseValue).FromCard(card).Targeting(enemy).Execute(ctx);
                }
                else
                {
                    var val = CombatSkillExecutor.CalcEffective(card, baseValue, enemy, play);
                    await CreatureCmd.Damage(ctx, enemy, val, ValueProp.Unpowered, applier, card);
                }
                await card.ApplyPowers(enemy, ctx);
            }
        }
    };

    protected override HashSet<CardTag> CanonicalTags =>
    [
        CardTags.CS,
        CardTags.ERZ3
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(12m, ValueProp.Move)
    ];

    public STWGoldenOrderGreatsword()
        : base(
            baseCost: 1,
            type: CardType.Attack,
            rarity: CardRarity.Common
        )
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await base.OnPlay(ctx, play);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
    
    protected override string PortraitPath => $"res://images/Extension/Cards/{GetType().Name}.png";
}