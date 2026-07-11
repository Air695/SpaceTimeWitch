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
public sealed class MirrorRealm : FieldCardTemplate
{
    public override FieldBackgroundType BackgroundType => FieldBackgroundType.Replace;
    public override string BackgroundPath => "res://images/SpaceTimeWitch/Field/MirrorRealmBg.png";

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    public MirrorRealm() : base(baseCost: 2, rarity: CardRarity.Rare) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        FieldCmd.BeginReplace();

        var existing = Owner.Creature.Powers.OfType<FieldPowerBase>().FirstOrDefault();
        if (existing != null)
            await PowerCmd.Remove(existing);

        FieldCmd.RestoreBackground();

        foreach (var creature in CombatState.Allies.Where(c => !c.IsDead))
        {
            await ApplyFieldPowerToCreature(choiceContext, cardPlay, creature);
        }

        FieldCmd.ApplyBackground(BackgroundType, BackgroundPath);
        FieldCmd.EndReplace();
    }

    protected override async Task ApplyFieldPowerToCreature(
        PlayerChoiceContext ctx, CardPlay play, Creature creature)
    {
        await PowerCmd.Apply<MirrorRealmPower>(ctx, creature, 1m, creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}