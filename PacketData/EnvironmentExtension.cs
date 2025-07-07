using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PointShop.ShopSystem;
using PointShopExtender.PacketManager;
using ReLogic.Content;
using System;
using System.Globalization;
using System.IO;
using Terraria;
using Terraria.IO;
using Terraria.Localization;
using Terraria.ModLoader;
using YamlDotNet.Serialization;

namespace PointShopExtender.PacketData;

public sealed class EnvironmentExtension : ExtensionWithInfo
{

    public int Priority { get; set; } = 0;
    public string ColorText { get; set; } = "FFFFFF";

    [YamlIgnore]
    public Color Color { get; set; }

    protected override string Category => "Environments";

    public void Register()
    {
        PointShopExtenderSystem.EnvironmentDisplayNameZH[Name] = DisplayNameZH ?? Name;
        PointShopExtenderSystem.EnvironmentDisplayNameEN[Name] = DisplayNameEN ?? Name;
        // Language.GetOrRegister($"Mods.PointShopExtender.Environments.{Name}", () => Name);

        PointShopSystem.RegisterGameEnvironment(ToGameEnvironmentExtended());
        //PointShopSystem.RegisterGameEnvironment(PointShopExtender.Instance, IconTexture, Name, player => RealCondition.ToFunc().Invoke(), Priority, Color);
    }

    public static EnvironmentExtension FromFile(string filePath)
    {
        var result = PointShopExtenderSystem.YamlDeserializer.Deserialize<EnvironmentExtension>(File.ReadAllText(filePath));
        result.RealCondition = RealCondition.FromString(result.Condition);
        result.RealCondition.Owner = result;
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
        if (!result.TryGetIconViaPath(filePath, out var icon))
            icon = ModAsset.EnvironmentIconDefault;
        result.IconTexture = icon;
        #endregion

        return result;
    }

    public void SetPriorityAndSave(int priority)
    {
        Priority = priority;
        SaveInfo();
    }

    public void SetColorAndSave(Color color)
    {
        Color = color;
        ColorText = color.Hex3();
        SaveInfo();
    }

    public void SetColorTextAndSave(string colorText)
    {
        if (!uint.TryParse(colorText, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var packetValue))
            return;
        uint b = packetValue & 0xFFu;
        uint g = (packetValue >> 8) & 0xFFu;
        uint r = (packetValue >> 16) & 0xFFu;
        Color color = Color.White;
        color.R = (byte)r;
        color.G = (byte)g;
        color.B = (byte)b;
        Color = color;
        ColorText = colorText;
        SaveInfo();
    }

    public GameEnvironment ToGameEnvironmentExtended()
    {
        return new ExtendedEnvironment(this);
    }

    protected override void OnCreateNew() => Packet.EnvironmentExtensions.Add(this);

    class ExtendedEnvironment : GameEnvironment
    {
        public EnvironmentExtension EnvironmentExtension { get; set; }
        public ExtendedEnvironment(EnvironmentExtension extension) : base(PointShopExtender.Instance, extension.IconTexture, extension.Name, extension.Priority, extension.Color)
        {
            EnvironmentExtension = extension;
        }
        public override string DisplayName => EnvironmentExtension.GetDisplayName();
    }
}
