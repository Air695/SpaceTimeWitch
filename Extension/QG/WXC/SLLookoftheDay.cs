using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SpaceTimeWitch.Cards;
using SpaceTimeWitch.Character;
using STS2RitsuLib.Interop.AutoRegistration;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.HoverTips;

namespace SpaceTimeWitch.Extension.QG.WXC;

/// <summary>
/// 1级异想体「今日之面容」— 区间0。选取后施加 STWLookoftheDayP 能力。
/// </summary>
[RegisterCard(typeof(STWLRA))]
public class SLLookoftheDay : SpaceTimeWitchCards, IAbnormalityCard
{
    // ─── IAbnormalityCard 实现 ───

    /// <summary>情感区间 0</summary>
    public int Interval => 0;

    /// <summary>绑定的异想体标签，与遗物的 AbnormalityTags 匹配</summary>
    public CardTag Tag => CardTags.WXC1;
    
    protected override IEnumerable<IHoverTip> CardSpecificHoverTips =>
    [
        new HoverTip(
            new LocString("cards", "NOPE"),
            new LocString("cards", "SL_LOOKOFTHE_DAY")
        ),
    ];

    /// <summary>选取后施加 STWLookoftheDayP 能力</summary>
    public async Task ApplyEffect(PlayerChoiceContext ctx, Player player)
    {
        await PowerCmd.Apply<SLLookoftheDayP>(ctx, player.Creature, 1, player.Creature, this);
    }

    // ─── SpaceTimeWitchCards 必要成员 ───

    protected override HashSet<CardTag> CanonicalTags => [CardTags.WXC1];

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    protected override string PortraitPath => $"res://images/Extension/EGO/{GetType().Name}.png";

    public SLLookoftheDay()
        : base(
            baseCost: 1,
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
