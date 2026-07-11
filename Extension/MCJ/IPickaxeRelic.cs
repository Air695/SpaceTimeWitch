namespace SpaceTimeWitch.Extension.MCJ;

/// <summary>
/// 镐类遗物通用接口 —— 统一操作任意等级镐的材料、升级进度、卡牌绑定、升级。
/// </summary>
public interface IPickaxeRelic
{
    /// <summary>当前材料数量（可读写，用于转移材料）。</summary>
    int MaterialCount { get; set; }

    /// <summary>当前升级进度（可读写）。</summary>
    int UpgradeProgressCount { get; set; }

    /// <summary>是否有足够材料可供消费。</summary>
    bool CanConsumeMaterial(int amount);

    /// <summary>消费指定数量的材料（会同步更新遗物描述显示）。</summary>
    void ConsumeMaterial(int amount);

    /// <summary>是否有下一级镐可供升级。</summary>
    bool HasNextTier { get; }

    /// <summary>升级到下一级镐，转移材料、绑定数据。返回 true 表示升级成功。</summary>
    Task<bool> UpgradeToNextTier();

    /// <summary>获取卡牌绑定的序列化数据（Card1~Card7）。</summary>
    string GetCardBindingsData();

    /// <summary>设置卡牌绑定的序列化数据（Card1~Card7）。</summary>
    void SetCardBindingsData(string data);

    /// <summary>刷新遗物描述中的动态变量显示。</summary>
    void RefreshDynamicVars();
}
