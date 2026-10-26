# A41b-clipboard-policy-impact

Read-only source audit, 2026-10-26; `reorg/A41-padwindow-split`, HEAD/dev `9e8e04d`, including A41a-hotkeys merge `2b85bfb` (implementation `2677946`). Compared product diff/history from `b54b476`. No builds, selftests, application launch/termination, or live checks. Only this report written; the explicit report-only instruction overrides routine STATUS updates (`AGENTS.md:18`).

References below are current `product/AntiNotepad/` paths: **N** = `UI/PadWindow.Navigation.cs`, **W** = `UI/PadWindow.cs`, **L** = `UI/PadWindow.Layout.cs`, **K** = `UI/PadWindow.Keys.cs`, **PV** = `Editor/PadView.cs`, **ST** = `SelfTest.cs`. **Plan** = `.coord/tasks/A41-padwindow-split-plan.md`; **R2** = `.coord/reports/A41-padwindow-split-impact-r2.md`. A42 references explicitly use the other branch's snapshot, not current-tree files.

## 1. Current capture path, callers and state

| Stage | State, ordering and evidence |
|---|---|
| Hook installation | `InstallHooks`: optional taskbar proxy, toggle registration, new-note registration, clipboard listener, taskbar apply, in that order. Listener posts `_ = OnClipboardChanged()` to the UI dispatcher; its Task is discarded (`W:117-123`). |
| Native notification | `Native.ListenClipboard` gets HWND; zero HWND returns a no-op disposable. Otherwise it installs WndProc callback, listens for `WM_CLIPBOARDUPDATE` (0x031D), and returns listener/hook teardown. No content is passed with the event (`Platform/Native.cs:12,19-30`). Window owns `_clipboardHook`, disposed at quit after hotkeys (`W:28,284-295`). |
| Own copy | Window-wide nullable `_ownClipboard` and `_lastCaptured` start null. `CopyText(text)` assigns the raw own string even if Clipboard is absent, then conditionally awaits `SetTextAsync`; no catch or rollback (`N:287-294`). |
| OCR first | If `_awaitingSnip` exists, elapsed seconds are strictly `<120`, and awaited `Ocr.ClipboardHasImage()` is true: clear awaiting flag, await `OcrIntoPane(_snipPane ?? _active)`, return. This precedes target selection and text privacy checks (`N:298-303`). |
| Target | Combine stack then slots, filter cached Clipboard mode, choose maximum Modified; absent target or Avalonia Clipboard returns. Target is selected before the text-read await (`N:304-305`). |
| Privacy/read | `Native.ClipboardIsPrivate()` returns before `TryGetTextAsync`; exceptions from that read return silently (`N:306-309`). Native recognizes the two exclusion formats and history DWORD=0; exceptions fail closed, but missing history format or failed OpenClipboard returns false (`Platform/Native.cs:35-53`). |
| Suppression | In order: null/blank, raw equality with own string, raw equality with last capture; then `target.PeekText().Contains(text.Trim())`. Comparisons are case-sensitive; containment is substring matching, before CRLF/indent conversion (`N:310-312`). |
| Acceptance | Assign raw `_lastCaptured`; obtain `PadView.DocumentFor(target)`; trim, CRLF->LF, add two spaces after every LF; append optional LF + `- ` + entry (`N:313-316`). |
| Effects | Successful hidden capture refreshes `_hiddenAt`, then calls `Edited()`; Edited restarts the save debounce, updates chrome and sleeps chrome (`N:317-318`; `W:366-375`). |

There are exactly two direct `CopyText` call sites: Ctrl+Shift+C copies the active whole editor text (`K:73,82,95,101`), and each NewPane subscribes `pane.Copy += text => _ = CopyText(text)` (`L:113-119`). Non-timer chip clicks invoke that event with `chip.Value`; timer chips toggle/reset instead (`PV:619-632`; event declaration `PV:76`).

**Menu Copy is a bypass, not another CopyText caller.** The editor context menu's Copy action calls `Editor.Copy()`; Cut similarly calls `Editor.Cut()` (`PV:735-748`). The host merely displays these supplied actions (`L:120`; `UI/PadMenu.cs:18,53-74`). No own-copy recording occurs on that route. Ordinary Ctrl+C also falls through the window switch to editor handling (only shifted C is intercepted: `K:95,99`); actual framework clipboard behavior was not run. Existing containment can suppress a selection copied out of the target clipboard note, but a selection from another note can be captured (`N:310-312`).

