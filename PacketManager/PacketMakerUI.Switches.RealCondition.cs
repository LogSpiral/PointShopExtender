using PointShopExtender.PacketData;
using SilkyUIFramework;
using SilkyUIFramework.BasicElements;
using SilkyUIFramework.Extensions;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
namespace PointShopExtender.PacketManager;

partial class PacketMakerUI
{
    /// <summary>
    /// 切换至实际条件编辑页面
    /// </summary>
    void SwitchToRealConditionEditor(RealCondition realCondition)
    {
        SwicthPageCommon();

        SetNextTargetSize(new(960, 350));
        PathTracker.AddNewPath("ConditionTypePage", () => Instance.SwitchToRealConditionEditor(realCondition));

        UIElementGroup container = new();
        container.LayoutType = LayoutType.Flexbox;
        container.FlexGrow = 1f;
        container.SetGap(8f);
        container.SetPadding(8f);
        container.SetWidth(0, 1f);
        container.SetLeft(0, 0, 0.5f);
        container.SetHeight(0, 1f);
        container.Join(MainPanel);

        for (int n = 0; n < 4; n++)
        {
            ConditionTypeElement conditionTypeElement = new((ConditionType)n, realCondition);
            container.AppendChild(conditionTypeElement);
        }
    }

    /// <summary>
    /// 切换至原版条件选择
    /// </summary>
    void SwitchToVanillaConditions(RealCondition realCondition)
    {
        SwicthPageCommon();
        SetNextTargetSize(new(700, 450));
        PathTracker.AddNewPath("ConditionVanillaPage", () => Instance.SwitchToVanillaConditions(realCondition));
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
            HashSet<VanillaConditionItemElement> inSearchItem = [];
            HashSet<VanillaConditionItemElement> others = [];
            foreach (var pair in PointShopExtenderSystem.VanillaConditionInstances)
            {
                List<string> matchingList = [pair.Key, pair.Value.Description.Value, pair.Value.Description.Key];
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
                    inSearchItem.Add(new(pair.Key, realCondition));
                else
                    others.Add(new(pair.Key, realCondition));
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
    /// 切换至模组环境选择
    /// </summary>
    void SwitchToModdedEnvironments(RealCondition realCondition)
    {
        SwicthPageCommon();
        SetNextTargetSize(new(700, 450));
        PathTracker.AddNewPath("ConditionModEnvironmentPage", () => Instance.SwitchToVanillaConditions(realCondition));
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
            HashSet<ModEnvironmentItemElement> inSearchItem = [];
            HashSet<ModEnvironmentItemElement> others = [];
            foreach (var pair in ModTypeLookup<ModBiome>.dict)
            {
                List<string> matchingList = [pair.Key, pair.Value.DisplayName.Value, pair.Value.DisplayName.Key];
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
                try
                {
                    if (find)
                        inSearchItem.Add(new(pair.Value, realCondition));
                    else
                        others.Add(new(pair.Value, realCondition));
                }
                catch
                {
                    continue;
                }
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
    /// 切换至模组boss选择
    /// </summary>
    void SwitchToModdedBosses(RealCondition realCondition)
    {
        SwicthPageCommon();
        SetNextTargetSize(new(700, 450));
        PathTracker.AddNewPath("ConditionModBossPage", () => Instance.SwitchToVanillaConditions(realCondition));
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
            HashSet<ModBossItemElement> inSearchItem = [];
            HashSet<ModBossItemElement> others = [];
            foreach (var pair in ModTypeLookup<ModNPC>.dict)
            {
                var type = pair.Value.Type;
                var sampleNPC = ContentSamples.NpcsByNetId[type];
                if (!sampleNPC.boss && !NPCID.Sets.ShouldBeCountedAsBoss[type]) continue;
                int index = NPCID.Sets.BossHeadTextures[type];
                NPCLoader.BossHeadSlot(sampleNPC, ref index);
                if (index == -1) continue;
                if (NPCID.Sets.NPCBestiaryDrawOffset.TryGetValue(type, out var value) && value is { Hide: true }) continue;
                List<string> matchingList = [pair.Key, pair.Value.DisplayName.Value, pair.Value.DisplayName.Key];
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
                try
                {
                    if (find)
                        inSearchItem.Add(new(pair.Value, realCondition));
                    else
                        others.Add(new(pair.Value, realCondition));
                }
                catch
                {
                    continue;
                }
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

