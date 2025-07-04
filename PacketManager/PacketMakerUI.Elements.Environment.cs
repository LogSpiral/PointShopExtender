using Microsoft.Xna.Framework.Graphics;
using PointShopExtender.PacketData;
using ReLogic.Content;
using SilkyUIFramework;
using SilkyUIFramework.BasicElements;
using SilkyUIFramework.Extensions;
using System.Linq;

namespace PointShopExtender.PacketManager;

partial class PacketMakerUI
{
    /// <summary>
    /// 环境块
    /// </summary>
    class EnvironmentItemElement : SingleItemPanel
    {
        bool CreateNew { get; set; }
        EnvironmentExtension Environment { get; init; }
        public EnvironmentItemElement(EnvironmentExtension environment, bool createNew) : base()
        {
            CreateNew = createNew;
            Environment = environment;
            if (createNew) return;

            if (IsChinese && environment.DisplayNameZH is { Length: > 0 } nameZh)
                SetText(nameZh);
            else if (environment.DisplayNameEN is { Length: > 0 } nameEn)
                SetText(nameEn);
            else if (environment.Name is { Length: > 0 } nameFile)
                SetText(nameFile);

            SetIcon(environment.IconTexture);
        }
        public override void OnLeftMouseClick(UIMouseEvent evt)
        {
            Instance.SwitchToEnvironmentInfoPage(Environment);
        }
    }

    /// <summary>
    /// 条件信息页
    /// </summary>
    class EnvironmentInfoElement : InfoPagePanel
    {
        EnvironmentExtension Environment { get; init; }
        public EnvironmentInfoElement(EnvironmentExtension environment)
        {
            Environment = environment;
            BuildPage();
            Image.Texture2D = environment.IconTexture;
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
            ContentTextEditablePanel FileNamePanel = new("FileName", Environment.Name);
            FileNamePanel.Join(textPanel);

            ContentTextEditablePanel DisplayNamePanel = new("DisplayName", Environment.DisplayNameZH);
            DisplayNamePanel.Join(textPanel);

            ContentTextEditablePanel DisplayNameEnPanel = new("DisplayNameEn", Environment.DisplayNameEN);
            DisplayNameEnPanel.Join(textPanel);

            ContentTextEditablePanel ColorTextPanel = new("ColorText", Environment.ColorText);
            ColorTextPanel.Join(textPanel);
        }
    }
}
