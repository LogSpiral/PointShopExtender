using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SilkyUIFramework;
using SilkyUIFramework.Animation;
using SilkyUIFramework.Graphics2D;
using Terraria;

namespace PointShopExtender.PacketManager;

partial class PacketMakerUI
{
    public AnimationTimer PageTimer { get; init; } = new();
    public AnimationTimer SizeTimer { get; init; } = new();
    public AnimationTimer SwitchTimer { get; init; } = new(3);
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
    static void SetNextTargetSize(Vector2 size)
    {
        LastTargetSize = CurrentTargetSize;
        CurrentTargetSize = size;
        PendingSwitchSize = true;
    }
    protected override void UpdateStatus(GameTime gameTime)
    {
        if (PendingSwitchSize)
        {
            SizeTimer.StartUpdate(true);
            PendingSwitchSize = false;
        }
        SizeTimer.Update(gameTime);
        MainPanel.IgnoreMouseInteraction = SizeTimer.IsCompleted;

        if (!SizeTimer.IsCompleted) 
        {
            Vector2 currentSize = Vector2.SmoothStep(LastTargetSize, CurrentTargetSize, SizeTimer.Schedule);
            MainPanel.SetSize(currentSize.X, currentSize.Y);
        }

        if (Active) SwitchTimer.StartUpdate();
        else SwitchTimer.StartReverseUpdate();

        SwitchTimer.Update(gameTime);



        UseRenderTarget = SwitchTimer.IsUpdating;
        Opacity = SwitchTimer.Lerp(0f, 1f);

        var center = Bounds.Center * Main.UIScale;
        RenderTargetMatrix =
            Matrix.CreateTranslation(-center.X, -center.Y, 0) *
            Matrix.CreateScale(SwitchTimer.Lerp(0.95f, 1f), SwitchTimer.Lerp(0.95f, 1f), 1) *
            Matrix.CreateTranslation(center.X, center.Y, 0);
        base.UpdateStatus(gameTime);
    }
}
