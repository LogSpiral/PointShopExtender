using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PointShop.Registrar;
using PointShop.ShopSystem;
using PointShopExtender.PacketData;
using ReLogic.Content;
using SilkyUIFramework;
using SilkyUIFramework.BasicComponents;
using SilkyUIFramework.BasicElements;
using SilkyUIFramework.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Utilities.FileBrowser;

namespace PointShopExtender.PacketManager;

partial class PacketMakerUI
{
    class ContentTextEditablePanel : UIElementGroup
    {
        UITextView ContentName { get; set; }
        SUIEditText ContentText { get; set; }
        public ContentTextEditablePanel(string Key, string currentValue)
        {
            SetWidth(0, 1);
            SetHeight(40, 0);
            LayoutType = LayoutType.Custom;
            SetGap(0);
            var nameContainer = new UIElementGroup();
            nameContainer.SetWidth(0, .335f);
            nameContainer.SetHeight(40);
            nameContainer.BackgroundColor = Color.Black * .35f;
            nameContainer.Border = 2f;
            nameContainer.BorderColor = Color.Black;

            ContentName = new UITextView();
            ContentName.Text = GetLocalizedTextValue(Key);
            ContentName.SetLeft(0, 0, 0.5f);

            var contentContainer = new UIElementGroup();
            contentContainer.SetWidth(0, .67f);
            contentContainer.SetLeft(0, 0, 1);
            contentContainer.SetHeight(40);
            contentContainer.BackgroundColor = Color.Black * .25f;
            contentContainer.Border = 2f;
            contentContainer.BorderColor = Color.Black;

            ContentText = new SUIEditText();
            ContentText.Text = currentValue ?? "";
            ContentText.SetLeft(0, 0, 0.5f);

            nameContainer.Join(this);
            contentContainer.Join(this);
            ContentName.Join(nameContainer);
            ContentText.Join(contentContainer);

            ContentText.OnTextChanged += OnTextChanged;
            ContentText.OnEnterKeyDown += OnEnterKeyDown;

        }
        public void SetBorderRadius(Vector4 radius)
        {
            ContentName.Parent?.BorderRadius = new Vector4(radius.X, 0, radius.Z, 0);
            ContentText.Parent?.BorderRadius = new Vector4(0, radius.Y, 0, radius.W);
        }
        public event Action OnTextChanged;
        public event Action OnEnterKeyDown;
    }
    class ConditionEditEntryPanel : UIElementGroup
    {
        UITextView ContentName { get; set; }
        UITextView ConditionText { get; set; }
        RealCondition RealCondition { get; set; }
        public ConditionEditEntryPanel(RealCondition realCondition, object owner)
        {
            SetWidth(0, 1);
            SetHeight(40, 0);
            LayoutType = LayoutType.Custom;
            var nameContainer = new UIElementGroup();
            nameContainer.SetWidth(0, .335f);
            nameContainer.SetHeight(40);
            nameContainer.BackgroundColor = Color.Black * .35f;
            nameContainer.Border = 2f;
            nameContainer.BorderColor = Color.Black;
            nameContainer.LeftMouseClick += delegate
            {
                RealCondition ??= new RealCondition();
                Instance.SwitchToRealConditionEditor(RealCondition, owner);
            };

            ContentName = new UITextView();
            ContentName.Text = realCondition?.GetLocalizedTypeName() ?? "Null";
            ContentName.SetLeft(0, 0, 0.5f);

            var contentContainer = new UIElementGroup();
            contentContainer.SetWidth(0, .67f);
            contentContainer.SetLeft(0, 0, 1);
            contentContainer.SetHeight(40);
            contentContainer.BackgroundColor = Color.Black * .25f;
            contentContainer.Border = 2f;
            contentContainer.BorderColor = Color.Black;
            contentContainer.LeftMouseClick += delegate
            {
                RealCondition ??= new RealCondition();
                switch (RealCondition.ConditionType)
                {
                    case ConditionType.Vanilla:
                        Instance.SwitchToVanillaConditions(RealCondition, owner);
                        break;

                    case ConditionType.ModEnvironment:
                        Instance.SwitchToModdedEnvironments(RealCondition, owner);
                        break;
                    case ConditionType.ModBoss:
                        Instance.SwitchToModdedBosses(RealCondition, owner);
                        break;
                    default:
                        Instance.SwitchToRealConditionEditor(RealCondition, owner);
                        break;
                }
            };

            ConditionText = new SUIEditText();
            ConditionText.Text = realCondition?.GetContentName() ?? "";
            ConditionText.SetLeft(0, 0, 0.5f);

            nameContainer.Join(this);
            contentContainer.Join(this);
            ContentName.Join(nameContainer);
            ConditionText.Join(contentContainer);

            ConditionText.IgnoreMouseInteraction = true;
            ContentName.IgnoreMouseInteraction = true;

            RealCondition = realCondition;

        }
        public void SetBorderRadius(Vector4 radius)
        {
            ContentName.Parent?.BorderRadius = new Vector4(radius.X, 0, radius.Z, 0);
            ConditionText.Parent?.BorderRadius = new Vector4(0, radius.Y, 0, radius.W);
        }

