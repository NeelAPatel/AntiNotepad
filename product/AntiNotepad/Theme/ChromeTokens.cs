// Shared chrome measurements, interface sizes and the save timer's default interval.
namespace AntiNotepad.Theme;

/// <summary>Shared measurements for interface chrome and its save timer.</summary>
internal static class ChromeTokens
{
    public const int SaveDelayDefaultMs = 400;
    public const int SaveDelayFiveSecondsMs = 5_000;
    public const int SaveDelayTenSecondsMs = 10_000;
    public const int SaveDelayThirtySecondsMs = 30_000;
    public const int SaveIdleGateMs = 1_000;
    public const int SavedLabelRefreshMs = SaveIdleGateMs;
    public const int SavedLabelMinuteMs = 60_000;
    public const int SavedLabelTimestampMs = 300_000;
    // Named exception to the Stroke scale: the pinned window ring is 4 px.
    public const double PinRingThickness = 4;
    public const double DockedRowHeight = 40;
    public const double TopScrollableWhitespace = DockedRowHeight;
    public const double ButtonSize = 32;
    public const double IconGlyphSize = 16;
    public const double GlyphIconBoxExtra = Space.Nudge;
    public const double BarEdgeInset = Space.Xs;
    public const double FirstItemInset = Space.M;
    public const double BadgeHeight = 20;
    public const double BadgeSpacing = Space.Xs;
    public const double BadgeRightInset = Space.S;
    public const double MetaSpacing = Space.S;
    public const double IconSpacing = Space.Xs;
    public const double BadgeCornerRadius = Shape.Key;
    public const double BadgeHorizontalPadding = Space.S;
    public const double BadgeTopPadding = 1;
    public const double BadgeBottomPadding = 2;
    public const double BadgeDarkFillAlpha = .15;
    public const double BadgeLightFillAlpha = .11;
    public const double DevBadgeLightTextInkMix = .62;
    public const double BadgeFontSize = 12;
    public const double SavedLabelFontSize = 12;
    public const double RunningTimerFontSize = 12;
    public const double TextEdgeFadeBand = Space.M;
    public const double TopRevealZone = DockedRowHeight / 2;
    public const double OverlayPagerRevealZone = 20;
    public const double FloatPagerRevealZone = FloatGap;

    public const double TopIconHoverRadius = 8;
    public const double PagerArrowHoverRadius = 8;
    public const double PagerCountHoverRadius = 8;
    public const double PagerDotHoverRadius = 8;
    public const double PagerRowSpacing = Space.Xs;
    public const double PagerCountFontSize = 12;
    public const double PagerCountBaseWidth = 32;
    public const double PagerCountDigitWidth = 14;
    public const double PagerSeparatorWidth = Shape.Thin;
    public const double PagerSeparatorHeight = Space.L;
    public const double PagerSeparatorMargin = Space.Xs;
    public const double PagerSeparatorAlpha = 0.5;
    public const double PagerDotSelectedWidth = 16;
    public const double PagerDotWidth = 8;
    public const double PagerDotHeight = 8;
    public const double PagerDotRadius = PagerDotHeight / 2;
    public const double PagerDotSelectedButtonWidth = 24;
    public const double PagerDotButtonWidth = 16;
    public const double PagerDotButtonHeight = ButtonSize;
    public const double PagerCountButtonHeight = ButtonSize;
    public const double BarCornerSize = PillHeight;
    public const int DeleteDebounceMs = 500;
    public const double PagerDotBorderThickness = Shape.Thin;

