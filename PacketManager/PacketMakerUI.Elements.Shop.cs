using Microsoft.Xna.Framework.Graphics;
using PointShopExtender.PacketData;
using ReLogic.Content;
using SilkyUIFramework;
using SilkyUIFramework.BasicElements;
using SilkyUIFramework.Extensions;
using System.Linq;
using Terraria.Audio;
using Terraria.ID;

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
        }
        protected override void OnSetIcon(Asset<Texture2D> texture)
        {
            // Packet.SetIconAndSave(texture, RootPath);
        }

        protected override void SwitchToContentPage(UIMouseEvent evt, UIView listeningElement)
        {
            // Instance.SwitchToBridgePage();
        }

        protected override void OnInitializeTextPanel(UIElementGroup textPanel)
        {
            ContentTextEditablePanel PacketNamePanel = new ("FileName", Shop.Name);
            PacketNamePanel.Join(textPanel);
        }
    }
}
