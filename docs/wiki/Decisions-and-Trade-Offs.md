# Decisions and trade-offs

These choices keep the scratchpad quick and make my AI-led work reviewable.

- **Windows 11 and fast show/hide:** I target a hotkey-summoned translucent scratchpad, using Avalonia and AvaloniaEdit on .NET.
- **Lab-first visible design:** I approve a lab before a visible design change reaches the app.
  - *A visual proposal gives me something concrete to judge before implementation.*
- **Local notes and delayed saving:** local files keep notes on the machine; a delay reduces writes, but pending edits remain vulnerable to abrupt termination. See [[Architecture and Data]].
- **Separate dev copies:** hand tests use isolated data so experiments do not operate on the installed copy.
- **Human gates:** I approve plans and release moves; directors merge items, and checks plus hand tests provide complementary evidence. See [[Dev Loop Overview]].
- **Small capability checks:** `--selftest` checks specific behavior, but it cannot establish that every interaction feels right.
- Read the [decision record](https://github.com/NeelAPatel/AntiNotepad/blob/main/docs/decisions.md) for current locked choices and their history.
