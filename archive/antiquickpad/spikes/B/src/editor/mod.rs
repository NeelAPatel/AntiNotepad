//! Source-preserving Markdown editor capability probe for Spike B.
//!
//! This keeps Markdown markers in the editable buffer. Headings and emphasis
//! markers are colored in place; task markers are edited as text by clicking
//! their source characters. It deliberately does not render a preview layer.

use std::{ops::Range, rc::Rc};

use gpui_kit::{
    base::input::{
        EditorState, FoldRange, HighlightStyleResolver, InputEdit, InputHighlighter, Rope,
    },
    component::input::Editor,
    *,
};

pub const FIXTURE: &str = "# AntiQuickPad scratchpad\n\
\n\
## Markdown stays editable\n\
This **bold text**, *italic text*, and ~~struck text~~ keep their visible source markers.\n\
\n\
- [ ] Click the [ ] marker to toggle the actual source text.\n\
- [ ] This deliberately long checkbox line demonstrates that the wrapped source editor keeps its marker and text together while the click target remains on the editable source: wrapped Markdown continues across the available editor width without switching to a separate preview or changing the underlying document.\n\
- [x] Click the [x] marker to toggle it back.\n\
\n\
Select several lines and press Tab or Shift+Tab to indent or outdent them.\n\
\n\
```python\n\
def greet(name):\n\
    message = f\"hello, {name}\"\n\
    return message\n\
```\n";

const LANG: &str = "antiquickpad-markdown";

/// Create the persistent source editor entity. The GPUI editor defaults to
/// soft wrapping, line numbers, folding, and two-space soft tabs.
pub fn create(text: impl Into<String>, window: &mut Window, cx: &mut App) -> Entity<EditorState> {
    let editor = cx.new(|cx| {
        EditorState::new(window, cx)
            .language(LANG)
            .soft_wrap(true)
            .line_number(true)
            .folding(true)
            .default_value(text.into())
    });
    editor.update(cx, |state, cx| {
        state.set_highlighter_factory(
            Rc::new(|language| {
                (language == LANG).then(|| {
                    Box::new(MarkdownSourceHighlighter::default()) as Box<dyn InputHighlighter>
                })
            }),
            cx,
        );
    });
    editor
}

/// Render the live editor and turn clicks on source checkbox markers into
/// undoable edits. The click is resolved by the editor's own source-byte
/// cursor mapping, so wrapped rows require no separate line geometry.
pub fn view(editor: &Entity<EditorState>) -> impl IntoElement {
    let state = editor.clone();
    div()
        .size_full()
        .on_mouse_down(MouseButton::Left, move |event, window, cx| {
            if event.click_count != 1 || event.modifiers.shift || event.modifiers.alt {
                return;
            }
            let _ = state.update(cx, |editor, cx| {
                let source = editor.text().to_string();
                let cursor = char_boundary_at_or_before(&source, editor.cursor());
                let line_start = source[..cursor].rfind('\n').map_or(0, |index| index + 1);
                let line_end = source[cursor..]
                    .find('\n')
                    .map_or(source.len(), |index| cursor + index);
                let line = &source[line_start..line_end];
                let marker_at = line.len() - line.trim_start().len();
                if !line[marker_at..].starts_with("- [") {
                    return;
                }
                let marker_start = line_start + marker_at + 3;
                let marker_end = marker_start + 1;
                let bytes = source.as_bytes();
                if !matches!(bytes.get(marker_start), Some(b' ' | b'x' | b'X'))
                    || bytes.get(marker_end) != Some(&b']')
                {
                    return;
                }
                // Reuse editor layout, including wrapping/scrolling, for a real marker hit.
                let hit_range = marker_start - 1..marker_end + 1;
                if !editor
                    .range_to_bounds(&hit_range)
                    .is_some_and(|bounds| bounds.contains(&event.position))
                {
                    return;
                }
                let current = &source[marker_start..marker_end];
                let replacement = if current == " " {
                    "x"
                } else if current.eq_ignore_ascii_case("x") {
                    " "
                } else {
                    return;
                };
                let old_cursor = cursor;
                editor.set_selected_range(marker_start..marker_end, cx);
                editor.replace(replacement, window, cx);
                editor.set_selected_range(old_cursor..old_cursor, cx);
            });
        })
        .child(
            Editor::new(editor)
                .h(relative(1.0))
                .appearance(false)
                .bordered(false),
        )
}

