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
        }


        protected override void OnInitializeTextPanel(UIElementGroup textPanel)
        {
            ContentTextEditablePanel FileNamePanel = new("FileName", Environment.Name);
            FileNamePanel.SetBorderRadius(new(24, 0, 0, 0));
            FileNamePanel.Join(textPanel);

            ContentTextEditablePanel DisplayNamePanel = new("DisplayName", Environment.DisplayNameZH);
            DisplayNamePanel.Join(textPanel);

            ContentTextEditablePanel DisplayNameEnPanel = new("DisplayNameEn", Environment.DisplayNameEN);
            DisplayNameEnPanel.Join(textPanel);

            ContentTextEditablePanel PriorityPanel = new("Priority", Environment.Priority.ToString());
            PriorityPanel.Join(textPanel);

            ContentTextEditablePanel ColorTextPanel = new("ColorText", Environment.ColorText);
            ColorTextPanel.Join(textPanel);

            ConditionEditEntryPanel ConditionPanel = new(Environment.RealCondition, Environment);
            ConditionPanel.SetBorderRadius(new(0, 0, 0, 24));
            ConditionPanel.Join(textPanel);
        }
    }
}