Snip state remains separate from dedupe state: RunOcr stores original note/document/caret and starts the clock; OCR later validates note membership, applies to original document/pane or background cached document, and can show/activate the pad (`N:334-345,349-402`). `Ocr.ClipboardHasImage` checks Windows Bitmap format; recognition reads that image asynchronously (`Platform/Ocr.cs:10-32`).

## 2. A39-motion-out-of-ui / A40-settings-change-event / A41a-hotkeys delta

- `git diff b54b476 HEAD` leaves the entire clipboard/OCR region unchanged; N's changes are settings writes in BindPanes, split and pin above it (`N:27-28,154-158,180,187`; capture remains `N:285-319`). Native/Ocr have no diff. R2's clipboard classification still applies (`R2:55,67,91`).
- A39-motion-out-of-ui (`14b08f9`): W gains Motion import; L gains Motion/Theme imports and host-provided ShowMenu, uses Editor.MenuGlyphs; K gains Motion import. Anim and ChromeTokens now live in Motion/Theme; MenuEntry lives in Editor (`W:10-14`; `L:10-15,119-129`; `K:5-8`; `Motion/Anim.cs:4-6`; `Theme/ChromeTokens.cs:1-4`; `Editor/MenuEntry.cs:1-12`). N's imports are unchanged, including Avalonia clipboard extensions (`N:1-8`).
- A40-settings-change-event (`1722c63`, `3d00b15`): W implements ISettingsHost and reads SettingsService.Current; interface forwarding includes hotkey registration, no clipboard method (`W:19,22,41-48,504-514`; `UI/ISettingsHost.cs:6-20`). Edited now calls the extracted four-line ScheduleSave seam; ordered settings reactions stay in K (`W:366-375`; `K:269-284`). None changes capture policy.
- A41a-hotkeys (`2677946`): replaces native slots with `_hotkeys`, constructs registrar with native factories and the existing dispatcher callbacks, forwards status/registration, disposes registrar immediately before clipboard hook (`W:29,35-54,291-292,450-452`). `Platform/HotkeyRegistrar.cs:6-54` separates registration adapter from deterministic ownership logic; ST adds HOTKEYS swap and three exact-source rules (`ST:27-29,283-285`). Clipboard listener remains window-owned (`W:121`).
- **The joined statements at W:121 are not an A41a leftover.** `git blame -L 117,122` attributes the line to `f20a3817` (V9-review-fixes, 2026-10-22); `git show f20a3817 -- product/Antipad/UI/PadWindow.cs` shows the join. It is already present in `b54b476` and untouched by `2677946`. Semicolon-separated statements execute in the same order; harmless formatting, leave it outside this audit's scope (`W:120-122`).

## 3. Pure policy boundary and proposed API

Propose **`Editor/ClipboardCapturePolicy.cs`**, `internal sealed class ClipboardCapturePolicy`, in `AntiNotepad.Editor`. This follows the explicit Editor proposal (`Plan:36,58`) and co-locates text formatting with editor helpers. Model would also satisfy today's dependency rules, but there is no note/store ownership here. Platform is appropriate for native hotkey ownership, not a reason to put every extracted policy there (`Platform/HotkeyRegistrar.cs:14-32`; `ST:275-288`). Do not place it in UI: the scanner includes the source type itself, making a whole-UI ban self-violate (`ST:324,404-420`).

Pure means deterministic string/state logic, not stateless. Move only the two raw nullable strings, own-copy recording, three initial filters, containment comparison, accepted-capture recording, and append formatting (`N:287-292,310-316`). No Note, PadView, TextDocument, Avalonia, Native, Ocr, Dispatcher, clock, callback or async API in this class. State stays per window, across target changes; rejection does not clear it, and successful acceptance records raw text before document effects (`N:287-313`).

Proposed phased API (preserves lazy PeekText and original operation order):

```csharp
void RecordOwnCopy(string text);                 // assign raw own text
TextCheck CheckText(string? text);               // Blank | OwnCopy | LastCaptured | Eligible
bool Decide(string text, string? existingText);  // eligible input only; contained => false;
                                                // else record raw last text and return true
static string FormatAppend(string text, bool endsWithLf);
```

