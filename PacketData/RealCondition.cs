using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace PointShopExtender.PacketData;

public class RealCondition
{
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
                        var vCondition = (Condition)typeof(Condition).GetField(content, BindingFlags.Public | BindingFlags.Static).GetValue(null) ?? throw new Exception(PointShopExtenderSystem.GetLocalizationText("PacketMakerUI.UnknownVanillaCondition"));
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
}
public enum ConditionType
{
    Vanilla,
    ModEnvironment,
    ModBoss
}
