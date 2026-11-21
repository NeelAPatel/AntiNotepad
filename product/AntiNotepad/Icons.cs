// Names each UI icon and binds its code to the font that draws it (root namespace so Editor menu data can hold icons).
using Avalonia.Media;

namespace AntiNotepad;

/// <summary>The font family that owns an icon code.</summary>
internal enum IconFont
{
    Segoe,
    Fluent
}

/// <summary>An icon code, its source glyph name when available, and the meaning shown by the UI.</summary>
internal sealed record Icon(string Name, IconFont Font, int Code, string? FontGlyphName, string Meaning)
{
    /// <summary>Returns the Unicode character used to draw this code.</summary>
    public string Character => char.ConvertFromUtf32(Code);
}

/// <summary>The single registry for icons used by application chrome and Settings.</summary>
internal static class Icons
{
    private static readonly FontFamily SegoeFamily = new("Segoe Fluent Icons, Segoe MDL2 Assets");
    private static readonly FontFamily FluentFamily = new($"{AppInfo.ResourceUri}Assets/Fonts#FluentSystemIcons-Regular");

    public static readonly Icon Delete = Segoe("Delete", 0xE74D, "Delete a note, selection, or open trash");
    public static readonly Icon NewNote = Segoe("New note", 0xE710, "Create a note");
    public static readonly Icon Back = Segoe("Back", 0xE76B, "Go to the previous note or leave trash");
    public static readonly Icon Forward = Segoe("Forward", 0xE76C, "Go to the next note");
    public static readonly Icon Search = Segoe("Search", 0xE721, "Search notes");
    public static readonly Icon Split = Segoe("Split", 0xE89F, "Toggle split view");
    public static readonly Icon KeepOnTop = Segoe("Keep on top", 0xE718, "Keep the window on top");
    public static readonly Icon KeepOnTopFilled = Segoe("Keep on top fill", 0xE842, "Fill for the pinned window icon");
    public static readonly Icon Settings = Segoe("Settings", 0xE713, "Open Settings");
    public static readonly Icon Hide = Segoe("Hide", 0xE921, "Hide the app");
    public static readonly Icon List = Segoe("List", 0xEA37, "Show the stack or list view");
    public static readonly Icon Grid = Segoe("Grid", 0xF0E2, "Show the slots or grid view");
    public static readonly Icon Close = Segoe("Close", 0xE711, "Close an overlay or window");
    public static readonly Icon Undo = Segoe("Undo", 0xE7A7, "Undo an edit");
    public static readonly Icon Redo = Segoe("Redo", 0xE7A6, "Redo an edit");
    public static readonly Icon SelectAll = Segoe("Select all", 0xE8B3, "Select all text");
    public static readonly Icon Cut = Segoe("Cut", 0xE8C6, "Cut selected text");
    public static readonly Icon Copy = Segoe("Copy", 0xE8C8, "Copy selected text");
    public static readonly Icon Paste = Segoe("Paste", 0xE77F, "Paste text");

