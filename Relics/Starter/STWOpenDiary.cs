using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.Commands;
using SpaceTimeWitch.Powers;
using SpaceTimeWitch.Scripts;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace SpaceTimeWitch.Relics.Starter;

[RegisterRelic(typeof(SpaceTimeWitchRelicPool))]
public class STWOpenDiary : SpaceTimeWitchRelics
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("ChronoMark", 3m),
        new DynamicVar("ChronoMarkPerTurn", 1m),
    ];

    public STWOpenDiary() : base(RelicRarity.Starter) { }

    public override async Task BeforeCombatStart()
    {
        Flash();

        // 预见特定敌人：施加对应防护能力
        await ApplyForesightPowers();

        await ChronoMark.Gain(Owner.Creature, (int)DynamicVars["ChronoMark"].BaseValue);
    }

    private async Task ApplyForesightPowers()
    {
        if (Owner?.Creature == null) return;
        var enemies = Owner.Creature.CombatState.Enemies;
        var hasLivingFog = enemies.Any(e => e.Monster is LivingFog);
        var hasInfestedPrism = enemies.Any(e => e.Monster is InfestedPrism);

        if (hasLivingFog)
            await PowerCmd.Apply<NoSmoggy>(null!, Owner.Creature, 1, Owner.Creature, null);

        if (hasInfestedPrism)
            await PowerCmd.Apply<NoTainted>(null!, Owner.Creature, 1, Owner.Creature, null);
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext ctx, Player player)
    {
        if (player.Creature != Owner?.Creature) return;
        await ChronoMark.Gain(Owner.Creature, (int)DynamicVars["ChronoMarkPerTurn"].BaseValue);
    }

    public override async Task AfterActEntered()
    {
        var rng = Owner.RunState.Rng.UpFront;
        var tagRelics = Owner.Relics.OfType<ITagRelic>().ToList();
        var ownedLimited = Owner.Relics.OfType<ITagRelic>()
            .Select(r => r.Class)
            .Where(c => TagRelicConfig.IsClassLimited(c)).ToHashSet();

        foreach (var relic in tagRelics)
        {
            var nextTypes = relic.NextTierRelicTypes.Concat(relic.NextTierWeights.Keys)
                .Distinct().ToList();
            if (nextTypes.Count == 0) continue;

            var allowed = nextTypes
                .Where(t => !ownedLimited.Contains(TagRelicRegistry.Entries[t].Class)
                    || TagRelicRegistry.Entries[t].Class == relic.Class).ToList();
            if (allowed.Count == 0) continue;

            var picked = TagRelicRegistry.WeightedPick(allowed,
                t => relic.NextTierWeights.GetValueOrDefault(t, 1.0), rng);
            if (picked == null) continue;

            var instance = ModelDb.GetById<RelicModel>(ModelDb.GetId(picked)).ToMutable();
            await RelicCmd.Replace((RelicModel)relic, instance);
        }
    }

    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"res://images/SpaceTimeWitch/Relics/{GetType().Name}.png",
        IconOutlinePath: $"res://images/SpaceTimeWitch/Relics/{GetType().Name}.png",
        BigIconPath: $"res://images/SpaceTimeWitch/Relics/{GetType().Name}.png"
    );
}