`CheckText` is read-only and checks Blank -> OwnCopy -> LastCaptured. `Decide` requires a nonblank, eligible string, uses `(existingText ?? "").Contains(text.Trim())`, and changes last state only on true. `FormatAppend` returns `(endsWithLf ? "" : "\n") + "- " + text.Trim().Replace("\r\n", "\n").Replace("\n", "\n  ")`. Null existing text is a defined defensive API input; actual Note.PeekText returns non-null, including its unavailable-body marker (`Model/Store.cs:177-182`). These are proposals implementing `N:310-316`, not current methods.

Window integration: eagerly create one policy before InstallHooks; CopyText calls RecordOwnCopy before its existing Clipboard test/write. OnClipboardChanged retains N:298-309, then CheckText; only Eligible reads `target.PeekText()` and calls Decide. After true, retain `var doc = PadView.DocumentFor(target)`, call FormatAppend with `doc.Text.EndsWith('\n')`, then Insert, hidden-time update, Edited (`N:290-318`; eager construction constraint `Plan:45`). No await between CheckText and Decide.

Leave OCR-first, privacy-before-read, target selection, every await, PeekText acquisition, DocumentFor/cache access, actual Insert, visibility/time and Edited in the window (`N:298-318`). Do not pass `isPrivate` into this policy despite `Plan:82`: privacy is enforced before text acquisition, and a post-read boolean cannot prove that boundary. The plan's privacy-test claim (`Plan:40`) exceeds what this pure API tests.

## 4. Ordering constraints and existing interleaving

| Preserve | Reason / source |
|---|---|
| RecordOwnCopy before Clipboard lookup/SetTextAsync | An update can be observed while the write awaits; suppression must already know the own raw text. Preserve recording even on absent clipboard/write failure (`N:292-293`). |
| OCR image check before target/privacy/text path | Snip image belongs to the saved OCR target, even without any clipboard-mode note. True branch clears awaiting state before OCR and returns; private check presently guards text capture only (`N:298-307,340-353`). |
| Strict snip window `<120` seconds | Exactly 120 seconds falls through to text; expired state is not cleared by timeout alone. Keep DateTime.Now semantics and fallback pane (`N:298-302,336-345`). |
| Private check before awaited text read | Avoid reading opted-out text. Keep read exception handling; do not normalize/read before this check (`N:305-309`; `Platform/Native.cs:35-53`). |
| Blank/own/last before PeekText, then containment before last assignment | Suppressed events avoid loading the target body; contained text must not overwrite last-capture state. PeekText may consult live text or read body without retaining it (`N:310-313`; `Model/Store.cs:176-189`). |
| Raw last assignment before DocumentFor/Insert | Preserve repeat suppression even if document acquisition or insertion fails/re-enters. Do not defer recording until successful insert (`N:313-316`). |
| Containment uses target.PeekText, not doc.Text | It is the chosen note's current body, not active pane or post-cache snapshot; test against trimmed raw input before newline normalization (`N:304,312,314`; `PV:202-216`). |
| Trim -> CRLF-to-LF -> indent every LF -> join | Boundary whitespace is discarded, internal indent survives plus two spaces, bare CR survives. Existing document ending LF needs no separator; all other endings, including an empty document, get one leading LF (`N:315-316`). |
| Insert -> hidden clock -> Edited | Only accepted hidden capture resets idle duration; Edited schedules persistence/chrome effects after insertion (`N:316-318`; `W:366-375`). |

Existing hazard: each native event posts an independent discarded Task; image and text awaits permit later events, own writes, navigation/deletion or clipboard changes to interleave. No semaphore, clipboard sequence snapshot or post-await target/privacy revalidation exists (`W:121`; `N:298-318`). Text processing after the read has no explicit await, but selected target and privacy observation may already be stale. Keep this behavior; a pure state class neither serializes events nor fixes the privacy/read race. `Ocr.ClipboardHasImage` currently has no normally-suspending success path, but the awaited interface remains (`Platform/Ocr.cs:10-13`).

## 5. A42-padview-commands interplay

`git branch -a` contains `reorg/A42-padview-commands` at `8d6fab9`; no `claude/a42-padview-commands-*` ref is available locally. Its commits add investigation documents, no implementation. **A42 plan** below means `8d6fab9:.coord/tasks/A42-padview-commands-plan.md`; **A42 impact report** means `8d6fab9:.coord/reports/A42-padview-commands-impact.md`. Its plan explicitly uses old baseline `b54b476` (`A42 plan:3-6`).

