using Microsoft.Xna.Framework.Graphics;
using PointShop.Registrar;
using ReLogic.Content;
using System.IO;

namespace PointShopExtender.PacketData;

public class ShopExtension
{
    public string Name { get; set; }

    public Asset<Texture2D> IconTexture { get; set; } = ModAsset.ShopIconDefault;

    public SimpleShopData SimpleShopData { get; set; }

    public void Register() 
    {
        ShopItemsRegistrar.RegisterShopData(PointShopExtender.Instance, SimpleShopData);
    }

    public static ShopExtension FromFile(string file) 
    {
        var result = new ShopExtension();
        var fileContent = File.ReadAllText(file);
        result.SimpleShopData = ShopItemsRegistrar.GetShopData(fileContent);
        result.Name = Path.GetFileNameWithoutExtension(file);
        return result;
    }

    public void Save(string path)
    {
        Directory.CreateDirectory(path);
        File.WriteAllText(Path.Combine(path, Name + ".yaml"), PointShopExtenderSystem.YamlSerializer.Serialize(SimpleShopData));
    }
}