    public const double PillInset = Space.Xs;
    public const double PillBorderThickness = Shape.Thin;
    public const double PillBorderAlpha = .3;
    public const double PillCornerRadius = 12;
    public const double PillBottomInset = Space.Xs;
    public const double FloatGap = Space.S;
    public const double PillHeight = ButtonSize + 2 * PillInset + 2 * PillBorderThickness;
    public const double PagerStripHeight = PillHeight + 2 * PillBottomInset;
    public const double PillShadowBlur = 12;
    public const double PillShadowOffsetY = 2;
    public const byte PillShadowDarkAlpha = 90;
    public const byte PillShadowLightAlpha = 28;
    public const double DividerPagerClearance = Space.S;
    public const double DividerWidth = Shape.Thin;
    public const double DividerTopMargin = Space.S;
    public const double DividerAlpha = .5;
    public const double ToastPagerClearance = Space.Nudge;
    public const double DividerBottomMargin = PagerStripHeight + DividerPagerClearance;
    public const double ToastBottomMargin = PagerStripHeight + ToastPagerClearance;
    public const double SearchOverlayBottomMargin = PagerStripHeight - PillInset;
    public const double HelpOverlayBottomMargin = DockedRowHeight;

    /// <summary>Stable window, popup, panel and row sizes named from the current layout.</summary>
    internal static class Size
    {
        public const double SettingsWindowWidth = 900, SettingsWindowHeight = 720;
        public const double SettingsWindowMinWidth = 640, SettingsWindowMinHeight = 460;
        public const double SettingsPageMaxWidth = 780;
        public const double ShortcutDialogWidth = 420;
        public const double SettingsSearchFieldHeight = 32;
        public const double CompletionWindowMinWidth = 300, CompletionWindowWidth = 340;
        public const double MenuPopupMinWidth = 224;
        public const double MenuActionRowHeight = 52, MenuRowHeight = 32, MenuIconColumnWidth = 28;
        public const double PadWindowMinWidth = 320, PadWindowMinHeight = 260;
        public const double ToastMaxWidth = 380, ToastTextMaxWidth = 300, SaveFailureReasonMaxWidth = 380;
        public const double SaveFailureCardMaxWidth = 420, SaveFailureCardMinWidth = 240;
        public const double SearchResultCardHeight = 152;
        public const double SettingsPrimaryRowMinHeight = 64, SettingsSecondaryRowMinHeight = 56;
        public const double SettingsSubrowIndent = 48;
        public const double SettingsTextBoxMinHeight = 40, SettingsTextBoxMaxHeight = 68;
        public const double SettingsDropdownHeight = 48;
        public const double SettingsIconBoxSize = 20;
        public const double CaretSampleHeight = 16, CaretPictureHeight = 32, MaterialPictureHeight = 48;
        public const double CaretBlockWidth = 8;
        public const double ToggleTrackWidth = 40, ToggleTrackHeight = 20;
        public const double AccentDotSize = 20, PaletteDotSize = 8;
        public const double PatternPictureHeight = 40, ScreenPictureHeight = 64;
        public const double MotionPreviewPadHeight = 220;

        // This transparent helper window exists only for taskbar presence and is never drawn.
        public const double HiddenTaskbarHelperSize = 1;
    }

    internal static class TopBackdrop
    {
        public const double DenseStop = 40;
        public const double ClearStop = 52;
        public const double Midpoint = 0.5;
        public const double DenseMaskAlpha = 1;
        public const double MidpointMaskAlpha = 0.5;
        public const double ClearMaskAlpha = 0;
        public const double BlurRadius = 10;
        public const double TintAlpha = 0.4;

        public static double DensePosition => DenseStop / ClearStop;
        public static double MidpointStop => DenseStop + (ClearStop - DenseStop) * Midpoint;
        public static double MidpointPosition => MidpointStop / ClearStop;

        public static double MaskAlphaAt(double y)
        {
            if (y <= DenseStop) return DenseMaskAlpha;
            if (y < MidpointStop)
                return DenseMaskAlpha + (MidpointMaskAlpha - DenseMaskAlpha) * ((y - DenseStop) / (MidpointStop - DenseStop));
            if (y < ClearStop)
                return MidpointMaskAlpha + (ClearMaskAlpha - MidpointMaskAlpha) * ((y - MidpointStop) / (ClearStop - MidpointStop));
            return ClearMaskAlpha;
        }
    }
}
