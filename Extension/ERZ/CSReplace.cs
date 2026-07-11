using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SpaceTimeWitch.Cards;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.CombatSkill;
using SpaceTimeWitch.Extension.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using SpaceTimeWitch.Cards.KeyWords;
using STS2RitsuLib.Keywords;

namespace SpaceTimeWitch.Extension.ERZ;

[RegisterCard(typeof(SpaceTimeWitchExCardPool))]
public class CSReplace : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
        CardTags.ERZ1,
        CardTags.ERZ2,
        CardTags.ERZ3
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
    ];


    public CSReplace()
        : base(
            baseCost: 0,
            type: CardType.Skill,
            rarity: CardRarity.Uncommon,
            target: TargetType.Self
        )
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var owner = Owner;
        if (owner?.Creature == null) return;
        
        await PowerCmd.Apply<STWCSReplace>(choiceContext,Owner.Creature,1, Owner.Creature, this);
        CombatSkillManager.GetState(Owner).IsSwapActive = true;
    }

    protected override void OnUpgrade()
    {
        AddKeyword(STWKeywords.Binding.GetModKeywordCardKeyword());
    }

    protected override string PortraitPath => $"res://images/Extension/Cards/{GetType().Name}.png";
}