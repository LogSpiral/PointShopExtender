using PointShop.Registrar;
using PointShop.ShopSystem;
using PointShopExtender.PacketData;
using SilkyUIFramework;
using SilkyUIFramework.BasicElements;
using SilkyUIFramework.Extensions;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Default;

namespace PointShopExtender.PacketManager;

partial class PacketMakerUI
{
    /// <summary>
    /// 切换至拓展商店列表
    /// </summary>
    void SwitchToShopItemPage()
    {
        SwicthPageCommon();
        SetNextTargetSize(new(700, 450));


        AddFilter();

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
            ShopItemElement createNew = new(new ShopExtension(), true);
            itemList.Container.AppendChild(createNew);
            HashSet<ShopItemElement> inSearchItem = [];
            HashSet<ShopItemElement> others = [];
            foreach (var shop in CurrentPack.ShopExtensions)
            {
                List<string> matchingList = [shop.Name];
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
                    inSearchItem.Add(new(shop, false));
                else
                    others.Add(new(shop, false));
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
    /// 切换至商店内容页面
    /// </summary>
    void SwitchToShopInfoPage(ShopExtension shop)
    {
        SwicthPageCommon();
        SetNextTargetSize(new(700, 110));


        ShopInfoElement shopInfoElement = new(shop);
        shopInfoElement.Join(MainPanel);
    }

    void SwitchToShopContentPage(ShopExtension shop) 
    {
        SwicthPageCommon();
        SetNextTargetSize(new(700, 450));

        ShopContentElement shopContentElement = new(shop);
        shopContentElement.Join(MainPanel);
    }

    void SwitchToSingleItemEditPage(SimpleShopItemGenerator simpleShopItemGenerator) 
    {
        SwicthPageCommon();
        SetNextTargetSize(new(760, 160));

        ShopItemEditorPage shopItemEditorPage = new(simpleShopItemGenerator);
        shopItemEditorPage.Join(MainPanel);
    }

    void SwitchToSingleShopItemPage(SimpleShopItemGenerator simpleShopItem) 
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
            HashSet<ShopSingleItemPanel> inSearchItem = [];
            HashSet<ShopSingleItemPanel> others = [];
            foreach (var pair in ContentSamples.ItemsByType)
            {
                var item = pair.Value;
                if (ItemID.Sets.Deprecated[pair.Key]) continue;
                if (pair.Key == ItemID.None || pair.Key == ModContent.ItemType<UnloadedItem>()) continue;
                List<string> matchingList = [item.Name,pair.Key.ToString()];
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
                    inSearchItem.Add(new(pair.Value));
                else
                    others.Add(new(pair.Value));
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

    void SwitchToUnlockConditionPage() 
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
            HashSet<UnlockConditionItemPanel> inSearchItem = [];
            HashSet<UnlockConditionItemPanel> others = [];
            foreach (var condition in PointShopSystem.UnlockConditions.Values) 
            {
                List<string> matchingList = [condition.DisplayName, condition.Name];
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
                    inSearchItem.Add(new(condition));
                else
                    others.Add(new(condition));
            }
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
                    inSearchItem.Add(new(condition));
                else
                    others.Add(new(condition));
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
}
