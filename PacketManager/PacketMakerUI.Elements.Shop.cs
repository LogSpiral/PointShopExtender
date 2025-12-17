using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PointShop;
using PointShop.Registrar;
using PointShop.ShopSystem;
using PointShop.UserInterfaces;
using PointShopExtender.PacketData;
using ReLogic.Content;
using SilkyUIFramework;
using SilkyUIFramework.Layout;
using SilkyUIFramework.Elements;
using SilkyUIFramework.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Default;

namespace PointShopExtender.PacketManager;

partial class PacketMakerUI
{
    /// <summary>
    /// 商店块
    /// </summary>
    class ShopItemElement : SingleItemPanel
    {
        bool CreateNew { get; set; }
        ShopExtension Shop { get; init; }
        public ShopItemElement(ShopExtension shop, bool createNew) : base()
        {
            CreateNew = createNew;
            Shop = shop;
            if (createNew) return;

            if (shop.Name is { Length: > 0 } nameFile)
                SetText(nameFile);

            SetIcon(shop.IconTexture);
        }
        public override void OnLeftMouseClick(UIMouseEvent evt)
        {
            Instance.SwitchToShopInfoPage(Shop);
        }
    }

    /// <summary>
    /// 商店信息页
    /// </summary>
    class ShopInfoElement : InfoPagePanel
    {
        ShopExtension Shop { get; init; }
        public ShopInfoElement(ShopExtension shop)
        {
            Shop = shop;
            BuildPage();
            Image.Texture2D = shop.IconTexture;

            ImagePanel.SetSize(90, 90, 0, 0);
            ImagePanel.SetLeft(20, 0, 0);
            ImagePanel.SetTop(0, 0.1f, 0);
            TextPanel.SetWidth(-70, 0.9f);
        }
        protected override void OnSetIcon(Asset<Texture2D> texture)
        {
            Shop.SetIconAndSave(texture);
        }
        protected override void OpenFileDialogueToSelectIcon(UIView listeningElement, UIMouseEvent evt)
        {
            if (string.IsNullOrEmpty(Shop.Name))
                GiveANameHint();
            else
                base.OpenFileDialogueToSelectIcon(listeningElement, evt);
        }
        protected override void OnInitializeTextPanel(UIElementGroup textPanel)
        {
            ContentTextEditablePanel FileNamePanel = new("FileName", Shop.Name);
            FileNamePanel.ContentText.ContentChanging += (sender, e) =>
            {
                var text = e.NewText;
                FileNameCommonCheck(ref text);
                return text;
            };
            FileNamePanel.ContentText.EndTakingInput += (sender, arg) =>
            {
                var current = arg.NewValue;
                var old = arg.OldValue;
                if (string.IsNullOrEmpty(current)) FileNamePanel.ContentText.Text = old;
                Shop.RenameFile(current);
            };
            FileNamePanel.SetBorderRadius(new(24, 0, 0, 0));
            FileNamePanel.Join(textPanel);

            ContentSingleTextPanel EditHintPanel = new("EditContent");
            EditHintPanel.LeftMouseClick += delegate
            {
                if (string.IsNullOrEmpty(Shop.Name))
                {
                    GiveANameHint();
                    return;
                }
                Instance.SwitchToShopContentPage(Shop);
            };
            EditHintPanel.SetBorderRadius(new(0, 0, 0, 24));
            EditHintPanel.Join(textPanel);
        }
    }

    class ShopContentElement : UIElementGroup
    {
        ShopExtension ShopExtension { get; set; }

        /// <summary>
        /// 菜单列表
        /// </summary>
        public SUIScrollView MenuListScrollView { get; private set; }
        /// <summary>
        /// 内容容器（所有内容的容器）
        /// </summary>
        public UIElementGroup ContentContainer { get; private set; }
        /// <summary>
        /// 商品表
        /// </summary>
        public SUIScrollView ShopItemTableScrollView { get; private set; }

        public static bool ShopItemTableIsDirty { get; set; } = true;

        /// <summary>
        /// 商店UI中当前显示的环境商店内部名称
        /// </summary>
        public static string CurrentEnvironmentName
        {
            get;
            set
            {
                if (field == value) return;
                field = value;
                ShopItemTableIsDirty = true;
            }
        } = "Forest";

