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
    /// 切换至环境列表
    /// </summary>
    void SwitchToEnvironmentItemPage()
    {
        SwicthPageCommon();
        AddFilter();
        SetNextTargetSize(new(700, 450));
        PathTracker.AddNewPath("EnvironmentExtensionPage", Instance.SwitchToEnvironmentItemPage);

        SUIScrollView itemList = new();
        itemList.SetPadding(8f);
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
            EnvironmentItemElement createNew = new(new EnvironmentExtension() { Packet = CurrentPack, Condition = "", RealCondition = new() }, true);
            itemList.Container.AppendChild(createNew);
            HashSet<EnvironmentItemElement> inSearchItem = [];
            HashSet<EnvironmentItemElement> others = [];
            foreach (var environment in CurrentPack.EnvironmentExtensions)
            {
                List<string> matchingList = [environment.Name, environment.DisplayNameEN, environment.DisplayNameZH];
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
                    inSearchItem.Add(new(environment, false));
                else
                    others.Add(new(environment, false));
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
    /// 切换至环境信息页面
    /// </summary>
    void SwitchToEnvironmentInfoPage(EnvironmentExtension environment)
    {
        SwicthPageCommon();
        SetNextTargetSize(new(800, 320));
        PathTracker.AddNewPath("EnvironmentInfoPage", () => SwitchToEnvironmentInfoPage(environment));


        EnvironmentInfoElement environmentInfoElement = new(environment);
        environmentInfoElement.Join(MainPanel);
    }
}
