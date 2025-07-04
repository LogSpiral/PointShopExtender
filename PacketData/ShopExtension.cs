using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.IO;

namespace PointShopExtender.PacketData;

public class ShopExtension
{
    public string FileContent = "";

    public string Name { get; set; }

    public Asset<Texture2D> IconTexture { get; set; } = ModAsset.ShopIconDefault;

    public void Register() 
    {
        PointShop.PointShop.AddShopItemByFile(PointShopExtender.Instance,FileContent);
    }

    public static ShopExtension FromFile(string file) 
    {
        var result = new ShopExtension();
        result.FileContent = File.ReadAllText(file);
        result.Name = Path.GetFileNameWithoutExtension(file);
        return result;
    }

    public void Save(string path)
    {
        Directory.CreateDirectory(path);
        File.WriteAllText(Path.Combine(path,Name + ".yaml"), FileContent);
    }
}
