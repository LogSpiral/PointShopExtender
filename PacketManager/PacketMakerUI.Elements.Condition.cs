using Microsoft.Xna.Framework.Graphics;
using PointShopExtender.PacketData;
using ReLogic.Content;
using SilkyUIFramework;
using SilkyUIFramework.BasicElements;
using SilkyUIFramework.Extensions;
using System;
using System.IO;
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
            Condition.SetIconAndSave(texture);
            // Packet.SetIconAndSave(texture, RootPath);
        }
        protected override void OpenFileDialogueToSelectIcon(UIView listeningElement, UIMouseEvent evt)
        {
            if (string.IsNullOrEmpty(Condition.Name))
                GiveANameHint();
            else
                base.OpenFileDialogueToSelectIcon(listeningElement, evt);
        }
        protected override void OnInitializeTextPanel(UIElementGroup textPanel)
        {
            ContentTextEditablePanel FileNamePanel = new("FileName", Condition.Name);
            FileNamePanel.SetBorderRadius(new(24, 0, 0, 0));
            FileNamePanel.ContentText.ContentChanging += (sender, e) =>
            {
                var text = e.NewText;
                FileNameCommonCheck(ref text);
                return text;
            };
            FileNamePanel.ContentText.EndTakingInput += (sender, arg) =>
            {
                var current = arg.NewValue;
                var old = arg.OldValue;
                if (string.IsNullOrEmpty(current)) FileNamePanel.ContentText.Text = old;
                Condition.RenameFile(current);
            };
            FileNamePanel.Join(textPanel);

            ContentTextEditablePanel DisplayNamePanel = new("DisplayName", Condition.DisplayNameZH);
            DisplayNamePanel.GotFocus += (sender, evt) => ExtensionFileNameCheckCommon(Condition, sender);
            DisplayNamePanel.ContentText.EndTakingInput += (sender, arg) =>
            {
                var current = arg.NewValue;
                Condition.SetDisplayNameAndSave(current);
            };
            DisplayNamePanel.Join(textPanel);

            ContentTextEditablePanel DisplayNameEnPanel = new("DisplayNameEn", Condition.DisplayNameEN);
            DisplayNameEnPanel.GotFocus += (sender, evt) => ExtensionFileNameCheckCommon(Condition, sender);
            DisplayNameEnPanel.ContentText.EndTakingInput += (sender, arg) =>
            {
                var current = arg.NewValue;
                Condition.SetDisplayNameEnAndSave(current);
            };
            DisplayNameEnPanel.Join(textPanel);

            ContentTextEditablePanel DescriptionPanel = new("Description", Condition.DescriptionZH);
            DescriptionPanel.GotFocus += (sender, evt) => ExtensionFileNameCheckCommon(Condition, sender);
            DescriptionPanel.ContentText.EndTakingInput += (sender, arg) =>
            {
                var current = arg.NewValue;
                Condition.SetDisplayNameEnAndSave(current);
            };
            DescriptionPanel.Join(textPanel);

            ContentTextEditablePanel DescriptionEnPanel = new("DescriptionEn", Condition.DescriptionEN);
            DescriptionEnPanel.GotFocus += (sender, evt) => ExtensionFileNameCheckCommon(Condition, sender);
            DescriptionEnPanel.ContentText.EndTakingInput += (sender, arg) =>
            {
                var current = arg.NewValue;
                Condition.SetDisplayNameEnAndSave(current);
            };
            DescriptionEnPanel.Join(textPanel);

            Condition.RealCondition.Owner = Condition;
            ConditionEditEntryPanel ConditionPanel = new(Condition.RealCondition, Condition);
            ConditionPanel.SetBorderRadius(new(0, 0, 0, 24));
            ConditionPanel.Join(textPanel);
        }
    }
}