        public ShopContentElement(ShopExtension shopExtension)
        {
            ShopExtension = shopExtension;
            SetWidth(0, 1);
            SetHeight(0, 1);

            // 菜单列表 and 商品列表

            ContentContainer = new UIElementGroup
            {
                LayoutType = LayoutType.Flexbox,
                FlexDirection = FlexDirection.Row,
                CrossAlignment = CrossAlignment.Stretch,
                CrossContentAlignment = CrossContentAlignment.Stretch,
                FlexWrap = false,
                Gap = new Vector2(0f),
            }.Join(this);
            ContentContainer.SetSize(0f, 450f, 1f);

            MenuListScrollView = new SUIScrollView
            {
                Gap = new Vector2(4f),
                Mask =
            {
                Border = 2,
                BorderRadius = new Vector4(4),
                BorderColor = Color.Black * 0.75f,
            },
                Container =
            {
                HiddenBox = HiddenBox.Inner,
                Gap = Size.Zero,
            }
            }.Join(ContentContainer);
            MenuListScrollView.SetPadding(4f);
            MenuListScrollView.SetSize(0f, 0f, 0.25f, 1f);

            UpdateMenuList();

            SUIDividingLine.Vertical(Color.Black * 0.75f).Join(ContentContainer);

            // 商品列表
            var rightContainer = new UIElementGroup
            {
                LayoutType = LayoutType.Flexbox,
                FlexWrap = false,
                FlexDirection = FlexDirection.Column,
                FlexGrow = 1f,
            }.Join(ContentContainer);
            rightContainer.SetHeight(0f, 1f);

            #region 过滤器

            Instance.SearchBarPanel.Join(rightContainer);

            #endregion

            // 商品表格
            ShopItemTableScrollView = new SUIScrollView
            {
                Gap = new Vector2(4),
                FlexGrow = 1f,
            }.Join(rightContainer);
            ShopItemTableScrollView.SetPadding(4f);
            ShopItemTableScrollView.SetWidth(0f, 1f);
            ShopItemTableScrollView.Container.Gap = new Vector2(4);
        }

        protected override void UpdateStatus(GameTime gameTime)
        {
            if (ShopItemTableIsDirty)
            {
                UpdateShopItemTable();
                ShopItemTableIsDirty = false;
            }
            base.UpdateStatus(gameTime);
        }

        public void UpdateMenuList()
        {
            var loadedEnvironments = PointShopSystem.Environments;
            HashSet<GameEnvironment> packEnvironments = [];



            HashSet<string> AddedEnvironmentName = [];
            foreach (var environment in loadedEnvironments)
                AddedEnvironmentName.Add(environment.Name);

            SUIDividingLine line = null;
            foreach (var extension in CurrentPack.EnvironmentExtensions)
            {
                if (AddedEnvironmentName.Contains(extension.Name)) continue;
                packEnvironments.Add(extension.ToGameEnvironmentExtended());
                AddedEnvironmentName.Add(extension.Name);
            }
            GameEnvironment[] commonArray = [new CommonEnvironment()];
            IEnumerable<GameEnvironment> environments = commonArray.Union(loadedEnvironments).Union(packEnvironments);

            foreach (var environment in environments)
            {
                var button = new SUIMenuComponent(environment).Join(MenuListScrollView.Container);
                button.LeftMouseDown += (_, _) =>
                {
                    CurrentEnvironmentName = environment.Name;
                };
                line = SUIDividingLine.Horizontal(SUIColor.Border * 0.75f).Join(MenuListScrollView.Container);
            }
            line.RemoveFromParent();
        }
        private static bool ShopItemFilters(Item item) => item.Name.Contains(Instance.SearchBox.Text.Trim());

