using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PointShop;
using PointShop.Registrar;
using PointShop.UserInterfaces;
using PointShopExtender.PacketData;
using ReLogic.Content;
using SilkyUIFramework;
using SilkyUIFramework.Attributes;
using SilkyUIFramework.BasicComponents;
using SilkyUIFramework.BasicElements;
using SilkyUIFramework.Extensions;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;

namespace PointShopExtender.PacketManager;

[RegisterUI("Vanilla: Radial Hotbars", $"{nameof(PointShopExtender)}: {nameof(PacketMakerUI)}")]
public partial class PacketMakerUI : BasicBody
{
    public override bool Enabled
    {
        get => Active || !SwitchTimer.IsReverseCompleted;
        set => Active = value;
    }

    #region 辅助用静态属性
    public static bool Active { get; set; }

    public static PacketMakerUI Instance { get; private set; }

    public static ExtensionPack CurrentPack { get; set; }

    public static bool IsChinese { get; set; }

    public static bool PendingUpdateList { get; set; }

    public static string CurrentSearchingText { get; set; }

    static Action UpdateMainPanel { get; set; }

    static Vector2 LastTargetSize { get; set; }
    static Vector2 CurrentTargetSize { get; set; }
    static bool PendingSwitchSize { get; set; }

    static bool PendingShopModified { get; set; }
    static ShopExtension PendingShop { get; set; }

    static string MouseHoverInfo { get; set; } = "";
    #endregion

    #region 四大面板
    public UIElementGroup MainPanel { get; private set; }

    public SUIDraggableView TitlePanel { get; private set; }

    public UIElementGroup ExtraInfoPanel { get; private set; }

    public UIElementGroup SearchBarPanel { get; private set; }

    SUIEditText SearchBox { get; set; }

    PathTrackerPanel PathTracker { get; set; }
    #endregion

    #region 开关 初始化
    public static void Open()
    {
        Active = true;
        IsChinese = Language.ActiveCulture == GameCulture.FromCultureName(GameCulture.CultureName.Chinese);
        SoundEngine.PlaySound(SoundID.MenuOpen);
        Instance.PathTracker.ReturnToMenu();
    }

    public static void Close()
    {
        SoundEngine.PlaySound(SoundID.MenuClose);
        Active = false;
        SavePendingShop();
    }

