using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using YamlDotNet.Serialization;
using static Terraria.Localization.NetworkText;

namespace PointShopExtender
{
    // Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
    public class PointShopExtender : Mod
    {
    }
    public class ExtenderSystem : ModSystem
    {
        public static HashSet<string> DownedBosses = [];
        public override void LoadWorldData(TagCompound tag)
        {
            if (tag.ContainsKey("DownedBosses"))
            {
                string[] names = tag.Get<string[]>("DownedBosses");
                int l = names.Length;
                for (int i = 0; i < l; i++)
                    DownedBosses.Add(names[i]);
            }

            base.LoadWorldData(tag);
        }
        public override void SaveWorldData(TagCompound tag)
        {
            tag["DownedBosses"] = DownedBosses.ToArray();
            base.SaveWorldData(tag);
        }
        public static IDeserializer YamlDeserializer { get; } = new DeserializerBuilder().Build();
        public override void Load()
        {
            if (!ModLoader.TryGetMod("PointShop", out var pointShop))
                return;
            MonoModHooks.Add(typeof(LocalizationLoader).GetMethod("LoadTranslations", BindingFlags.Static | BindingFlags.NonPublic), LocalziationModify);
            base.Load();
        }
        static List<(string key, string value)> LocalziationModify(Func<Mod, GameCulture, List<(string key, string value)>> orig, Mod mod, GameCulture gameCulture)
        {
            var result = orig.Invoke(mod, gameCulture);

            if (gameCulture.Name == "zh-Hans")
            {
                foreach (var key in EnvironmentDisplayNameZH.Keys)
                    result.Add(($"Mods.PointShopExtender.Environments.{key}", EnvironmentDisplayNameZH[key]));
                foreach (var key in ConditionDisplayNameZH.Keys)
                {
                    result.Add(($"Mods.PointShopExtender.UnlockCondition.{key}.DisplayName", ConditionDisplayNameZH[key]));
                    result.Add(($"Mods.PointShopExtender.UnlockCondition.{key}.Description", ConditionDescriptionZH[key]));
                }
            }

            else
            {
                foreach (var key in EnvironmentDisplayNameEN.Keys)
                    result.Add(($"Mods.PointShopExtender.Environments.{key}", EnvironmentDisplayNameEN[key]));
                foreach (var key in ConditionDisplayNameEN.Keys)
                {
                    result.Add(($"Mods.PointShopExtender.UnlockCondition.{key}.DisplayName", ConditionDisplayNameEN[key]));
                    result.Add(($"Mods.PointShopExtender.UnlockCondition.{key}.Description", ConditionDescriptionEN[key]));
                }
            }

            return result;
        }
        public override void PostSetupContent()
        {
            if (!ModLoader.TryGetMod("PointShop", out var pointShop))
                return;

            //var assembly = pointShop.GetType().Assembly;
            //var allTypes = assembly.GetTypes();
            //Type systemType = allTypes[24];
            //var lst = systemType.GetField("_environments", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);


            var mainPath = Path.Combine(Main.SavePath, "Mods", nameof(PointShopExtender));

            if (!Directory.Exists(mainPath))
            {
                Directory.CreateDirectory(mainPath);

                Utils.OpenFolder(mainPath);

                var stream = Mod.GetFileStream("ExampleShopPack.zip");
                ZipFile.ExtractToDirectory(stream, Path.Combine(mainPath, "ExampleShopPack"), Encoding.GetEncoding("GBK"));
                stream.Close();
            }

            foreach (var dir in Directory.GetDirectories(mainPath))
            {
                var subPath = Path.Combine(dir, "Conditions");
                if (!Directory.Exists(subPath))
                    Directory.CreateDirectory(subPath);
                else
                    foreach (var file in Directory.GetFiles(subPath, "*.yaml"))
                    {
                        ConditionMetaData metaData = YamlDeserializer.Deserialize<ConditionMetaData>(File.ReadAllText(file));
                        metaData.Register(pointShop, Mod, file);
                    }

                subPath = Path.Combine(dir, "Environments");
                if (!Directory.Exists(subPath))
                    Directory.CreateDirectory(subPath);
                else
                    foreach (var file in Directory.GetFiles(subPath, "*.yaml"))
                    {
                        EnvironmentMetaData metaData = YamlDeserializer.Deserialize<EnvironmentMetaData>(File.ReadAllText(file));
                        metaData.Register(pointShop, Mod, file);
                    }
                subPath = Path.Combine(dir, "Shops");
                if (!Directory.Exists(subPath))
                    Directory.CreateDirectory(subPath);
                else
                    foreach (var file in Directory.GetFiles(subPath, "*.yaml"))
                        pointShop.Call("AddShopItemByFile", Mod, File.ReadAllText(file));
            }




            base.PostSetupContent();
        }

        public static Dictionary<string, string> EnvironmentDisplayNameZH = [];
        public static Dictionary<string, string> ConditionDisplayNameZH = [];
        public static Dictionary<string, string> ConditionDescriptionZH = [];

        public static Dictionary<string, string> EnvironmentDisplayNameEN = [];
        public static Dictionary<string, string> ConditionDisplayNameEN = [];
        public static Dictionary<string, string> ConditionDescriptionEN = [];
    }
    public class EnvironmentMetaData
    {
        public string Name { get; set; }
        public string DisplayNameZH { get; set; }
        public string DisplayNameEN { get; set; }
        public string Icon { get; set; }
        public string Condition { get; set; }
        public int Priority { get; set; }

