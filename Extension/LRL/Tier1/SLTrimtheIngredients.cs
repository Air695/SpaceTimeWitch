using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.Cards;
using SpaceTimeWitch.Powers;
using SpaceTimeWitch.Commands;
using SpaceTimeWitch.Extension.Powers;

namespace SpaceTimeWitch.Extension.LRL.Tier1
;

[RegisterCard(typeof(SpaceTimeWitchExCardPool))]
public class SLTrimtheIngredients : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
        CardTags.LRL1
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6m,ValueProp.Move),
        new PowerVar<STWBleed>(2m)
    ];


    public SLTrimtheIngredients()
        : base(
            baseCost:1,
            type: CardType.Attack,
            rarity: CardRarity.Uncommon,
            target: TargetType.AnyEnemy
        )
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var owner = Owner;
        if (owner?.Creature == null) return;

        ArgumentNullException.ThrowIfNull(play.Target, "cardPlay.Target");

        // 造成 6 点伤害
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(play.Target!)
            .Execute(choiceContext);

        // 给予 2 层流血
        var bleedAmount = DynamicVars["STWBleed"].IntValue;
        await PowerCmd.Apply<STWBleed>(choiceContext, play.Target!, bleedAmount, owner.Creature, this);

        // 获得等同于目标流血层数的格挡
        var targetBleed = play.Target!.Powers.OfType<STWBleed>().FirstOrDefault();
        var totalBleed = targetBleed?.Amount ?? 0;
        if (totalBleed > 0)
        {
            await CreatureCmd.GainBlock(owner.Creature, totalBleed, ValueProp.Move, null);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars["STWBleed"].UpgradeValueBy(1m);
    }

    protected override string PortraitPath => $"res://images/Extension/Cards/{GetType().Name}.png";
}