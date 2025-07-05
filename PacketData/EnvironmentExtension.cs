using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PointShop.ShopSystem;
using PointShopExtender.PacketManager;
using ReLogic.Content;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using Terraria.Localization;
using Terraria.ModLoader;
using YamlDotNet.Core.Tokens;
using YamlDotNet.Serialization;

namespace PointShopExtender.PacketData;

public class EnvironmentExtension
{
    public string Name { get; set; }
    public string DisplayNameZH { get; set; }
    public string DisplayNameEN { get; set; }
    public string Icon { get; set; }
    public string Condition { get; set; }
    public int Priority { get; set; }
    public string ColorText { get; set; }

    [YamlIgnore]
    public Asset<Texture2D> IconTexture { get; set; } = ModAsset.EnvironmentIconDefault;

    [YamlIgnore]
    public RealCondition RealCondition { get; set; }

    [YamlIgnore]
    public Color Color { get; set; }

    public void Register()
    {
        PointShopExtenderSystem.EnvironmentDisplayNameZH[Name] = DisplayNameZH ?? Name;
        PointShopExtenderSystem.EnvironmentDisplayNameEN[Name] = DisplayNameEN ?? Name;
        Language.GetOrRegister($"Mods.PointShopExtender.Environments.{Name}", () => Name);

        PointShopSystem.RegisterGameEnvironment(PointShopExtender.Instance, IconTexture, Name, player => RealCondition.ToFunc().Invoke(), Priority, Color);
    }

    public static EnvironmentExtension FromFile(string filePath)
    {
        var result = PointShopExtenderSystem.YamlDeserializer.Deserialize<EnvironmentExtension>(File.ReadAllText(filePath));
        result.RealCondition = RealCondition.FromString(result.Condition);
        #region 生成颜色
        result.Color = Color.White;
        if (uint.TryParse(result.ColorText, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var packetValue))
        {
            uint b = packetValue & 0xFFu;
            uint g = (packetValue >> 8) & 0xFFu;
            uint r = (packetValue >> 16) & 0xFFu;
            Color color = Color.White;
            color.R = (byte)r;
            color.G = (byte)g;
            color.B = (byte)b;
            result.Color = color;
        }
        else
            result.ColorText = "FFFFFF";
        #endregion

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

    public void Save(string path)
    {
        Directory.CreateDirectory(path);
        var content = PointShopExtenderSystem.YamlSerializer.Serialize(this, typeof(EnvironmentExtension));
        File.WriteAllText(Path.Combine(path,Name + ".yaml"), content);
    }

    public string GetDisplayName()
    {
        if (PacketMakerUI.IsChinese && DisplayNameZH is { Length: > 0 } nameZh)
            return nameZh;
        else if (DisplayNameEN is { Length: > 0 } nameEn)
            return nameEn;
        //else if (Name is { Length: > 0 } nameFile)
        //return nameFile;
        return Name;
    }
}
