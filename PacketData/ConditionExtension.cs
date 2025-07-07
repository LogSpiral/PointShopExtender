using Microsoft.Xna.Framework.Graphics;
using PointShop.ShopSystem;
using PointShopExtender.PacketManager;
using ReLogic.Content;
using System;
using System.IO;
using Terraria.Localization;
using Terraria.ModLoader;

namespace PointShopExtender.PacketData;

public sealed class ConditionExtension : ExtensionWithInfo
{
    public string DescriptionZH { get; set; } = "";
    public string DescriptionEN { get; set; } = "";

    protected override string Category => "Conditions";

    public void Register()
    {
        PointShopExtenderSystem.ConditionDisplayNameZH[Name] = DisplayNameZH ?? Name;
        PointShopExtenderSystem.ConditionDisplayNameEN[Name] = DisplayNameEN ?? Name;
        PointShopExtenderSystem.ConditionDescriptionZH[Name] = DescriptionZH ?? "";
        PointShopExtenderSystem.ConditionDescriptionEN[Name] = DescriptionEN ?? "";

        // Language.GetOrRegister($"Mods.PointShopExtender.UnlockCondition.{Name}.DisplayName", () => Name);
        // Language.GetOrRegister($"Mods.PointShopExtender.UnlockCondition.{Name}.Description", () => "");

        PointShopSystem.RegisterUnlockCondition(new ExtendedUnlockCondition(this));
    }

    public static ConditionExtension FromFile(string filePath)
    {
        var result = PointShopExtenderSystem.YamlDeserializer.Deserialize<ConditionExtension>(File.ReadAllText(filePath));
        result.RealCondition = RealCondition.FromString(result.Condition);
        result.RealCondition.Owner = result;


        if (!result.TryGetIconViaPath(filePath, out var icon))
            icon = ModAsset.UnlockConditionIconDefault;
        result.IconTexture = icon;


        return result;
    }

    protected override void OnCreateNew() => Packet.ConditionExtensions.Add(this);

    public void SetDescriptionAndSave(string description)
    {
        DescriptionZH = description;
        SaveInfo();
    }

    public void SetDescriptionEnAndSave(string descriptionEn)
    {
        DescriptionEN = descriptionEn;
        SaveInfo();
    }

    protected override void ExtraAutoSetting(string text, bool isChinese)
    {
        var extraHint = PointShopExtenderSystem.GetLocalizationText($"PacketMakerUI.{RealCondition.ConditionType switch
        {
            ConditionType.Vanilla => "Satisfy",
            ConditionType.ModEnvironment => "Surround",
            ConditionType.ModBoss => "Defeat",
            _ => ""
        }}");
        if (RealCondition.ConditionType is ConditionType.None)
            extraHint = "";

        if (isChinese)
            DescriptionZH = extraHint + text;
        else
            DescriptionEN = extraHint + text;
    }

    public string GetDescription()
    {
        if (PacketMakerUI.IsChinese && DescriptionZH is { Length: > 0 } descriptionZh)
            return descriptionZh;
        else if (DescriptionEN is { Length: > 0 } descriptionEn)
            return descriptionEn;
        //else if (Name is { Length: > 0 } nameFile)
        //return nameFile;
        return "";
    }

    class ExtendedUnlockCondition : SimpleUnlockCondition
    {
        ConditionExtension ConditionExtension { get; set; }
        public ExtendedUnlockCondition(ConditionExtension conditionExtension) : base(PointShopExtender.Instance, conditionExtension.Name, conditionExtension.IconTexture, conditionExtension.RealCondition.ToFunc())
        {
            ConditionExtension = conditionExtension;
        }
        public override string DisplayName => ConditionExtension.GetDisplayName();
        public override string Description => ConditionExtension.GetDescription();
    }
}
