using PointShopExtender.PacketData;
using SilkyUIFramework.BasicElements;
using SilkyUIFramework.Extensions;
using System.Linq;

namespace PointShopExtender.PacketManager;

partial class PacketMakerUI
{
    /// <summary>
    /// 切换至解锁条件列表
    /// </summary>
    void SwitchToConditionItemPage()
    {
        MainPanel.RemoveAllChildren();

        SUIScrollView packetItemList = new();
        packetItemList.SetMargin(8f);
        packetItemList.SetWidth(0, 1f);
        packetItemList.SetLeft(0, 0, 0.5f);
        packetItemList.SetHeight(0, 0.9f);
        packetItemList.SetTop(0, 0.1f);
        packetItemList.Join(MainPanel);

        ConditionItemElement createNew = new(new ConditionExtension(), true);
        packetItemList.Container.AppendChild(createNew);

        foreach (var condition in CurrentPack.ConditionExtensions)
        {
            ConditionItemElement singleItem = new(condition, false);
            packetItemList.Container.AppendChild(singleItem);
        }
    }

    /// <summary>
    /// 切换至解锁条件信息页面
    /// </summary>
    void SwitchToConditionInfoPage(ConditionExtension condition)
    {
        MainPanel.RemoveAllChildren();

        ConditionInfoElement conditionInfoElement = new(condition);
        conditionInfoElement.Join(MainPanel);
    }
}