using Microsoft.Xna.Framework.Graphics;
using PointShop.Registrar;
using ReLogic.Content;
using System.Collections.Generic;
using System.IO;
using Terraria;

namespace PointShopExtender.PacketData;

public class ShopExtension : ExtensionBase
{
    public Asset<Texture2D> IconTexture { get; set; } = ModAsset.ShopIconDefault;

    public SimpleShopData SimpleShopData { get; set; } = new SimpleShopData();

    protected override string Category => "Shops";

    public void Register()
    {
        ShopItemsRegistrar.RegisterShopData(PointShopExtender.Instance, SimpleShopData);
    }

    public static ShopExtension FromFile(string file, string folderPath)
    {
        var result = new ShopExtension();
        var fileContent = File.ReadAllText(file);
        result.SimpleShopData = ShopItemsRegistrar.ConvertYamlStringToShopData(fileContent);
        result.Name = Path.GetFileNameWithoutExtension(file);

        var iconPath = Path.Combine(folderPath, result.Name + "_Icon.png");

        if (File.Exists(iconPath))
        {
            using var stream = File.OpenRead(iconPath);
            result.IconTexture = PointShopExtender.Instance.Assets.CreateUntracked<Texture2D>(stream, iconPath);
        }
        return result;
    }

    public void Save(string path = null)
    {
        path ??= DefaultPath;
        Directory.CreateDirectory(path);
        List<string> pendingRemove = [];

        foreach (var pair in SimpleShopData.EnvironmentShopItems) 
        {
            if (pair.Value is null or { Count: < 1 })
                pendingRemove.Add(pair.Key);
        }
        foreach (var key in pendingRemove)
            SimpleShopData.EnvironmentShopItems.Remove(key);

        File.WriteAllText(Path.Combine(path, Name + ".yaml"), PointShopExtenderSystem.YamlSerializer.Serialize(SimpleShopData));

        SaveIcon(path);
    }

    public void SaveIcon(string path = null) 
    {
        path ??= DefaultPath;
        if (IconTexture != ModAsset.ShopIconDefault && IconTexture?.Value is { } iconTexture)
        {
            Main.RunOnMainThread(() =>
            {
                using var stream = new FileStream(Path.Combine(path, $"{Name}_Icon.png"), FileMode.Create);
                iconTexture.SaveAsPng(stream, iconTexture.Width, iconTexture.Height);
            });
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
            Packet.ShopExtensions.Add(this);
        }
    }

    public void SetIconAndSave(Asset<Texture2D> texture) 
    {
        IconTexture = texture;
        SaveIcon();
    }
}
