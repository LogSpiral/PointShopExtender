using Microsoft.Xna.Framework;
using PointShop.ShopSystem;
using PointShopExtender.PacketData;
using SilkyUIFramework;
using SilkyUIFramework.Elements;
using SilkyUIFramework.Extensions;
using SilkyUIFramework.Layout;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;

namespace PointShopExtender.PacketManager;

partial class PacketMakerUI
{
    static readonly string RootPath = PointShopExtenderSystem.RootPath;

    static LocalizedText GetLocalizedText(string suffix) => Language.GetText($"Mods.PointShopExtender.{nameof(PacketMakerUI)}.{suffix}");
    static string GetLocalizedTextValue(string suffix) => GetLocalizedText(suffix).Value;
    static void HandleMouseOverColorText(UITextView textView)
    {
        textView.TextColor = Color.Lerp(Color.Gray, Color.White, textView.HoverTimer.Schedule);
        textView.RecalculateHeight();
    }
    static void HandleMouseOverColorPanel(UIView view) => view.BackgroundColor = Color.Black * MathHelper.Lerp(0.25f, 0.1f, view.HoverTimer.Schedule);
    static void HandleMouseOverColorImage(SUIImage image) => image.ImageColor = Color.White * MathHelper.Lerp(0.5f, 1f, image.HoverTimer.Schedule);

    void SwicthPageCommon()
    {
        MainPanel.RemoveAllChildren();
        UpdateMainPanel = null;
        SoundEngine.PlaySound(SoundID.MenuOpen);
    }

    static bool IsVaild(char character) => !PointShopExtenderSystem.InvaildNameChars.Contains(character);

    static void FileNameCommonCheck(ref string text) => text = new string([.. text.Where(c => !PointShopExtenderSystem.InvaildNameChars.Contains(c))]);

    static void DigitsCommonCheck(ref string text) => text = new string([.. text.Where(c => char.IsDigit(c) || c is '.' or '-' or ',')]);

    static void ColorTextCommonCheck(ref string text) => text = new string([.. text.Where(c => char.IsDigit(c) || c is 'A' or 'B' or 'C' or 'D' or 'E' or 'F')]);

    static void GiveANameHint() => Main.NewText(GetLocalizedTextValue("GiveANamePlz"), Color.Red);
    static void ChooseAItemHint() => Main.NewText(GetLocalizedTextValue("ChooseAItemFirst"), Color.Red);

    static void ExtensionFileNameCheckCommon(INamedFileClass extension, UIView element)
    {
        if (string.IsNullOrEmpty(extension.Name))
        {
            // element.SilkyUI.SetFocus(null);
            PointShopExtender.UpdateFocusedElementCall();
            GiveANameHint();
        }
    }

    static void SavePendingShop()
    {
        if (PendingShopModified && PendingShop != null)
        {
            PendingShopModified = false;
            PendingShop.Save();
            PendingShop = null;
        }
    }

    static void SetScrollViewDirection(SUIScrollView scrollView, Direction direction)
    {
        bool isVertical = direction is Direction.Vertical;
        var container = scrollView.Container;
        container.FlexDirection = isVertical ? FlexDirection.Row : FlexDirection.Column;
        container.FitWidth = !isVertical;
        container.FitHeight = isVertical;
    }
}
