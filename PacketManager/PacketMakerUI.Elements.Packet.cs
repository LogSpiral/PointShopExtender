using Microsoft.VisualBasic.FileIO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PointShopExtender.PacketData;
using ReLogic.Content;
using SilkyUIFramework;
using SilkyUIFramework.BasicElements;
using SilkyUIFramework.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
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

            SetText(extensionPack.GetDisplayName());
            SetIcon(extensionPack.Icon);

            bool favorited = PointShopExtenderSystem.FavList.Contains(pack.Name);
            var button = FastImageButton(favorited ? ModAsset.Favorite : ModAsset.Delete);
            button.MouseEnter += delegate
            {
                MouseHoverInfo = GetLocalizedTextValue(favorited ? "Favorited" : "Delete");
            };
            button.MouseLeave += delegate
            {
                MouseHoverInfo = "";
            };
            if (!favorited)
                button.LeftMouseClick += delegate
                {
                    FileSystem.DeleteDirectory(Path.Combine(RootPath, pack.Name), UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                    PendingUpdateList = true;
                    PointShopExtenderSystem.ExtensionPacks.RemoveWhere(pak => pak.Name == pack.Name);
                };
            button.SetTop(0, 0, 0);
            button.SetLeft(0, 0, 1);
            button.SetSize(32, 32);
            button.Join(this);
        }
        public override void OnLeftMouseClick(UIMouseEvent evt)
        {
            if (evt.Source != this) return;
            Instance.SwitchToPacketInfoPage(pack);
        }
        public override void OnRightMouseClick(UIMouseEvent evt)
        {
            if (CreateNew) return;
            SoundEngine.PlaySound(SoundID.Unlock);
            if (!PointShopExtenderSystem.BanList.Add(pack.Name))
                PointShopExtenderSystem.BanList.Remove(pack.Name);
            File.WriteAllLines(RootPath + "BanList.txt", PointShopExtenderSystem.BanList);
        }
        public override void OnMiddleMouseClick(UIMouseEvent evt)
        {
            if (CreateNew) return;
            SoundEngine.PlaySound(SoundID.Research);
            SoundEngine.PlaySound(SoundID.ResearchComplete);
            if (!PointShopExtenderSystem.FavList.Add(pack.Name))
                PointShopExtenderSystem.FavList.Remove(pack.Name);
            File.WriteAllLines(RootPath + "FavList.txt", PointShopExtenderSystem.FavList);
            PendingUpdateList = true;
            base.OnMiddleMouseClick(evt);
        }
        protected override void UpdateStatus(GameTime gameTime)
        {
            base.UpdateStatus(gameTime);
            if (CreateNew) return;
            Color c;
            bool currentBan = PointShopExtenderSystem.BanList.Contains(pack.Name);
            bool loadTimeBan = PointShopExtenderSystem.LoadTimeBanList.Contains(pack.Name);
            if (currentBan)
            {
                if (loadTimeBan)
                    c = Color.Gray;
                else
                    c = Color.Red * .5f;
            }
            else
            {
                if (loadTimeBan)
                    c = Color.Cyan * .5f;
                else
                    c = Color.Black * .25f;
            }

            if (PointShopExtenderSystem.FavList.Contains(pack.Name))
                c = Color.Lerp(c, SUIColor.Highlight, 0.4f);

            BackgroundColor = Color.Lerp(BackgroundColor, c, 0.25f);


        }

        public override void OnMouseEnter(UIMouseEvent evt)
        {
            if (CreateNew) return;
            if (evt.Source != this) return;
            string msg = "";
            bool currentBan = PointShopExtenderSystem.BanList.Contains(pack.Name);
            bool loadTimeBan = PointShopExtenderSystem.LoadTimeBanList.Contains(pack.Name);
            if (currentBan)
            {
                if (loadTimeBan)
                    msg = GetLocalizedTextValue("Disabled");
                else
                    msg = GetLocalizedTextValue("ReloadToDisable");
            }
            else
            {
                if (loadTimeBan)
                    msg = GetLocalizedTextValue("ReloadToEnable");
            }
            if (!string.IsNullOrEmpty(msg))
                MouseHoverInfo = msg;
            base.OnMouseEnter(evt);
        }
        public override void OnMouseLeave(UIMouseEvent evt)
        {
            if (CreateNew) return;
            if (evt.Source != this) return;
            MouseHoverInfo = "";
            base.OnMouseLeave(evt);
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
            ImagePanel.SetSize(280, 280, 0, 0);
            ImagePanel.SetTop(4, 0.05f, 0);
            ImagePanel.SetLeft(10, 0, 0);
        }
        protected override void OnSetIcon(Asset<Texture2D> texture)
        {
            Packet.SetIconAndSave(texture, RootPath);
        }

        protected override void OnInitializeTextPanel(UIElementGroup textPanel)
        {
            ContentTextEditablePanel PacketNamePanel = new("FileName", Packet.PackName);
            PacketNamePanel.ContentText.PreTextChangeEvent += FileNameCommonCheck;
            PacketNamePanel.ContentText.EndTakingInput += (old, current) =>
            {
                if (string.IsNullOrEmpty(current)) PacketNamePanel.ContentText.Text = old;
                Packet.RenamePack(current);
            };
            PacketNamePanel.SetBorderRadius(new(24, 0, 0, 0));
            PacketNamePanel.Join(textPanel);

            ContentTextEditablePanel DisplayNamePanel = new("DisplayName", Packet.DisplayName);
            DisplayNamePanel.GotFocus += (evt, elem) => ExtensionFileNameCheckCommon(Packet, elem);
            DisplayNamePanel.ContentText.EndTakingInput += (old, current) =>
            {
                Packet.SetDispalyNameAndSave(current);
            };
            DisplayNamePanel.Join(textPanel);

            ContentTextEditablePanel DisplayNameEnPanel = new("DisplayNameEn", Packet.DisplayNameEn);
            DisplayNameEnPanel.GotFocus += (evt, elem) => ExtensionFileNameCheckCommon(Packet, elem);
            DisplayNameEnPanel.ContentText.EndTakingInput += (old, current) =>
            {
                Packet.SetDispalyNameEnAndSave(current);
            };
            DisplayNameEnPanel.Join(textPanel);

            ContentTextEditablePanel AuthorNamePanel = new("AuthorName", Packet.AuthorName);
            AuthorNamePanel.GotFocus += (evt, elem) => ExtensionFileNameCheckCommon(Packet, elem);
            AuthorNamePanel.ContentText.EndTakingInput += (old, current) =>
            {
                Packet.SetAuthorNameAndSave(current);
            };
            AuthorNamePanel.Join(textPanel);

            ContentTextEditablePanel VersionPanel = new("Version", Packet.PackVersion);
            VersionPanel.GotFocus += (evt, elem) => ExtensionFileNameCheckCommon(Packet, elem);
            VersionPanel.ContentText.EndTakingInput += (old, current) =>
            {
                Packet.SetVersionAndSave(current);
            };
            //VersionPanel.SetBorderRadius(new(0, 0, 0, 24));
            VersionPanel.Join(textPanel);

            ContentSingleTextPanel EditHintPanel = new("EditContent");

            EditHintPanel.LeftMouseClick += delegate
            {
                if (string.IsNullOrEmpty(Packet.PackName))
                {
                    GiveANameHint();
                    return;
                }
                Instance.SwitchToBridgePage();
            };
            EditHintPanel.SetBorderRadius(new(0, 0, 0, 24));
            EditHintPanel.Join(textPanel);
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
            SetHeight(0, 0.9f);
            SetTop(0, 0, 0.5f);
            SetWidth(0, 0.325f);
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