        public override void OnLeftMouseClick(UIMouseEvent evt)
        {
            RealCondition ??= new RealCondition();

        }
    }
    class ContentSingleTextPanel : UIElementGroup
    {
        UITextView ContentName { get; set; }
        public ContentSingleTextPanel(string Key)
        {
            SetWidth(0, 1);
            SetHeight(40, 0);
            LayoutType = LayoutType.Custom;
            var nameContainer = new UIElementGroup();
            nameContainer.SetWidth(0, 1f);
            nameContainer.SetHeight(40);
            nameContainer.BackgroundColor = Color.Black * .35f;
            nameContainer.Border = 2f;
            nameContainer.BorderColor = Color.Black;

            ContentName = new UITextView();
            ContentName.Text = GetLocalizedTextValue(Key);
            ContentName.SetLeft(0, 0, 0.5f);

            nameContainer.Join(this);
            ContentName.Join(nameContainer);

        }
        public void SetBorderRadius(Vector4 radius)
        {
            ContentName.Parent?.BorderRadius = radius;
        }
    }
    class UnlockConditionEntryPanel : UIElementGroup
    {
        SUIImage ConditionIcon { get; set; }
        UITextView ConditionName { get; set; }

        public UnlockConditionEntryPanel(SimpleShopItemGenerator simpleShopItem)
        {
            string unlockConditionText = simpleShopItem.UnlockCondition ?? "";
            string conditionName = "";
            Asset<Texture2D> iconImage = ModAsset.UnlockConditionIconDefault;
            if (!PointShopSystem.UnlockConditions.TryGetValue(unlockConditionText, out var condition))
            {
                var extension = CurrentPack.ConditionExtensions.FirstOrDefault(condition => condition.Name == unlockConditionText);
                if (extension != null)
                {
                    iconImage = extension.IconTexture;
                    conditionName = extension.GetDisplayName();
                }
                else
                {
                    iconImage = ModAsset.NoneConditionIcon;
                    conditionName = GetLocalizedTextValue("NoneCondition");
                }
            }
            else
            {
                iconImage = condition.Icon;
                conditionName = condition.DisplayName;
            }
            SetWidth(0, 1);
            SetHeight(40, 0);
            LayoutType = LayoutType.Custom;
            SetGap(0);
            var nameContainer = new UIElementGroup();
            nameContainer.SetWidth(0, .335f);
            nameContainer.SetHeight(40);
            nameContainer.BackgroundColor = Color.Black * .35f;
            nameContainer.Border = 2f;
            nameContainer.BorderColor = Color.Black;

            ConditionIcon = new SUIImage(iconImage);
            ConditionIcon.SetLeft(0, 0, 0.5f);

            var contentContainer = new UIElementGroup();
            contentContainer.SetWidth(0, .67f);
            contentContainer.SetLeft(0, 0, 1);
            contentContainer.SetHeight(40);
            contentContainer.BackgroundColor = Color.Black * .25f;
            contentContainer.Border = 2f;
            contentContainer.BorderColor = Color.Black;

            ConditionName = new UITextView();
            ConditionName.Text = conditionName;
            ConditionName.SetLeft(0, 0, 0.5f);

            nameContainer.Join(this);
            contentContainer.Join(this);
            ConditionIcon.Join(nameContainer);
            ConditionName.Join(contentContainer);
        }
        public void SetBorderRadius(Vector4 radius)
        {
            ConditionIcon.Parent?.BorderRadius = new Vector4(radius.X, 0, radius.Z, 0);
            ConditionName.Parent?.BorderRadius = new Vector4(0, radius.Y, 0, radius.W);
        }

        public override void OnLeftMouseClick(UIMouseEvent evt)
        {
            Instance.SwitchToUnlockConditionPage();
            base.OnLeftMouseClick(evt);
        }
    }
    abstract class SingleItemPanel : UIElementGroup
    {
        protected readonly static Asset<Texture2D> CreateNewIcon = ModAsset.CreateNew;
        SUIImage Icon { get; set; }
        UITextView Text { get; set; }
        protected void SetIcon(Asset<Texture2D> texture)
        {
            Icon?.Texture2D = texture;
        }
        protected void SetText(string text)
        {
            Text?.Text = text;
        }
        protected SingleItemPanel()
        {
            BackgroundColor = Color.Black * .25f;
            BorderColor = Color.Black * .5f;

            Border = 2f;
            BorderRadius = new Vector4(8f);
            FlexDirection = FlexDirection.Column;
            FlexGrow = 1f;
            LayoutType = LayoutType.Custom;
            SetMargin(4);
            SetWidth(0, 0.325f);
            SetHeight(300f, 0);


            Icon = new SUIImage(CreateNewIcon);
            Icon.SetTop(0, 0, 0.5f);
            Icon.SetLeft(0, 0, 0.5f);
            Icon.ImageAlign = new Vector2(.5f);

            Text = new UITextView();
            Text.Text = GetLocalizedTextValue("CreateNew");
            Text.WordWrap = true;
            Text.SetTop(0, -0.1f, 1f);
            Text.SetLeft(0, 0, 0.5f);
            Text.SetWidth(0, 0.8f);
            Text.SetMaxWidth(0, 0.8f);

            Icon.Join(this);
            Text.Join(this);
        }

