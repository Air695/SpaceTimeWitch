using Godot;
using SpaceTimeWitch.Nodes;
using STS2RitsuLib;
using STS2RitsuLib.Combat.SecondaryResources;

namespace SpaceTimeWitch.Scripts;

/// <summary>
/// ChronoMark（时痕）资源系统 — 基于 RitsuLib SecondaryResource。
/// 替代旧有的 ChronoMarkPower + 手写 UI（NChronoMarkCounter、ChronoMarkIconSwapper）。
/// </summary>
public static class ModChronoResources
{
    public static SecondaryResourceDefinition Definition { get; private set; } = null!;
    public static string Id { get; private set; } = string.Empty;

    public static void Register()
    {
        var registry = RitsuLibFramework.GetSecondaryResourceRegistry(Entry.ModId);

        Definition = registry.Register("chrono_mark", new SecondaryResourceDefinition(
            defaultAmount: 0,
            baseMaxAmount: null,
            turnStartPolicy: SecondaryResourceTurnStartPolicy.None,
            persistencePolicy: SecondaryResourcePersistencePolicy.Combat,
            smallIconPath: "res://images/SpaceTimeWitch/UI/ChronoMark.png",
            largeIconPath: "res://images/SpaceTimeWitch/UI/ChronoMark.png"
        ));
        Id = Definition.Id;

        // 战斗 UI 计数器 — 放在能量计数器旁
        registry.RegisterCombatUi(
            "chrono_mark_counter",
            parent =>
            {
                var counter = new NChronoMarkResourceCounter(Definition.LargeIconPath);
                var energyCounter = parent.GetNode<Control>("%EnergyCounterContainer");
                counter.Position = energyCounter.Position + new Vector2(120, -60);
                return counter;
            },
            ctx => ((NChronoMarkResourceCounter)ctx.Node).Bind(ctx.Player, Id)
        );

        // 卡牌费用图标 — 放在能量图标旁
        registry.RegisterCardUi(
            "chrono_mark_card_ui",
            parent =>
            {
                var ui = NSecondaryResourceCardCostUi.Create(Id, new SecondaryResourceCardCostUiStyle
                {
                    IconSize = new Vector2(48, 48),
                    FontSize = 22,
                });
                var energyIcon = parent.GetNode<TextureRect>("%EnergyIcon");
                ui.Position = energyIcon.Position + new Vector2(0, 70);
                return ui;
            },
            ctx => ctx.Node.Refresh(ctx)
        );

        registry.AlwaysShowInCombatUi(Definition.LocalId);
    }
}
