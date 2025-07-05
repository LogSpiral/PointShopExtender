using Microsoft.Xna.Framework.Graphics;
using PointShopExtender.PacketData;
using ReLogic.Content;
using SilkyUIFramework;
using SilkyUIFramework.BasicElements;
using SilkyUIFramework.Extensions;
using System;
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


            SetText(condition.GetDisplayName());
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

            ImagePanel.SetSize(280, 280, 0, 0);
            ImagePanel.SetTop(4, 0.05f, 0);
            ImagePanel.SetLeft(10, 0, 0);
        }
        protected override void OnSetIcon(Asset<Texture2D> texture)
        {
            // Packet.SetIconAndSave(texture, RootPath);
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

            ConditionEditEntryPanel ConditionPanel = new(Condition.RealCondition, Condition);
            ConditionPanel.SetBorderRadius(new(0, 0, 0, 24));
            ConditionPanel.Join(textPanel);
        }
    }
}
