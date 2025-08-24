using PointShop.Registrar;
using PointShop.ShopSystem;
using PointShopExtender.PacketData;
using SilkyUIFramework;
using SilkyUIFramework.BasicElements;
using SilkyUIFramework.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
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
        SavePendingShop();
        SetNextTargetSize(new(700, 450));
        PathTracker.AddNewPath("ShopExtensionPage", Instance.SwitchToShopItemPage);

        AddFilter();

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
            ShopItemElement createNew = new(new ShopExtension() { Packet = CurrentPack }, true);
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
        PendingShop = shop;
        SetNextTargetSize(new(700, 110));
        PathTracker.AddNewPath("ShopInfoPage", () => Instance.SwitchToShopInfoPage(shop));

        ShopInfoElement shopInfoElement = new(shop);
        shopInfoElement.Join(MainPanel);
    }

    void SwitchToShopContentPage(ShopExtension shop)
    {
        SwicthPageCommon();
        SetNextTargetSize(new(700, 450));
        PathTracker.AddNewPath("ShopPreviewPage", () => Instance.SwitchToShopContentPage(shop));
        ShopContentElement.ShopItemTableIsDirty = true;
        ShopContentElement shopContentElement = new(shop);
        shopContentElement.Join(MainPanel);
    }

    void SwitchToSingleItemEditPage(SimpleShopItemGenerator simpleShopItemGenerator, Action appendToListCallBack)
    {
        SwicthPageCommon();
        SetNextTargetSize(new(760, 160));
        PathTracker.AddNewPath("ShopItemInfoPage", () => Instance.SwitchToSingleItemEditPage(simpleShopItemGenerator, appendToListCallBack));

        ShopItemEditorPage shopItemEditorPage = new(simpleShopItemGenerator, appendToListCallBack);
        shopItemEditorPage.Join(MainPanel);
    }

    void SwitchToSingleShopItemPage(SimpleShopItemGenerator simpleShopItem, Action appendCallback)
    {
        SwicthPageCommon();

        AddFilter();
        SetNextTargetSize(new(700, 450));
        PathTracker.AddNewPath("ShopItemBrowser", () => Instance.SwitchToSingleShopItemPage(simpleShopItem, appendCallback));

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
            int counter = 0;
            HashSet<Item> inSearchItem = [];
            HashSet<Item> others = [];

            foreach (var pair in ContentSamples.ItemsByType)
            {
                var item = pair.Value;
                if (ItemID.Sets.Deprecated[pair.Key]) continue;
                if (pair.Key == ItemID.None || pair.Key == ModContent.ItemType<UnloadedItem>() || pair.Key == CreateNewDummyItem.ID()) continue;
                List<string> matchingList = [item.Name, pair.Key.ToString()];
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
                    inSearchItem.Add(pair.Value);
                else
                    others.Add(pair.Value);
            }
            foreach (var item in inSearchItem)
            {
                var itemPanel = new ShopSingleItemPanel(item, simpleShopItem, appendCallback) { BorderColor = SUIColor.Highlight };
                itemList.Container.AppendChild(itemPanel);
                counter++;
                if (counter >= 500) break;
            }
            foreach (var item in others)
            {
                if (counter >= 500) break;
                counter++;
                itemList.Container.AppendChild(new ShopSingleItemPanel(item, simpleShopItem, appendCallback));
            }
        };
    }

    void SwitchToUnlockConditionPage(SimpleShopItemGenerator simpleShopItem)
    {
        SwicthPageCommon();

        AddFilter();
        SetNextTargetSize(new(700, 450));
        PathTracker.AddNewPath("UnlockConditionPage", () => Instance.SwitchToUnlockConditionPage(simpleShopItem));

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
            HashSet<UnlockConditionItemPanel> inSearchItem = [];
            HashSet<UnlockConditionItemPanel> others = [];
            HashSet<string> loadedCondition = [];
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
                    inSearchItem.Add(new(condition, simpleShopItem));
                else
                    others.Add(new(condition, simpleShopItem));
                loadedCondition.Add(condition.Name);
            }
            foreach (var condition in CurrentPack.ConditionExtensions)
            {
                if (loadedCondition.Contains(condition.Name)) continue;
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
                    inSearchItem.Add(new(condition, simpleShopItem));
                else
                    others.Add(new(condition, simpleShopItem));
            }

            foreach (var item in inSearchItem)
            {
                item.BorderColor = SUIColor.Highlight;
                itemList.Container.AppendChild(item);
            }
            itemList.Container.AppendChild(new UnlockConditionItemPanel(default(ConditionExtension), simpleShopItem));
            foreach (var item in others)
                itemList.Container.AppendChild(item);
        };
    }
}
