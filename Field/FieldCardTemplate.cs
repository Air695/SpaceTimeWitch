using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using SpaceTimeWitch.Cards;
using STS2RitsuLib.Scaffolding.Content;

namespace SpaceTimeWitch.Field;

public abstract class FieldCardTemplate : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags => [CardTags.Field];

    /// <summary>
    /// 覆写为 None，使所有针对 Power 类型的机械互动（费用修改、打出触发、生成筛选等）
    /// 自动跳过场地卡。视觉上由 RitsuLib 的 AssetProfile 覆写保持 Power 外观。
    /// </summary>
    public override CardType Type => CardType.None;

    protected override PileType GetResultPileTypeForCardPlay() => PileType.None;

    // ── 子类必须覆写 ──

    /// <summary>场地背景行为类型</summary>
    public abstract FieldBackgroundType BackgroundType { get; }

    /// <summary>背景资源路径</summary>
    public abstract string BackgroundPath { get; }

    /// <summary>向指定 Creature 应用场地能力（子类调用 PowerCmd.Apply&lt;T&gt;(creature, ...)）</summary>
    protected abstract Task ApplyFieldPowerToCreature(PlayerChoiceContext ctx, CardPlay play, Creature creature);

    // ── 构造 ──

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/SpaceTimeWitch/Field/{GetType().Name}.png",
        FramePath: "res://images/SpaceTimeWitch/UI/SpaceTimeWitchPowerCardFrame.png"
    );

    protected FieldCardTemplate(int baseCost, CardRarity rarity)
        : base(baseCost, CardType.Power, rarity, TargetType.Self) { }

    // ── 打出流程：标记替换 → 移除旧场地 → 应用新场地能力 → 切换背景 → 取消标记 ──

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        FieldCmd.BeginReplace();

        // 1. 移除施法者身上的旧场地（PowerCmd_Remove_Patch 检测到 IsReplacing，放行）
        var existing = Owner.Creature.Powers.OfType<FieldPowerBase>().FirstOrDefault();
        if (existing != null)
            await PowerCmd.Remove(existing);

        FieldCmd.RestoreBackground();

        // 2. 应用新场地能力（仅施法者，但 Harmony Patch 会阻止其被外部效果移除，
        //    即使施法者死亡，能力依旧保留在尸体上，Hook 继续触发）
        await ApplyFieldPowerToCreature(choiceContext, cardPlay, Owner.Creature);

        // 3. 切换背景
        FieldCmd.ApplyBackground(BackgroundType, BackgroundPath);
        FieldCmd.EndReplace();
    }
}