    protected override void OnInitialize()
    {
        Instance = this;
        FitWidth = true;
        FitHeight = true;
        BorderRadius = new Vector4(8);
        BackgroundColor = SUIColor.Background * .75f;
        BorderColor = SUIColor.Border;
        SetGap(0);
        SetTop(0, .5f, 0);
        SetLeft(0, .5f, 0);
        #region TitlePanel
        TitlePanel = new SUIDraggableView(this);
        TitlePanel.SetSize(700, 40, 0);
        TitlePanel.BorderRadius = new Vector4(8, 8, 0, 0);
        TitlePanel.BackgroundColor = Color.Black * .25f;
        TitlePanel.Join(this);
        TitlePanel.LayoutType = LayoutType.Custom;
        var titleText = new UITextView();
        titleText.Text = GetLocalizedTextValue("Title");
        titleText.TextAlign = new(0, .5f);
        titleText.TextScale = .45f;
        titleText.SetSize(0, 0, .25f, 1f);
        titleText.SetPadding(12, 0);
        titleText.UseDeathText();
        titleText.Join(TitlePanel);
        titleText.LeftMouseClick += delegate
        {
            PathTracker.ReturnToMenu();
        };
        titleText.OnUpdate += delegate
        {
            HandleMouseOverColorText(titleText);
        };
        titleText.MouseEnter += delegate
        {
            MouseHoverInfo = GetLocalizedTextValue("ReturnToMenu");
        };
        titleText.MouseLeave += delegate
        {
            MouseHoverInfo = "";
        };
        var ReturnButton = FastImageButton(ModAsset.ReturnBack);

        ReturnButton.SetLeft(FontAssets.DeathText.Value.MeasureString(titleText.Text).X * .45f + 20, 0, 0);
        ReturnButton.LeftMouseClick += delegate { PathTracker.ReturnToPreviousPage(); };
        ReturnButton.OnUpdate += delegate
        {
            ReturnButton.IgnoreMouseInteraction = PathTracker.InMenu;
            ReturnButton.ImageColor = ReturnButton.HoverTimer.Lerp(Color.White * .5f, Color.White) * (PathTracker.InMenu ? .5f : 1f);
        };
        ReturnButton.Join(TitlePanel);
        ReturnButton.MouseEnter += delegate
        {
            MouseHoverInfo = GetLocalizedTextValue("ReturnToLast");
        };
        ReturnButton.MouseLeave += delegate
        {
            MouseHoverInfo = "";
        };
        var SUICross = new SUICross(SUIColor.Warn * 0.75f, SUIColor.Border * 0.75f)
        {
            CrossSize = 22f,
            CrossRounded = 3.5f,
            CrossBorderHoverColor = SUIColor.Highlight,
            CrossBackgroundHoverColor = SUIColor.Warn,
            BoxSizing = BoxSizing.Content,
        }.Join(TitlePanel);
        SUICross.SetSize(24, 0, 0f, 1f);
        SUICross.SetPadding(12f, 0f);
        SUICross.SetLeft(0, 0, 1);
        SUICross.LeftMouseDown += delegate { Close(); };
        SUIDividingLine.Horizontal(Color.Black * 0.75f).Join(this);

        #endregion

        PathTracker = new PathTrackerPanel();
        PathTracker.SetHeight(0, 0);
        PathTracker.Join(this);
        PathTracker.PathList.ScrollBar.BackgroundColor = default;
        PathTracker.PathList.ScrollBar.BarColor = default;


        #region MainPanel
        MainPanel = new UIElementGroup();
        MainPanel.SetSize(700, 450);
        CurrentTargetSize = new(700, 450);
        LastTargetSize = new(700, 450);
        MainPanel.BorderRadius = new Vector4(16);
        MainPanel.Join(this);
        MainPanel.BackgroundColor = default;
        InitializeFilter(MainPanel);
        #endregion

        #region ExtraPanel
        SUIDividingLine.Horizontal(Color.Black * 0.75f).Join(this);

        ExtraInfoPanel = new UIElementGroup();
        ExtraInfoPanel.BackgroundColor = Color.Black * 0.25f;
        ExtraInfoPanel.BorderRadius = new Vector4(0f, 0f, 8f, 8f);
        ExtraInfoPanel.SetGap(8f);
        ExtraInfoPanel.SetSize(0, 30, 1);
        ExtraInfoPanel.SetPadding(12, 0);
        ExtraInfoPanel.Join(this);

        var FolderImage = FastImageButton(TextureAssets.Camera[6]);
        FolderImage.LeftMouseClick += delegate { Utils.OpenFolder(RootPath); };
        FolderImage.Join(ExtraInfoPanel);
        FolderImage.MouseEnter += delegate
        {
            MouseHoverInfo = GetLocalizedTextValue("OpenFolder");
        };
        FolderImage.MouseLeave += delegate
        {
            MouseHoverInfo = "";
        };
        SUIImage PathImage = FastImageButton(TextureAssets.CraftToggle[3]);
        PathImage.LeftMouseClick += delegate
        {
            if (PathTimer.IsForwardCompleted)
                PathTimer.StartReverseUpdate();
            else if (PathTimer.IsReverseCompleted)
                PathTimer.StartUpdate();
        };
        PathImage.Join(ExtraInfoPanel);
        PathImage.MouseEnter += delegate
        {
            MouseHoverInfo = GetLocalizedTextValue("ShowPath");
        };
        PathImage.MouseLeave += delegate
        {
            MouseHoverInfo = "";
        };

        SUIImage AutoImage = FastImageButton(PointShopExtenderSystem.AutoInfoMode ? ModAsset.AutoInfoModeActive : ModAsset.AutoInfoModeDeactive);
        AutoImage.LeftMouseClick += delegate
        {
            PointShopExtenderSystem.AutoInfoMode = !PointShopExtenderSystem.AutoInfoMode;
            AutoImage.Texture2D = PointShopExtenderSystem.AutoInfoMode ? ModAsset.AutoInfoModeActive : ModAsset.AutoInfoModeDeactive;
        };
        AutoImage.Join(ExtraInfoPanel);
        AutoImage.MouseEnter += delegate
        {
            MouseHoverInfo = GetLocalizedTextValue("AutoInfo");
        };
        AutoImage.MouseLeave += delegate
        {
            MouseHoverInfo = "";
        };
        #endregion

        // SwitchToPacketItemPage();
        PathTracker.AddNewPath("PacketExtensionViewPage", Instance.SwitchToPacketItemPage);

        base.OnInitialize();
    }
    #endregion

