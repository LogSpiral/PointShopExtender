using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PointShop;
using PointShop.Registrar;
using PointShop.ShopSystem;
using PointShop.UserInterfaces;
using PointShopExtender.PacketData;
using ReLogic.Content;
using SilkyUIFramework;
using SilkyUIFramework.BasicComponents;
using SilkyUIFramework.BasicElements;
using SilkyUIFramework.Extensions;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.ModLoader.Default;
using static Terraria.GameContent.Animations.IL_Actions.NPCs;

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
            // Packet.SetIconAndSave(texture, RootPath);
        }

        protected override void OnInitializeTextPanel(UIElementGroup textPanel)
        {
            ContentTextEditablePanel PacketNamePanel = new("FileName", Shop.Name);
            PacketNamePanel.SetBorderRadius(new(24, 0, 0, 0));
            PacketNamePanel.Join(textPanel);

            ContentSingleTextPanel EditHintPanel = new("EditContent");
            EditHintPanel.LeftMouseClick += delegate
            {
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
            var environments = PointShopSystem.Environments;

            for (int i = 0; i < environments.Count; i++)
            {
                var environment = environments[i];
                var button = new SUIMenuComponent(environment).Join(MenuListScrollView.Container);

                button.LeftMouseDown += (_, _) =>
                {
                    CurrentEnvironmentName = environment.Name;
                };

                if (i + 1 != environments.Count)
                {
                    SUIDividingLine.Horizontal(SUIColor.Border * 0.75f).Join(MenuListScrollView.Container);
                }
            }
        }
        private static bool ShopItemFilters(Item item) => item.Name.Contains(Instance.SearchBox.Text.Trim());

        /// <summary>
        /// 更新物品表格
        /// </summary>
        public void UpdateShopItemTable()
        {
            var dict = ShopExtension.SimpleShopData.EnvironmentShopItems;
            if (!dict.TryGetValue(CurrentEnvironmentName, out var list))
            {
                list = [];
                dict.Add(CurrentEnvironmentName, list);
            }

            ShopItemTableScrollView.Container.RemoveAllChildren();

            ShopItemTableScrollView.Container.AppendChild(new SimpleShopItemPreviewElement(new SimpleShopItemGenerator()));
            foreach (var shopItem in list)
            {
                if (!int.TryParse(shopItem.Type, out var id))
                {
                    if (!ItemID.Search.TryGetId(shopItem.Type, out id))
                        id = ModContent.ItemType<UnloadedItem>();
                }
                var item = ContentSamples.ItemsByType[id];

                if (!ShopItemFilters(item)) continue;

                ShopItemTableScrollView.Container.AppendChild(new SimpleShopItemPreviewElement(shopItem));
            }
        }
    }

    class ShopItemEditorPage : InfoPagePanel
    {
        SimpleShopItemGenerator ShopItem { get; set; }
        public ShopItemEditorPage(SimpleShopItemGenerator shopItem)
        {

            ShopItem = shopItem;
            BuildPage();
            if (!int.TryParse(shopItem.Type, out var id))
            {
                if (!ItemID.Search.TryGetId(shopItem.Type, out id))
                    id = ModContent.ItemType<UnloadedItem>();
            }
            Image.Texture2D = TextureAssets.Item[id];

            ImagePanel.SetSize(135, 135, 0, 0);
            ImagePanel.SetLeft(20, 0, 0);
            ImagePanel.SetTop(0, 0.1f, 0);
            TextPanel.SetWidth(-120, 0.9f);
        }
        protected override void OpenFileDialogueToSelectIcon(UIMouseEvent evt, UIView listeningElement)
        {
            Instance.SwitchToSingleShopItemPage(ShopItem);
        }

        protected override void OnInitializeTextPanel(UIElementGroup textPanel)
        {
            ContentTextEditablePanel PricePanel = new ("Price", ShopItem.Prices.ToString());
            PricePanel.SetBorderRadius(new(24,0,0,0));
            PricePanel.Join(textPanel);

            ContentTextEditablePanel QuantityPanel = new("Quantity", ShopItem.Quantity.ToString());
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
        public ShopSingleItemPanel(Item item)
        {
            SetIcon(TextureAssets.Item[item.type]);
            SetText(item.Name);

            SetWidth(0, 0.19f);
            SetHeight(150f, 0);
        }
    }

    class UnlockConditionItemPanel : SingleItemPanel
    {
        public UnlockConditionItemPanel(ConditionExtension condition) : base()
        {
            if (IsChinese && condition.DisplayNameZH is { Length: > 0 } nameZh)
                SetText(nameZh);
            else if (condition.DisplayNameEN is { Length: > 0 } nameEn)
                SetText(nameEn);
            else if (condition.Name is { Length: > 0 } nameFile)
                SetText(nameFile);

            SetIcon(condition.IconTexture);
        }
        public UnlockConditionItemPanel(UnlockCondition condition) : base()
        {
            SetText(condition.DisplayName);
            SetIcon(condition.Icon);
        }
    }
}
