using SpaceTimeWitch.Scripts;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace SpaceTimeWitch.Cards.KeyWords;

[RegisterOwnedCardKeyword(nameof(Binding), CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
[RegisterOwnedCardKeyword(nameof(CombatSkill), CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
[RegisterOwnedCardKeyword(nameof(Soulbinding), CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
public class STWKeywords
{
    public static readonly string Binding = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Binding));
    public static readonly string CombatSkill = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(CombatSkill));
    public static readonly string Soulbinding = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Soulbinding));
}