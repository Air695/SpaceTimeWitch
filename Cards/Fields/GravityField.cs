using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.Field;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Cards.Fields;

[RegisterCard(typeof(SpaceTimeWitchCardPool))]
public sealed class GravityField : FieldCardTemplate
{

    public override FieldBackgroundType BackgroundType => FieldBackgroundType.Replace;

    public override string BackgroundPath => "res://images/SpaceTimeWitch/Field/GravityFieldBg.png";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
    ];

    public GravityField() : base(
        baseCost: 0,
        rarity: CardRarity.Common
        )
    {
        SetChronoMarkCost(1);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        FieldCmd.BeginReplace();

        var existing = Owner.Creature.Powers.OfType<FieldPowerBase>().FirstOrDefault();
        if (existing != null)
            await PowerCmd.Remove(existing);

        FieldCmd.RestoreBackground();

        var allTargets = CombatState.Allies
            .Concat(CombatState.Enemies)
            .Where(c => !c.IsDead);
        foreach (var creature in allTargets)
        {
            await ApplyFieldPowerToCreature(choiceContext, cardPlay, creature);
        }

        FieldCmd.ApplyBackground(BackgroundType, BackgroundPath);
        FieldCmd.EndReplace();
    }

    protected override async Task ApplyFieldPowerToCreature(
        PlayerChoiceContext ctx, CardPlay play, Creature creature)
    {
        await PowerCmd.Apply<GravityFieldPower>(ctx, creature, 1m, creature, this);
    }

    protected override void OnUpgrade()
    {
        SetChronoMarkCost(1);
    }
}