**Correction to the A41b brief's DocumentFor premise:** A42's plan explicitly leaves the static cache and public PadWindow-facing facades in PadView and excludes cache extraction (`A42 plan:20-25,43`). Its impact report marks DocumentFor “stay,” and defers DocumentCache to an optional later task (`.coord/reports/A42-padview-commands-impact.md:88,206-207`). Therefore the available A42 plan requires **no edit** to clipboard `N:314` DocumentFor or `N:316` doc.Insert. A42's named Insert helper is list-continuation caret insertion, which stays in PadView with BringCaretToView (`A42 plan:39`; current `PV:860-864`), not clipboard append.

**Prefer A41b before A42**, matching A42's documented sequence (`A42 plan:5`). The earlier A41 plan's “after A42 or rebase” warning was conditional on A42 moving DocumentFor (`Plan:80`); current A42 scope excludes that move. Conflict surface: additive SelfTest table/rule edits and shifted line numbers; no expected shared capture-body edits (`A42 plan:36-43,92`; `N:310-318`). If a later revised A42 actually extracts the cache, its `N:314` adapter call and possibly `N:316` append become the two shared lines; keep policy independent of those APIs and rebase/re-audit. A42 itself still needs its latest-dev delta audit (`A42 plan:6`; impact report:237).

## 6. Concrete selftest table and LAYERS proposals

Add a small `CLIPBOARD policy` result alongside existing capability checks (`ST:19-29`; `Plan:55-60`). Below `C` = CheckText, `D` = Decide, `F` = FormatAppend; use fresh state per row unless explicitly sequential. Expected strings use escaped newlines.

| Case | Pure API input / expected result | Existing contract |
|---|---|---|
| Null, empty, whitespace | C(null), C(""), C(" \t\r\n") => Blank | `N:310` |
| Own raw text | RecordOwnCopy("x"); C("x") => OwnCopy; C(" x ") / C("X") => Eligible | `N:292,310` |
| Repeat last | D("x", "clipboard") => true; C("x") => LastCaptured; C(" x ") => Eligible | `N:310,313` |
| Already contained | D(" cat ", "clipboard\nconcatenate") => false; substring counts, no word boundaries. D("Cat", same) => true | `N:312` |
| Rejection keeps last | D("old", "clipboard") true; D("cat", "cat") false; C("old") remains LastCaptured, C("cat") Eligible | `N:312-313` |
| CRLF + internal indent | F(" \r\nalpha\r\n  beta\r\n ", false) => "\n- alpha\n    beta" | `N:315-316` |
| Internal LF / bare CR | F("a\nb", true) => "- a\n  b"; F("a\rb", true) => "- a\rb" | `N:315` |
| Trailing LF join | F(" x ", true) => "- x"; F(" x ", false) => "\n- x"; false also models empty document | `N:316` |
| Null existing text | D("x", null) true; F("x", false) => "\n- x"; defensive null contract only, not a nullable Note body | `Model/Store.cs:177-182`; `N:312,316` |
| Dedupe before formatting | D("a\r\nb", "a\nb") true: containment does not normalize CRLF. D(" a ", "a") false: it does trim | `N:312,315` |
| Check precedence | RecordOwnCopy(" "); C(" ") => Blank. D("x", "clipboard") true, RecordOwnCopy("x"), C("x") => OwnCopy, not LastCaptured | `N:310` |
| Record before effect | After D("x", "clipboard") true, simulate no insert; C("x") still LastCaptured. Own recording persists even with no simulated write | `N:292-293,313-316` |

The phased API tests check precedence directly. Window diff review must separately verify no PeekText for rejected inputs and recording-before-I/O; table tests cannot prove actual framework/native call ordering (`N:292-318`; `ST:344-365`).

Use existing `LayerRule` schema unchanged (`ST:96-104,404-420`), exact source `typeof(Editor.ClipboardCapturePolicy)`:

- Namespace targets `"UI"`, `"Platform"`, `"Motion"` fit directly; Platform covers Native/Ocr, UI covers PadWindow. Existing Editor->UI is already a broad guard (`ST:275,281-288,517-521`).
- Exact targets `typeof(Editor.PadView)` and `typeof(Program)` cover dependencies that those namespace rows miss. Optionally forbid `"Model"` to enforce the string-only API; there is no need for Note (`N:312` stays adapter-side).
- Exact external targets `typeof(Avalonia.Controls.Window)`, `typeof(Avalonia.Controls.Control)`, `typeof(Avalonia.Threading.Dispatcher)`, `typeof(AvaloniaEdit.Document.TextDocument)` are supported by ExactTo; external namespace strings such as `"Avalonia"` are not, because InNamespace prepends AntiNotepad (`ST:413-414,517-518`). These exact bans are useful examples, not proof of zero references to every external library type.
- Mirror A41a's exact-source rows rather than introducing another scanner (`ST:283-285`). Source/nested generated types, fields, signatures and IL token references are scanned; reflection strings/local-only types/attributes are not a comprehensive purity guarantee (`ST:318-339,404-420,543-562,618-642`). No LayerRule proves order or absence of runtime I/O hidden behind a delegate.

