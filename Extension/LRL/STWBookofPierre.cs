using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.Extension.QG;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Interactions.RightClick;
using STS2RitsuLib.Scaffolding.Content;
using SpaceTimeWitch.Cards;
using SpaceTimeWitch.Relics;

namespace SpaceTimeWitch.Extension.LRL;

[RegisterRelic(typeof(SpaceTimeWitchExRelicPool))]
public class STWBookofPierre : QGRelicBase, ITagRelic
{
    private TagRelicData? _data;
    private TagRelicData Data => _data ??= TagRelicRegistry.Entries[GetType()];

    public CardTag AssociatedTag => Data.Tag;
    public string CharacterGroup => Data.Group;
    public string Class => Data.Class;
    public int Tier => Data.Tier;
    public IReadOnlyList<Type> NextTierRelicTypes => Data.NextTierTypes ?? [];
    public double Weight => Data.Weight;
    public IReadOnlyDictionary<Type, double> NextTierWeights => Data.NextTierWeights ?? new Dictionary<Type, double>();
    public override int MaxRelicLevel => 3;
    public override IReadOnlyList<CardTag> AbnormalityTags => [CardTags.WXC];

    public override async Task OnRightClick(ModRightClickExecutionContext context)
    {
        if (Owner == null) return;

        var maxLevel = EmotionSystem.GetMaxRelicLevel(Owner);
        if (EmotionSystem.GetLevel(Owner) >= maxLevel) return;
        if (!EmotionSystem.CanLevelUp(Owner)) return;

        var oldLevel = EmotionSystem.GetLevel(Owner);
        var prePos = EmotionSystem.GetPositive(Owner);
        var preNeg = EmotionSystem.GetNegative(Owner);

        if (!await EmotionSystem.TryLevelUp(Owner)) return;

        Flash();

        // 立即抽取（而非延迟到下回合）
        await EmotionDrawSystem.PerformDraw(context.PlayerChoiceContext!, Owner,
            oldLevel, prePos, preNeg);
    }

    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"res://images/Extension/Relics/{GetType().Name}.png",
        IconOutlinePath: $"res://images/Extension/Relics/{GetType().Name}.png",
        BigIconPath: $"res://images/Extension/Relics/{GetType().Name}.png"
    );
}
