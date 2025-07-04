using PointShopExtender.PacketData;
using SilkyUIFramework.BasicElements;
using SilkyUIFramework.Extensions;
using System.Linq;

namespace PointShopExtender.PacketManager;

partial class PacketMakerUI
{
    /// <summary>
    /// 切换至环境列表
    /// </summary>
    void SwitchToEnvironmentItemPage()
    {
        MainPanel.RemoveAllChildren();

        SUIScrollView packetItemList = new();
        packetItemList.SetMargin(8f);
        packetItemList.SetWidth(0, 1f);
        packetItemList.SetLeft(0, 0, 0.5f);
        packetItemList.SetHeight(0, 0.9f);
        packetItemList.SetTop(0, 0.1f);
        packetItemList.Join(MainPanel);

        EnvironmentItemElement createNew = new(new EnvironmentExtension(), true);
        packetItemList.Container.AppendChild(createNew);

        foreach (var environment in CurrentPack.EnvironmentExtensions)
        {
            EnvironmentItemElement singleItem = new(environment, false);
            packetItemList.Container.AppendChild(singleItem);
        }
    }

    /// <summary>
    /// 切换至环境信息页面
    /// </summary>
    void SwitchToEnvironmentInfoPage(EnvironmentExtension environment)
    {
        MainPanel.RemoveAllChildren();

        EnvironmentInfoElement environmentInfoElement = new(environment);
        environmentInfoElement.Join(MainPanel);
    }
}
