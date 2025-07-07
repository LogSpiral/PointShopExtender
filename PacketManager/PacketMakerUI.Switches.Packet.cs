using PointShopExtender.PacketData;
using SilkyUIFramework;
using SilkyUIFramework.BasicElements;
using SilkyUIFramework.Extensions;
using System.Collections.Generic;
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
        CurrentPack = null;

        SwicthPageCommon();
        SetNextTargetSize(new(700, 450));
        PathTracker.AddNewPath("PacketExtensionViewPage", Instance.SwitchToPacketItemPage);
        AddFilter();
        SavePendingShop();
        SUIScrollView itemList = new();


        itemList.SetMargin(8f);
        itemList.SetWidth(0, 1f);
        itemList.SetLeft(0, 0, 0.5f);
        itemList.SetHeight(-40, 1f);
        itemList.Join(MainPanel);

        UpdateMainPanel = () =>
        {
            if (!PendingUpdateList) return;
            PendingUpdateList = false;
            itemList.Container.RemoveAllChildren();
            itemList.ScrollBar.ScrollByTop();

            PacketItemElement createNew = new(new ExtensionPack(), true);
            itemList.Container.AppendChild(createNew);


            HashSet<PacketItemElement> inSearchItem = [];
            HashSet<PacketItemElement> others = [];
            foreach (var pack in PointShopExtenderSystem.ExtensionPacks)
            {
                List<string> matchingList = [pack.PackName, pack.DisplayName, pack.DisplayNameEn];
                bool find = false;
                if (!string.IsNullOrEmpty(CurrentSearchingText))
                    foreach (var match in matchingList)
                    {
                        if (match.Contains(CurrentSearchingText))
                        {
                            find = true;
                            break;
                        }
                    }
                if (PointShopExtenderSystem.FavList.Contains(pack.PackName))
                    find = true;
                if (find)
                    inSearchItem.Add(new(pack, false));
                else
                    others.Add(new(pack, false));
            }
            foreach (var item in inSearchItem)
            {
                item.BorderColor = SUIColor.Highlight;
                itemList.Container.AppendChild(item);
            }
            foreach (var item in others)
                itemList.Container.AppendChild(item);
        };
    }

    /// <summary>
    /// 切换至拓展包信息页
    /// </summary>
    void SwitchToPacketInfoPage(ExtensionPack extensionPack)
    {
        CurrentPack = extensionPack;
        SwicthPageCommon();
        SavePendingShop();
        SetNextTargetSize(new(800, 320));
        PathTracker.AddNewPath("PacketInfoPage", () => Instance.SwitchToPacketInfoPage(extensionPack));

        PacketInfoElement packetInfoElement = new(extensionPack);
        packetInfoElement.Join(MainPanel);
    }

    /// <summary>
    /// 切换至中转页面
    /// </summary>
    void SwitchToBridgePage()
    {
        SwicthPageCommon();
        SavePendingShop();
        SetNextTargetSize(new(700, 350));
        PathTracker.AddNewPath("ExtensionTypePage", Instance.SwitchToBridgePage);

        UIElementGroup container = new();
        container.LayoutType = LayoutType.Flexbox;
        container.FlexGrow = 1f;
        container.SetGap(8f);
        container.SetMargin(8f);
        container.SetWidth(0, 1f);
        container.SetLeft(0, 0, 0.5f);
        container.SetHeight(0, 1f);
        container.Join(MainPanel);

        for (int n = 0; n < 3; n++)
        {
            BridgeElement bridgeElement = new((BridgeState)n);
            container.AppendChild(bridgeElement);
        }


    }
}
