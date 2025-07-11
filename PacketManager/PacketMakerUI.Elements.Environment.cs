using Microsoft.Xna.Framework.Graphics;
using PointShopExtender.PacketData;
using ReLogic.Content;
using SilkyUIFramework;
using SilkyUIFramework.BasicElements;
using SilkyUIFramework.Extensions;
using System.IO;
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


            SetText(environment.GetDisplayName());
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

            ImagePanel.SetSize(280, 280, 0, 0);
            ImagePanel.SetTop(4, 0.05f, 0);
            ImagePanel.SetLeft(10, 0, 0);
        }
        protected override void OnSetIcon(Asset<Texture2D> texture)
        {
            // Packet.SetIconAndSave(texture, RootPath);
            Environment.SetIconAndSave(texture);
        }
        protected override void OpenFileDialogueToSelectIcon(UIMouseEvent evt, UIView listeningElement)
        {
            if (string.IsNullOrEmpty(Environment.Name))
                GiveANameHint();
            else
                base.OpenFileDialogueToSelectIcon(evt, listeningElement);
        }

        protected override void OnInitializeTextPanel(UIElementGroup textPanel)
        {
            ContentTextEditablePanel FileNamePanel = new("FileName", Environment.Name);
            FileNamePanel.ContentText.OnInput += FileNameCommonCheck;
            FileNamePanel.ContentText.EndTakingInput += (sender, arg) =>
            {
                var current = arg.NewValue;
                var old = arg.OldValue;
                if (string.IsNullOrEmpty(current)) FileNamePanel.ContentText.Text = old;
                Environment.RenameFile(current);
            };
            FileNamePanel.SetBorderRadius(new(24, 0, 0, 0));
            FileNamePanel.Join(textPanel);

            ContentTextEditablePanel DisplayNamePanel = new("DisplayName", Environment.DisplayNameZH);
            DisplayNamePanel.GotFocus += (evt, elem) => ExtensionFileNameCheckCommon(Environment, elem);
            DisplayNamePanel.ContentText.EndTakingInput += (sender, arg) =>
            {
                var current = arg.NewValue;
                Environment.SetDisplayNameAndSave(current);
            };
            DisplayNamePanel.Join(textPanel);

            ContentTextEditablePanel DisplayNameEnPanel = new("DisplayNameEn", Environment.DisplayNameEN);
            DisplayNameEnPanel.GotFocus += (evt, elem) => ExtensionFileNameCheckCommon(Environment, elem);
            DisplayNameEnPanel.ContentText.EndTakingInput += (sender, arg) =>
            {
                var current = arg.NewValue;
                Environment.SetDisplayNameEnAndSave(current);
            };
            DisplayNameEnPanel.Join(textPanel);

            ContentTextEditablePanel PriorityPanel = new("Priority", Environment.Priority.ToString());
            PriorityPanel.GotFocus += (evt, elem) => ExtensionFileNameCheckCommon(Environment, elem);
            PriorityPanel.ContentText.OnInput += DigitsCommonCheck;
            PriorityPanel.ContentText.EndTakingInput += (sender, arg) =>
            {
                var current = arg.NewValue;
                var d = double.Parse(current);
                int p = (int)d;
                Environment.SetPriorityAndSave(p);
                PriorityPanel.ContentText.Text = p.ToString();
            };
            PriorityPanel.Join(textPanel);

            ContentTextEditablePanel ColorTextPanel = new("ColorText", Environment.ColorText);
            ColorTextPanel.GotFocus += (evt, elem) => ExtensionFileNameCheckCommon(Environment, elem);
            ColorTextPanel.ContentText.OnInput += ColorTextCommonCheck;
            ColorTextPanel.ContentText.MaxWordLength = 6;
            ColorTextPanel.ContentText.EndTakingInput += (sender, arg) =>
            {
                var current = arg.NewValue;
                Environment.SetColorTextAndSave(current);
            };
            ColorTextPanel.Join(textPanel);

            Environment.RealCondition.Owner = Environment;
            ConditionEditEntryPanel ConditionPanel = new(Environment.RealCondition, Environment);
            ConditionPanel.SetBorderRadius(new(0, 0, 0, 24));
            ConditionPanel.Join(textPanel);
        }
    }
}
