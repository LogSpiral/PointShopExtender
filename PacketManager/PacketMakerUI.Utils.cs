using Microsoft.Xna.Framework;
using PointShopExtender.PacketData;
using SilkyUIFramework;
using SilkyUIFramework.BasicComponents;
using SilkyUIFramework.BasicElements;
using SilkyUIFramework.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;

namespace PointShopExtender.PacketManager;

partial class PacketMakerUI
{
    static readonly string RootPath = Path.Combine(Main.SavePath, "Mods", nameof(PointShopExtender));

    static LocalizedText GetLocalizedText(string suffix) => Language.GetText($"Mods.PointShopExtender.{nameof(PacketMakerUI)}.{suffix}");
    static string GetLocalizedTextValue(string suffix) => GetLocalizedText(suffix).Value;
    static void HandleMouseOverColorText(UITextView textView) => textView.TextColor = Color.Lerp(Color.Gray, Color.White, textView.HoverTimer.Schedule);
    static void HandleMouseOverColorPanel(UIView view) => view.BackgroundColor = Color.Black * MathHelper.Lerp(0.25f, 0.1f, view.HoverTimer.Schedule);
    static void HandleMouseOverColorImage(SUIImage image) => image.ImageColor = Color.White * MathHelper.Lerp(0.5f, 1f, image.HoverTimer.Schedule);

    void SwicthPageCommon()
    {
        MainPanel.RemoveAllChildren();
        UpdateMainPanel = null;
        SoundEngine.PlaySound(SoundID.MenuOpen);
    }
}
