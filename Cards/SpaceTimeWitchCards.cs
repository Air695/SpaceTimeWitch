using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using SpaceTimeWitch.Scripts;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Scaffolding.Content;

namespace SpaceTimeWitch.Cards;

public abstract class SpaceTimeWitchCards(
    int baseCost,
    CardType type,
    CardRarity rarity,
    TargetType target,
    bool showInCardLibrary = true
) : ModCardTemplate(baseCost, type, rarity, target, showInCardLibrary), ISecondaryResourceHookListener
{
    decimal ISecondaryResourceHookListener.ModifySecondaryResourceCost(
        SecondaryResourceCostContext context, decimal cost)
    {
        if (context.Definition.Id == ModChronoResources.Id
            && IsFreeViaEffect(context.Card))
        {
            return 0m;
        }
        return cost;
    }
    protected override HashSet<CardTag> CanonicalTags => [];

    public override bool GainsBlock => _gainsBlock ??= CanonicalVars.Any(v => v is BlockVar);

    private bool? _gainsBlock;

    public static readonly LocString SharedChoosePoolPrompt = new("cards", "STW_SHARED_CHOOSE_POOL");
    public static readonly LocString SharedChooseCardPrompt = new("cards", "STW_SHARED_CHOOSE_CARD");
    public static readonly LocString SharedDepositPrompt = new("cards", "STW_DEPOSIT");
    public static readonly LocString SharedWithdrawPrompt = new("cards", "STW_WITHDRAW");

    /// <summary>
    /// 设置 ChronoMark（时痕）消耗。在子类构造函数或 OnUpgrade 中调用。
    /// 底层使用 RitsuLib SecondaryResource 的 SecondaryCosts 系统。
    /// </summary>
    protected void SetChronoMarkCost(int cost)
    {
        if (cost > 0)
            this.SecondaryCosts().Set(ModChronoResources.Id, cost);
        else
            this.SecondaryCosts().Clear(ModChronoResources.Id);
    }

    /// <summary>
    /// 检查是否有药水/ Corruption / FreeAttackPower 等效果使卡牌免费打出。
    /// 供 ModifySecondaryResourceCost Hook 调用，使 ChronoMark 消耗与能量费用同步免除。
    /// </summary>
    public static bool IsFreeViaEffect(CardModel card)
    {
        if (card.Owner?.Creature?.CombatState == null) return false;

        // ① 全球"设为0"效果：大探测值只有无条件设为0的效果才会返回0
        if (Hook.ModifyEnergyCostInCombat(card.Owner.Creature.CombatState, card, 100m) == 0m)
            return true;

        // ② 本地修正：SkillPotion/BulletTime/Madness 的 SetToFreeThisTurn()
        if (card.EnergyCost.HasLocalModifiers
            && card.EnergyCost.GetWithModifiers(CostModifiers.Local) == 0)
            return true;

        return false;
    }

    /// <summary>
    /// ChronoMark 费用由 ModifySecondaryResourceCost Hook 动态修正，
    /// 免费时自动为 0，RitsuLib 框架自行处理可打出性和扣除。
    /// </summary>
    protected override bool IsPlayable
    {
        get
        {
            if (!base.IsPlayable)
                return false;

            return true; // ChronoMark 费用由 Hook 动态修正，框架自行检查
        }
    }

    /// <summary>
    /// ChronoMark 扣除由 SecondaryResource 框架自动处理，子类覆写 OnPlay 时无需调用 base。
    /// </summary>
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
    }
    protected virtual string PortraitPath =>
        $"res://images/SpaceTimeWitch/Cards/{GetType().Name}.png";

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: PortraitPath,
        FramePath: type switch
        {
            CardType.Attack => "res://images/SpaceTimeWitch/UI/SpaceTimeWitchAttackCardFrame.png",
            CardType.Skill => "res://images/SpaceTimeWitch/UI/SpaceTimeWitchSkillCardFrame.png",
            CardType.Power => "res://images/SpaceTimeWitch/UI/SpaceTimeWitchPowerCardFrame.png",
            CardType.Curse => null,
            CardType.Status => null,
            _ => null
        }
    );

    // 各卡牌自定义追加的悬浮提示（子类覆写此项，通过 CardHoverTipsPatch 注入，显示在最上方）
    protected virtual IEnumerable<IHoverTip> CardSpecificHoverTips => [];

    // 供 CardHoverTipsPatch 访问（protected 成员外部不可见）
    internal IEnumerable<IHoverTip> ExposedCardSpecificHoverTips => CardSpecificHoverTips;
    internal IEnumerable<DynamicVar> ExposedCanonicalVars => CanonicalVars;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [];
    protected override IEnumerable<string> RegisteredKeywordIds => [];
}
