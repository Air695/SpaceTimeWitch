using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SpaceTimeWitch.Cards;
using SpaceTimeWitch.Character;
using STS2RitsuLib.Interop.AutoRegistration;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.HoverTips;

namespace SpaceTimeWitch.Extension.QG.WXC;

[RegisterCard(typeof(STWLRA))]
public class SLGooeyWaste : SpaceTimeWitchCards, IAbnormalityCard
{
    public int Interval => 0;

    public CardTag Tag => CardTags.WXC3;
    
    protected override IEnumerable<IHoverTip> CardSpecificHoverTips =>
    [
        new HoverTip(
            new LocString("cards", "NOPE"),
            new LocString("cards", "SL_GOOEY_WASTE")
        ),
    ];

    public async Task ApplyEffect(PlayerChoiceContext ctx, Player player)
    {
        await PowerCmd.Apply<SLGooeyWasteP>(ctx, player.Creature, 1, player.Creature, this);
    }

    protected override HashSet<CardTag> CanonicalTags => [CardTags.WXC3];

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    protected override string PortraitPath => $"res://images/Extension/EGO/{GetType().Name}.png";

    public SLGooeyWaste()
        : base(
            baseCost: 3,
            type: CardType.Power,
            rarity: CardRarity.Ancient,
            target: TargetType.Self
        )
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
    }

    protected override void OnUpgrade() { }
}