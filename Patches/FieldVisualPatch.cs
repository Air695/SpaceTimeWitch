namespace SpaceTimeWitch.Patches;

/// <summary>
/// FieldVisualPatch 已简化：
/// 卡牌视觉（Frame/PortraitBorder/AncientTextBg）由 RitsuLib 的 content_asset_override 补丁
/// 根据 FieldCardTemplate.AssetProfile 自动处理，无需额外的 Harmony 补丁。
/// GetResultPileType 改为 FieldCardTemplate 直接覆写。
/// 此文件保留为空壳，防止旧 DLL 残留。
/// </summary>
