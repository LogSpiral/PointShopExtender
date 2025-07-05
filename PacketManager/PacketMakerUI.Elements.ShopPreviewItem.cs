using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PointShop;
using PointShop.Items;
using PointShop.Registrar;
using PointShop.ShopSystem;
using PointShop.UserInterfaces;
using SilkyUIFramework;
using SilkyUIFramework.BasicComponents;
using SilkyUIFramework.BasicElements;
using SilkyUIFramework.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Default;

namespace PointShopExtender.PacketManager;

partial class PacketMakerUI
{
    public class SimpleShopItemPreviewElement : UIElementGroup
    {

        SimpleShopItemGenerator SimpleShopItem { get; set; }

        public SUIImage Image { get; protected set; }
        public UIElementGroup BuyButton { get; protected set; }
        public SUIDividingLine SUIDividingLine { get; protected set; }
        public SUICoverView CoverView { get; protected set; }

        public SimpleShopItemPreviewElement(SimpleShopItemGenerator simpleShopItem)
        {
            SimpleShopItem = simpleShopItem;

            LayoutType = LayoutType.Flexbox;
            FlexDirection = FlexDirection.Column;

            BorderRadius = new Vector4(4f);
            Border = 2;
            BorderColor = SUIColor.Border * 0.75f;
            BackgroundColor = Color.Black * 0.25f;

            FlexGrow = 1f;
            SetSize(100f, 160f);

            Create();

            // 购买按钮
            BuyButton = new UIElementGroup()
            {
                BorderRadius = new Vector4(0f, 0f, 2f, 2f),
                LayoutType = LayoutType.Flexbox,
                FlexDirection = FlexDirection.Row,
                MainAlignment = MainAlignment.Center,
                CrossAlignment = CrossAlignment.Center,
            }.Join(this);
            BuyButton.SetSize(0f, 28f, 1f);
            BuyButton.OnUpdateStatus += (gameTime) =>
            {
                BuyButton.BackgroundColor = BuyButton.HoverTimer.Lerp(SUIColor.Highlight * 0.05f, Color.White * 0.1f);
            };

            // 价格左边的积分币 ItemSlot
            var coinSlot = new SUIItemSlot
            {
                Item = new Item(ModContent.ItemType<PointCoin>()),
                ItemAlign = new Vector2(0f, 0.5f),
                ItemScale = 0.8f,
                BackgroundColor = Color.Transparent,
                BorderColor = Color.Transparent,
                ItemInteractive = false,
            }.Join(BuyButton);
            coinSlot.SetSize(20, 0f, 0f, 1f);

            // 价格
            var prices = new UITextView()
            {
                Text = $"{simpleShopItem.Prices:#,##0}",
                TextAlign = new Vector2(0.5f, 0.5f),
                TextScale = 0.8f,
                TextColor = Color.White,
            }.Join(BuyButton);
            prices.SetHeight(0f, 1f);

            if (PointShopSystem.TryGetUnlockCondition(simpleShopItem.UnlockCondition, out var unlockCondition))
            {
                CoverView = new SUICoverView(unlockCondition.Icon, unlockCondition.DisplayName, unlockCondition.Description);
                CoverView.Join(this);
                CoverView.OnUpdate += delegate
                {
                    CoverView.BackgroundColor = CoverView.HoverTimer.Lerp(new Color(0.5f, 0.5f, 0.5f) * 0.25f, new Color(0.5f, 0.5f, 0.5f) * 0.05f);
                    CoverView.Icon.ImageColor = CoverView.HoverTimer.Lerp(Color.White, Color.White * .2f);
                };
            }
        }

        protected void Create()
        {
            bool createNew = string.IsNullOrEmpty(SimpleShopItem.Type);

            int id = 0;
            if (!createNew)
                if (!int.TryParse(SimpleShopItem.Type, out id))
                {
                    if (!ItemID.Search.TryGetId(SimpleShopItem.Type, out id))
                        id = ModContent.ItemType<UnloadedItem>();
                }
            var item = ContentSamples.ItemsByType[id];
            Color rarityColor = Color.White;
            if (ItemRarity._rarities.TryGetValue(item.rare, out var color))
            {
                rarityColor = color;
            }

            var ShopItemName = new UITextView
            {
                BorderRadius = new Vector4(2f, 2f, 0f, 0f),
                Text = createNew ? GetLocalizedTextValue("CreateNew") : $"{item.Name}",
                BackgroundColor = rarityColor * 0.05f,
                TextColor = rarityColor,
                TextScale = 0.65f,
                TextAlign = new Vector2(0.5f),
                FitWidth = false,
                FitHeight = false,
            }.Join(this);
            ShopItemName.SetSize(0f, 28f, 1f);

            SUIDividingLine = SUIDividingLine.Horizontal(Color.Black * 0.5f).Join(this);

            SUIItemSlot = new SUIItemSlot
            {
                Item = createNew ? new Item() : new Item(item.type, item.stack),
                Border = 0,
                BorderColor = Color.Transparent,
                BackgroundColor = Color.Transparent,
                ItemInteractive = false,
                ItemIconSizeLimit = 50,
                StackAlign = new Vector2(0.7f, 0.7f),
                StackFormat = "{0}",
                FlexGrow = 1f,
                FlexShrink = 1f,
            }.Join(this);
            SUIItemSlot.SetHeight(0f, 0.5f);
            SUIItemSlot.SetWidth(0f, 1f);

            SUIDividingLine = SUIDividingLine.Horizontal(Color.Black * 0.5f).Join(this);
        }

        protected override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);

            //if (BuyButton.IsMouseHovering)
            //    Main.hoverItemName = LanguageHelper.GetTextByPointShop("Buy").Value;
        }
        public SUIItemSlot SUIItemSlot;

        public override void OnLeftMouseClick(UIMouseEvent evt)
        {
            Instance.SwitchToSingleItemEditPage(SimpleShopItem);
        }
    }
}
