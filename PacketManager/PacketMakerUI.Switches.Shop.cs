using SilkyUIFramework.Extensions;
using SilkyUIFramework.BasicElements;
using System.Linq;
using PointShopExtender.PacketData;

namespace PointShopExtender.PacketManager;

partial class PacketMakerUI
{
    /// <summary>
    /// 切换至拓展商店列表
    /// </summary>
    void SwitchToShopItemPage()
    {
        MainPanel.RemoveAllChildren();

        SUIScrollView packetItemList = new();
        packetItemList.SetMargin(8f);
        packetItemList.SetWidth(0, 1f);
        packetItemList.SetLeft(0, 0, 0.5f);
        packetItemList.SetHeight(0, 0.9f);
        packetItemList.SetTop(0, 0.1f);
        packetItemList.Join(MainPanel);

        ShopItemElement createNew = new(new ShopExtension(), true);
        packetItemList.Container.AppendChild(createNew);

        foreach (var shop in CurrentPack.ShopExtensions)
        {
            ShopItemElement singleItem = new(shop, false);
            packetItemList.Container.AppendChild(singleItem);
        }
    }


    /// <summary>
    /// 切换至商店内容页面
    /// </summary>
    void SwitchToShopInfoPage(ShopExtension shop)
    {
        MainPanel.RemoveAllChildren();

        ShopInfoElement shopInfoElement = new(shop);
        shopInfoElement.Join(MainPanel);
    }
}
