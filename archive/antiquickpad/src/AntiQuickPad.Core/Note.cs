namespace AntiQuickPad.Core;

/// <summary>A single Markdown note and its last editor caret position.</summary>
public sealed record Note(string Id, string Text, int CaretOffset);
