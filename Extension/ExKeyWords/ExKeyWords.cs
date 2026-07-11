using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using SpaceTimeWitch.Scripts;

namespace SpaceTimeWitch.Extension.ExKeyWords;

[RegisterOwnedCardKeyword(nameof(Individual), CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
[RegisterOwnedCardKeyword(nameof(Summation), CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
public class ExK
{
    public static readonly CardKeyword Individual =
        ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Individual)).GetModCardKeyword();
    public static readonly CardKeyword Summation =
        ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Summation)).GetModCardKeyword();
}
