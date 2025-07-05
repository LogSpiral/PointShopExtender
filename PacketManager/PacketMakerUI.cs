using Microsoft.Xna.Framework;
using PointShop;
using PointShopExtender.PacketData;
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
    public static bool Active { get; private set; }

    public static PacketMakerUI Instance { get; private set; }

    public static ExtensionPack CurrentPack { get; set; }

    public static bool IsChinese { get; set; }

    public static bool PendingUpdateList { get; set; }

    public static string CurrentSearchingText { get; set; }

    static Action UpdateMainPanel { get; set; }

    static Vector2 LastTargetSize { get; set; }
    static Vector2 CurrentTargetSize { get; set; }
    static bool PendingSwitchSize { get; set; }
    #endregion

    #region 四大面板
    public UIElementGroup MainPanel { get; private set; }

    public SUIDraggableView TitlePanel { get; private set; }

    public UIElementGroup ExtraInfoPanel { get; private set; }

    public UIElementGroup SearchBarPanel { get; private set; }

    SUIEditText SearchBox { get; set; }


    #endregion

    #region 开关 初始化
    public static void Open()
    {
        Active = true;
        IsChinese = Language.ActiveCulture == GameCulture.FromCultureName(GameCulture.CultureName.Chinese);
        Instance.DragOffset = default;
        SoundEngine.PlaySound(SoundID.MenuOpen);
        var targetVector = Main.MouseScreen / Main.ScreenSize.ToVector2();
        targetVector -= new Vector2(.5f);
        targetVector *= Main.GameZoomTarget * Main.ForcedMinimumZoom;
        targetVector += new Vector2(.5f);
        Instance.SetLeft(0, targetVector.X - .5f, .5f);
        Instance.SetTop(0, targetVector.Y - .5f, .5f);

        Instance.SwitchToPacketItemPage();


    }

    public static void Close()
    {
        SoundEngine.PlaySound(SoundID.MenuClose);
        Active = false;
    }

    protected override void OnInitialize()
    {
        Instance = this;
        FitWidth = true;
        FitHeight = true;
        BorderRadius = new Vector4(6);
        BackgroundColor = SUIColor.Background * .75f;
        BorderColor = SUIColor.Border;
        SetGap(0);

        #region TitlePanel
        TitlePanel = new SUIDraggableView(this);
        TitlePanel.SetSize(0, 40, 1);
        TitlePanel.BorderRadius = new Vector4(6, 6, 0, 0);
        TitlePanel.BackgroundColor = Color.Black * .25f;
        TitlePanel.Join(this);

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
            CurrentPack = null;
            SwitchToPacketItemPage();
        };
        titleText.OnUpdate += delegate
        {
            HandleMouseOverColorText(titleText);
        };
        SUIDividingLine.Horizontal(Color.Black * 0.75f).Join(this);

        #endregion

        #region MainPanel
        MainPanel = new UIElementGroup();
        MainPanel.SetSize(700, 450);
        CurrentTargetSize = new(700, 450);
        LastTargetSize = new(700, 450);
        //MainPanel.BorderRadius = new Vector4(16);
        MainPanel.Join(this);
        MainPanel.BackgroundColor = default;
        InitializeFilter(MainPanel);
        #endregion

        #region ExtraPanel
        SUIDividingLine.Horizontal(Color.Black * 0.75f).Join(this);

        ExtraInfoPanel = new UIElementGroup();
        ExtraInfoPanel.BackgroundColor = Color.Black * 0.25f;
        ExtraInfoPanel.BorderRadius = new Vector4(0f, 0f, 6f, 6f);
        ExtraInfoPanel.SetGap(8f);
        ExtraInfoPanel.SetSize(0, 30, 1);
        ExtraInfoPanel.SetPadding(12, 0);
        ExtraInfoPanel.Join(this);

        SUIImage FolderImage = new SUIImage(TextureAssets.Camera[6]);
        FolderImage.OnUpdate += delegate { HandleMouseOverColorImage(FolderImage); };
        FolderImage.LeftMouseClick += delegate { Utils.OpenFolder(RootPath); };
        FolderImage.Join(ExtraInfoPanel);
        #endregion
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
        searchBox.OnTextChanged += () =>
        {
            PendingUpdateList = true;
            CurrentSearchingText = searchBox.Text;
        };
        searchBox.SetPadding(8f);
        searchBox.SetHeight(0f, 1f);
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
    }

    void AddFilter(UIElementGroup? Parent = null)
    {
        Parent ??= MainPanel;
        Parent.FlexDirection = FlexDirection.Column;
        SearchBarPanel.Join(Parent);
        SearchBox.Text = "";
        PendingUpdateList = true;
    }
    #endregion

    protected override void Update(GameTime gameTime)
    {
        UpdateMainPanel?.Invoke();
        base.Update(gameTime);
    }


}
