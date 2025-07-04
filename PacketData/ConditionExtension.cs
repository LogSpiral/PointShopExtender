using Microsoft.Xna.Framework.Graphics;
using PointShop.ShopSystem;
using ReLogic.Content;
using System;
using System.IO;
using System.Reflection;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using YamlDotNet.Serialization;
using static Terraria.Localization.NetworkText;

namespace PointShopExtender.PacketData;

public sealed class ConditionExtension
{
    public string Name { get; set; }
    public string DisplayNameZH { get; set; }
    public string DisplayNameEN { get; set; }
    public string DescriptionZH { get; set; }
    public string DescriptionEN { get; set; }
    public string Icon { get; set; }
    public string Condition { get; set; }

    [YamlIgnore]
    public Asset<Texture2D> IconTexture { get; set; } = ModAsset.UnlockConditionIconDefault;

    [YamlIgnore]
    public RealCondition RealCondition { get; set; }

    public void Save(string path)
    {
        Directory.CreateDirectory(path);
        var content = PointShopExtenderSystem.YamlSerializer.Serialize(this, typeof(ConditionExtension));
        File.WriteAllText(Path.Combine(path, Name + ".yaml"), content);
    }

    public void Register()
    {
        PointShopExtenderSystem.ConditionDisplayNameZH[Name] = DisplayNameZH ?? Name;
        PointShopExtenderSystem.ConditionDisplayNameEN[Name] = DisplayNameEN ?? Name;
        PointShopExtenderSystem.ConditionDescriptionZH[Name] = DescriptionZH ?? "";
        PointShopExtenderSystem.ConditionDescriptionEN[Name] = DescriptionEN ?? "";

        Language.GetOrRegister($"Mods.PointShopExtender.UnlockCondition.{Name}.DisplayName", () => Name);
        Language.GetOrRegister($"Mods.PointShopExtender.UnlockCondition.{Name}.Description", () => "");

        PointShopSystem.RegisterUnlockCondition(PointShopExtender.Instance, Name, IconTexture, RealCondition.ToFunc());
    }

    public static ConditionExtension FromFile(string filePath)
    {
        var result = PointShopExtenderSystem.YamlDeserializer.Deserialize<ConditionExtension>(File.ReadAllText(filePath));
        result.RealCondition = RealCondition.FromString(result.Condition);

        #region 加载图标
        var iconPath = result.Icon;
        if (!ModContent.RequestIfExists<Texture2D>(iconPath, out var icon))
        {
            if (!File.Exists(iconPath))
                iconPath = Path.Combine(Path.GetFullPath(filePath)[..^Path.GetFileName(filePath).Length], $"{(iconPath == "" ? result.Name : iconPath)}.png");

            using var stream = File.OpenRead(iconPath);
            icon = PointShopExtender.Instance.Assets.CreateUntracked<Texture2D>(stream, iconPath);
        }
        result.IconTexture = icon;
        #endregion


        return result;
    }
}