Future authorized implementation guard proof: one violation at a time inside the policy, e.g. temporary `Window?` field, `typeof(Native)` return, `typeof(Motion.Anim)` return, `typeof(PadView)` return, `typeof(Program)` return, and corresponding Dispatcher/Control/TextDocument/Model reference for each added row. A `typeof` expression emits a scanned type token, avoiding optimizer removal of unused locals (`ST:618-623`). Build/selftest must report the named nonzero `LAYERS ClipboardCapturePolicy->...` and fail; remove each mutation, rebuild and prove zero refs/PASS (`ST:344-365`; `Plan:60`). Do none of those mutations/builds in this audit.

## 7. Value check and recommendation

**Small value, optional, worth doing now only as a tightly scoped parity test.** The region is 33 lines (`N:287-319`); about eight lines' worth are pure: two fields, own assignment, initial suppression, containment predicate, last assignment, normalization, separator/bullet expression (`N:287-292,310-316`). PeekText and doc access stay outside; adapters and tests may increase total LOC. This is not a meaningful PadWindow size reduction.

The test removes a concrete risk of accidental own/repeat echo, changed substring semantics, normalized-vs-raw dedupe, lost indentation, and newline-join drift. It does **not** establish password-manager privacy, native notification reliability, correct target selection, OCR destination safety, await serialization or persistence (`N:298-318`; `Platform/Native.cs:35-53`). R2/Plan's privacy wording needs that qualification (`R2:91,129`; `Plan:40`).

No direct hotkey/show/hide change is needed. There is real indirect hot-path contact: accepted hidden capture sets `_hiddenAt` (`N:317`); ShowResident uses that timestamp to decide whether to OpenFreshNote (`W:142-143`). Hidden capture therefore postpones fresh-note creation. OCR can independently show/activate (`N:365-366`); Edited triggers save/chrome work (`W:366-375`). Preserve these effects and add no dispatcher hop.

Recommendation: complete A41b before A42 if continuing the approved sequence; skip/defer it if the only goal is line reduction. A42 does not currently create a clipboard conflict, so waiting for it buys little (`A42 plan:5,20-25,39`). Later implementation hand gate: hidden external capture, pad chip/whole-note copy suppression, standard-menu-copy bypass, opted-out text, snip then navigation/deletion, and idle reopen after hidden capture (`Plan:26,63`; `N:298-318,340-402`; `W:142-143`).

## 8. Uncertain / audit limits

- Source/history evidence only; no claim of current build/selftest success, actual Windows formats, OCR behavior, latency or focus. Relevant mechanisms are `W:117-123,137-169`, `N:298-318,363-402`, `Platform/Native.cs:19-53`.
- Clipboard sequence/privacy can change between check and read, selected target can become stale, discarded async tasks can overlap/fault; occurrence and frequency were not measured (`W:121`; `N:298-318`). No race fix is proposed here.
- Standard menu bypass is proven by local action routing (`PV:746`), whereas exact AvaloniaEdit keyboard/Cut clipboard behavior is not runtime-verified (`K:95,99`; `PV:745-746`). Do not silently route those through CopyText in this refactor.
- Only locally available A42 refs were inspected; its old plan could be revised later. Present evidence explicitly defers DocumentFor/cache moves (`8d6fab9:A42 plan:6,20-25`; A42 impact report:207).
- Null existing text is proposed defensive API behavior; real PeekText includes an unavailable-body marker rather than null. Preserve that marker by passing the actual PeekText result; no new body-read policy (`Model/Store.cs:177-189`).
- Final scope check: preserve the three initially untracked files (`.coord/reports/A41-padwindow-split-impact-r2.director.codex.md`, `.coord/reports/A41-padwindow-split-impact.director.codex.md`, `.coord/tasks/A41b-clipboard-policy-impact.md`). Only this report added; STATUS, product, git metadata and build outputs untouched. Verification: final `git status --short --untracked-files=all`, `git diff --name-only`, report line count.