        protected override void UpdateStatus(GameTime gameTime)
        {
            HandleMouseOverColorPanel(this);
            base.UpdateStatus(gameTime);
        }

        public override void OnMouseEnter(UIMouseEvent evt)
        {
            SoundEngine.PlaySound(SoundID.MenuTick);
            base.OnMouseEnter(evt);
        }
    }

    abstract class InfoPagePanel : UIElementGroup
    {
        protected SUIImage Image { get; private set; }
        protected UIElementGroup ImagePanel { get; private set; }
        protected UIElementGroup TextPanel { get; private set; }
        // protected UITextView EditHint { get; private set; }

        protected void BuildPage()
        {
            SetWidth(0, 1f);
            SetHeight(0, 1f);

            LayoutType = LayoutType.Custom;

            InitalizeImage();

            InitializeText();

            // InitializeEditHint();
        }

        void InitalizeImage()
        {
            ImagePanel = new UIElementGroup();
            ImagePanel.SetTop(0, 0, 0.5f);
            ImagePanel.SetHeight(0, 0.7f);
            ImagePanel.SetWidth(0, .3f);
            ImagePanel.SetLeft(0, 0.025f);
            ImagePanel.SetMargin(4f);
            ImagePanel.LeftMouseClick += OpenFileDialogueToSelectIcon;
            ImagePanel.Border = 2f;
            ImagePanel.BorderColor = Color.Black * .5f;
            ImagePanel.BorderRadius = new(8);
            ImagePanel.BackgroundColor = Color.Black * .25f;
            ImagePanel.OnUpdate += delegate { HandleMouseOverColorPanel(ImagePanel); };
            ImagePanel.MouseEnter += delegate
            {
                SoundEngine.PlaySound(SoundID.MenuTick);
            };
            ImagePanel.Join(this);


            Image = new SUIImage(ModAsset.PacketIconDefault);
            Image.SetLeft(0, 0, 0.5f);
            Image.SetTop(0, 0, 0.5f);
            Image.IgnoreMouseInteraction = true;
            Image.Join(ImagePanel);
            Image.ImageAlign = new Vector2(.5f);
        }

        void InitializeText()
        {
            TextPanel = new();
            TextPanel.SetWidth(0, 0.60f);
            TextPanel.SetLeft(0, -0.025f, 1);
            TextPanel.SetHeight(0, 0.9f);
            TextPanel.SetTop(0, 0.05f);
            TextPanel.SetMargin(4);
            //TextPanel.BackgroundColor = Color.Black * .5f;
            TextPanel.Join(this);
            TextPanel.LayoutType = LayoutType.Flexbox;
            TextPanel.FlexDirection = FlexDirection.Column;
            TextPanel.SetGap(8f);
            OnInitializeTextPanel(TextPanel);
        }

        /*void InitializeEditHint()
        {
            EditHint = new UITextView();
            EditHint.Text = GetLocalizedTextValue("EditContent");
            EditHint.SetLeft(0, -0.025f, 1);
            EditHint.SetTop(0, -0.05f, 1);
            EditHint.OnUpdate += delegate
            {
                HandleMouseOverColorText(EditHint);
            };
            EditHint.MouseEnter += delegate
            {
                SoundEngine.PlaySound(SoundID.MenuTick);
            };
            EditHint.LeftMouseClick += SwitchToContentPage;
            EditHint.Join(this);
        }*/

        protected virtual void OpenFileDialogueToSelectIcon(UIMouseEvent evt, UIView listeningElement)
        {
            ExtensionFilter[] extensions = [new ExtensionFilter("Image files", "png")];

            string text = FileBrowser.OpenFilePanel("Open icon", extensions);
            if (text != null)
            {
                using var fileStream = File.OpenRead(text);
                var texture = PointShopExtender.Instance.Assets.CreateUntracked<Texture2D>(fileStream, text);
                Image.Texture2D = texture;
                OnSetIcon(texture);
            }
        }

        protected abstract void OnSetIcon(Asset<Texture2D> texture);

        protected abstract void OnInitializeTextPanel(UIElementGroup textPanel);
    }

}
