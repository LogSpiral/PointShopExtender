using PointShopExtender.PacketData;
using SilkyUIFramework;
using SilkyUIFramework.BasicElements;
using SilkyUIFramework.Extensions;
using System.IO;
using System.Linq;
using Terraria;

namespace PointShopExtender.PacketManager;

partial class PacketMakerUI
{
    /// <summary>
    /// 切换至拓展包物品页
    /// </summary>
    void SwitchToPacketItemPage()
    {
        MainPanel.RemoveAllChildren();

        SUIScrollView packetItemList = new();
        packetItemList.SetMargin(8f);
        packetItemList.SetWidth(0, 1f);
        packetItemList.SetLeft(0, 0, 0.5f);
        packetItemList.SetHeight(0, 0.9f);
        packetItemList.SetTop(0, 0.1f);
        packetItemList.Join(MainPanel);

        PacketItemElement createNew = new(new ExtensionPack(), true);
        packetItemList.Container.AppendChild(createNew);

        //for (int n = 0; n < 5; n++)
        //{
        //    PacketItemElement createNew = new PacketItemElement(new ExtensionPack(), true);

        //    packetItemList.Container.AppendChild(createNew);

        //}

        var mainPath = Path.Combine(Main.SavePath, "Mods", nameof(PointShopExtender));

        foreach (var pack in PointShopExtenderSystem.ExtensionPacks)
        {
            PacketItemElement singleItem = new(pack, false);
            packetItemList.Container.AppendChild(singleItem);
        }
    }

    /// <summary>
    /// 切换至拓展包信息页
    /// </summary>
    void SwitchToPacketInfoPage(ExtensionPack extensionPack)
    {
        CurrentPack = extensionPack;
        MainPanel.RemoveAllChildren();

        PacketInfoElement packetInfoElement = new(extensionPack);
        packetInfoElement.Join(MainPanel);
    }

    /// <summary>
    /// 切换至中转页面
    /// </summary>
    void SwitchToBridgePage()
    {
        MainPanel.RemoveAllChildren();


        UIElementGroup container = new();
        container.LayoutType = LayoutType.Flexbox;
        container.FlexGrow = 1f;
        container.SetGap(8f);
        container.SetMargin(8f);
        container.SetWidth(0, 1f);
        container.SetLeft(0, 0, 0.5f);
        container.SetHeight(0, 0.9f);
        container.SetTop(0, 0.1f);
        container.Join(MainPanel);

        for (int n = 0; n < 3; n++)
        {
            BridgeElement bridgeElement = new((BridgeState)n);
            container.AppendChild(bridgeElement);
        }


    }
}
