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
    class ConditionItemElement : SingleItemPanel
    {
        bool CreateNew { get; set; }
        ConditionExtension Condition { get; init; }
        public ConditionItemElement(ConditionExtension condition, bool createNew) : base()
        {
            CreateNew = createNew;
            Condition = condition;
            if (createNew) return;

            if (IsChinese && condition.DisplayNameZH is { Length: > 0 } nameZh)
                SetText(nameZh);
            else if (condition.DisplayNameEN is { Length: > 0 } nameEn)
                SetText(nameEn);
            else if (condition.Name is { Length: > 0 } nameFile)
                SetText(nameFile);

            SetIcon(condition.IconTexture);
        }
        public override void OnLeftMouseClick(UIMouseEvent evt)
        {
            Instance.SwitchToConditionInfoPage(Condition);
        }
    }

    /// <summary>
    /// 条件信息页
    /// </summary>
    class ConditionInfoElement : InfoPagePanel
    {
        ConditionExtension Condition { get; init; }
        public ConditionInfoElement(ConditionExtension condition)
        {
            Condition = condition;
            BuildPage();
            Image.Texture2D = condition.IconTexture;
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
            ContentTextEditablePanel FileNamePanel = new("FileName", Condition.Name);
            FileNamePanel.Join(textPanel);

            ContentTextEditablePanel DisplayNamePanel = new("DisplayName", Condition.DisplayNameZH);
            DisplayNamePanel.Join(textPanel);

            ContentTextEditablePanel DisplayNameEnPanel = new("DisplayNameEn", Condition.DisplayNameEN);
            DisplayNameEnPanel.Join(textPanel);

            ContentTextEditablePanel DescriptionPanel = new("Description", Condition.DescriptionZH);
            DescriptionPanel.Join(textPanel);

            ContentTextEditablePanel DescriptionEnPanel = new("DescriptionEn", Condition.DescriptionEN);
            DescriptionEnPanel.Join(textPanel);
        }
    }
}
