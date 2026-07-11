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
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;

namespace SpaceTimeWitch.Extension.QG.WXC;

[RegisterCard(typeof(STWEGO))]
public class SLTodayExpression : SpaceTimeWitchCards, IEGOCard
{
    public CardTag Tag => CardTags.WXCE;

    protected override HashSet<CardTag> CanonicalTags =>
    [
        CardTags.WXCE
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(30m,ValueProp.Move)
    ];
    
    protected override IEnumerable<IHoverTip> CardSpecificHoverTips =>
    [
        new HoverTip(
            new LocString("cards", "NOPE"),
            new LocString("cards", "SL_TODAY_EXPRESSION")
        ),
    ];


    public SLTodayExpression()
        : base(
            baseCost:2,
            type: CardType.Skill,
            rarity: CardRarity.Rare,
            target: TargetType.AnyEnemy
        )
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust,CardKeyword.Retain];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var owner = Owner;
        if (owner?.Creature == null) return;

        await CreatureCmd.GainBlock(owner.Creature, DynamicVars.Block, play);

        var block = owner.Creature.Block;
        if (block > 0)
        {
            await DamageCmd.Attack(block)
                .FromCard(this)
                .Targeting(play.Target)
                .Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(10m);
    }

    protected override string PortraitPath => $"res://images/Extension/EGO/{GetType().Name}.png";
}