        /// <summary>
        /// 更新物品表格
        /// </summary>
        public void UpdateShopItemTable()
        {
            List<SimpleShopItemGenerator> list;
            if (CurrentEnvironmentName is "Common")
                list = ShopExtension.SimpleShopData.CommonItems;
            else
            {
                var dict = ShopExtension.SimpleShopData.EnvironmentShopItems;
                if (!dict.TryGetValue(CurrentEnvironmentName, out list))
                {
                    list = [];
                    dict.Add(CurrentEnvironmentName, list);
                }
            }


            ShopItemTableScrollView.Container.RemoveAllChildren();

            var newItem = new SimpleShopItemGenerator();
            ShopItemTableScrollView.Container.AddChild(new SimpleShopItemPreviewElement(newItem, () => list.Add(newItem)));
            foreach (var shopItem in list)
            {
                if (!int.TryParse(shopItem.Type, out var id))
                {
                    if (!ItemID.Search.TryGetId(shopItem.Type, out id))
                        id = ModContent.ItemType<UnloadedItem>();
                }
                var item = ContentSamples.ItemsByType[id];

                if (!ShopItemFilters(item)) continue;

                ShopItemTableScrollView.Container.AddChild(new SimpleShopItemPreviewElement(shopItem, null));
            }
        }
    }

    class ShopItemEditorPage : InfoPagePanel
    {
        SimpleShopItemGenerator ShopItem { get; set; }
        SUIItemSlot ItemSlot { get; set; }
        Action AppendCallBack { get; set; }
        public ShopItemEditorPage(SimpleShopItemGenerator shopItem, Action appendToListCallBack)
        {
            AppendCallBack = appendToListCallBack;
            // LayoutType = LayoutType.Custom;
            ShopItem = shopItem;
            BuildPage();
            int id;
            if (string.IsNullOrEmpty(shopItem.Type))
                id = CreateNewDummyItem.ID();
            else if (!int.TryParse(shopItem.Type, out id))
            {
                if (!ItemID.Search.TryGetId(shopItem.Type, out id))
                    id = ModContent.ItemType<UnloadedItem>();
            }

            // Image.Texture2D = TextureAssets.Item[ItemID.None];

            ImagePanel.RemoveAllChildren();

            ItemSlot = new SUIItemSlot();
            ItemSlot.SetLeft(0, 0, 0.5f);
            ItemSlot.SetTop(0, 0, 0.5f);
            ItemSlot.Item = ContentSamples.ItemsByType[id];
            ItemSlot.ItemAlign = new(.5f);
            ItemSlot.Join(ImagePanel);
            ItemSlot.ItemInteractive = false;

            ImagePanel.SetSize(135, 135, 0, 0);
            ImagePanel.SetLeft(20, 0, 0);
            ImagePanel.SetTop(0, 0.1f, 0);
            TextPanel.SetWidth(-120, 0.9f);
        }
        protected override void OpenFileDialogueToSelectIcon(UIView listeningElement, UIMouseEvent evt)
        {
            Instance.SwitchToSingleShopItemPage(ShopItem, AppendCallBack);
        }

        protected override void OnInitializeTextPanel(UIElementGroup textPanel)
        {
            ContentTextEditablePanel PricePanel = new("Price", ShopItem.Prices.ToString());
            PricePanel.ContentText.GotFocus += (sender, evt) =>
            {
                if (string.IsNullOrEmpty(ShopItem.Type))
                {
                    // sender.SilkyUI.SetFocus(null);
                    PointShopExtender.UpdateFocusedElementCall();
                    ChooseAItemHint();
                    return;
                }
            };
            PricePanel.ContentText.ContentChanging += (sender, e) =>
            {
                var text = e.NewText;
                DigitsCommonCheck(ref text);
                return text;
            };
            PricePanel.ContentText.EndTakingInput += (sender, arg) =>
            {
                var current = arg.NewValue;
                var old = arg.OldValue;
                if (int.TryParse(current, out var value) && value >= 0)
                {
                    ShopItem.Prices = value;
                    PendingShopModified = true;
                }
                else
                    PricePanel.ContentText.Text = old;

            };
            PricePanel.SetBorderRadius(new(24, 0, 0, 0));
            PricePanel.Join(textPanel);

            ContentTextEditablePanel QuantityPanel = new("Quantity", ShopItem.Quantity.ToString());
            QuantityPanel.ContentText.GotFocus += (sender, evt) =>
            {
                if (string.IsNullOrEmpty(ShopItem.Type))
                {
                    // sender.SilkyUI.SetFocus(null);
                    PointShopExtender.UpdateFocusedElementCall();
                    ChooseAItemHint();
                    return;
                }
            };
            QuantityPanel.ContentText.ContentChanging += (sender, e) =>
            {
                var text = e.NewText;
                DigitsCommonCheck(ref text);
                return text;
            };
            QuantityPanel.ContentText.EndTakingInput += (sender, arg) =>
            {
                var current = arg.NewValue;
                var old = arg.OldValue;
                if (int.TryParse(current, out var value) && value >= 0)
                {
                    ShopItem.Quantity = value;
                    PendingShopModified = true;
                }
                else
                    QuantityPanel.ContentText.Text = old;
            };
            QuantityPanel.Join(textPanel);

            UnlockConditionEntryPanel UnlockConditionPanel = new(ShopItem);
            UnlockConditionPanel.SetBorderRadius(new(0, 0, 0, 24));
            UnlockConditionPanel.Join(textPanel);

        }