    #region 初始化辅助函数
    void InitializeFilter(UIElementGroup Container)
    {
        var searchBarContainer = new UIElementGroup
        {
            LayoutType = LayoutType.Flexbox,
            MainAlignment = MainAlignment.Start,
            CrossAlignment = CrossAlignment.Center,
            CrossContentAlignment = CrossContentAlignment.Center,
            Gap = new Vector2(4),
            Padding = new Margin(4f, 4f, 4f, 0f),
        }.Join(Container);
        searchBarContainer.SetWidth(0f, 1f);
        searchBarContainer.SetHeight(36f, 0f);
        SearchBarPanel = searchBarContainer;
        var searchBar = new UIElementGroup
        {
            BorderRadius = new Vector4(4f),
            Border = 2,
            BorderColor = SUIColor.Border * 0.75f,
            BackgroundColor = SUIColor.Background * 0.25f,
            FlexGrow = 1f,
        }.Join(searchBarContainer);
        searchBar.SetHeight(0f, 1f);

        // 搜索文字
        var searchText = new UITextView
        {
            Text = $"{LanguageHelper.GetTextByPointShop("NameFilter")}",
            TextScale = 0.8f,
            TextAlign = new Vector2(0.5f),
            BorderRadius = new Vector4(2f, 0f, 2f, 0f),
            BackgroundColor = SUIColor.Background * 0.5f,
            FlexShrink = 1f,
            FitWidth = true,
            FitHeight = false,
        }.Join(searchBar);
        searchText.SetPadding(12f, 0f);
        searchText.SetHeight(0f, 1f);

        SUIDividingLine.Vertical(Color.Black * 0.75f).Join(searchBar);

        var searchBox = new SUIEditText
        {
            BackgroundColor = SUIColor.Border * 0.25f,
            TextAlign = new Vector2(0f, 0.5f),
            TextScale = 0.8f,
            CursorFlashColor = Color.White,
            FlexGrow = 1f,
            FitWidth = false,
            FitHeight = false,
        }.Join(searchBar);
        searchBox.ContentChanged += (sender, e) =>
        {
            PendingUpdateList = true;
            CurrentSearchingText = searchBox.Text;
        };
        searchBox.SetPadding(8f);
        searchBox.SetHeight(0f, 1f);
        searchBox.OnUpdate += delegate { searchBox.BackgroundColor = searchBox.HoverTimer.Lerp(SUIColor.Background * 0.5f, Color.White * 0.05f); };
        SearchBox = searchBox;
        SUIDividingLine.Vertical(Color.Black * 0.75f).Join(searchBar);

        // 清空
        var clearText = new UITextView
        {
            Text = $"{LanguageHelper.GetTextByPointShop("Clear")}",
            TextScale = 0.8f,
            TextAlign = new Vector2(0.5f),
            BorderRadius = new Vector4(0f, 2f, 0f, 2f),
            BackgroundColor = SUIColor.Background * 0.5f,
            FitWidth = true,
            FitHeight = false,
        }.Join(searchBar);
        clearText.LeftMouseDown += (_, _) => searchBox.Text = string.Empty;
        clearText.SetPadding(12f, 0f);
        clearText.SetHeight(0f, 1f);
        clearText.OnUpdate += delegate { clearText.BackgroundColor = clearText.HoverTimer.Lerp(SUIColor.Background * 0.5f, Color.White * 0.05f); };
    }

    void AddFilter(UIElementGroup? Parent = null)
    {
        Parent ??= MainPanel;
        Parent.FlexDirection = FlexDirection.Column;
        SearchBarPanel.Join(Parent);
        SearchBox.Text = "";
        PendingUpdateList = true;
    }

    static SUIImage FastImageButton(Asset<Texture2D> texture)
    {
        SUIImage FolderImage = new SUIImage(texture);
        FolderImage.OnUpdate += delegate { HandleMouseOverColorImage(FolderImage); };
        FolderImage.FitHeight = false;
        FolderImage.FitWidth = false;
        FolderImage.SetSize(32, 32);
        FolderImage.SetTop(0, 0, .5f);
        FolderImage.ImageAlign = new(.5f);
        return FolderImage;
    }
    #endregion

    protected override void Update(GameTime gameTime)
    {
        UpdateMainPanel?.Invoke();
        base.Update(gameTime);
    }


}
