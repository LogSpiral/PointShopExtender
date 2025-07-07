using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SilkyUIFramework;
using SilkyUIFramework.Animation;
using SilkyUIFramework.Graphics2D;
using System;
using Terraria;

namespace PointShopExtender.PacketManager;

partial class PacketMakerUI
{
    public AnimationTimer PageTimer { get; init; } = new();
    public AnimationTimer SizeTimer { get; init; } = new();
    public AnimationTimer SwitchTimer { get; init; } = new(3);
    public AnimationTimer PathTimer { get; init; } = new();
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
        if (!string.IsNullOrEmpty(MouseHoverInfo))
            Main.hoverItemName = MouseHoverInfo;
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
            SetMaxWidth(currentSize.X);
        }
        else if (Math.Abs(MainPanel.Width.Pixels - CurrentTargetSize.X) > 2 || Math.Abs(MainPanel.Height.Pixels - CurrentTargetSize.Y) > 2)
            PendingSwitchSize = true;

        if (Active) SwitchTimer.StartUpdate();
        else SwitchTimer.StartReverseUpdate();

        SwitchTimer.Update(gameTime);


        PathTimer.Update(gameTime);
        if (!PathTimer.IsCompleted)
        {
            PathTracker.SetHeight(40 * PathTimer.Schedule);
            PathTracker.SetMaxHeight(40 * PathTimer.Schedule);
            PathTracker.BackgroundColor = PathTracker.BorderColor = Color.Black * (.5f * PathTimer.Schedule);

            PathTracker.PathList.ScrollBar.BarColor = PathTimer.IsForward ? (Color.Black * .2f, Color.Black * .3f) : (default, default);
            PathTracker.PathList.ScrollBar.BackgroundColor = Color.Black * (.25f * PathTimer.Schedule);
        }

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