#[derive(Default)]
struct MarkdownSourceHighlighter {
    runs: Vec<(Range<usize>, &'static str)>,
    folds: Vec<FoldRange>,
}

impl InputHighlighter for MarkdownSourceHighlighter {
    fn language(&self) -> SharedString {
        LANG.into()
    }

    fn update(
        &mut self,
        _edit: Option<InputEdit>,
        text: &Rope,
        folding: bool,
        _window: &mut Window,
        _cx: &mut Context<EditorState>,
    ) {
        let source = text.to_string();
        let mut runs = Vec::new();
        let mut folds = Vec::new();
        let mut headings = Vec::new();
        let mut in_python = false;
        let mut fence_start = 0usize;
        let mut line_number = 0usize;
        for (line_start, line_with_newline) in
            source.split_inclusive('\n').scan(0usize, |offset, line| {
                let start = *offset;
                *offset += line.len();
                Some((start, line))
            })
        {
            let line = line_with_newline.trim_end_matches('\n');
            let trimmed = line.trim_start();
            let indent = line.len() - trimmed.len();
            let start = line_start + indent;
            let end = line_start + line.len();
            if trimmed.starts_with("```") {
                runs.push((start..end, "fence"));
                if in_python {
                    if folding && line_number > fence_start + 1 {
                        folds.push(FoldRange::new(fence_start, line_number));
                    }
                    in_python = false;
                } else {
                    in_python = trimmed[3..].trim().eq_ignore_ascii_case("python");
                    fence_start = line_number;
                }
            } else if in_python {
                python_runs(line_with_newline, line_start, &mut runs);
            } else if let Some(level) = heading_level(trimmed) {
                headings.push((line_number, level));
                runs.push((
                    start..end,
                    match level {
                        1 => "heading1",
                        2 => "heading2",
                        3 => "heading3",
                        4 => "heading4",
                        5 => "heading5",
                        _ => "heading6",
                    },
                ));
            } else {
                markdown_runs(line, line_start, &mut runs);
            }
            line_number += 1;
        }
        if folding {
            for (index, (start_line, level)) in headings.iter().copied().enumerate() {
                let end_line = headings[index + 1..]
                    .iter()
                    .find(|(_, next_level)| *next_level <= level)
                    .map_or(line_number.saturating_sub(1), |(line, _)| *line);
                if end_line > start_line + 1 {
                    folds.push(FoldRange::new(start_line, end_line));
                }
            }
        }
        runs.sort_by_key(|(range, _)| range.start);
        self.runs = runs;
        self.folds = folds;
    }

    fn styles(
        &self,
        range: &Range<usize>,
        resolver: &dyn HighlightStyleResolver,
    ) -> Vec<(Range<usize>, HighlightStyle)> {
        let mut result = Vec::new();
        let mut cursor = range.start;
        for (run, name) in &self.runs {
            if run.end <= range.start || run.start >= range.end {
                continue;
            }
            let start = run.start.max(range.start);
            let end = run.end.min(range.end);
            if start < cursor {
                continue;
            }
            if cursor < start {
                result.push((cursor..start, HighlightStyle::default()));
            }
            result.push((start..end, style(resolver, name)));
            cursor = end;
        }
        if cursor < range.end {
            result.push((cursor..range.end, HighlightStyle::default()));
        }
        result
    }

