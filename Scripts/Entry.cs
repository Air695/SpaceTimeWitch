using System.Linq;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Modding;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.Extension.QG;
using SpaceTimeWitch.Extension.QG.WXC;
using SpaceTimeWitch.Nodes;
using SpaceTimeWitch.Relics;
using STS2RitsuLib.Models.Capabilities;
using STS2RitsuLib;
using STS2RitsuLib.CardPiles;
using STS2RitsuLib.Content;
using STS2RitsuLib.Data;
using STS2RitsuLib.Interop;
using STS2RitsuLib.Settings;
using STS2RitsuLib.Utils.Persistence;
using STS2RitsuLib.Scaffolding.Cards.HandOutline;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using SpaceTimeWitch.Cards.Basic;
using SpaceTimeWitch.Cards.Ancient;
using SpaceTimeWitch.Relics.Starter;

namespace SpaceTimeWitch.Scripts;

[ModInitializer(nameof(Init))]
public class Entry
{
    public const string ModId = "SpaceTimeWitch";
    public static readonly MegaCrit.Sts2.Core.Logging.Logger Logger = RitsuLibFramework.CreateLogger(ModId);

    /// <summary>个人空间牌堆类型，由 RitsuLib 注册。</summary>
    public static PileType PersonalSpacePile;

    /// <summary>标签遗物卡池查看牌堆类型，由 RitsuLib 注册。</summary>
    public static PileType TagRelicPoolPile;

    // DataStore key
    private const string SettingsDataKey = "settings";


    public static void Init()
    {
        var assembly = Assembly.GetExecutingAssembly();
        RitsuLibFramework.EnsureGodotScriptsRegistered(assembly, Logger);
        ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);
        var registry = ModContentRegistry.For(ModId);
        registry.RegisterSharedCardPool(typeof(SpaceTimeWitchExCardPool));
        registry.RegisterCardLibraryCompendiumSharedPoolFilter(
            stableId: "SpaceTimeWitchExCardPool",
            iconTexturePath: "res://images/SpaceTimeWitch/UI/ChronoMark.png",
            cardPoolType: typeof(SpaceTimeWitchExCardPool)
        );
        registry.RegisterSharedCardPool(typeof(STWEGO));
        registry.RegisterCardLibraryCompendiumSharedPoolFilter(
            stableId: "STWEGO",
            iconTexturePath: "res://images/Extension/UI/1.png",
            cardPoolType: typeof(STWEGO)
        );
        registry.RegisterSharedCardPool(typeof(STWLRA));
        registry.RegisterCardLibraryCompendiumSharedPoolFilter(
            stableId: "STWLRA",
            iconTexturePath: "res://images/Extension/UI/1.png",
            cardPoolType: typeof(STWLRA)
        );
        registry.RegisterSharedRelicPool(typeof(SpaceTimeWitchExRelicPool));
        RitsuLibFramework.RegisterArchaicToothTranscendenceMapping<ChronoScribe, GravenEternityDawn>();
        RitsuLibFramework.RegisterTouchOfOrobasRefinementMapping<STWDiary, STWOpenDiary>();
      
        
        // 注册个人空间牌堆（RitsuLib 卡牌堆系统）
        var pileRegistry = ModCardPileRegistry.For(ModId);
        PersonalSpacePile = pileRegistry.RegisterOwned("personal_space", new ModCardPileSpec
        {
            Scope = ModCardPileScope.CombatOnly,
            Style = ModCardPileUiStyle.BottomRight,
            Anchor = new ModCardPileAnchor(ModCardPileAnchorKind.BottomRightPrimary, new Vector2(100, -80)),
            IconPath = "res://images/SpaceTimeWitch/UI/PersonalSpace.png",
            VisibleWhen = ctx => ctx.Player != null && (ctx.Pile?.Cards.Count ?? 0) > 0,
        }).PileType;

        // 注册标签遗物卡池查看牌堆（顶栏牌组按钮右侧）
        TagRelicPoolPile = pileRegistry.RegisterOwned("tag_relic_pool", new ModCardPileSpec
        {
            Scope = ModCardPileScope.RunPersistent,
            Style = ModCardPileUiStyle.TopBarDeck,
            Anchor = new ModCardPileAnchor(ModCardPileAnchorKind.TopBarAfterDeck, Vector2.Zero),
            IconPath = "res://images/SpaceTimeWitch/Relics/ChronoAstrolabe.png",
            VisibleWhen = ctx => ctx.Player != null
                && ctx.Player.Relics.OfType<ITagRelic>().Any(),
        }).PileType;

        // 注册 DataStore
        ModDataStore.For(ModId).Register(
            key: SettingsDataKey,
            fileName: "settings.json",
            scope: SaveScope.Global,
            defaultFactory: () => new SpaceTimeWitchSettingsData(),
            autoCreateIfMissing: true);

        // 注册 ChronoMark 次级资源（替代旧 ChronoMarkPower）
        ModChronoResources.Register();

