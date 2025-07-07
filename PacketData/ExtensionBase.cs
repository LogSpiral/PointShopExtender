using Microsoft.Xna.Framework.Graphics;
using PointShopExtender.PacketManager;
using ReLogic.Content;
using System;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using YamlDotNet.Serialization;

namespace PointShopExtender.PacketData;
public interface INamedFileClass
{
    string Name { get; }
}
public abstract class ExtensionBase : INamedFileClass
{
    public string Name { get; set; } = "";

    [YamlIgnore]
    public ExtensionPack Packet { get; set; }

    protected static readonly string RootPath = PointShopExtenderSystem.RootPath;

    protected string DefaultPath => Path.Combine(RootPath, Packet.PackName, Category);

    protected abstract string Category { get; }
}

public abstract class ExtensionWithInfo : ExtensionBase
{
    public string DisplayNameZH { get; set; } = "";
    public string DisplayNameEN { get; set; } = "";
    public string Icon { get; set; } = "";
    public string Condition { get; set; } = "";

    [YamlIgnore]
    public Asset<Texture2D> IconTexture { get; set; } = ModAsset.EnvironmentIconDefault;

    [YamlIgnore]
    public RealCondition RealCondition { get; set; }

    protected bool TryGetIconViaPath(string filePath, out Asset<Texture2D> icon)
    {
        var iconPath = Icon;
        if (!ModContent.RequestIfExists(iconPath, out icon))
        {
            if (!File.Exists(iconPath))
                iconPath = Path.Combine(Path.GetFullPath(filePath)[..^Path.GetFileName(filePath).Length], $"{(iconPath == "" ? Name : iconPath)}.png");


            if (File.Exists(iconPath))
            {
                using var stream = File.OpenRead(iconPath);
                icon = PointShopExtender.Instance.Assets.CreateUntracked<Texture2D>(stream, iconPath);
                return true;
            }
        }
        return icon != null;
    }

    public void SaveInfo(string? path = null)
    {
        path ??= DefaultPath;
        Directory.CreateDirectory(path);
        var content = PointShopExtenderSystem.YamlSerializer.Serialize(this);
        var filePath = Path.Combine(path, Name + ".yaml");
        File.WriteAllText(filePath, content);
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

    public void SaveIcon(string path)
    {
        if (IconTexture?.Value is { } iconTexture)
        {
            Main.RunOnMainThread(() =>
            {
                using var stream = new FileStream(Path.Combine(path, $"{Name}_Icon.png"), FileMode.Create);
                iconTexture.SaveAsPng(stream, iconTexture.Width, iconTexture.Height);
            });
        }
    }

    public void SetIconAndSave(Asset<Texture2D> texture, string? path = null)
    {
        path ??= DefaultPath;
        Icon = $"{Name}_Icon";
        IconTexture = texture;
        SaveIcon(path);
        SaveInfo(path);
    }

    public void Save(string? path = null)
    {
        path ??= DefaultPath;
        var filePath = Path.Combine(path, Name + ".yaml");
        SaveInfo(path);

        if (Icon == Name + "_Icon") //!TryGetIconViaPath(filePath, out _)
        {
            SaveIcon(path);
        }
    }

    public void RenameFile(string newName, string? folderPath = null)
    {
        folderPath ??= DefaultPath;
        if (string.IsNullOrEmpty(newName))
            return;

        var oldPath = Path.Combine(folderPath, Name);
        var newPath = Path.Combine(folderPath, newName);
        if (!string.IsNullOrEmpty(Name) && File.Exists(oldPath + ".yaml"))
        {
            if (Icon == Name + "_Icon")
                Icon = newName + "_Icon";
            Name = newName;
            Save(); // 保存新的

            // 删除之前的
            File.Delete(oldPath + ".yaml");
            if (File.Exists(oldPath + "_Icon.png"))
                File.Move(oldPath + "_Icon.png", newPath + "_Icon.png");
            return;
        }
        else
        {
            Name = newName;
            Save();
            OnCreateNew();
        }
    }

    public void SetDisplayNameAndSave(string displayName)
    {
        DisplayNameZH = displayName;
        SaveInfo();
    }

    public void SetDisplayNameEnAndSave(string displayNameEn)
    {
        DisplayNameEN = displayNameEn;
        SaveInfo();
    }

    public void SaveAfterSetCondition()
    {
        var extensionInfo = this;
        var realCondition = RealCondition;
        Condition = realCondition.ToString();
        if (PointShopExtenderSystem.AutoInfoMode)
        {
            var (icon, localizedText) = realCondition.GetInfo();
            if (icon != null)
            {
                extensionInfo.IconTexture = icon;
                extensionInfo.Icon = extensionInfo.Name + "_Icon";
            }
            if (localizedText != null)
            {
                var displayName = localizedText.Value;
                var isChinese = PacketMakerUI.IsChinese;
                if (isChinese)
                    extensionInfo.DisplayNameZH = displayName;
                else
                    extensionInfo.DisplayNameEN = localizedText.Value;

                ExtraAutoSetting(displayName, isChinese);
            }
            extensionInfo.Save();
        }
        else
        {
            extensionInfo.SaveInfo();
        }
    }

    protected virtual void ExtraAutoSetting(string text, bool isChinese)
    {

    }

    protected abstract void OnCreateNew();
}
