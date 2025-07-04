using Microsoft.Xna.Framework.Graphics;
using PointShopExtender.PacketData;
using ReLogic.Content;
using SilkyUIFramework;
using SilkyUIFramework.BasicElements;
using SilkyUIFramework.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;

namespace PointShopExtender.PacketManager;

partial class PacketMakerUI
{
    /// <summary>
    /// 拓展包块
    /// </summary>
    class PacketItemElement : SingleItemPanel
    {
        bool CreateNew { get; set; }
        ExtensionPack pack { get; init; }
        public PacketItemElement(ExtensionPack extensionPack, bool createNew, bool readOnlyMode = false) : base()
        {
            CreateNew = createNew;
            pack = extensionPack;
            if (createNew) return;

            if (IsChinese && extensionPack.DisplayName is { Length: > 0 } nameZh)
                SetText(nameZh);
            else if (extensionPack.DisplayNameEn is { Length: > 0 } nameEn)
                SetText(nameEn);
            else if (extensionPack.PackName is { Length: > 0 } nameFile)
                SetText(nameFile);

            SetIcon(extensionPack.Icon);
        }
        public override void OnLeftMouseClick(UIMouseEvent evt)
        {
            if (CreateNew)
                Instance.SwitchToPacketInfoPage(pack);
            else
            {
                // TODO 切换是否启用
                SoundEngine.PlaySound(SoundID.Unlock);
            }
        }
        public override void OnRightMouseClick(UIMouseEvent evt)
        {
            if (!CreateNew)
                Instance.SwitchToPacketInfoPage(pack);
        }
        public override void OnMiddleMouseClick(UIMouseEvent evt)
        {
            if (CreateNew) return;
            SoundEngine.PlaySound(SoundID.Research);
            SoundEngine.PlaySound(SoundID.ResearchComplete);
            // TODO 添加收藏功能
            base.OnMiddleMouseClick(evt);
        }
    }

    /// <summary>
    /// 拓展包信息页
    /// </summary>
    class PacketInfoElement : InfoPagePanel
    {
        ExtensionPack Packet { get; init; }
        public PacketInfoElement(ExtensionPack extensionPack)
        {
            Packet = extensionPack;
            BuildPage();
            Image.Texture2D = Packet.Icon;
        }
        protected override void OnSetIcon(Asset<Texture2D> texture)
        {
            Packet.SetIconAndSave(texture, RootPath);
        }

        protected override void SwitchToContentPage(UIMouseEvent evt, UIView listeningElement)
        {
            Instance.SwitchToBridgePage();
        }

        protected override void OnInitializeTextPanel(UIElementGroup textPanel)
        {
            ContentTextEditablePanel PacketNamePanel = new ("FileName", Packet.PackName);
            PacketNamePanel.Join(textPanel);

            ContentTextEditablePanel DisplayNamePanel = new("DisplayName", Packet.DisplayName);
            DisplayNamePanel.Join(textPanel);

            ContentTextEditablePanel DisplayNameEnPanel = new("DisplayNameEn", Packet.DisplayNameEn);
            DisplayNameEnPanel.Join(textPanel);

            ContentTextEditablePanel AuthorNamePanel = new("AuthorName", Packet.AuthorName);
            AuthorNamePanel.Join(textPanel);

            ContentTextEditablePanel VersionPanel = new("Version", Packet.PackVersion);
            VersionPanel.Join(textPanel);
        }
    }

    /// <summary>
    /// 桥梁作用，转接到三项其中一个的编辑
    /// </summary>
    class BridgeElement : SingleItemPanel
    {
        BridgeState State { get; init; }
        public BridgeElement(BridgeState state) : base()
        {
            switch (state)
            {
                case BridgeState.Shop:
                    {
                        SetIcon(ModAsset.ShopIcon);
                        SetText(GetLocalizedTextValue("Shop"));
                        break;
                    }
                case BridgeState.Environment:
                    {
                        SetIcon(ModAsset.EnvironmentIcon);
                        SetText(GetLocalizedTextValue("Environment"));
                        break;
                    }
                case BridgeState.Condition:
                    {
                        SetIcon(ModAsset.UnlockConditionIcon);
                        SetText(GetLocalizedTextValue("UnlockCondition"));
                        break;
                    }
            }
            State = state;
            SetHeight(0, 1f);
            SetWidth(0, 0.25f);
            SetMargin(16f);
        }
        public override void OnLeftMouseClick(UIMouseEvent evt)
        {
            switch (State)
            {
                case BridgeState.Shop:
                    {
                        Instance.SwitchToShopItemPage();
                        break;
                    }
                case BridgeState.Environment:
                    {
                        Instance.SwitchToEnvironmentItemPage();
                        break;
                    }
                case BridgeState.Condition:
                    {
                        Instance.SwitchToConditionItemPage();
                        break;
                    }
            }
            base.OnLeftMouseClick(evt);
        }
    }
    enum BridgeState
    {
        Shop,
        Environment,
        Condition
    }



}
