// A35d help overlay, based on UI/HelpOverlay.cs and UI/Shortcuts.cs.
(() => {
  const LAB = window.LAB;
  LAB.overlays = LAB.overlays || {};
  // HelpRows groups assigned defaults in table order, then appends fixed keys.
  const groups = [
    ['Anywhere', [
      ['Ctrl+Alt+Shift+S', 'Show / hide AntiNotepad (change in Settings).'],
      ['Esc', 'Hide the pad or close a panel.']
    ]],
    ['Notes', [
      ['Ctrl+N', 'Create a note.'],
      ['Alt+Right', 'Move to the next note; at the end, create a note. In a slot, move to the next slot.'],
      ['Alt+Left', 'Move to the previous note. In a slot, move to the previous slot.']
    ]],
    ['Search', [['Ctrl+F', 'Toggle Search.']]],
    ['Window', [
      ['F1', 'Show this shortcut list.'],
      ['Ctrl+T', 'Keep the pad above other windows.'],
      ['Ctrl+Shift+Q', 'Quit AntiNotepad.']
    ]],
    ['Slots', [['Ctrl+\\', 'Show a slot beside the current note.']]],
    ['Writing', [
      ['Ctrl++', 'Increase note text size.'],
      ['Ctrl+-', 'Decrease note text size.'],
      ['Ctrl+Shift+C', 'Copy all note text.'],
      ['Ctrl+Shift+O', 'Read text from a screen snip.'],
      ['Ctrl+Shift+[', 'Fold the block at the caret.'],
      ['Ctrl+Shift+]', 'Unfold the block at the caret.'],
      ['Enter', 'Continue a list or accept a completion.'],
      ['Tab / Shift+Tab', 'Indent list items at text start.'],
      ['Ctrl+[ / Ctrl+]', 'Change list level anywhere.'],
      ['Ctrl+V', 'Paste.'],
      ['Ctrl+Z / Ctrl+Y', 'Undo / redo (Ctrl+Z can undo a delete toast).'],
      ['Ctrl+Home', 'Scroll to the top of the note.'],
      ['Ctrl+click', 'Open a link.']
    ]],
    ['Line 1 modes', [
      ['math', 'Live answers, variables, units, %, sum'],
      ['list', 'Every line is a checkbox'],
      ['timer', '25m focus · Enter starts'],
      ['code py', 'Syntax colors'],
      ['clipboard', 'Collects what you copy']
    ]]
  ];
  LAB.overlays.help = {
    mount(host, ctx = {}) {
      // Cover the editor, while retaining the window's acrylic surface behind the sheet.
      const cover = document.createElement('div');
      Object.assign(cover.style, { position: 'absolute', inset: '40px 0 0' });
      LAB.backdrop(cover);
      const scroll = document.createElement('div');
      scroll.className = 'fl-scroll';
      scroll.tabIndex = 0;
      scroll.setAttribute('aria-label', 'Shortcuts');
      Object.assign(scroll.style, {
        position: 'absolute', inset: 'calc(40px + var(--snap-12-top-margin)) var(--snap-12-side-margin) 40px',
        overflowX: 'hidden', overflowY: 'auto', color: 'var(--ink)',
        fontFamily: "'Segoe UI Variable Text', 'Segoe UI', sans-serif"
      });
      LAB.mark(scroll, 12);
      const panel = document.createElement('div');
      panel.style.margin = '0 var(--snap-13-panel-right) 12px 8px';
      LAB.mark(panel, 13);
      const close = () => {
        if (typeof ctx.close === 'function') ctx.close();
        else if (typeof ctx.closeOverlay === 'function') ctx.closeOverlay();
        else host.replaceChildren();
      };
      panel.append(LAB.kit.pad.panelHeader('Shortcuts', close));
      for (const [title, rows] of groups) {
        panel.append(LAB.kit.settings.section(title));
        for (const [keys, description] of rows) {
          const row = document.createElement('div');
          Object.assign(row.style, {
            display: 'grid', gridTemplateColumns: 'minmax(150px, max-content) minmax(0, 1fr)',
            margin: '3px 8px', alignItems: 'stretch'
          });
          const key = document.createElement('div');
          key.textContent = keys;
          Object.assign(key.style, {
            background: 'light-dark(color-mix(in srgb, var(--ink) 6%, transparent), color-mix(in srgb, var(--ink) 9%, transparent))', borderRadius: '4px',
            padding: '2px 8px', fontFamily: "'Cascadia Mono', Consolas, monospace",
            fontSize: '12px', lineHeight: 'normal'
          });
          const text = document.createElement('div');
          text.textContent = description;
          Object.assign(text.style, {
            color: 'var(--muted)', fontSize: '12px', marginLeft: '12px',
            alignSelf: 'center', lineHeight: 'normal', overflowWrap: 'break-word'
          });
          row.append(key, text);
          panel.append(row);
        }
      }
      scroll.append(panel);
      host.replaceChildren(cover, scroll);
      scroll.focus({ preventScroll: true });
      return { unmount() { cover.remove(); scroll.remove(); } };
    }
  };
})();