    fn fold_ranges(&self, _text: &Rope) -> Vec<FoldRange> {
        self.folds.clone()
    }
}

fn style(resolver: &dyn HighlightStyleResolver, name: &str) -> HighlightStyle {
    resolver.style(name).unwrap_or_else(|| {
        let (hue, weight) = match name {
            "heading1" => (0.56, 800),
            "heading2" => (0.48, 700),
            "heading3" => (0.12, 700),
            "heading4" => (0.78, 700),
            "heading5" => (0.32, 700),
            "heading6" => (0.06, 700),
            "fence" => (0.58, 600),
            "python_keyword" => (0.78, 700),
            "python_string" => (0.31, 500),
            "python_comment" => (0.0, 400),
            "python_number" => (0.12, 600),
            "bold" => (0.48, 700),
            "italic" => (0.48, 400),
            "strikethrough" => (0.0, 400),
            _ => (0.0, 400),
        };
        HighlightStyle {
            color: Some(hsla(hue, 0.55, 0.72, 1.0)),
            font_weight: (weight != 400).then_some(FontWeight(weight as f32)),
            font_style: (name == "italic").then_some(FontStyle::Italic),
            strikethrough: (name == "strikethrough").then_some(StrikethroughStyle {
                thickness: px(1.0),
                color: Some(hsla(hue, 0.55, 0.72, 1.0)),
            }),
            ..Default::default()
        }
    })
}

fn heading_level(line: &str) -> Option<usize> {
    let level = line.chars().take_while(|ch| *ch == '#').count();
    (1..=6).contains(&level).then_some(level)
}

fn markdown_runs(line: &str, base: usize, runs: &mut Vec<(Range<usize>, &'static str)>) {
    let mut offset = 0;
    while offset < line.len() {
        let remaining = &line[offset..];
        let (marker, kind) = if remaining.starts_with("**") {
            ("**", "bold")
        } else if remaining.starts_with("~~") {
            ("~~", "strikethrough")
        } else if remaining.starts_with('*') || remaining.starts_with('_') {
            (&line[offset..offset + 1], "italic")
        } else if remaining.starts_with('`') {
            ("`", "italic")
        } else {
            offset += line[offset..].chars().next().unwrap().len_utf8();
            continue;
        };
        let content_start = offset + marker.len();
        let Some(close_rel) = line[content_start..].find(marker) else {
            offset += marker.len();
            continue;
        };
        let end = content_start + close_rel + marker.len();
        runs.push((base + offset..base + end, kind));
        offset = end;
    }
}

fn char_boundary_at_or_before(source: &str, offset: usize) -> usize {
    let mut offset = offset.min(source.len());
    while !source.is_char_boundary(offset) {
        offset -= 1;
    }
    offset
}

fn python_runs(line: &str, base: usize, runs: &mut Vec<(Range<usize>, &'static str)>) {
    let bytes = line.as_bytes();
    let mut i = 0;
    while i < bytes.len() {
        if bytes[i] == b'#' {
            runs.push((base + i..base + bytes.len(), "python_comment"));
            break;
        }
        if bytes[i] == b'\'' || bytes[i] == b'"' {
            let quote = bytes[i];
            let start = i;
            i += 1;
            while i < bytes.len() && bytes[i] != quote {
                i += 1;
            }
            if i < bytes.len() {
                i += 1;
            }
            runs.push((base + start..base + i, "python_string"));
            continue;
        }
        if bytes[i].is_ascii_digit() {
            let start = i;
            i += 1;
            while i < bytes.len() && (bytes[i].is_ascii_digit() || bytes[i] == b'.') {
                i += 1;
            }
            runs.push((base + start..base + i, "python_number"));
            continue;
        }
        if bytes[i].is_ascii_alphabetic() || bytes[i] == b'_' {
            let start = i;
            i += 1;
            while i < bytes.len() && (bytes[i].is_ascii_alphanumeric() || bytes[i] == b'_') {
                i += 1;
            }
            if matches!(
                &line[start..i],
                "def"
                    | "return"
                    | "if"
                    | "else"
                    | "for"
                    | "in"
                    | "class"
                    | "import"
                    | "from"
                    | "as"
                    | "True"
                    | "False"
                    | "None"
                    | "and"
                    | "or"
                    | "not"
            ) {
                runs.push((base + start..base + i, "python_keyword"));
            }
            continue;
        }
        i += 1;
    }
}
