using Godot;
using STS2RitsuLib;
using STS2RitsuLib.Combat.SecondaryResources;

namespace SpaceTimeWitch.Extension.QG;

/// <summary>
/// QG 次级资源注册 — 正面/负面情感 + 战斗UI计数器。
/// </summary>
public static class QGResources
{
    public static string PositiveId { get; private set; } = string.Empty;
    public static string NegativeId { get; private set; } = string.Empty;

    private static bool s_registered;
    private static NSecondaryResourceCounter? s_positiveCounter;
    private static NSecondaryResourceCounter? s_negativeCounter;

    public static void ShowCounters()
    {
        if (s_positiveCounter != null) s_positiveCounter.Visible = true;
        if (s_negativeCounter != null) s_negativeCounter.Visible = true;
    }

    public static void HideCounters()
    {
        if (s_positiveCounter != null) s_positiveCounter.Visible = false;
        if (s_negativeCounter != null) s_negativeCounter.Visible = false;
    }

    public static void Register(string modId)
    {
        if (s_registered) return;
        s_registered = true;

        var registry = RitsuLibFramework.GetSecondaryResourceRegistry(modId);

        var posDef = registry.Register("qg_positive", new SecondaryResourceDefinition(
            defaultAmount: 0,
            baseMaxAmount: EmotionSystem.GetCap(0),
            turnStartPolicy: SecondaryResourceTurnStartPolicy.None,
            persistencePolicy: SecondaryResourcePersistencePolicy.Combat,
            smallIconPath: "res://images/Extension/UI/Positive.png",
            largeIconPath: "res://images/Extension/UI/Positive.png"
        ));
        PositiveId = posDef.Id;

        var negDef = registry.Register("qg_negative", new SecondaryResourceDefinition(
            defaultAmount: 0,
            baseMaxAmount: EmotionSystem.GetCap(0),
            turnStartPolicy: SecondaryResourceTurnStartPolicy.None,
            persistencePolicy: SecondaryResourcePersistencePolicy.Combat,
            smallIconPath: "res://images/Extension/UI/Negative.png",
            largeIconPath: "res://images/Extension/UI/Negative.png"
        ));
        NegativeId = negDef.Id;

        // 移除 AlwaysShowInCombatUi — 改为获取情感后才显示

        // 正面情感计数器（时痕下方），初始隐藏
        registry.RegisterCombatUi(
            "qg_positive_counter",
            parent =>
            {
                var counter = NSecondaryResourceCounter.Create(posDef,
                    new SecondaryResourceCounterStyle
                    {
                        FontSize = 24,
                        PositiveColor = Colors.Green,
                        IconStyle = SecondaryResourceIconStyle.Default with
                        {
                            Size = new Vector2(48, 48),
                        },
                    });
                counter.Visible = false;
                var ec = parent.GetNode<Control>("%EnergyCounterContainer");
                counter.Position = ec.Position + new Vector2(152, 10);
                s_positiveCounter = counter;
                return counter;
            },
            ctx => ctx.Node.Bind(ctx.Player)
        );

        // 负面情感计数器（正面下方），初始隐藏
        registry.RegisterCombatUi(
            "qg_negative_counter",
            parent =>
            {
                var counter = NSecondaryResourceCounter.Create(negDef,
                    new SecondaryResourceCounterStyle
                    {
                        FontSize = 24,
                        PositiveColor = Colors.Red,
                        IconStyle = SecondaryResourceIconStyle.Default with
                        {
                            Size = new Vector2(48, 48),
                        },
                    });
                counter.Visible = false;
                var ec = parent.GetNode<Control>("%EnergyCounterContainer");
                counter.Position = ec.Position + new Vector2(152, 60);
                s_negativeCounter = counter;
                return counter;
            },
            ctx => ctx.Node.Bind(ctx.Player)
        );
    }
}
