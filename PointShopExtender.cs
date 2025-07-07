using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using PointShop;
using PointShopExtender.PacketData;
using PointShopExtender.PacketManager;
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
using Terraria.GameInput;
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
        public static PointShopExtender Instance { get; private set; }
        public static PointShop.PointShop PointShopMod { get; private set; }

        public override void Load()
        {
            Instance = this;
            PointShopMod = PointShop.PointShop.Instance;
            base.Load();
        }
    }
    public class PointShopExtenderSystem : ModSystem
    {
        public static readonly string RootPath = Path.Combine(Main.SavePath, "Mods", nameof(PointShopExtender));
        public static LocalizedText GetLocalization(string suffix) => Language.GetText($"Mods.{nameof(PointShopExtender)}.{suffix}");
        public static string GetLocalizationText(string suffix) => GetLocalization(suffix).Value;

        public static HashSet<char> InvaildNameChars { get; private set; }
        public static HashSet<ExtensionPack> ExtensionPacks { get; } = [];

        public static bool AutoInfoMode { get; set; } = true;

        public static Dictionary<string, Condition> VanillaConditionInstances { get; } = [];

        public static HashSet<string> LoadTimeBanList { get; } = [];

        public static HashSet<string> BanList { get; } = [];

        public static HashSet<string> FavList { get; } = [];


        #region 管理模组boss击败情况

        // TODO 从boss列表中获取boss击败情况
        public static HashSet<string> DownedBosses { get; } = [];
        public override void LoadWorldData(TagCompound tag)
        {
            if (tag.ContainsKey("DownedBosses"))
            {
                string[] names = tag.Get<string[]>("DownedBosses");
                int l = names.Length;
                for (int i = 0; i < l; i++)
                    DownedBosses.Add(names[i]);
            }

            PacketMakerUI.IsChinese = Language.ActiveCulture == GameCulture.FromCultureName(GameCulture.CultureName.Chinese);
            base.LoadWorldData(tag);
        }
        public override void SaveWorldData(TagCompound tag)
        {
            tag["DownedBosses"] = DownedBosses.ToArray();
            PacketMakerUI.Active = false;
            base.SaveWorldData(tag);
        }
        #endregion

        #region 杂项
        public static IDeserializer YamlDeserializer { get; } = new DeserializerBuilder().Build();

        public static ISerializer YamlSerializer { get; } = new SerializerBuilder().Build();

        public static ModKeybind ShowManagerKeyBind { get; private set; }

        public override void Load()
        {
            ShowManagerKeyBind = new ModKeybind(Mod, "ShowPacketMakerUI", "P");
            KeybindLoader.RegisterKeybind(ShowManagerKeyBind);

            //MonoModHooks.Add(typeof(LocalizationLoader).GetMethod("LoadTranslations", BindingFlags.Static | BindingFlags.NonPublic), LocalziationModify);

            #region InitCondition
            VanillaConditionInstances.Clear();
            var sfldInfos = typeof(Condition).GetFields(BindingFlags.Public | BindingFlags.Static);
            foreach (var fld in sfldInfos)
            {
                if (fld.GetValue(null) is Condition condition)
                    VanillaConditionInstances.Add(fld.Name, condition);
            }
            #endregion

            InvaildNameChars = [.. Path.GetInvalidFileNameChars()];

            if (File.Exists(RootPath + "BanList.txt"))
            {
                var lines = File.ReadAllLines(RootPath + "BanList.txt");
                foreach (var line in lines) 
                {
                    BanList.Add(line);
                    LoadTimeBanList.Add(line);
                }
            }
            if (File.Exists(RootPath + "FavList.txt"))
            {
                var lines = File.ReadAllLines(RootPath + "FavList.txt");
                foreach (var line in lines)
                    FavList.Add(line);
            }
            base.Load();
        }
        [Obsolete]
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
        #endregion

        #region 加载拓展包

        static void DecompressExamplePack()
        {
            var mainPath = Path.Combine(Main.SavePath, "Mods", nameof(PointShopExtender));
            if (!Directory.Exists(mainPath))
            {
                Directory.CreateDirectory(mainPath);

                Utils.OpenFolder(mainPath);

                var stream = PointShopExtender.Instance.GetFileStream("ExampleShopPack.zip");
                ZipFile.ExtractToDirectory(stream, Path.Combine(mainPath, "ExampleShopPack"), Encoding.GetEncoding("GBK"));
                stream.Close();
            }
        }

        static void LoadPacks()
        {
            var mainPath = Path.Combine(Main.SavePath, "Mods", nameof(PointShopExtender));
            ExtensionPacks.Clear();
            foreach (var dir in Directory.GetDirectories(mainPath))
            {
                var pack = ExtensionPack.FromDirectory(dir);
                ExtensionPacks.Add(pack);
            }

            foreach (var pack in ExtensionPacks)
                if (!BanList.Contains(pack.Name))
                    pack.Register();
        }

        public override void PostSetupContent()
        {
            DecompressExamplePack();

            LoadPacks();
        }

        [Obsolete]
        static void CopyTest()
        {
            var pack = ExtensionPacks.First();
            var name = pack.PackName;
            pack.PackName = "CopyPack";
            var mainPath = Path.Combine(Main.SavePath, "Mods", nameof(PointShopExtender));
            pack.Save(mainPath);
            pack.PackName = name;
        }
        #endregion

        #region 拓展包本地化文本
        public static Dictionary<string, string> EnvironmentDisplayNameZH = [];
        public static Dictionary<string, string> ConditionDisplayNameZH = [];
        public static Dictionary<string, string> ConditionDescriptionZH = [];

        public static Dictionary<string, string> EnvironmentDisplayNameEN = [];
        public static Dictionary<string, string> ConditionDisplayNameEN = [];
        public static Dictionary<string, string> ConditionDescriptionEN = [];
        #endregion

    }
    public class PointShopExtenderBossManager : GlobalNPC
    {
        public override void OnKill(NPC npc)
        {
            if (npc.boss && npc.ModNPC is ModNPC modNPC && !PointShopExtenderSystem.DownedBosses.Contains(modNPC.FullName))
                PointShopExtenderSystem.DownedBosses.Add(modNPC.FullName);

            base.OnKill(npc);
        }
    }


    public class PointShopExtenderPlayer : ModPlayer
    {
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (PointShopExtenderSystem.ShowManagerKeyBind.JustPressed)
            {
                if (PacketMakerUI.Active)
                    PacketMakerUI.Close();
                else
                    PacketMakerUI.Open();
            }
            base.ProcessTriggers(triggersSet);
        }
    }
}