        protected override void OnSetIcon(Asset<Texture2D> texture)
        {

        }
    }

    class ShopSingleItemPanel : SingleItemPanel
    {
        SUIItemSlot ItemSlot { get; set; }
        SimpleShopItemGenerator SimpleShopItem { get; set; }
        Action AppendCallBack { get; set; }
        public ShopSingleItemPanel(Item item, SimpleShopItemGenerator simpleShopItem, Action appendCallBack)
        {
            AppendCallBack = appendCallBack;
            SimpleShopItem = simpleShopItem;
            // SetIcon(TextureAssets.Item[item.type]);
            ItemSlot = new SUIItemSlot();
            ItemSlot.SetLeft(0, 0, 0.5f);
            ItemSlot.SetTop(0, 0, 0.5f);
            ItemSlot.Item = item;
            ItemSlot.ItemAlign = new(.5f);
            ItemSlot.Join(this);
            ItemSlot.ItemInteractive = false;

            SetIcon(TextureAssets.Item[ItemID.None]);
            SetText(item.Name);

            SetWidth(0, 0.19f);
            SetHeight(150f, 0);

        }

        public override void OnLeftMouseClick(UIMouseEvent evt)
        {
            if (ItemSlot.Item.ModItem is { } modItem)
                SimpleShopItem.Type = modItem.FullName;
            else
                SimpleShopItem.Type = ItemSlot.Item.type.ToString();
            PendingShopModified = true;
            AppendCallBack?.Invoke();
            Instance.PathTracker.ReturnToPreviousPage();
            base.OnLeftMouseClick(evt);
        }
    }

    class UnlockConditionItemPanel : SingleItemPanel
    {
        SimpleShopItemGenerator SimpleShopItem { get; set; }
        string Condition { get; set; }
        public UnlockConditionItemPanel(ConditionExtension condition, SimpleShopItemGenerator simpleShopItem) : base()
        {
            if (condition is null)
            {
                SetText(GetLocalizedTextValue("NoneCondition"));
                SetIcon(ModAsset.NoneConditionIcon);
                Condition = "";
            }
            else
            {
                SetText(condition.GetDisplayName());
                SetIcon(condition.IconTexture);
                Condition = condition.Name;
            }
            SimpleShopItem = simpleShopItem;
        }
        public UnlockConditionItemPanel(UnlockCondition condition, SimpleShopItemGenerator simpleShopItem) : base()
        {
            SetText(condition.DisplayName);
            SetIcon(condition.Icon);
            SimpleShopItem = simpleShopItem;
            Condition = condition.Name;
        }

        public override void OnLeftMouseClick(UIMouseEvent evt)
        {
            SimpleShopItem.UnlockCondition = Condition;
            PendingShopModified = true;
            Instance.PathTracker.ReturnToPreviousPage();
            base.OnLeftMouseClick(evt);
        }
    }
    class CommonEnvironment : GameEnvironment
    {
        public CommonEnvironment() : base(PointShopExtender.Instance, ModAsset.EnvironmentIconDefault, "Common", 0, default)
        {

        }
        public override string DisplayName => GetLocalizedTextValue("CommonEnvironment");
    }
}
