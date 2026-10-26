namespace AntiNotepad.Theme;

/// <summary>Shared measurements for the pad's top bar and pager.</summary>
internal static class ChromeTokens
{
    public const double DockedRowHeight = 40;
    public const double TopScrollableWhitespace = DockedRowHeight;
    public const double ButtonSize = 32;
    public const double IconGlyphSize = 16;
    public const double GlyphIconBoxExtra = 2;
    public const double BarEdgeInset = 4;
    public const double FirstItemInset = 10;
    public const double BadgeHeight = 20;
    public const double BadgeSpacing = 4;
    public const double BadgeRightInset = 6;
    public const double MetaSpacing = 8;
    public const double IconSpacing = 3;
    public const double BadgeCornerRadius = 6;
    public const double BadgeHorizontalPadding = 7;
    public const double BadgeTopPadding = 1;
    public const double BadgeBottomPadding = 2;
    public const double BadgeDarkFillAlpha = 0.18;
    public const double BadgeLightFillAlpha = 0.13;
    public const double BadgeFontSize = 11.5;
    public const double SavedLabelFontSize = 12;
    public const double RunningTimerFontSize = 11.5;
    public const double TextEdgeFadeBand = 12;
    public const double TopRevealZone = DockedRowHeight / 2;
    public const double OverlayPagerRevealZone = 20;
    public const double FloatPagerRevealZone = FloatGap;

    public const double TopIconHoverRadius = 8;
    public const double PagerArrowHoverRadius = 8;
    public const double PagerCountHoverRadius = 8;
    public const double PagerDotHoverRadius = 9;
    public const double PagerRowSpacing = 3;
    public const double PagerCountFontSize = 11.5;
    public const double PagerCountBaseWidth = 30;
    public const double PagerCountDigitWidth = 14;
    public const double PagerSeparatorWidth = 1;
    public const double PagerSeparatorHeight = 14;
    public const double PagerSeparatorMargin = 5;
    public const double PagerSeparatorAlpha = 0.5;
    public const double PagerDotSelectedWidth = 16;
    public const double PagerDotWidth = 7;
    public const double PagerDotHeight = 7;
    public const double PagerDotRadius = PagerDotHeight / 2;
    public const double PagerDotSelectedButtonWidth = 24;
    public const double PagerDotButtonWidth = 16;
    public const double PagerDotButtonHeight = 18;
    public const double PagerDotBorderThickness = 1.2;

    public const double PillInset = 4;
    public const double PillBorderThickness = 1;
    public const double PillBorderAlpha = 0.35;
    public const double PillCornerRadius = 12;
    public const double PillBottomInset = 4;
    public const double FloatGap = 8;
    public const double PillHeight = ButtonSize + 2 * PillInset + 2 * PillBorderThickness;
    public const double PagerStripHeight = PillHeight + 2 * PillBottomInset;
    public const double PillShadowBlur = 14;
    public const double PillShadowOffsetY = 3;
    public const byte PillShadowDarkAlpha = 90;
    public const byte PillShadowLightAlpha = 28;
    public const double DividerPagerClearance = 6;
    public const double DividerWidth = 1;
    public const double DividerTopMargin = 6;
    public const double DividerAlpha = 0.45;
    public const double ToastPagerClearance = 2;
    public const double DividerBottomMargin = PagerStripHeight + DividerPagerClearance;
    public const double ToastBottomMargin = PagerStripHeight + ToastPagerClearance;

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
