using SpaceTimeWitch.Extension.MCJ.Card1;
using SpaceTimeWitch.Extension.MCJ.Card2;
using SpaceTimeWitch.Extension.MCJ.Card3;
using SpaceTimeWitch.Extension.MCJ.Card4;
using SpaceTimeWitch.Extension.MCJ.Card5;
using SpaceTimeWitch.Extension.MCJ.Card6;
using SpaceTimeWitch.Extension.MCJ.Card7;

namespace SpaceTimeWitch.Extension.MCJ;

/// <summary>
/// 镐类遗物的卡牌绑定管理器 —— 管理 Card1~Card7 七个槽位各自绑定的卡牌类型。
/// 绑定在游戏过程中可灵活更换；升级镐不会重置绑定。
///
/// 存储方式：通过 SavedAttachedState&lt;string&gt; 持久化，本类负责序列化/反序列化。
/// 格式：逗号分隔的 Type.FullName。空槽位为空字符串。
/// </summary>
public class PickaxeCardBindings
{
    public const int SlotCount = 7;

    private readonly Type?[] _slots;

    private PickaxeCardBindings(Type?[] slots)
    {
        _slots = slots;
    }

    /// <summary>Card1~Card7 的卡牌类型（可读写）。索引 0 = Card1，以此类推。</summary>
    public Type? this[int slot]
    {
        get => slot >= 0 && slot < SlotCount ? _slots[slot] : null;
        set { if (slot >= 0 && slot < SlotCount) _slots[slot] = value; }
    }

    /// <summary>所有槽位的只读快照。</summary>
    public IReadOnlyList<Type?> AllSlots => _slots;

    /// <summary>设置指定槽位的卡牌类型。</summary>
    public void SetSlot(int slot, Type cardType)
    {
        if (slot >= 0 && slot < SlotCount)
            _slots[slot] = cardType;
    }

    /// <summary>获取指定槽位的卡牌类型。</summary>
    public Type? GetSlot(int slot) => this[slot];

    /// <summary>替换指定槽位的卡牌，返回旧类型。</summary>
    public Type? SwapSlot(int slot, Type newCardType)
    {
        var old = _slots[slot];
        _slots[slot] = newCardType;
        return old;
    }

    /// <summary>清空指定槽位。</summary>
    public void ClearSlot(int slot) => _slots[slot] = null;

    // ── 序列化 ───────────────────────────────────────

    /// <summary>序列化为持久化字符串（逗号分隔的 Type.FullName）。空槽位为空字符串。</summary>
    public string Serialize()
    {
        var names = _slots.Select(t => t?.FullName ?? "");
        return string.Join(",", names);
    }

    /// <summary>从持久化字符串反序列化。</summary>
    public static PickaxeCardBindings Deserialize(string data)
    {
        var slots = new Type?[SlotCount];
        if (string.IsNullOrEmpty(data))
            return new PickaxeCardBindings(slots);

        var names = data.Split(',');
        for (int i = 0; i < Math.Min(names.Length, SlotCount); i++)
        {
            if (!string.IsNullOrEmpty(names[i]))
                slots[i] = Type.GetType(names[i]);
        }
        return new PickaxeCardBindings(slots);
    }

    // ── 默认值（所有镐类共用）───────────────────────

    /// <summary>所有镐类遗物的默认卡牌绑定（木镐/石镐/铁镐等均相同）。</summary>
    public static PickaxeCardBindings CreateDefaults() => new(new[]
    {
        typeof(STWWoodenSword),    // Card1
        typeof(STWBow),            // Card2
        typeof(STWDoor),           // Card3
        typeof(STWLeatherBoots),   // Card4
        typeof(STWWoodenSpear),    // Card5
        typeof(STWShield),         // Card6
        typeof(STWBeacon1),        // Card7
    });

    /// <summary>默认绑定的序列化字符串（供 SavedAttachedState 默认值）。</summary>
    public static string DefaultsSerialized => CreateDefaults().Serialize();
}
