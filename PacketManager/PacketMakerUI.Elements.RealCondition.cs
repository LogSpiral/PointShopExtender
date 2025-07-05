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
        object Owner { get; init; }
        public ConditionTypeElement(ConditionType state, RealCondition realCondition,object owner) : base()
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
            Owner = owner;
            SetHeight(0, 0.9f);
            SetTop(0, 0, 0.5f);
            SetWidth(0, 0.24f);
            SetMargin(16f);
        }
        public override void OnLeftMouseClick(UIMouseEvent evt)
        {
            switch (State)
            {
                case ConditionType.None:
                    {
                        RealCondition.ConditionType = ConditionType.None;
                        RealCondition.ConditionContent = "";

                        if (Owner is ConditionExtension conditionExtension)
                            Instance.SwitchToConditionInfoPage(conditionExtension);

                        if (Owner is EnvironmentExtension environmentExtension)
                            Instance.SwitchToEnvironmentInfoPage(environmentExtension);
                        break;
                    }
                case ConditionType.Vanilla:
                    {
                        Instance.SwitchToVanillaConditions(RealCondition, Owner);
                        break;
                    }
                case ConditionType.ModEnvironment:
                    {
                        Instance.SwitchToModdedEnvironments(RealCondition, Owner);
                        break;
                    }
                case ConditionType.ModBoss:
                    {
                        Instance.SwitchToModdedBosses(RealCondition, Owner);
                        break;
                    }
            }
            base.OnLeftMouseClick(evt);
        }
    }

    abstract class ConditionSetterItemElement : SingleItemPanel
    {
        protected RealCondition RealCondition { get; set; }
        protected object Owner { get; set; }
        protected ConditionSetterItemElement(RealCondition realCondition, object owner) : base()
        {
            RealCondition = realCondition;
            Owner = owner;
            //SetWidth(0, 0.19f);
            //SetHeight(150f, 0);
        }
        public override void OnLeftMouseClick(UIMouseEvent evt)
        {
            if (Owner is ConditionExtension conditionExtension)
                Instance.SwitchToConditionInfoPage(conditionExtension);

            if (Owner is EnvironmentExtension environmentExtension)
                Instance.SwitchToEnvironmentInfoPage(environmentExtension);
        }
    }

    class VanillaConditionItemElement : ConditionSetterItemElement
    {
        string ConditionFieldName { get; set; }
        public VanillaConditionItemElement(string name, RealCondition realCondition, object owner) : base(realCondition, owner)
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
        public ModEnvironmentItemElement(ModBiome modBiome, RealCondition realCondition, object owner) : base(realCondition, owner)
        {
            ModBiome = modBiome;

            string text = modBiome.BestiaryIcon;
            if (modBiome.Name is "AstralCaveDesert")
                text = text.Replace("CaveDesert", "DesertCave"); // FKU CLMT
            SetIcon(ModContent.Request<Texture2D>(text));

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
        public ModBossItemElement(ModNPC modBoss, RealCondition realCondition, object owner) : base(realCondition, owner)
        {
            ModBoss = modBoss;

            try
            {
                var index = NPCID.Sets.BossHeadTextures[modBoss.Type];
                NPCLoader.BossHeadSlot(ContentSamples.NpcsByNetId[modBoss.Type], ref index);
                SetIcon(TextureAssets.NpcHeadBoss[index]);
            }
            catch 
            {
                Main.NewText(modBoss.Name);
            }

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