    public static readonly Icon ThemeTab = Fluent("Theme tab", 0xF2F5, "ic_fluent_color_20_regular", "Open Theme settings");
    public static readonly Icon Accent = Fluent("Accent", 0xF2F5, "ic_fluent_color_20_regular", "Set the accent color");
    public static readonly Icon UiTab = Fluent("UI tab", 0xF592, "ic_fluent_paint_brush_20_regular", "Open UI settings");
    public static readonly Icon Background = Fluent("Background", 0xF592, "ic_fluent_paint_brush_20_regular", "Choose the window background");
    public static readonly Icon EditorTab = Fluent("Editor tab", 0xF386, "ic_fluent_document_edit_20_regular", "Open Editor settings");
    public static readonly Icon FeelTab = Fluent("Feel tab", 0xEC8D, "ic_fluent_tap_single_20_regular", "Open Feel settings");
    public static readonly Icon System = Fluent("System", 0xF359, "ic_fluent_desktop_20_regular", "Open System settings");
    public static readonly Icon TaskbarIcon = Fluent("Taskbar icon", 0xF359, "ic_fluent_desktop_20_regular", "Choose the taskbar icon");
    public static readonly Icon Keyboard = Fluent("Keyboard", 0xF4B8, "ic_fluent_keyboard_20_regular", "Open keyboard shortcut settings");
    public static readonly Icon StorageTab = Fluent("Storage tab", 0xF418, "ic_fluent_folder_20_regular", "Open Storage settings");
    public static readonly Icon NotesFolder = Fluent("Notes folder", 0xF418, "ic_fluent_folder_20_regular", "Open the notes folder");
    public static readonly Icon About = Fluent("About", 0xF4A3, "ic_fluent_info_20_regular", "Open About settings");
    public static readonly Icon SettingsSearch = Fluent("Settings search", 0xEA7C, "ic_fluent_search_16_regular", "Search Settings");
    public static readonly Icon LightMode = Fluent("Light mode", 0xF8A1, "ic_fluent_weather_sunny_20_regular", "Choose the light palette");
    public static readonly Icon DarkMode = Fluent("Dark mode", 0xF87D, "ic_fluent_weather_moon_20_regular", "Choose the dark palette");
    public static readonly Icon CaretGlide = Fluent("Caret glide", 0xE0CD, "ic_fluent_arrow_fit_20_regular", "Glide the caret between positions");
    public static readonly Icon SidePane = Fluent("Side pane", 0xE8B0, "ic_fluent_panel_left_20_regular", "Choose the side pane palette");
    public static readonly Icon Tint = Fluent("Tint", 0xF595, "ic_fluent_paint_bucket_20_regular", "Adjust background tint");
    public static readonly Icon Font = Fluent("Font", 0xF7E4, "ic_fluent_text_font_20_regular", "Choose the font");
    public static readonly Icon FontSize = Fluent("Font size", 0xF7E6, "ic_fluent_text_font_size_20_regular", "Choose the font size");
    public static readonly Icon LineSpacing = Fluent("Line spacing", 0xF7F6, "ic_fluent_text_line_spacing_20_regular", "Choose line spacing");
    public static readonly Icon PaperTone = Fluent("Paper tone", 0xF2F9, "ic_fluent_color_fill_20_regular", "Choose the paper tone");
    public static readonly Icon Backing = Fluent("Backing", 0xF462, "ic_fluent_grid_20_regular", "Choose the backing");
    public static readonly Icon Answers = Fluent("Answers", 0xF20A, "ic_fluent_calculator_20_regular", "Show inline calculation answers");
    public static readonly Icon CleanMarkdown = Fluent("Clean Markdown", 0xE7DB, "ic_fluent_markdown_20_regular", "Clean Markdown syntax while editing");
    public static readonly Icon Folding = Fluent("Folding", 0xECF4, "ic_fluent_text_collapse_20_regular", "Fold Markdown sections");
    public static readonly Icon ShortLinks = Fluent("Short links", 0xF4E4, "ic_fluent_link_20_regular", "Show shortened links");
    public static readonly Icon CodeLanguages = Fluent("Code languages", 0xF2EF, "ic_fluent_code_20_regular", "Choose code languages");
    public static readonly Icon VerticalScroll = Fluent("Vertical scroll", 0xE077, "ic_fluent_arrow_autofit_height_20_regular", "Choose vertical scrolling");
    public static readonly Icon HorizontalScroll = Fluent("Horizontal scroll", 0xF18D, "ic_fluent_arrow_swap_20_regular", "Choose horizontal scrolling");
    public static readonly Icon ReduceMotion = Fluent("Reduce motion", 0xF103, "ic_fluent_accessibility_20_regular", "Reduce motion");
    public static readonly Icon NewNoteHotkey = Fluent("New note hotkey", 0xF8BD, "ic_fluent_window_new_20_regular", "Open on a new note hotkey");
    public static readonly Icon OpenAt = Fluent("Open at", 0xEE77, "ic_fluent_window_location_target_20_regular", "Choose where the pad opens");
    public static readonly Icon FreshNote = Fluent("Fresh note", 0xF2DD, "ic_fluent_clock_20_regular", "Choose when a fresh note opens");
    public static readonly Icon KeepOnTopSetting = Fluent("Keep on top setting", 0xF601, "ic_fluent_pin_20_regular", "Keep the window on top");
    public static readonly Icon HideWhenClickAway = Fluent("Hide when I click away", 0xEBD0, "ic_fluent_subtract_20_regular", "Hide when I click away");
    public static readonly Icon StartWithWindows = Fluent("Start with Windows", 0xF60E, "ic_fluent_power_20_regular", "Start with Windows");
    public static readonly Icon RestoreDefaults = Fluent("Restore defaults", 0xF19F, "ic_fluent_arrow_reset_20_regular", "Restore default settings");
    public static readonly Icon SettingsChevron = Fluent("Settings chevron", 0xF2A2, "ic_fluent_chevron_down_16_regular", "Expand or collapse settings");
    public static readonly Icon RecoveredNotes = Fluent("Recovered notes", 0xF47E, "ic_fluent_history_20_regular", "Open recovered notes");
    public static readonly Icon AppData = Fluent("App data", 0xE542, "ic_fluent_document_settings_20_regular", "Open app data");

    public static IReadOnlyList<Icon> All { get; } =
    [
        Delete, NewNote, Back, Forward, Search, Split, KeepOnTop, KeepOnTopFilled, Settings, Hide, List, Grid, Close,
        Undo, Redo, SelectAll, Cut, Copy, Paste, ThemeTab, Accent, UiTab, Background, EditorTab, FeelTab, System,
        TaskbarIcon, Keyboard, StorageTab, NotesFolder, About, SettingsSearch, LightMode, DarkMode, CaretGlide,
        SidePane, Tint, Font, FontSize, LineSpacing, PaperTone, Backing, Answers, CleanMarkdown, Folding, ShortLinks,
        CodeLanguages, VerticalScroll, HorizontalScroll, ReduceMotion, NewNoteHotkey, OpenAt, FreshNote,
        KeepOnTopSetting, HideWhenClickAway, StartWithWindows, RestoreDefaults, SettingsChevron, RecoveredNotes, AppData
    ];

    /// <summary>Returns the font family declared by an icon.</summary>
    public static FontFamily FontFamily(IconFont font) => font switch
    {
        IconFont.Segoe => SegoeFamily,
        IconFont.Fluent => FluentFamily,
        _ => throw new ArgumentOutOfRangeException(nameof(font))
    };

    private static Icon Segoe(string name, int code, string meaning) => new(name, IconFont.Segoe, code, null, meaning);

    private static Icon Fluent(string name, int code, string glyphName, string meaning) =>
        new(name, IconFont.Fluent, code, glyphName, meaning);
}
