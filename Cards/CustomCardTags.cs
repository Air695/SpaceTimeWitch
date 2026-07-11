using MegaCrit.Sts2.Core.Entities.Cards;

namespace SpaceTimeWitch.Cards;

public static class CardTags
{
    public const CardTag Reproduce = (CardTag)2105001;
    public const CardTag MarkA     = (CardTag)2105002;
    public const CardTag Field     = (CardTag)2105003;
    public const CardTag LJSK     = (CardTag)2105004;
    public const CardTag FSK     = (CardTag)2105005;
    public const CardTag CS     = (CardTag)2105006;
    
    
    
    public const CardTag ERZ1       = (CardTag)210501;
    public const CardTag ERZ2       = (CardTag)210502;
    public const CardTag ERZ3       = (CardTag)210503;
    public const CardTag DCB1       = (CardTag)210504;
    public const CardTag DCB2       = (CardTag)210505;
    public const CardTag DCB3       = (CardTag)210506;
    public const CardTag LRL1       = (CardTag)210507;
    public const CardTag LRL2       = (CardTag)210508;
    public const CardTag LRL3       = (CardTag)210509;

    public const CardTag MCJ       = (CardTag)211001;
    
    public const CardTag WXC       = (CardTag)212001;
    public const CardTag WXC1       = (CardTag)212002;
    public const CardTag WXC2       = (CardTag)212003;
    public const CardTag WXC3       = (CardTag)212004;
    public const CardTag WXCE       = (CardTag)212005;

    public static readonly IReadOnlyDictionary<CardTag, string> CustomTagToKeywordKey =
        new Dictionary<CardTag, string>
        {
            { Reproduce, "REPRODUCE" },
            { Field,     "FIELD"     },
            { LJSK,      "LJSK"      },
            { FSK,       "FSK"       },
            { WXC,       "WXC"       },
        };
}