        public void Register(Mod pointShop, Mod mod, string filePath)
        {

            if (!ModContent.RequestIfExists<Texture2D>(Icon, out var icon))
            {
                if (!File.Exists(Icon))
                    Icon = Path.Combine(Path.GetFullPath(filePath)[..^Path.GetFileName(filePath).Length], $"{(Icon == "" ? Name : Icon)}.png");
                icon = mod.Assets.CreateUntracked<Texture2D>(File.OpenRead(Icon), Icon);

            }
            ExtenderSystem.EnvironmentDisplayNameZH[Name] = DisplayNameZH ?? Name;
            ExtenderSystem.EnvironmentDisplayNameEN[Name] = DisplayNameEN ?? Name;
            Language.GetOrRegister($"Mods.PointShopExtender.Environments.{Name}", () => Name);

            Func<Player, bool> condition;
            if (Condition == null || Condition.Length == 0)
                condition = player => true;
            else
            {
                var infos = Condition.Split('|');
                if (infos.Length != 2)
                    throw new Exception("条件信息应当包含两部分，第一部分标明条件类型，第二部分表明该类型下的一个具体值，中间用竖线|分隔");
                switch (infos[0])
                {
                    case "VanillaCondition":
                        {
                            var vCondition = (Condition)typeof(Condition).GetField(infos[1], BindingFlags.Public | BindingFlags.Static).GetValue(null) ?? throw new Exception("未知的原版条件");
                            condition = player => vCondition.IsMet();
                            break;
                        }
                    case "DownedBoss":
                        {
                            if (ModLoader.HasMod(infos[1].Split('/')[0]))
                                condition = player => ExtenderSystem.DownedBosses.Contains(infos[1]);
                            else
                                condition = player => true;
                            break;
                        }
                    case "InBiome":
                        {
                            if (!ModContent.TryFind<ModBiome>(infos[1], out var biome))
                                condition = player => true;
                            //throw new Exception("未知的环境");
                            else
                                condition = player => player.InModBiome(biome);
                            break;
                        }
                    default:
                        throw new Exception("未知的条件类型");
                }
            }

            pointShop.Call("RegisterGameEnvironment", mod, icon, Name, condition, Priority, Color.White);
        }
    }
    public class ConditionMetaData
    {
        public string Name { get; set; }
        public string DisplayNameZH { get; set; }
        public string DisplayNameEN { get; set; }
        public string DescriptionZH { get; set; }
        public string DescriptionEN { get; set; }
        public string Icon { get; set; }
        public string Condition { get; set; }

        public void Register(Mod pointShop, Mod mod, string filePath)
        {
            if (!ModContent.RequestIfExists<Texture2D>(Icon, out var icon))
            {
                if (!File.Exists(Icon))
                    Icon = Path.Combine(Path.GetFullPath(filePath)[..^Path.GetFileName(filePath).Length], $"{(Icon == "" ? Name : Icon)}.png");
                icon = mod.Assets.CreateUntracked<Texture2D>(File.OpenRead(Icon), Icon);

            }

            ExtenderSystem.ConditionDisplayNameZH[Name] = DisplayNameZH ?? Name;
            ExtenderSystem.ConditionDisplayNameEN[Name] = DisplayNameEN ?? Name;
            ExtenderSystem.ConditionDescriptionZH[Name] = DescriptionZH ?? "";
            ExtenderSystem.ConditionDescriptionEN[Name] = DescriptionEN ?? "";

            Language.GetOrRegister($"Mods.PointShopExtender.UnlockCondition.{Name}.DisplayName", () => Name);
            Language.GetOrRegister($"Mods.PointShopExtender.UnlockCondition.{Name}.Description", () => "");


            Func<bool> condition;
            if (Condition == null || Condition.Length == 0)
                condition = () => true;
            else
            {
                var infos = Condition.Split('|');
                if (infos.Length != 2)
                    throw new Exception("条件信息应当包含两部分，第一部分标明条件类型，第二部分表明该类型下的一个具体值，中间用竖线|分隔");
                switch (infos[0])
                {
                    case "VanillaCondition":
                        {
                            var vCondition = (Condition)typeof(Condition).GetField(infos[1], BindingFlags.Public | BindingFlags.Static).GetValue(null) ?? throw new Exception("未知的原版条件");
                            condition = vCondition.Predicate;
                            break;
                        }
                    case "DownedBoss":
                        {
                            if (ModLoader.HasMod(infos[1].Split('/')[0]))
                                condition = () => ExtenderSystem.DownedBosses.Contains(infos[1]);
                            else
                                condition = () => true;
                            break;
                        }
                    case "InBiome":
                        {
                            if (!ModContent.TryFind<ModBiome>(infos[1], out var biome))
                                condition = () => true;
                            //throw new Exception("未知的环境");
                            else
                                condition = () => Main.LocalPlayer.InModBiome(biome);
                            break;
                        }
                    default:
                        throw new Exception("未知的条件类型");
                }
            }
            pointShop.Call("RegisterCondition", mod, Name, icon, condition);

        }
    }
    public class PointShopExtenderBossManager : GlobalNPC
    {
        public override void OnKill(NPC npc)
        {
            if (npc.boss && npc.ModNPC is ModNPC modNPC && !ExtenderSystem.DownedBosses.Contains(modNPC.FullName))
                ExtenderSystem.DownedBosses.Add(modNPC.FullName);

            base.OnKill(npc);
        }
    }
}
