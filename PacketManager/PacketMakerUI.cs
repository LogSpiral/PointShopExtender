using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PointShopExtender.PacketData;
using SilkyUIFramework;
using SilkyUIFramework.Attributes;
using SilkyUIFramework.BasicElements;
using SilkyUIFramework.Extensions;
using SilkyUIFramework.Graphics2D;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;

namespace PointShopExtender.PacketManager;

[RegisterUI("Vanilla: Radial Hotbars", $"{nameof(PointShopExtender)}: {nameof(PacketMakerUI)}")]
public partial class PacketMakerUI : BasicBody
{
    public static bool Active { get; private set; }

    public override bool Enabled { get => Active; set => Active = value; }

    public static PacketMakerUI Instance { get; private set; }

    public SUIDraggableView MainPanel { get; private set; }

    static readonly string RootPath = Path.Combine(Main.SavePath, "Mods", nameof(PointShopExtender));

    //public static string CurrentPath { get; private set; }

    public static ExtensionPack CurrentPack { get; set; }

    public static bool IsChinese { get; set; }
    public static void Open()
    {
        Active = true;
        IsChinese = Language.ActiveCulture == GameCulture.FromCultureName(GameCulture.CultureName.Chinese);
        Instance.DragOffset = default;
        SoundEngine.PlaySound(SoundID.MenuOpen);
        var targetVector = Main.MouseScreen / Main.ScreenSize.ToVector2();
        targetVector -= new Vector2(.5f);
        targetVector *= Main.GameZoomTarget * Main.ForcedMinimumZoom;
        targetVector += new Vector2(.5f);
        Instance.SetLeft(0, targetVector.X - .5f, .5f);
        Instance.SetTop(0, targetVector.Y - .5f, .5f);

        Instance.SwitchToPacketItemPage();

        Instance.BackgroundColor = SUIColor.Background * .75f;
        Instance.BorderColor = SUIColor.Border;
        Instance.MainPanel.BackgroundColor = default;
    }

    public static void Close()
    {
        SoundEngine.PlaySound(SoundID.MenuClose);
        Active = false;
    }

    protected override void OnInitialize()
    {
        Instance = this;
        FitWidth = true;
        FitHeight = true;
        BorderRadius = new Vector4(16);

        MainPanel = new SUIDraggableView(this);
        MainPanel.SetSize(700, 450);
        MainPanel.BorderRadius = new Vector4(16);
        MainPanel.Join(this);
        BorderColor = Color.Transparent;

        base.OnInitialize();
    }

    protected override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        if (BlurMakeSystem.BlurAvailable)
        {
            if (BlurMakeSystem.SingleBlur)
            {
                var batch = Main.spriteBatch;
                batch.End();
                BlurMakeSystem.KawaseBlur();
                batch.Begin();
            }

            SDFRectangle.SampleVersion(BlurMakeSystem.BlurRenderTarget,
                Bounds.Position * Main.UIScale, Bounds.Size * Main.UIScale, BorderRadius * Main.UIScale, Matrix.Identity);
        }

        base.Draw(gameTime, spriteBatch);
    }

    static string GetLocalizationText(string suffix)=>PointShopExtenderSystem.GetLocalizationText($"{nameof(PacketMakerUI)}.{suffix}");
}
