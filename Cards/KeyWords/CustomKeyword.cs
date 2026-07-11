using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace SpaceTimeWitch.Cards.KeyWords;

public static class CustomKeyword
{
    /// <param name="vars">动态变量，其值会替换本地化文本中的 {VarName} 占位符</param>
    public static IHoverTip GetHoverTip(string key, params DynamicVar[] vars)
    {
        var title = new LocString("card_keywords", key + ".title");
        var desc = new LocString("card_keywords", key + ".description");
        foreach (var v in vars)
        {
            title.Add(v);
            desc.Add(v);
        }
        return new HoverTip(title, desc);
    }
}
