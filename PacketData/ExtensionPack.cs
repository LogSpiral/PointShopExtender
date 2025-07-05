using Microsoft.Xna.Framework.Graphics;
using PointShopExtender.PacketManager;
using ReLogic.Content;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Terraria;
using Terraria.GameContent;

namespace PointShopExtender.PacketData;

public sealed class ExtensionPack
{
    public string PackName { get; set; } = "";
    public string DisplayName { get; private set; } = "";
    public string DisplayNameEn { get; private set; } = "";
    public string AuthorName { get; private set; } = "";
    public string PackVersion { get; private set; } = "";

    public Asset<Texture2D> Icon { get; private set; } = ModAsset.PacketIconDefault;

    public HashSet<ShopExtension> ShopExtensions { get; init; } = [];
    public HashSet<EnvironmentExtension> EnvironmentExtensions { get; init; } = [];
    public HashSet<ConditionExtension> ConditionExtensions { get; init; } = [];


    public void Register()
    {
        foreach (EnvironmentExtension environment in EnvironmentExtensions)
            environment.Register();

        foreach (ConditionExtension conditionExtension in ConditionExtensions)
            conditionExtension.Register();

        foreach (ShopExtension shop in ShopExtensions)
            shop.Register();
    }

    public static ExtensionPack FromDirectory(string packPath)
    {
        #region 初始化
        var result = new ExtensionPack();
        var packName = Path.GetFileName(packPath);
        result.PackName = packName;
        #endregion

        #region 加载文本信息
        var packInfoPath = Path.Combine(packPath, "PackInfo.txt");
        if (File.Exists(packInfoPath))
        {
            var info = File.ReadAllLines(packInfoPath);
            var length = info.Length;
            if (length > 0)
                result.DisplayName = info[0];
            if (length > 1)
                result.DisplayNameEn = info[1];
            if (length > 2)
                result.AuthorName = info[2];
            if (length > 3)
                result.PackVersion = info[3];
        }
        #endregion

        #region 加载图标
        var packIconPath = Path.Combine(packPath, "icon.png");
        if (File.Exists(packIconPath))
        {
            using var stream = File.OpenRead(packIconPath);
            result.Icon = PointShopExtender.Instance.Assets.CreateUntracked<Texture2D>(stream, packIconPath);
        }
        #endregion

        #region 加载拓展条件
        var conditionPath = Path.Combine(packPath, "Conditions");
        if (Directory.Exists(conditionPath))
            foreach (var file in Directory.GetFiles(conditionPath, "*.yaml"))
            {
                var condition = ConditionExtension.FromFile(file);
                result.ConditionExtensions.Add(condition);
            }
        #endregion

        #region 加载拓展环境
        var environmentPath = Path.Combine(packPath, "Environments");
        if (Directory.Exists(environmentPath))
            foreach (var file in Directory.GetFiles(environmentPath, "*.yaml"))
            {
                var environment = EnvironmentExtension.FromFile(file);
                result.EnvironmentExtensions.Add(environment);
            }
        #endregion

        #region 加载拓展商店
        var shopPath = Path.Combine(packPath, "Shops");
        if (Directory.Exists(shopPath))
            foreach (var file in Directory.GetFiles(shopPath, "*.yaml"))
            {
                var shop = ShopExtension.FromFile(file);
                result.ShopExtensions.Add(shop);
            }
        #endregion
        return result;
    }

    public void Save(string rootPath)
    {
        #region 初始化
        var packPath = Path.Combine(rootPath, PackName);
        Directory.CreateDirectory(packPath);
        #endregion

        SaveInfo(rootPath);
        SaveIcon(rootPath);

        #region 保存拓展条件
        foreach (var condition in ConditionExtensions)
            condition.Save(Path.Combine(packPath, "Conditions"));
        #endregion

        #region 保存拓展环境
        foreach (var environment in EnvironmentExtensions)
            environment.Save(Path.Combine(packPath, "Environments"));
        #endregion

        #region 保存拓展商店
        foreach (var shop in ShopExtensions)
            shop.Save(Path.Combine(packPath, "Shops"));
        #endregion
    }

    public void SaveInfo(string rootPath)
    {
        var packPath = Path.Combine(rootPath, PackName);
        #region 保存文本信息
        File.WriteAllLines(Path.Combine(packPath, "PackInfo.txt"), [DisplayName, DisplayNameEn, AuthorName, PackVersion]);
        #endregion
    }

    public void SaveIcon(string rootPath)
    {
        var packPath = Path.Combine(rootPath, PackName);
        #region 保存图标
        if (Icon?.Value is { } iconTexture)
        {
            Main.RunOnMainThread(() =>
            {
                using var stream = new FileStream(Path.Combine(packPath, "icon.png"), FileMode.Create);
                iconTexture.SaveAsPng(stream, iconTexture.Width, iconTexture.Height);
            });
        }
        #endregion
    }

    public void SetIconAndSave(Asset<Texture2D> icon, string rootPath)
    {
        Icon = icon;
        SaveIcon(rootPath);
    }

    public string GetDisplayName()
    {
        if (PacketMakerUI.IsChinese && DisplayName is { Length: > 0 } nameZh)
            return nameZh;
        else if (DisplayNameEn is { Length: > 0 } nameEn)
            return nameEn;
        //else if (Name is { Length: > 0 } nameFile)
        //return nameFile;
        return PackName;
    }
}