        // 注册 QG 情感系统（次级资源 + 进度条）
        QGInit.Register(ModId);

        RegisterModSettings();

        // SLFriendP 标记卡牌泛红光
        ModCardHandOutlineRegistry.Register<CardModel>(ModCardHandOutlineRules.Fixed(
            card => SLFriendP.MarkedCard != null && card == SLFriendP.MarkedCard,
            Colors.Red));

        var harmony = new Harmony("SpaceTimeWitch.TemporalFlow");
        harmony.PatchAll();
    }

    private static void RegisterModSettings()
    {
        // 根绑定
        var rootBinding = new ModSettingsValueBinding<SpaceTimeWitchSettingsData,
            SpaceTimeWitchSettingsData>(
            ModId, SettingsDataKey, SaveScope.Global,
            s => s, (_, _) => { });

        // 概率子项绑定（给自定义控件用）
        var commonWeightBinding = new ModSettingsValueBinding<SpaceTimeWitchSettingsData, int>(
            ModId, SettingsDataKey, SaveScope.Global,
            static s => s.CommonWeight,
            static (s, v) => s.CommonWeight = v);

        var uncommonWeightBinding = new ModSettingsValueBinding<SpaceTimeWitchSettingsData, int>(
            ModId, SettingsDataKey, SaveScope.Global,
            static s => s.UncommonWeight,
            static (s, v) => s.UncommonWeight = v);

        var rareWeightBinding = new ModSettingsValueBinding<SpaceTimeWitchSettingsData, int>(
            ModId, SettingsDataKey, SaveScope.Global,
            static s => s.RareWeight,
            static (s, v) => s.RareWeight = v);

        var lockBinding = new ModSettingsValueBinding<SpaceTimeWitchSettingsData, LockTarget>(
            ModId, SettingsDataKey, SaveScope.Global,
            static s => s.LockedRarity,
            static (s, v) => s.LockedRarity = v);

        // 复现可选卡牌数量绑定
        var discoverOfferCountBinding = new ModSettingsValueBinding<SpaceTimeWitchSettingsData, int>(
            ModId, SettingsDataKey, SaveScope.Global,
            static s => s.DiscoverOfferCount,
            static (s, v) => s.DiscoverOfferCount = v);

        // 同步初始值到运行时
        SpaceTimeWitchSettings.SyncFrom(
            commonWeightBinding.Read(),
            uncommonWeightBinding.Read(),
            rareWeightBinding.Read(),
            lockBinding.Read());

        SpaceTimeWitchSettings.SyncDiscoverOfferCount(discoverOfferCountBinding.Read());

        // 配置页
        RitsuLibFramework.RegisterModSettings(ModId, page =>
        {
            page.AddSection("probability", probability =>
            {
                probability.WithTitle(ModSettingsText.LocString(
                    "settings_ui", "SETTINGS.PROBABILITY", "概率控制"));
                probability.AddCustom("probability_control",
                    ModSettingsText.Literal(""),
                    host =>
                    {
                        var title = ModSettingsText.LocString(
                            "settings_ui", "SETTINGS.PROBABILITY", "概率控制").Resolve();
                        var lockC = ModSettingsText.LocString(
                            "settings_ui", "SETTINGS.LOCK_COMMON", "锁定普通").Resolve();
                        var lockU = ModSettingsText.LocString(
                            "settings_ui", "SETTINGS.LOCK_UNCOMMON", "锁定罕见").Resolve();
                        var lockR = ModSettingsText.LocString(
                            "settings_ui", "SETTINGS.LOCK_RARE", "锁定稀有").Resolve();
                        var reset = ModSettingsText.LocString(
                            "settings_ui", "SETTINGS.RESET_PROBABILITY", "重置为默认").Resolve();
                        return new NProbabilitySettingsControl(
                            host, rootBinding,
                            commonWeightBinding, uncommonWeightBinding, rareWeightBinding,
                            lockBinding,
                            title, lockC, lockU, lockR, reset);
                    },
                    ModSettingsText.Literal(""),
                    () => true);
            });

            page.AddSection("discover", discover =>
            {
                discover.WithTitle(ModSettingsText.LocString(
                    "settings_ui", "SETTINGS.DISCOVER", "复现设置"));
                discover.AddCustom("discover_offer_count",
                    ModSettingsText.Literal(""),
                    host =>
                    {
                        var title = ModSettingsText.LocString(
                            "settings_ui", "SETTINGS.DISCOVER_OFFER_COUNT", "可选卡牌数量").Resolve();
                        var reset = ModSettingsText.LocString(
                            "settings_ui", "SETTINGS.RESET_DISCOVER", "重置为默认").Resolve();
                        return new NDiscoverOfferCountControl(
                            host, rootBinding, discoverOfferCountBinding,
                            title, reset);
                    },
                    ModSettingsText.Literal(""),
                    () => true);
            });
        });
    }
}
