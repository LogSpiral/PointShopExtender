using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Localization;

namespace PointShopExtender.PacketManager;

partial class PacketMakerUI
{
    static LocalizedText GetLocalizedText(string suffix) => Language.GetText($"Mods.PointShopExtender.PacketMakerUI.{suffix}");
    static string GetLocalizedTextValue(string suffix) => GetLocalizedText(suffix).Value;
}
