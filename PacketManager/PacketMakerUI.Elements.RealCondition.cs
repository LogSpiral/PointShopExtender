using Microsoft.Xna.Framework.Graphics;
using PointShopExtender.PacketData;
using SilkyUIFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace PointShopExtender.PacketManager;

partial class PacketMakerUI
{
    class ConditionTypeElement : SingleItemPanel
    {
        ConditionType State { get; init; }
        RealCondition RealCondition { get; set; }
        public ConditionTypeElement(ConditionType state, RealCondition realCondition) : base()
        {
            switch (state)
            {
                case ConditionType.None:
                    {
                        SetIcon(ModAsset.NoneConditionIcon);
                        SetText(GetLocalizedTextValue("NoneCondition"));
                        break;
                    }
                case ConditionType.Vanilla:
                    {
                        SetIcon(ModAsset.VanillaConditionIcon);
                        SetText(GetLocalizedTextValue("VanillaCondition"));
                        break;
                    }
                case ConditionType.ModEnvironment:
                    {
                        SetIcon(ModAsset.ModEnvironmentIcon);
                        SetText(GetLocalizedTextValue("ModEnvironment"));
                        break;
                    }
                case ConditionType.ModBoss:
                    {
                        SetIcon(ModAsset.ModBossIcon);
                        SetText(GetLocalizedTextValue("ModBoss"));
                        break;
                    }
            }
            RealCondition = realCondition;
            State = state;
            SetHeight(0, 0.9f);
            SetTop(0, 0, 0.5f);
            SetWidth(0, 0.24f);
            SetPadding(16f);
        }
        public override void OnLeftMouseClick(UIMouseEvent evt)
        {
            switch (State)
            {
                case ConditionType.None:
                    {
                        RealCondition.ConditionType = ConditionType.None;
                        RealCondition.ConditionContent = "";
                        RealCondition.SaveFile();
                        Instance.PathTracker.ReturnToPreviousPage();
                        break;
                    }
                case ConditionType.Vanilla:
                    {
                        Instance.SwitchToVanillaConditions(RealCondition);
                        break;
                    }
                case ConditionType.ModEnvironment:
                    {
                        Instance.SwitchToModdedEnvironments(RealCondition);
                        break;
                    }
                case ConditionType.ModBoss:
                    {
                        Instance.SwitchToModdedBosses(RealCondition);
                        break;
                    }
            }
            base.OnLeftMouseClick(evt);
        }
    }

    abstract class ConditionSetterItemElement : SingleItemPanel
    {
        protected RealCondition RealCondition { get; set; }
        protected ConditionSetterItemElement(RealCondition realCondition) : base()
        {
            RealCondition = realCondition;
        }
        public override void OnLeftMouseClick(UIMouseEvent evt)
        {
            RealCondition.SaveFile();
            Instance.PathTracker.ReturnToPreviousPage(2);
        }
    }

    class VanillaConditionItemElement : ConditionSetterItemElement
    {
        string ConditionFieldName { get; set; }
        public VanillaConditionItemElement(string name, RealCondition realCondition) : base(realCondition)
        {
            ConditionFieldName = name;
            if (PointShopExtenderSystem.VanillaConditionInstances.TryGetValue(name, out var condition))
                SetText(condition.Description.Value);
        }
        public override void OnLeftMouseClick(UIMouseEvent evt)
        {
            RealCondition.ConditionType = ConditionType.Vanilla;
            RealCondition.ConditionContent = ConditionFieldName;
            base.OnLeftMouseClick(evt);
        }
    }

    class ModEnvironmentItemElement : ConditionSetterItemElement
    {
        ModBiome ModBiome { get; set; }
        public ModEnvironmentItemElement(ModBiome modBiome, RealCondition realCondition) : base(realCondition)
        {
            ModBiome = modBiome;

            string text = modBiome.BestiaryIcon;
            if (modBiome.Name is "AstralCaveDesert")
                text = text.Replace("CaveDesert", "DesertCave"); // FKU CLMT

            if (ModContent.RequestIfExists<Texture2D>(text, out var icon))
                SetIcon(icon);

            SetText(modBiome.DisplayName.Value);
        }
        public override void OnLeftMouseClick(UIMouseEvent evt)
        {
            RealCondition.ConditionType = ConditionType.ModEnvironment;
            RealCondition.ConditionContent = ModBiome.FullName;
            base.OnLeftMouseClick(evt);
        }
    }

    class ModBossItemElement : ConditionSetterItemElement
    {
        ModNPC ModBoss { get; set; }
        public ModBossItemElement(ModNPC modBoss, RealCondition realCondition) : base(realCondition)
        {
            ModBoss = modBoss;

            var index = NPCID.Sets.BossHeadTextures[modBoss.Type];
            NPCLoader.BossHeadSlot(ContentSamples.NpcsByNetId[modBoss.Type], ref index);
            SetIcon(TextureAssets.NpcHeadBoss[index]);

            SetText(modBoss.DisplayName.Value);
        }
        public override void OnLeftMouseClick(UIMouseEvent evt)
        {
            RealCondition.ConditionType = ConditionType.ModBoss;
            RealCondition.ConditionContent = ModBoss.FullName;
            base.OnLeftMouseClick(evt);
        }
    }
}
