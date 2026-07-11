using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using SpaceTimeWitch.Cards;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.Commands;
using SpaceTimeWitch.Scripts;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace SpaceTimeWitch.Relics.Starter;

[RegisterRelic(typeof(SpaceTimeWitchRelicPool))]
[RegisterCharacterStarterRelic(typeof(SpaceTimeWitch.Character.SpaceTimeWitch))]
public class STWDiary : SpaceTimeWitchRelics
{
    public STWDiary()
        : base(RelicRarity.Starter)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("ChronoMark", 3m),
    ];

    private bool _hasSelectedTagRelics;

    [SavedProperty]
    public bool HasSelectedTagRelics
    {
        get => _hasSelectedTagRelics;
        set
        {
            AssertMutable();
            _hasSelectedTagRelics = value;
        }
    }

    public override async Task BeforeCombatStart()
    {
        Flash();
        await ChronoMark.Gain(
            Owner.Creature,
            DynamicVars["ChronoMark"].BaseValue);

        // 只在首次战斗触发一次，抽取 N 个遗物
        if (HasSelectedTagRelics) return;
        HasSelectedTagRelics = true;

        // ★ 自定义每次抽取的数量
        const int relicsToPick = 3; // 改成你想要的数字

        var rng = Owner.RunState.Rng.UpFront;

        // 已拥有的受限 class，避免选取同 class 遗物（读取配置中的去重开关）
        var ownedLimitedClasses = Owner.Relics
            .OfType<ITagRelic>()
            .Select(r => r.Class)
            .Where(c => TagRelicConfig.IsClassLimited(c))
            .ToHashSet();

        // 用于避免重复抽取同一个遗物类型
        var pickedRelicIds = new HashSet<ModelId>();

        for (int i = 0; i < relicsToPick; i++)
        {
            var candidates = ModelDb.RelicPool<SpaceTimeWitchExRelicPool>()
                .AllRelics
                .OfType<ITagRelic>()
                .Where(r => r.Tier == 1
                            && !ownedLimitedClasses.Contains(r.Class)
                            && !pickedRelicIds.Contains(((RelicModel)r).Id)) // 防止重复
                .GroupBy(r => r.CharacterGroup)
                .ToList();

            if (candidates.Count == 0) break;

            // 按角色组权重选取一个组（读取配置中的组权重）
            var group = TagRelicRegistry.WeightedPick(candidates,
                g => TagRelicConfig.GetEffectiveGroupWeight(g.Key),
                rng);
            if (group == null) continue;

            // 在组内按遗物权重选取一个
            var list = group.ToList();
            var picked = TagRelicRegistry.WeightedPick(list, r => r.Weight, rng);
            if (picked == null) continue;

            var instance = ((RelicModel)picked).ToMutable();
            await RelicCmd.Obtain(instance, Owner);

            // 记录已抽取的遗物 ID，避免下次重复
            pickedRelicIds.Add(((RelicModel)picked).Id);

            // 如果该遗物 class 是受限类型，也加入排除列表
            if (TagRelicConfig.IsClassLimited(picked.Class))
                ownedLimitedClasses.Add(picked.Class);
        }
    }
    
    public override async Task AfterActEntered()
    {
        var rng = Owner.RunState.Rng.UpFront;
        var tagRelics = Owner.Relics.OfType<ITagRelic>().ToList();
        // 升级时同样过滤已拥有的受限 class（排除正在被替换的遗物自身）
        var ownedLimitedClasses = Owner.Relics
            .OfType<ITagRelic>()
            .Select(r => r.Class)
            .Where(c => TagRelicConfig.IsClassLimited(c))
            .ToHashSet();

        foreach (var relic in tagRelics)
        {
            // 合并 NextTierRelicTypes 与 NextTierWeights.Keys，确保两种定义方式都能生效
            var nextTypes = relic.NextTierRelicTypes
                .Concat(relic.NextTierWeights.Keys)
                .Distinct()
                .ToList();
            if (nextTypes.Count == 0) continue;

            // 过滤掉与已拥有受限 class 冲突的升级分支（排除自身 class）
            var allowedNext = nextTypes
                .Where(t => !ownedLimitedClasses.Contains(
                    TagRelicRegistry.Entries[t].Class)
                    || TagRelicRegistry.Entries[t].Class == relic.Class)
                .ToList();
            if (allowedNext.Count == 0) continue;

            var nextWeights = relic.NextTierWeights;
            var picked = TagRelicRegistry.WeightedPick(allowedNext,
                t => nextWeights.TryGetValue(t, out var w) ? w : 1.0,
                rng);
            if (picked == null) continue;
            var nextId = ModelDb.GetId(picked);
            var instance = ModelDb.GetById<RelicModel>(nextId).ToMutable();
            await RelicCmd.Replace((RelicModel)relic, instance);
        }
    }
    
    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"res://images/SpaceTimeWitch/Relics/{GetType().Name}.png",
        IconOutlinePath: $"res://images/SpaceTimeWitch/Relics/{GetType().Name}.png",
        BigIconPath: $"res://images/SpaceTimeWitch/Relics/{GetType().Name}.png"
    );
}