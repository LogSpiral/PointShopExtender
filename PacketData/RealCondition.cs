using Microsoft.Xna.Framework.Graphics;
using PointShopExtender.PacketManager;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace PointShopExtender.PacketData;

public class RealCondition
{
    public ExtensionWithInfo Owner { get; set; }

    public ConditionType ConditionType;

    public string ConditionContent = "";

    public override string ToString()
    {
        if (ConditionContent == "") return "";
        return $"{ConditionType switch
        {
            ConditionType.Vanilla => "VanillaCondition",
            ConditionType.ModBoss => "DownedBoss",
            ConditionType.ModEnvironment => "InBiome",
            _ => ""
        }}|{ConditionContent}";
    }

    public Func<bool> ToFunc()
    {
        Func<bool> condition;
        if (ConditionContent is { Length: > 0 } content)
        {
            switch (ConditionType)
            {
                case ConditionType.Vanilla:
                    {
                        if (!PointShopExtenderSystem.VanillaConditionInstances.TryGetValue(content, out var vCondition))
                            throw new Exception(PointShopExtenderSystem.GetLocalizationText("PacketMakerUI.UnknownVanillaCondition"));
                        condition = vCondition.Predicate;
                        break;
                    }
                case ConditionType.ModBoss:
                    {
                        if (ModLoader.HasMod(content.Split('/')[0]))
                            condition = () => PointShopExtenderSystem.DownedBosses.Contains(content);
                        else
                            condition = () => true;
                        break;
                    }
                case ConditionType.ModEnvironment:
                    {
                        if (!ModContent.TryFind<ModBiome>(content, out var biome))
                            condition = () => true;
                        else
                            condition = () => Main.LocalPlayer.InModBiome(biome);
                        break;
                    }
                default:
                    {
                        condition = () => true;
                        break;
                    }
            }
        }
        else
            condition = () => true;

        return condition;
    }

    public static RealCondition FromString(string Condition)
    {
        var result = new RealCondition();
        if (Condition == null || Condition.Length == 0)
            return result;
        var infos = Condition.Split('|');
        if (infos.Length != 2)
            throw new Exception(PointShopExtenderSystem.GetLocalizationText("PacketMakerUI.MalformedConditionException"));
        switch (infos[0])
        {
            case "VanillaCondition":
                {
                    result.ConditionType = ConditionType.Vanilla;
                    break;
                }
            case "DownedBoss":
                {
                    result.ConditionType = ConditionType.ModBoss;
                    break;
                }
            case "InBiome":
                {
                    result.ConditionType = ConditionType.ModEnvironment;
                    break;
                }
            default:
                throw new Exception(PointShopExtenderSystem.GetLocalizationText("PacketMakerUI.UnknownConditionType"));
        }
        result.ConditionContent = infos[1];
        return result;
    }

    public string GetLocalizedTypeName()
    {
        return
            PointShopExtenderSystem.GetLocalizationText($"PacketMakerUI.{ConditionType switch
            {
                ConditionType.Vanilla => "VanillaCondition",
                ConditionType.ModEnvironment => "ModEnvironment",
                ConditionType.ModBoss => "ModBoss",
                ConditionType.None or _ => "NoneCondition"
            }}");
    }

    public string GetContentName()
    {
        string result = "";
        if (ConditionContent is { Length: > 0 } content)
        {
            switch (ConditionType)
            {
                case ConditionType.Vanilla:
                    {
                        var vCondition = (Condition)typeof(Condition).GetField(content, BindingFlags.Public | BindingFlags.Static).GetValue(null) ?? throw new Exception(PointShopExtenderSystem.GetLocalizationText("PacketMakerUI.UnknownVanillaCondition"));
                        result = vCondition.Description.Value;
                        break;
                    }
                case ConditionType.ModBoss:
                    {
                        if (ModLoader.HasMod(content.Split('/')[0]) && ModContent.TryFind<ModNPC>(content, out var modNPC))
                            result = modNPC.DisplayName.Value;
                        else
                            result = PointShopExtenderSystem.GetLocalizationText("PacketMakerUI.Unloaded");
                        break;
                    }
                case ConditionType.ModEnvironment:
                    {
                        if (ModContent.TryFind<ModBiome>(content, out var biome))
                            result = biome.DisplayName.Value;
                        else
                            result = PointShopExtenderSystem.GetLocalizationText("PacketMakerUI.Unloaded");
                        break;
                    }
            }
        }
        return result;
    }

    public void SaveFile() => Owner?.SaveAfterSetCondition();
    public (Asset<Texture2D>, LocalizedText) GetInfo()
    {
        Asset<Texture2D> asset = null;
        LocalizedText localizedText = null;
        switch (ConditionType)
        {
            case ConditionType.None:
                {
                    asset = ModAsset.NoneConditionIcon;
                    localizedText = PointShopExtenderSystem.GetLocalization("PacketMakerUI.NoneCondition");
                    break;
                }
            case ConditionType.Vanilla:
                {
                    asset = ModAsset.VanillaConditionIcon;
                    if (PointShopExtenderSystem.VanillaConditionInstances.TryGetValue(ConditionContent, out var condition))
                        localizedText = condition.Description;
                    break;
                }
            case ConditionType.ModEnvironment:
                {

                    if (ModContent.TryFind<ModBiome>(ConditionContent, out var biome))
                    {
                        string text = biome.BestiaryIcon;
                        if (biome.Name is "AstralCaveDesert")
                            text = text.Replace("CaveDesert", "DesertCave"); // FKU CLMT
                        if (ModContent.RequestIfExists<Texture2D>(text, out var result))
                            asset = result;

                        localizedText = biome.DisplayName;
                    }
                    break;
                }
            case ConditionType.ModBoss:
                {
                    if (ModContent.TryFind<ModNPC>(ConditionContent, out var npc))
                    {
                        var index = NPCID.Sets.BossHeadTextures[npc.Type];
                        NPCLoader.BossHeadSlot(ContentSamples.NpcsByNetId[npc.Type], ref index);
                        asset = TextureAssets.NpcHeadBoss[index];

                        localizedText = npc.DisplayName;
                    }
                    break;
                }
        }
        return (asset, localizedText);
    }
}
public enum ConditionType
{
    None,
    Vanilla,
    ModEnvironment,
    ModBoss
}
