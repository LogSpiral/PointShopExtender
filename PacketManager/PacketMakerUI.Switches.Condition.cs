using PointShopExtender.PacketData;
using SilkyUIFramework;
using SilkyUIFramework.BasicElements;
using SilkyUIFramework.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace PointShopExtender.PacketManager;

partial class PacketMakerUI
{
    /// <summary>
    /// 切换至解锁条件列表
    /// </summary>
    void SwitchToConditionItemPage()
    {
        SwicthPageCommon();

        AddFilter();
        SetNextTargetSize(new(700, 450));


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
            ConditionItemElement createNew = new(new ConditionExtension(), true);
            itemList.Container.AppendChild(createNew);
            HashSet<ConditionItemElement> inSearchItem = [];
            HashSet<ConditionItemElement> others = [];
            foreach (var condition in CurrentPack.ConditionExtensions)
            {
                List<string> matchingList = [condition.Name, condition.DisplayNameEN, condition.DisplayNameZH];
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
                if (find)
                    inSearchItem.Add(new(condition, false));
                else
                    others.Add(new(condition, false));
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
    /// 切换至解锁条件信息页面
    /// </summary>
    void SwitchToConditionInfoPage(ConditionExtension condition)
    {
        SwicthPageCommon();
        SetNextTargetSize(new(800, 320));


        ConditionInfoElement conditionInfoElement = new(condition);
        conditionInfoElement.Join(MainPanel);
    }
}